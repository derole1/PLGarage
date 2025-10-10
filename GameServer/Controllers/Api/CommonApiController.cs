using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GameServer.Implementation.Common;
using GameServer.Models.Config;
using GameServer.Models.PlayerData;
using GameServer.Models.PlayerData.PlayerCreations;
using GameServer.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace GameServer.Controllers.Api
{
    public class CommonApiController : Controller
    {
        private readonly Database database;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IUserStore<IdentityUser> userStore;

        public CommonApiController(Database database, SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, IUserStore<IdentityUser> userStore)
        {
            this.database = database;
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.userStore = userStore;
        }

        [HttpGet]
        [Route("api/GetInstanceName")]
        public IActionResult GetInstanceName()
        {
            return Content(ServerConfig.Instance.InstanceName);
        }

        [Route("api/Gateway")]
        public async Task StartServerCommunication()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                Guid ServerID = Guid.Empty;
                if (Request.Headers.TryGetValue("server_id", out StringValues server_id))
                    ServerID = Guid.Parse(server_id);
                await ServerCommunication.HandleConnection(webSocket, ServerID);
            }
        }

        [HttpGet]
        [Route("api/VotePackage")]
        public IActionResult GetVotingOptions(int trackId)
        {
            Guid ServerID = Guid.Empty;
            if (Request.Headers.TryGetValue("server_id", out StringValues server_id))
                ServerID = Guid.Parse(server_id);

            if (ServerCommunication.GetServer(ServerID) == null)
                return StatusCode(403);

            List<int> TrackIDs = [];

            Random random = new();
            var creations = database.PlayerCreations
                .Include(x => x.Downloads)
                .Where(match => match.Type == PlayerCreationType.TRACK && !match.IsMNR
                    && match.PlayerCreationId != trackId && match.Platform == Platform.PS3)
                .OrderByDescending(p => p.Downloads.Count)
                .Select(p => p.PlayerCreationId);

            var count = creations.Count();
            if (count > 3)
            {
                creations = creations.Skip(random.Next(count-3));
            }

            var list = creations.Take(3).ToList();
            foreach (var creation in list)
            {
                TrackIDs.Add(creation);
            }

            return Content(JsonConvert.SerializeObject(TrackIDs));
        }

        [HttpGet]
        [Route("api/player_count")]
        public IActionResult PlayerCount()
        {
            return Content(Session.GetSessions()
                .Where(x => x.LastPing.AddMinutes(1) < DateTime.UtcNow)
                .Count()
                .ToString());
        }

        [HttpPost]
        [Route("api/login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            var result = await signInManager.PasswordSignInAsync(username, password, false, false);
            if (result.Succeeded)
                return Ok();
            else
                return Unauthorized();
        }

        [HttpPost]
        [Route("api/register")]
        public async Task<IActionResult> Register(string username, string password)
        {
            if (!ServerConfig.Instance.EnableAccountRegistration)
                return Forbid();

            var user = Activator.CreateInstance<IdentityUser>();

            await userStore.SetUserNameAsync(user, username, CancellationToken.None);
            var result = await userManager.CreateAsync(user, password);

            if (result.Succeeded)
                return Ok();
            else
                return BadRequest();
        }

        [HttpPost]
        [Route("api/logout")]
        public async Task<IActionResult> Logout(string login, string password)
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            return Ok();
        }

        protected override void Dispose(bool disposing)
        {
            database.Dispose();
            base.Dispose(disposing);
        }
    }
}
