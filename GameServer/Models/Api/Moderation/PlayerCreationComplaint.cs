using System.ComponentModel.DataAnnotations.Schema;
using GameServer.Models.PlayerData;
using GameServer.Models.PlayerData.PlayerCreations;
using GameServer.Models.Request;

namespace GameServer.Models.Api.Moderation
{
    public class PlayerCreationComplaint
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }

        public int PlayerId { get; set; }
        public string PlayerUsername { get; set; }

        public int PlayerCreationId { get; set; }
        public string PlayerCreationName { get; set; }
        public string PlayerCreationDescription { get; set; }

        public PlayerComplaintReason Reason { get; set; }
        public string Comments { get; set; }
    }
}
