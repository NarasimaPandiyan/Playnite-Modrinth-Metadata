using Newtonsoft.Json;

namespace ModrinthModpacksMetadata.Models
{
    public class ModrinthTeamMember
    {
        [JsonProperty("team_id")]
        public string TeamId { get; set; }

        [JsonProperty("user")]
        public ModrinthUser User { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("accepted")]
        public bool Accepted { get; set; }
    }

    public class ModrinthUser
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; set; }

        [JsonProperty("bio")]
        public string Bio { get; set; }
    }
}
