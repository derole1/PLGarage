using System.ComponentModel.DataAnnotations.Schema;
using GameServer.Models.PlayerData;
using GameServer.Models.Request;

namespace GameServer.Models.Api.Moderation
{
    public class PlayerComplaint
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public string Username { get; set; }

        public int PlayerId { get; set; }
        public string PlayerUsername { get; set; }
        public string PlayerQuote { get; set; }

        public PlayerComplaintReason Reason { get; set; }
        public string Comments { get; set; }
    }
}
