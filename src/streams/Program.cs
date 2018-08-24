using System;
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;
using models;
using models.Model;

namespace streams
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var source0 =
                Source.ActorPublisher<User>(ApiActorPublisher<User>.Props(@"http://localhost:5000/api/users"))
                    .Select(x => new UserTuple(x, null, null));
            var source1 =
                Source.ActorPublisher<UserProperties>(
                        ApiActorPublisher<UserProperties>.Props(@"http://localhost:5000/api/properties"))
                    .Select(x => new UserTuple(null, x, null));
            var source2 =
                Source.ActorPublisher<UserAuthTypes>(
                        ApiActorPublisher<UserAuthTypes>.Props(@"http://localhost:5000/api/auth"))
                    .Select(x => new UserTuple(null, null, x));


            var graph = RunnableGraph.FromGraph(GraphDsl.Create(b =>
            {
                var merge = b.Add(new Merge<UserTuple>(3));
                var s0 = b.Add(source0);
                var s1 = b.Add(source1);
                var s2 = b.Add(source2);
                var throttling = Flow.Create<Account>()
                    .Throttle(1, TimeSpan.FromMilliseconds(500), 1, ThrottleMode.Shaping);

                var acc = Flow.Create<UserTuple>()
                    .Via(new KeyAccumulator<UserTuple, int?, Account>(
                        keySelector: x => x.Id(),
                        flushWhen: buffer => buffer.IsReady(),
                        mapper: buffer => buffer.Map()));

                var sink = b.Add(Sink.ForEach<Account>(Console.WriteLine));

                b.From(s0).To(merge.In(0));
                b.From(s1).To(merge.In(1));
                b.From(s2).To(merge.In(2));
                b.From(merge.Out).Via(acc).Via(throttling).To(sink);

                return ClosedShape.Instance;
            }));

            using (var system = ActorSystem.Create("system"))
            using (var materializer = system.Materializer())
            {
                graph.Run(materializer);

                Console.ReadLine();
            }
        }
    }
}