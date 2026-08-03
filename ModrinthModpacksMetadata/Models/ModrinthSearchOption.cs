using Playnite.SDK;

namespace ModrinthModpacksMetadata.Models
{
    public class ModrinthSearchOption : GenericItemOption
    {
        public ModrinthSearchHit Hit { get; set; }

        public ModrinthSearchOption(ModrinthSearchHit hit)
        {
            Hit = hit;
            Name = hit.Title;
            
            var details = new System.Collections.Generic.List<string>();
            if (!string.IsNullOrWhiteSpace(hit.Author))
            {
                details.Add($"By {hit.Author}");
            }
            if (hit.Downloads > 0)
            {
                details.Add($"{hit.Downloads:N0} downloads");
            }
            if (!string.IsNullOrWhiteSpace(hit.ProjectType))
            {
                details.Add($"[{hit.ProjectType}]");
            }

            Description = string.Join(" • ", details);
            if (!string.IsNullOrWhiteSpace(hit.Description))
            {
                Description += $"\n{hit.Description}";
            }
        }
    }
}
