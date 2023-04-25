using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Reflection;
using WesternRestApiCoreInMemory.Models;
using WesternRestApiCoreInMemory.Utils;

namespace WesternRestApiCoreInMemory.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GuidController : ControllerBase
    {
        private const string _userCacheKey = "USER_CACHE_KEY";
        private IAppSettings _appSettings;
        private ILogger<GuidController> _logger;

        private readonly IMemoryCache _cache;//https://learn.microsoft.com/en-us/aspnet/core/performance/caching/memory?view=aspnetcore-7.0
        public GuidController(IMemoryCache cache, IAppSettings appSettings, ILogger<GuidController> logger)
        {
            _cache = cache;
            _appSettings = appSettings;
            _logger = logger;
        }


        // POST: api/User
        [HttpPost]
        public IActionResult PostUser(User user)
        {
            _logger.LogInformation("Entered into PostUser.");
            User newUser = new User();
            //Set user name from user request
            newUser.Name = user.Name;

            //Check if received expiry is valid, if valid set expiry time
            if (HelperClass.IsValidExpiryDateTime(Convert.ToString(user.Expire)))
            {
                _logger.LogInformation($"PostUser - Using userProvided Expire value: {user.Expire}.");
                newUser.Expire = user.Expire;
            }
            else
            {
                _logger.LogInformation($"PostUser - Using default Expire value: {_appSettings.ExpiryInDays}.");
                newUser.Expire = (int?)HelperClass.ToUnixTime(DateTime.UtcNow.AddDays(Convert.ToDouble(_appSettings.ExpiryInDays)));
            }

            //Check if received Guid is valid, if not create new GUID and set 
            Guid userProvidedGuid;
            bool isValidGuid = Guid.TryParse(user.Guid, out userProvidedGuid);
            if (Guid.TryParse(user.Guid, out userProvidedGuid))
            {
                _logger.LogInformation($"PostUser - Using default Guid value: {user.Guid}.");
                newUser.Guid = userProvidedGuid.ToString("N").ToUpper();
            }
            else
            {                
                newUser.Guid = Guid.NewGuid().ToString("N").ToUpper();
                _logger.LogInformation($"PostUser - Using newly generated Guid value: {newUser.Guid}.");
            }

            if (!_cache.TryGetValue(_userCacheKey, out List<User> users))
            {
                users = new List<User>();
                users.Add(newUser);
                _cache.Set(_userCacheKey, users);
            }
            else
            {
                //if the user with same key exists, still it will add the users list with the same key (duplicate entry).
                users.Add(newUser);
                _cache.Set(_userCacheKey, users);
            }

            return CreatedAtAction(null, newUser);
        }


        // GET: api/User
        [HttpGet]
        public IEnumerable<User> GetUsers()
        {
            _logger.LogInformation($"Entered into GetUsers.");
            if (!_cache.TryGetValue(_userCacheKey, out List<User> users))
            {
                users = new List<User>();
                _cache.Set(_userCacheKey, users);
            }

            return users;
        }

        // GET: api/User/0B559EA1B6634044AC80883852808786
        [HttpGet("{id}")]
        public ActionResult<User> GetUser(string id)
        {
            _logger.LogInformation($"Entered into GetUser.");

            //Validate Guid
            Guid userProvidedGuid;
            bool isValidGuid = Guid.TryParse(id, out userProvidedGuid);
            if (!Guid.TryParse(id, out userProvidedGuid))
            {
                _logger.LogInformation($"GetUser - Invalid Guid: {id}. - BadRequest");
                return BadRequest();
            }

            if (!_cache.TryGetValue(_userCacheKey, out List<User> users))
            {
                users = new List<User>();
                _cache.Set(_userCacheKey, users);
            }

            var user = users.Find(u => u.Guid == id);
            if (user == null)
            {
                _logger.LogInformation($"GetUser - User not found for guid {id}.");
                return NotFound($"GetUser - User not found for guid {id}.");
            }

            if (HelperClass.IsActiveGuid(Convert.ToDouble(user.Expire)))
            {
                return user;
            }
            else
            {
                _logger.LogInformation($"GetUser - User expired {user.Expire}.");
                return BadRequest();
            }
        }



        // PUT: api/User/97059036D5844D669E487DE04642BB39
        [HttpPut("{id}")]
        public IActionResult PutUser(string id, User user)
        {
            _logger.LogInformation($"Entered into PutUser.");
            if (!_cache.TryGetValue(_userCacheKey, out List<User> users))
            {
                users = new List<User>();
                _cache.Set(_userCacheKey, users);
            }

            var index = users.FindIndex(u => u.Guid == id);

            if (index == -1)
            {
                _logger.LogInformation($"PutUser - User not found for Guid {id}.");
                return NotFound($"User not found for Guid {id}");
            }

            if (HelperClass.IsActiveGuid(Convert.ToDouble(user.Expire)))
            {
                user.Guid = id;//Guid is safe here.                 

                if (string.IsNullOrEmpty(user.Name))
                {
                    user.Name = users[index].Name;
                }
                users[index] = user;
            }
            else
            {
                _logger.LogInformation($"PutUser - User expired {user.Expire}.");
                return BadRequest();
            }

            return Ok(user);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteUser(string id)
        {
            _logger.LogInformation($"Entered into DeleteUser.");
            if (!_cache.TryGetValue(_userCacheKey, out List<User> users))
            {
                users = new List<User>();
                _cache.Set(_userCacheKey, users);
            }

            var index = users.FindIndex(u => u.Guid == id);

            if (index == -1)
            {
                _logger.LogInformation($"DeleteUser - User not found for Guid: {id}.");
                return NotFound($"User not found for Guid {id}");
            }

            users.RemoveAt(index);

            return Ok();
        }

    }
}
