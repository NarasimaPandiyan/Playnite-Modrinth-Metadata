using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ModrinthModpacksMetadata.Models;
using ModrinthModpacksMetadata.Services;
using ModrinthModpacksMetadata.Settings;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;

namespace ModrinthModpacksMetadata
{
    public class ModrinthMetadataProvider : OnDemandMetadataProvider
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly MetadataRequestOptions options;
        private readonly ModrinthModpacksMetadataPlugin plugin;
        private readonly ModrinthMetadataSettings settings;
        private readonly IPlayniteAPI playniteApi;

        private ModrinthProject cachedProject;
        private bool projectLoaded = false;

        public ModrinthMetadataProvider(MetadataRequestOptions options, ModrinthModpacksMetadataPlugin plugin)
        {
            this.options = options;
            this.plugin = plugin;
            this.settings = plugin.PluginSettings;
            this.playniteApi = plugin.PlayniteApi;
        }

        public override List<MetadataField> AvailableFields => new List<MetadataField>
        {
            MetadataField.Name,
            MetadataField.Description,
            MetadataField.CoverImage,
            MetadataField.BackgroundImage,
            MetadataField.Icon,
            MetadataField.Developers,
            MetadataField.Publishers,
            MetadataField.Genres,
            MetadataField.Tags,
            MetadataField.Links,
            MetadataField.ReleaseDate
        };

        private static string CleanSearchQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return string.Empty;
            }

            string cleaned = query.Trim();

            // Strips leading "minecraft", "minecraft:", "minecraft -", "minecraft --" case-insensitively
            cleaned = Regex.Replace(cleaned, @"^minecraft\s*[:\-]*\s*", "", RegexOptions.IgnoreCase);

            // Strips "(minecraft)" or "- minecraft" at the end
            cleaned = Regex.Replace(cleaned, @"\s*[\(\-]?\s*minecraft\s*[\)]?$", "", RegexOptions.IgnoreCase);

            cleaned = cleaned.Trim();
            return string.IsNullOrWhiteSpace(cleaned) ? query.Trim() : cleaned;
        }

        private static string GetHighResImageUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return url;
            }

            // Strip size suffixes like _96.png, _128.png, _305.webp, _400.png, _96.webp, _512.webp etc.
            // to fetch the original high-resolution image from Modrinth CDN.
            return Regex.Replace(url, @"_\d{2,4}\.(png|jpg|jpeg|webp)$", ".$1", RegexOptions.IgnoreCase);
        }

        private ModrinthProject GetProject()
        {
            if (projectLoaded)
            {
                return cachedProject;
            }

            projectLoaded = true;

            using (var client = new ModrinthApiClient(plugin.Version))
            {
                if (options.IsBackgroundDownload)
                {
                    string rawQuery = options.GameData.Name;
                    string queryName = CleanSearchQuery(rawQuery);
                    if (string.IsNullOrWhiteSpace(queryName))
                    {
                        return null;
                    }

                    var searchResponse = Task.Run(() => client.SearchProjectsAsync(queryName, settings.ProjectType, settings.MaxSearchResults)).GetAwaiter().GetResult();
                    if (searchResponse?.Hits == null || searchResponse.Hits.Count == 0)
                    {
                        logger.Info($"No Modrinth results found for background query: '{queryName}' (raw: '{rawQuery}')");
                        return null;
                    }

                    // Try exact slug or title match first
                    string sanitizedQuery = queryName.Trim().ToLowerInvariant().Replace(" ", "-");
                    var exactHit = searchResponse.Hits.FirstOrDefault(h => 
                        string.Equals(h.Slug, sanitizedQuery, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(h.Title, queryName, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(h.Title, rawQuery, StringComparison.OrdinalIgnoreCase)) 
                        ?? searchResponse.Hits.FirstOrDefault();

                    if (exactHit != null)
                    {
                        cachedProject = Task.Run(() => client.GetProjectAsync(exactHit.Slug)).GetAwaiter().GetResult();
                    }
                }
                else
                {
                    // Interactive / manual metadata download
                    string initialQuery = CleanSearchQuery(options.GameData?.Name ?? string.Empty);

                    List<GenericItemOption> SearchFunction(string query)
                    {
                        if (string.IsNullOrWhiteSpace(query))
                        {
                            return new List<GenericItemOption>();
                        }

                        string cleanedQuery = CleanSearchQuery(query);
                        var results = Task.Run(() => client.SearchProjectsAsync(cleanedQuery, settings.ProjectType, settings.MaxSearchResults)).GetAwaiter().GetResult();
                        if (results?.Hits == null)
                        {
                            return new List<GenericItemOption>();
                        }

                        return results.Hits.Select(h => (GenericItemOption)new ModrinthSearchOption(h)).ToList();
                    }

                    var selected = playniteApi.Dialogs.ChooseItemWithSearch(
                        SearchFunction(initialQuery),
                        SearchFunction,
                        initialQuery,
                        "Select Modrinth Modpack Metadata");

                    if (selected is ModrinthSearchOption searchOption && searchOption.Hit != null)
                    {
                        cachedProject = Task.Run(() => client.GetProjectAsync(searchOption.Hit.Slug)).GetAwaiter().GetResult();
                    }
                }
            }

            return cachedProject;
        }

        public override string GetName(GetMetadataFieldArgs args)
        {
            var project = GetProject();
            return project?.Title ?? base.GetName(args);
        }

        public override string GetDescription(GetMetadataFieldArgs args)
        {
            var project = GetProject();
            if (project == null)
            {
                return base.GetDescription(args);
            }

            if (settings.UseFullMarkdownDescription && !string.IsNullOrWhiteSpace(project.Body))
            {
                return MarkdownToHtmlConverter.Convert(project.Body);
            }

            return project.Description ?? base.GetDescription(args);
        }

        public override MetadataFile GetCoverImage(GetMetadataFieldArgs args)
        {
            var project = GetProject();
            if (project == null)
            {
                return base.GetCoverImage(args);
            }

            if (settings.CoverSource == "Gallery" && project.Gallery != null && project.Gallery.Count > 0)
            {
                var featured = project.Gallery.FirstOrDefault(g => g.Featured) ?? project.Gallery.FirstOrDefault();
                if (featured != null && !string.IsNullOrWhiteSpace(featured.Url))
                {
                    return new MetadataFile(GetHighResImageUrl(featured.Url));
                }
            }

            if (!string.IsNullOrWhiteSpace(project.IconUrl))
            {
                return new MetadataFile(GetHighResImageUrl(project.IconUrl));
            }

            return base.GetCoverImage(args);
        }

        public override MetadataFile GetBackgroundImage(GetMetadataFieldArgs args)
        {
            var project = GetProject();
            if (project == null)
            {
                return base.GetBackgroundImage(args);
            }

            if (settings.BackgroundSource == "Gallery" && project.Gallery != null && project.Gallery.Count > 0)
            {
                var featured = project.Gallery.FirstOrDefault(g => g.Featured) ?? project.Gallery.FirstOrDefault();
                if (featured != null && !string.IsNullOrWhiteSpace(featured.Url))
                {
                    return new MetadataFile(GetHighResImageUrl(featured.Url));
                }
            }

            if (!string.IsNullOrWhiteSpace(project.IconUrl))
            {
                return new MetadataFile(GetHighResImageUrl(project.IconUrl));
            }

            return base.GetBackgroundImage(args);
        }

        public override MetadataFile GetIcon(GetMetadataFieldArgs args)
        {
            var project = GetProject();
            if (project != null && !string.IsNullOrWhiteSpace(project.IconUrl))
            {
                return new MetadataFile(GetHighResImageUrl(project.IconUrl));
            }

            return base.GetIcon(args);
        }

        public override IEnumerable<MetadataProperty> GetDevelopers(GetMetadataFieldArgs args)
        {
            var project = GetProject();
            if (project == null)
            {
                return base.GetDevelopers(args);
            }

            var developers = new HashSet<MetadataProperty>();

            if (settings.FetchTeamMembers && !string.IsNullOrWhiteSpace(project.Team))
            {
                using (var client = new ModrinthApiClient(plugin.Version))
                {
                    var members = Task.Run(() => client.GetTeamMembersAsync(project.Team)).GetAwaiter().GetResult();
                    if (members != null && members.Count > 0)
                    {
                        foreach (var m in members)
                        {
                            if (m.User != null)
                            {
                                string name = !string.IsNullOrWhiteSpace(m.User.Name) ? m.User.Name : m.User.Username;
                                if (!string.IsNullOrWhiteSpace(name))
                                {
                                    developers.Add(new MetadataNameProperty(name));
                                }
                            }
                        }
                    }
                }
            }

            if (developers.Count == 0 && !string.IsNullOrWhiteSpace(project.Slug))
            {
                developers.Add(new MetadataNameProperty(project.Slug));
            }

            return developers;
        }

        public override IEnumerable<MetadataProperty> GetPublishers(GetMetadataFieldArgs args)
        {
            var project = GetProject();
            if (project == null)
            {
                return base.GetPublishers(args);
            }

            return new HashSet<MetadataProperty>
            {
                new MetadataNameProperty("Modrinth")
            };
        }

        public override IEnumerable<MetadataProperty> GetGenres(GetMetadataFieldArgs args)
        {
            var project = GetProject();
            if (project == null)
            {
                return base.GetGenres(args);
            }

            var genres = new HashSet<MetadataProperty>();
            var allCategories = new List<string>();
            if (project.Categories != null) allCategories.AddRange(project.Categories);
            if (project.AdditionalCategories != null) allCategories.AddRange(project.AdditionalCategories);

            foreach (var cat in allCategories.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(cat)) continue;
                string formatted = FormatName(cat);
                genres.Add(new MetadataNameProperty(formatted));
            }

            return genres;
        }

        public override IEnumerable<MetadataProperty> GetTags(GetMetadataFieldArgs args)
        {
            var project = GetProject();
            if (project == null)
            {
                return base.GetTags(args);
            }

            var tags = new HashSet<MetadataProperty>();

            // Modloader Tags
            if (settings.AddModloaderTags && project.Loaders != null)
            {
                foreach (var loader in project.Loaders)
                {
                    if (string.IsNullOrWhiteSpace(loader)) continue;
                    tags.Add(new MetadataNameProperty($"Modloader: {FormatName(loader)}"));
                }
            }

            // Environment Tags
            if (settings.AddEnvironmentTags)
            {
                if (!string.IsNullOrWhiteSpace(project.ClientSide))
                {
                    tags.Add(new MetadataNameProperty($"Client: {FormatName(project.ClientSide)}"));
                }
                if (!string.IsNullOrWhiteSpace(project.ServerSide))
                {
                    tags.Add(new MetadataNameProperty($"Server: {FormatName(project.ServerSide)}"));
                }
            }

            // Minecraft Version Tags
            if (settings.AddMinecraftVersionTags && project.GameVersions != null)
            {
                foreach (var version in project.GameVersions)
                {
                    if (string.IsNullOrWhiteSpace(version)) continue;
                    tags.Add(new MetadataNameProperty($"MC {version}"));
                }
            }

            // License Tag
            if (project.License != null)
            {
                string licName = project.License.Name ?? project.License.Id;
                if (!string.IsNullOrWhiteSpace(licName))
                {
                    tags.Add(new MetadataNameProperty($"License: {licName.ToUpperInvariant()}"));
                }
            }

            return tags;
        }

        public override IEnumerable<Link> GetLinks(GetMetadataFieldArgs args)
        {
            var project = GetProject();
            if (project == null)
            {
                return base.GetLinks(args);
            }

            var links = new List<Link>();

            if (!string.IsNullOrWhiteSpace(project.Slug))
            {
                string projectUrl = $"https://modrinth.com/{project.ProjectType ?? "modpack"}/{project.Slug}";
                links.Add(new Link("Modrinth Page", projectUrl));
            }

            if (!string.IsNullOrWhiteSpace(project.SourceUrl))
            {
                links.Add(new Link("Source Code", project.SourceUrl));
            }

            if (!string.IsNullOrWhiteSpace(project.IssuesUrl))
            {
                links.Add(new Link("Issue Tracker", project.IssuesUrl));
            }

            if (!string.IsNullOrWhiteSpace(project.WikiUrl))
            {
                links.Add(new Link("Wiki", project.WikiUrl));
            }

            if (!string.IsNullOrWhiteSpace(project.DiscordUrl))
            {
                links.Add(new Link("Discord", project.DiscordUrl));
            }

            if (project.DonationUrls != null)
            {
                foreach (var don in project.DonationUrls)
                {
                    if (!string.IsNullOrWhiteSpace(don.Url))
                    {
                        string label = !string.IsNullOrWhiteSpace(don.Platform) ? $"Donate ({don.Platform})" : "Donate";
                        links.Add(new Link(label, don.Url));
                    }
                }
            }

            return links;
        }

        public override ReleaseDate? GetReleaseDate(GetMetadataFieldArgs args)
        {
            var project = GetProject();
            if (project?.Published != null)
            {
                var pub = project.Published.Value;
                return new ReleaseDate(pub.Year, pub.Month, pub.Day);
            }

            return base.GetReleaseDate(args);
        }

        private static string FormatName(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
            string text = raw.Replace("-", " ").Replace("_", " ");
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text.ToLower());
        }
    }
}
