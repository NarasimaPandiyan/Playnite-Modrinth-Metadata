using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Controls;
using ModrinthModpacksMetadata.Settings;
using Playnite.SDK;
using Playnite.SDK.Plugins;

namespace ModrinthModpacksMetadata
{
    public class ModrinthModpacksMetadataPlugin : MetadataPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        public ModrinthMetadataSettings PluginSettings { get; private set; }
        public string Version { get; private set; }

        public override Guid Id => Guid.Parse("a5e4d29e-1736-4d2b-9878-7517c2f6d201");

        public override string Name => "Modrinth Modpacks";

        public override List<MetadataField> SupportedFields => new List<MetadataField>
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

        public ModrinthModpacksMetadataPlugin(IPlayniteAPI api) : base(api)
        {
            PluginSettings = new ModrinthMetadataSettings(this);
            Properties = new MetadataPluginProperties
            {
                HasSettings = true
            };

            try
            {
                Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
            }
            catch
            {
                Version = "1.0.0";
            }
        }

        public override OnDemandMetadataProvider GetMetadataProvider(MetadataRequestOptions options)
        {
            return new ModrinthMetadataProvider(options, this);
        }

        public override ISettings GetSettings(bool initialize)
        {
            return PluginSettings;
        }

        public override UserControl GetSettingsView(bool initialize)
        {
            return new ModrinthMetadataSettingsView();
        }
    }
}
