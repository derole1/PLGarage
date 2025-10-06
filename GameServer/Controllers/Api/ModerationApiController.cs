using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameServer.Implementation.Common;
using GameServer.Models.Api.Moderation;
using GameServer.Models.Config;
using GameServer.Models.PlayerData;
using GameServer.Models.PlayerData.PlayerCreations;
using GameServer.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace GameServer.Controllers.Common
{
    public class ModerationApiController : Controller
    {
        private readonly Database database;

        public ModerationApiController(Database database)
        {
            this.database = database;
        }

        [HttpPost]
        [Authorize(Roles = "Moderator")]
        [Route("/api/moderation/set_user_ban")]
        public IActionResult SetBan(int id, bool isBanned)
        {
            var user = database.Users
                .FirstOrDefault(match => match.UserId == id);

            if (user == null)
                return NotFound();

            user.IsBanned = isBanned;
            database.SaveChanges();

            return Ok();
        }

        [HttpPost]
        [Authorize(Roles = "Moderator")]
        [Route("/api/moderation/set_creation_status")]
        public IActionResult SetModerationStatus(int id, ModerationStatus status)
        {
            var creation = database.PlayerCreations
                .FirstOrDefault(match => match.PlayerCreationId == id);

            if (creation == null)
                return NotFound();

            creation.ModerationStatus = status;
            database.SaveChanges();

            return Ok();
        }

        [HttpGet]
        [Authorize(Roles = "Moderator")]
        [Route("/api/moderation/grief_reports")]
        public IActionResult GetGriefReports(string context, int? from)
        {
            return Json(database.GriefReports
                .Include(x => x.User)
                .Where(match => from == null || match.UserId == from)
                .Select(x => new GriefReport
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    Username = x.User.Username,
                    Context = x.Context,
                    Reason = x.Reason,
                    Comments = x.Comments
                }));
        }

        [HttpGet]
        [Authorize(Roles = "Moderator")]
        [Route("/api/moderation/grief_reports/{id}")]
        public IActionResult GetGriefReport(int id)
        {
            var report = database.GriefReports
                .Include(x => x.User)
                .FirstOrDefault(match => match.Id == id);
            if (report == null)
                return NotFound();
            else
                return Json(new GriefReport
                {
                    Id = report.Id,
                    UserId = report.UserId,
                    Username = report.User.Username,
                    Context = report.Context,
                    Reason = report.Reason,
                    Comments = report.Comments
                });
        }

        [HttpGet]
        [Authorize(Roles = "Moderator")]
        [Route("/api/moderation/grief_reports/{id}/data.xml")]
        public IActionResult GetGriefReportDataFile(int id)
        {
            var file = UserGeneratedContentUtils.LoadGriefReportData(id, "data.xml");

            if (file != null)
                return File(file, "application/xml;charset=utf-8");
            else
                return NotFound();
        }

        [HttpGet]
        [Authorize(Roles = "Moderator")]
        [Route("/api/moderation/grief_reports/{id}/preview.png")]
        public IActionResult GetGriefReportPreview(int id)
        {
            var file = UserGeneratedContentUtils.LoadGriefReportData(id, "preview.png");

            if (file != null)
                return File(file, "image/png");
            else
                return NotFound();
        }

        [HttpGet]
        [Authorize(Roles = "Moderator")]
        [Route("/api/moderation/player_complaints")]
        public IActionResult GetPlayerComplaints(int? from, int? playerID)
        {
            return Json(database.PlayerComplaints
                .Include(x => x.User)
                .Include(x => x.Player)
                .Where(match => from == null || match.UserId == from)
                .Select(x => new PlayerComplaint
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    Username = x.User.Username,
                    PlayerId = x.PlayerId,
                    PlayerUsername = x.Player.Username,
                    PlayerQuote = x.Player.Quote,
                    Reason = x.Reason,
                    Comments = x.Comments
                }));
        }

        [HttpGet]
        [Authorize(Roles = "Moderator")]
        [Route("/api/moderation/player_complaints/{id}")]
        public IActionResult GetPlayerComplaint(int id)
        {
            var report = database.PlayerComplaints
                .Include(x => x.User)
                .Include(x => x.Player)
                .FirstOrDefault(match => match.Id == id);
            if (report == null)
                return NotFound();
            else
                return Json(new PlayerComplaint
                {
                    Id = report.Id,
                    UserId = report.UserId,
                    Username = report.User.Username,
                    PlayerId = report.PlayerId,
                    PlayerUsername = report.Player.Username,
                    PlayerQuote = report.Player.Quote,
                    Reason = report.Reason,
                    Comments = report.Comments
                });
        }

        [HttpGet]
        [Authorize(Roles = "Moderator")]
        [Route("/api/moderation/player_creation_complaints")]
        public IActionResult GetPlayerCreationComplaints(int? from, int? playerID, int? playerCreationID)
        {
            return Json(database.PlayerCreationComplaints
                .Include(x => x.User)
                .Include(x => x.Player)
                .Include(x => x.PlayerCreation)
                .Where(match => from == null || match.UserId == from)
                .Select(x => new PlayerCreationComplaint
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    Username = x.User.Username,
                    PlayerId = x.PlayerId,
                    PlayerUsername = x.Player.Username,
                    PlayerCreationName = x.PlayerCreation.Name,
                    PlayerCreationDescription = x.PlayerCreation.Description,
                    Reason = x.Reason,
                    Comments = x.Comments
                }));
        }

        [HttpGet]
        [Authorize(Roles = "Moderator")]
        [Route("/api/moderation/player_creation_complaints/{id}")]
        public IActionResult GetPlayerCreationComplaint(int id)
        {
            var report = database.PlayerCreationComplaints
                .Include(x => x.User)
                .Include(x => x.Player)
                .Include(x => x.PlayerCreation)
                .FirstOrDefault(match => match.Id == id);
            if (report == null)
                return NotFound();
            else
                return Json(new PlayerCreationComplaint
                {
                    Id = report.Id,
                    UserId = report.UserId,
                    Username = report.User.Username,
                    PlayerId = report.PlayerId,
                    PlayerUsername = report.Player.Username,
                    PlayerCreationName = report.PlayerCreation.Name,
                    PlayerCreationDescription = report.PlayerCreation.Description,
                    Reason = report.Reason,
                    Comments = report.Comments
                });
        }

        [HttpGet]
        [Authorize(Roles = "Moderator")]
        [Route("/api/moderation/player_creation_complaints/{id}/preview.png")]
        public IActionResult GetPlayerCreationComplaintPreview(int id)
        {
            var file = UserGeneratedContentUtils.LoadPlayerCreationComplaintData(id, "preview.png");

            if (file != null)
                return File(file, "image/png");
            else
                return NotFound();
        }

        protected override void Dispose(bool disposing)
        {
            database.Dispose();
            base.Dispose(disposing);
        }
    }
}
