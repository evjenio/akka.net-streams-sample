using System;
using Akka.Streams;
using Akka.Streams.Stage;
using System.Collections.Generic;

namespace streams
{
    public class Accumulator<T> : GraphStage<FlowShape<T, IEnumerable<T>>>
    {
        private readonly Predicate<IEnumerable<T>> _predicate;

        public Accumulator(Predicate<IEnumerable<T>> predicate)
        {
            _predicate = predicate;
            Shape = new FlowShape<T, IEnumerable<T>>(In, Out);
        }

        public Inlet<T> In { get; } = new Inlet<T>("Accumulator.in");

        public Outlet<IEnumerable<T>> Out { get; } = new Outlet<IEnumerable<T>>("Accumulator.out");

        public override FlowShape<T, IEnumerable<T>> Shape { get; }

        protected override GraphStageLogic CreateLogic(Attributes inheritedAttributes) => new Logic(this);

        private sealed class Logic : InAndOutGraphStageLogic
        {
            private readonly Accumulator<T> _stage;
            private List<T> _buffer;

            public Logic(Accumulator<T> stage) 
                : base(stage.Shape)
            {
                _stage = stage;
                _buffer = new List<T>();
                SetHandler(stage.In, stage.Out, this);
            }

            /// <inheritdoc />
            public override void OnPush()
            {
                _buffer.Add(Grab(_stage.In));
                if (_stage._predicate(_buffer))
                {
                    PushAndClearBuffer();
                }
                else
                {
                    Pull(_stage.In);
                }
            }

            public override void OnUpstreamFinish()
            {
                if (_stage._predicate(_buffer))
                {
                    PushAndClearBuffer();
                }

                CompleteStage();
            }

            /// <inheritdoc />
            public override void OnPull()
            {
                Pull(_stage.In);
            }

            private void PushAndClearBuffer()
            {
                var buffer = _buffer;
                _buffer = new List<T>();
                Push(_stage.Out, buffer);
            }
        }
    }
}