using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Akka;
using Akka.Actor;
using Akka.Dispatch;
using Akka.Streams.Actors;
using Newtonsoft.Json;

namespace streams
{
    public sealed class ApiActorPublisher<T> : ActorPublisher<T>
    {
        private readonly Uri api;
        private readonly HttpClient httpClient;
        private IReadOnlyCollection<T> items;
        private int emited;

        public ApiActorPublisher(string api)
        {
            this.api = new Uri(api);
            httpClient = new HttpClient();
        }

        private async Task GetAsync()
        {
            if (items == null)
            {
                items = await GetItems();
            }

            var batch = items.Skip(emited).Take((int)TotalDemand).ToArray();
            if (batch.Length == 0)
            {
                OnCompleteThenStop();
            }
            foreach (var item in batch)
            {
                OnNext(item);
                emited++;
            }
        }

        private async Task<List<T>> GetItems()
        {
            string response = null;

            try
            {
                response = await httpClient.GetStringAsync(api);
            }
            catch (WebException cause)
            {
                OnErrorThenStop(cause);
            }
            catch (Exception cause)
            {
                OnError(cause);
            }

            return response == null ? new List<T>() : JsonConvert.DeserializeObject<List<T>>(response);
        }

        protected override void PostStop()
        {
            base.PostStop();
            httpClient?.Dispose();
        }

        public static Props Props(string api) =>
            Akka.Actor.Props.Create(() => new ApiActorPublisher<T>(api));

        protected override bool Receive(object message) => message.Match()
            .With<Request>(request =>
            {
                ActorTaskScheduler.RunTask(GetAsync);
            })
            .With<Cancel>(cancel => OnCompleteThenStop())
            .WasHandled;
    }
}