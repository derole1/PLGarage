using System;
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
    public class BombdApiController : Controller
    {
        private readonly Database database;
        private readonly SignInManager<IdentityUser> signInManager;

        public BombdApiController(Database database, SignInManager<IdentityUser> signInManager)
        {
            this.database = database;
            this.signInManager = signInManager;
        }

        [HttpGet]
        [Route("api/bombd/player_count")]
        public IActionResult PlayerCount()
        {
            return Content("0");
        }

        [HttpGet]
        [Route("api/bombd/game_room_count")]
        public IActionResult GameRoomCount(string type)
        {
            switch (type)
            {
                default:
                    return Content("0");
            }
        }

        protected override void Dispose(bool disposing)
        {
            database.Dispose();
            base.Dispose(disposing);
        }
    }
}
