using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GameServer.Models.PlayerData;

namespace GameServer.Models.Api.Moderation
{
    public class GriefReport
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public string Username { get; set; }

        public string Context { get; set; }
        public string Reason { get; set; }
        public string Comments { get; set; }
    }
}
