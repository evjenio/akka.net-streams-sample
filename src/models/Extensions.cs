using System;
using System.Collections.Generic;
using System.Linq;
using models.Model;

namespace models
{
    public static class Extensions
    {
        public static bool IsReady(this IEnumerable<UserTuple> buffer)
        {
            return buffer.Any(i => i.Item1 != null) &&
                   buffer.Any(i => i.Item2 != null) &&
                   buffer.Any(i => i.Item3 != null);
        }

        public static Account Map(this IEnumerable<UserTuple> buffer)
        {
            if (buffer.GroupBy(x => x.Id()).Count() != 1)
            {
                throw new InvalidOperationException();
            }

            return new Account
            {
                Id = (int) buffer.First().Id(),
                AuthTypes = buffer.First(x => x.Item3 != null).Item3.AuthTypes,
                Email = buffer.First(x => x.Item2 != null).Item2.Email,
                Name = buffer.First(x => x.Item1 != null).Item1.Name,
                Phone = buffer.First(x => x.Item2 != null).Item2.Phone
            };
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> enumerable)
        {
            var list = enumerable.ToList();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            return list;
        }
    }
}