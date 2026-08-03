using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ModrinthModpacksMetadata.Models
{
    public class ModrinthProject
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("project_type")]
        public string ProjectType { get; set; }

        [JsonProperty("team")]
        public string Team { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("published")]
        public DateTime? Published { get; set; }

        [JsonProperty("updated")]
        public DateTime? Updated { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("license")]
        public ModrinthLicense License { get; set; }

        [JsonProperty("client_side")]
        public string ClientSide { get; set; }

        [JsonProperty("server_side")]
        public string ServerSide { get; set; }

        [JsonProperty("downloads")]
        public long Downloads { get; set; }

        [JsonProperty("followers")]
        public long Followers { get; set; }

        [JsonProperty("categories")]
        public List<string> Categories { get; set; } = new List<string>();

        [JsonProperty("additional_categories")]
        public List<string> AdditionalCategories { get; set; } = new List<string>();

        [JsonProperty("game_versions")]
        public List<string> GameVersions { get; set; } = new List<string>();

        [JsonProperty("loaders")]
        public List<string> Loaders { get; set; } = new List<string>();

        [JsonProperty("icon_url")]
        public string IconUrl { get; set; }

        [JsonProperty("issues_url")]
        public string IssuesUrl { get; set; }

        [JsonProperty("source_url")]
        public string SourceUrl { get; set; }

        [JsonProperty("wiki_url")]
        public string WikiUrl { get; set; }

        [JsonProperty("discord_url")]
        public string DiscordUrl { get; set; }

        [JsonProperty("donation_urls")]
        public List<ModrinthDonationUrl> DonationUrls { get; set; } = new List<ModrinthDonationUrl>();

        [JsonProperty("gallery")]
        public List<ModrinthGalleryImage> Gallery { get; set; } = new List<ModrinthGalleryImage>();
    }

    public class ModrinthLicense
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class ModrinthDonationUrl
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("platform")]
        public string Platform { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class ModrinthGalleryImage
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("featured")]
        public bool Featured { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("ordering")]
        public int Ordering { get; set; }
    }
}
