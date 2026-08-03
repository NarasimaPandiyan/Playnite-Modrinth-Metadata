using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ModrinthModpacksMetadata.Models
{
    public class ModrinthSearchResponse
    {
        [JsonProperty("hits")]
        public List<ModrinthSearchHit> Hits { get; set; } = new List<ModrinthSearchHit>();

        [JsonProperty("offset")]
        public int Offset { get; set; }

        [JsonProperty("limit")]
        public int Limit { get; set; }

        [JsonProperty("total_hits")]
        public int TotalHits { get; set; }
    }

    public class ModrinthSearchHit
    {
        [JsonProperty("project_id")]
        public string ProjectId { get; set; }

        [JsonProperty("project_type")]
        public string ProjectType { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("categories")]
        public List<string> Categories { get; set; } = new List<string>();

        [JsonProperty("display_categories")]
        public List<string> DisplayCategories { get; set; } = new List<string>();

        [JsonProperty("versions")]
        public List<string> Versions { get; set; } = new List<string>();

        [JsonProperty("downloads")]
        public long Downloads { get; set; }

        [JsonProperty("follows")]
        public long Follows { get; set; }

        [JsonProperty("icon_url")]
        public string IconUrl { get; set; }

        [JsonProperty("date_created")]
        public DateTime? DateCreated { get; set; }

        [JsonProperty("date_modified")]
        public DateTime? DateModified { get; set; }

        [JsonProperty("latest_version")]
        public string LatestVersion { get; set; }

        [JsonProperty("license")]
        public string License { get; set; }

        [JsonProperty("client_side")]
        public string ClientSide { get; set; }

        [JsonProperty("server_side")]
        public string ServerSide { get; set; }

        [JsonProperty("gallery")]
        public List<string> Gallery { get; set; } = new List<string>();

        [JsonProperty("featured_gallery")]
        public string FeaturedGallery { get; set; }
    }
}
