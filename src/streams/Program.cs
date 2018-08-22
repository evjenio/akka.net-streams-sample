using System;
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;
using models.Model;

namespace streams
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var source0 =
                Source.ActorPublisher<User>(ApiActorPublisher<User>.Props(@"http://localhost:5000/api/users"));
            var source1 =
                Source.ActorPublisher<UserProperties>(
                    ApiActorPublisher<UserProperties>.Props(@"http://localhost:5000/api/properties"));
            var source2 =
                Source.ActorPublisher<UserAuthTypes>(
                    ApiActorPublisher<UserAuthTypes>.Props(@"http://localhost:5000/api/auth"));

            var graph = RunnableGraph.FromGraph(GraphDsl.Create(b =>
            {
                var zip = b.Add(ZipWith.Apply<User, UserProperties, UserAuthTypes, Account>(AccountZipper.Zip));
                var s0 = b.Add(source0);
                var s1 = b.Add(source1);
                var s2 = b.Add(source2);
                var throttling = Flow.Create<Account>()
                    .Throttle(1, TimeSpan.FromMilliseconds(500), 1, ThrottleMode.Shaping);
                var sink = b.Add(Sink.ForEach<Account>(Console.WriteLine));

                b.From(s0).To(zip.In0);
                b.From(s1).To(zip.In1);
                b.From(s2).To(zip.In2);
                b.From(zip.Out).Via(throttling).To(sink);

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