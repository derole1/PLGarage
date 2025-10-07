﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameServer.Implementation.Common;
using GameServer.Models.Config;
using GameServer.Models.PlayerData;
using GameServer.Models.PlayerData.PlayerCreations;
using GameServer.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
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

        public CommonApiController(Database database, SignInManager<IdentityUser> signInManager)
        {
            this.database = database;
            this.signInManager = signInManager;
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
                .Count()
                .ToString());
        }

        [HttpPost]
        [Route("api/login")]
        public async Task<IActionResult> Login(string login, string password)
        {
            var result = await signInManager.PasswordSignInAsync(login, password, false, false);
            if (result.Succeeded)
                return Ok();
            else
                return Unauthorized();
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
