using System.Linq;
using models;
using models.Model;
using Microsoft.AspNetCore.Mvc;

namespace stubs.Controllers
{
    [Route("api")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        [HttpGet("auth")]
        public UserAuthTypes[] AuthTypes()
        {
            return Enumerable.Range(0, 20).Select(n => new UserAuthTypes
            {
                Id = n,
                AuthTypes = new[] {$"bearer{n}", $"password{n}"}
            }).Shuffle().ToArray();
        }

        [HttpGet("properties")]
        public UserProperties[] Properties()
        {
            return Enumerable.Range(0, 20).Select(n => new UserProperties
            {
                Id = n,
                Email = $"User{n}@example.com",
                Phone = n.GetHashCode().ToString()
            }).Shuffle().ToArray();
        }

        [HttpGet("users")]
        public User[] Users()
        {
            return Enumerable.Range(0, 20).Select(n => new User
            {
                Id = n,
                Name = $"User{n}"
            }).Shuffle().ToArray();
        }
    }
}