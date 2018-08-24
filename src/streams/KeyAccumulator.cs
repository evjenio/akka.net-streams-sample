using System;
using Akka.Streams;
using Akka.Streams.Stage;
using System.Collections.Generic;

namespace streams
{
    public class KeyAccumulator<TIn, TKey, TOut> : GraphStage<FlowShape<TIn, TOut>>
    {
        private readonly Func<TIn, TKey> _keySelector;
        private readonly Predicate<IEnumerable<TIn>> _flushWhen;
        private readonly Func<IEnumerable<TIn>, TOut> _mapper;

        public KeyAccumulator(Func<TIn,TKey> keySelector, Predicate<IEnumerable<TIn>> flushWhen, Func<IEnumerable<TIn>, TOut> mapper)
        {
            _keySelector = keySelector;
            _flushWhen = flushWhen;
            _mapper = mapper;
            Shape = new FlowShape<TIn, TOut>(In, Out);
        }

        public Inlet<TIn> In { get; } = new Inlet<TIn>("KeyAccumulator.in");

        public Outlet<TOut> Out { get; } = new Outlet<TOut>("KeyAccumulator.out");

        public override FlowShape<TIn, TOut> Shape { get; }

        protected override GraphStageLogic CreateLogic(Attributes inheritedAttributes) => new Logic(this);

        private sealed class Logic : InAndOutGraphStageLogic
        {
            private readonly KeyAccumulator<TIn, TKey, TOut> _stage;
            private readonly IDictionary<TKey, List<TIn>> _buffers;

            public Logic(KeyAccumulator<TIn, TKey, TOut> stage) 
                : base(stage.Shape)
            {
                _stage = stage;
                _buffers = new Dictionary<TKey, List<TIn>>();
                SetHandler(stage.In, stage.Out, this);
            }

            /// <inheritdoc />
            public override void OnPush()
            {
                var item = Grab(_stage.In);
                var key = _stage._keySelector(item);
                if (!_buffers.ContainsKey(key))
                {
                    _buffers[key] = new List<TIn>();
                }

                var buffer = _buffers[key];
                buffer.Add(item);

                if (_stage._flushWhen(buffer))
                {
                    PushAndClearBuffer(key);
                }
                else
                {
                    Pull(_stage.In);
                }
            }

            public override void OnUpstreamFinish()
            {
                CompleteStage();
            }

            /// <inheritdoc />
            public override void OnPull()
            {
                Pull(_stage.In);
            }

            private void PushAndClearBuffer(TKey key)
            {
                var buffer = _buffers[key];
                _buffers.Remove(key);
                Push(_stage.Out, _stage._mapper(buffer));
            }
        }
    }
}