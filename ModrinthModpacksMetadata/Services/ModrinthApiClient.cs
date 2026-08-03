using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using ModrinthModpacksMetadata.Models;
using Newtonsoft.Json;
using Playnite.SDK;

namespace ModrinthModpacksMetadata.Services
{
    public class ModrinthApiClient : IDisposable
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly HttpClient httpClient;
        private const string BaseUrl = "https://api.modrinth.com/v2/";

        public ModrinthApiClient(string userAgentVersion = "1.0.0")
        {
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(BaseUrl);
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd($"Playnite-Modrinth-Metadata-Plugin/{userAgentVersion} (Playnite Metadata Extension)");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.Timeout = TimeSpan.FromSeconds(15);
        }

        public async Task<ModrinthSearchResponse> SearchProjectsAsync(string query, string projectTypeFilter = "modpack", int limit = 10)
        {
            try
            {
                var builder = new UriBuilder(BaseUrl + "search");
                var queryParams = HttpUtility.ParseQueryString(string.Empty);
                
                if (!string.IsNullOrWhiteSpace(query))
                {
                    queryParams["query"] = query.Trim();
                }

                queryParams["limit"] = limit.ToString();

                if (!string.IsNullOrWhiteSpace(projectTypeFilter) && projectTypeFilter != "all")
                {
                    // Build facets JSON array, e.g. [["project_type:modpack"]] or [["project_type:modpack", "project_type:mod"]]
                    var types = projectTypeFilter.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    var facetList = new List<string>();
                    foreach (var t in types)
                    {
                        facetList.Add($"\"project_type:{t.Trim()}\"");
                    }

                    string facetJson = $"[[{string.Join(",", facetList)}]]";
                    queryParams["facets"] = facetJson;
                }

                builder.Query = queryParams.ToString();

                var response = await httpClient.GetAsync(builder.Uri);
                if (!response.IsSuccessStatusCode)
                {
                    logger.Warn($"Modrinth search failed with status {response.StatusCode} for query: {query}");
                    return new ModrinthSearchResponse();
                }

                string json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ModrinthSearchResponse>(json) ?? new ModrinthSearchResponse();
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error searching Modrinth for query: '{query}'");
                return new ModrinthSearchResponse();
            }
        }

        public async Task<ModrinthProject> GetProjectAsync(string idOrSlug)
        {
            if (string.IsNullOrWhiteSpace(idOrSlug))
            {
                return null;
            }

            try
            {
                string encodedId = HttpUtility.UrlEncode(idOrSlug.Trim().ToLowerInvariant());
                var response = await httpClient.GetAsync($"project/{encodedId}");
                if (!response.IsSuccessStatusCode)
                {
                    logger.Warn($"Modrinth get project failed with status {response.StatusCode} for ID/Slug: {idOrSlug}");
                    return null;
                }

                string json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ModrinthProject>(json);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error fetching Modrinth project: '{idOrSlug}'");
                return null;
            }
        }

        public async Task<List<ModrinthTeamMember>> GetTeamMembersAsync(string teamId)
        {
            if (string.IsNullOrWhiteSpace(teamId))
            {
                return new List<ModrinthTeamMember>();
            }

            try
            {
                var response = await httpClient.GetAsync($"team/{teamId}/members");
                if (!response.IsSuccessStatusCode)
                {
                    return new List<ModrinthTeamMember>();
                }

                string json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<ModrinthTeamMember>>(json) ?? new List<ModrinthTeamMember>();
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error fetching Modrinth team members for team: '{teamId}'");
                return new List<ModrinthTeamMember>();
            }
        }

        public void Dispose()
        {
            httpClient?.Dispose();
        }
    }
}
