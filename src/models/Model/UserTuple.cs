using System;
using System.Linq;

namespace models.Model
{
    public class UserTuple : Tuple<User, UserProperties, UserAuthTypes>
    {
        public UserTuple(User item1, UserProperties item2, UserAuthTypes item3)
            : base(item1, item2, item3)
        {
        }

        public int? Id() => new IIdentity[] {Item1, Item2, Item3}.Select(x => x?.Id).FirstOrDefault(x => x.HasValue);
    }
}