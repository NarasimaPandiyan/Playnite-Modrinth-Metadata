# Modrinth Modpacks Metadata Plugin for Playnite

A feature-rich **Playnite Metadata Extension** that fetches detailed metadata for **Minecraft Modpacks** directly from the official **Modrinth API v2 (Labrinth)**.

![Icon](icon.png)

## Features

- 🔍 **Interactive & Automatic Search**:
  - **Manual/Interactive Download**: Triggers a live search modal with instant search suggestions as you type, allowing you to select the exact Modrinth project.
  - **Background Sync**: Intelligently matches your Playnite library game titles using exact slug/title matching or top Modrinth search results without interrupting your workflow.
- 🎨 **Rich Visuals**:
  - **Cover Image**: Project Icon or First Featured Gallery Screenshot.
  - **Background Image**: First Featured Gallery Screenshot or Project Icon.
  - **Icon**: Project Icon.
- 📝 **Formatted Descriptions**:
  - Automatically converts full Modrinth project Markdown descriptions (`body`) into clean HTML rendered natively in Playnite's game details panel.
- 🏷️ **Categorization & Tagging**:
  - **Genres**: Modpack categories (e.g. *Optimization*, *Technology*, *Magic*, *Adventure*, *Quests*, *RPG*, *Utility*).
  - **Tags**: 
    - Modloader support (e.g. `Modloader: Fabric`, `Modloader: Forge`, `Modloader: NeoForge`, `Modloader: Quilt`).
    - Environment compatibility (e.g. `Client: Required`, `Server: Optional`).
    - Supported Minecraft versions (e.g. `MC 1.20.1`, `MC 1.20.4`).
    - Project License (e.g. `License: MIT`).
- 👥 **Developers & Publishers**:
  - Fetches project team members and contributors from Modrinth's team API.
  - Sets Publisher to **Modrinth**.
- 🔗 **Comprehensive Links**:
  - Modrinth Project Page
  - Source Code repository
  - Issue Tracker
  - Wiki
  - Discord server
  - Donation links (Patreon, Ko-fi, etc.)
- 📅 **Release Date**:
  - Exact project publication date.
- ⚙️ **Customizable Settings**:
  - Choose between filtering by *Modpacks Only*, *Mods & Modpacks*, or *All Projects*.
  - Choose default Cover and Background image sources.
  - Toggle specific tag categories (Modloaders, Environment, Minecraft Versions).
  - Toggle full Markdown description conversion vs 1-line summary.

---

## Project Structure

```
ModrinthModpacksMetadata/
├── ModrinthModpacksMetadata.csproj    # SDK-style .NET Framework 4.6.2 project
├── extension.yaml                     # Playnite plugin manifest
├── icon.png                           # 64x64 extension icon
├── ModrinthModpacksMetadataPlugin.cs # Plugin entry point & lifecycle
├── ModrinthMetadataProvider.cs        # OnDemandMetadataProvider implementation
├── Models/
│   ├── ModrinthProject.cs             # DTO for Modrinth project API
│   ├── ModrinthSearchResponse.cs      # DTO for Modrinth search API
│   ├── ModrinthTeamMember.cs          # DTO for Modrinth team members API
│   └── ModrinthSearchOption.cs        # GenericItemOption extension for Playnite dialogs
├── Services/
│   ├── ModrinthApiClient.cs           # Async HttpClient with custom User-Agent
│   └── MarkdownToHtmlConverter.cs     # Markdown to Playnite HTML converter
└── Settings/
    ├── ModrinthMetadataSettings.cs    # ISettings model & configuration
    ├── ModrinthMetadataSettingsView.xaml # WPF configuration UI
    └── ModrinthMetadataSettingsView.xaml.cs
```

---

## Installation & Usage

### Option 1: Install via `.pext` Package (Recommended)
1. Double-click the pre-built `ModrinthModpacksMetadata_v1.0.0.pext` file in the parent folder, or drag and drop it into **Playnite**.
2. Click **Install**.
3. Restart Playnite when prompted.

### Option 2: Developer Mode / Building from Source
1. Open `ModrinthModpacksMetadata.sln` or run `dotnet build -c Release`.
2. Open Playnite settings > **Add-ons** > **Extension Manager** > **For developers**.
3. Add the build output folder:
   `...\ModrinthModpacksMetadata\bin\Release\net462`
4. Restart Playnite.

---

## Modrinth API Compliance
This plugin identifies itself to Modrinth using an official User-Agent header:
`User-Agent: Playnite-Modrinth-Metadata-Plugin/1.0.0 (Playnite Metadata Extension)`
in accordance with [Modrinth API Documentation](https://docs.modrinth.com/).
