using System.Collections.Generic;
using Playnite.SDK;
using Playnite.SDK.Data;

namespace ModrinthModpacksMetadata.Settings
{
    public class ModrinthMetadataSettings : ObservableObject, ISettings
    {
        private readonly ModrinthModpacksMetadataPlugin plugin;

        private string projectType = "modpack";
        public string ProjectType
        {
            get => projectType;
            set => SetValue(ref projectType, value);
        }

        private string coverSource = "Icon";
        public string CoverSource
        {
            get => coverSource;
            set => SetValue(ref coverSource, value);
        }

        private string backgroundSource = "Gallery";
        public string BackgroundSource
        {
            get => backgroundSource;
            set => SetValue(ref backgroundSource, value);
        }

        private int maxSearchResults = 10;
        public int MaxSearchResults
        {
            get => maxSearchResults;
            set => SetValue(ref maxSearchResults, value);
        }

        private bool addMinecraftVersionTags = true;
        public bool AddMinecraftVersionTags
        {
            get => addMinecraftVersionTags;
            set => SetValue(ref addMinecraftVersionTags, value);
        }

        private bool addModloaderTags = true;
        public bool AddModloaderTags
        {
            get => addModloaderTags;
            set => SetValue(ref addModloaderTags, value);
        }

        private bool addEnvironmentTags = true;
        public bool AddEnvironmentTags
        {
            get => addEnvironmentTags;
            set => SetValue(ref addEnvironmentTags, value);
        }

        private bool useFullMarkdownDescription = true;
        public bool UseFullMarkdownDescription
        {
            get => useFullMarkdownDescription;
            set => SetValue(ref useFullMarkdownDescription, value);
        }

        private bool fetchTeamMembers = true;
        public bool FetchTeamMembers
        {
            get => fetchTeamMembers;
            set => SetValue(ref fetchTeamMembers, value);
        }

        // Editing state copy
        private ModrinthMetadataSettings editingClone;

        public ModrinthMetadataSettings()
        {
        }

        public ModrinthMetadataSettings(ModrinthModpacksMetadataPlugin plugin)
        {
            this.plugin = plugin;
            var savedSettings = plugin.LoadPluginSettings<ModrinthMetadataSettings>();
            if (savedSettings != null)
            {
                ProjectType = savedSettings.ProjectType;
                CoverSource = savedSettings.CoverSource;
                BackgroundSource = savedSettings.BackgroundSource;
                MaxSearchResults = savedSettings.MaxSearchResults;
                AddMinecraftVersionTags = savedSettings.AddMinecraftVersionTags;
                AddModloaderTags = savedSettings.AddModloaderTags;
                AddEnvironmentTags = savedSettings.AddEnvironmentTags;
                UseFullMarkdownDescription = savedSettings.UseFullMarkdownDescription;
                FetchTeamMembers = savedSettings.FetchTeamMembers;
            }
        }

        public void BeginEdit()
        {
            editingClone = Serialization.GetClone(this);
        }

        public void CancelEdit()
        {
            if (editingClone != null)
            {
                ProjectType = editingClone.ProjectType;
                CoverSource = editingClone.CoverSource;
                BackgroundSource = editingClone.BackgroundSource;
                MaxSearchResults = editingClone.MaxSearchResults;
                AddMinecraftVersionTags = editingClone.AddMinecraftVersionTags;
                AddModloaderTags = editingClone.AddModloaderTags;
                AddEnvironmentTags = editingClone.AddEnvironmentTags;
                UseFullMarkdownDescription = editingClone.UseFullMarkdownDescription;
                FetchTeamMembers = editingClone.FetchTeamMembers;
            }
        }

        public void EndEdit()
        {
            plugin.SavePluginSettings(this);
        }

        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();
            return true;
        }
    }
}
