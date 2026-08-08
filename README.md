# 🎮 Modrinth Modpacks Metadata Plugin for Playnite

[![GitHub Release](https://img.shields.io/github/v/release/NarasimaPandiyan/Playnite-Modrinth-Metadata?color=7289da&label=Release&logo=github)](https://github.com/NarasimaPandiyan/Playnite-Modrinth-Metadata/releases/latest)
[![Playnite SDK](https://img.shields.io/badge/PlayniteSDK-6.11.0-blue.svg)](https://playnite.link)
[![Framework](https://img.shields.io/badge/.NET%20Framework-4.6.2-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

A high-performance metadata provider extension for **Playnite** that automatically fetches rich metadata, high-resolution artwork, and formatted descriptions for **Minecraft modpacks** directly from the **Modrinth API**.

---

## ✨ Features

- 🔍 **Smart Title Cleaning**: Automatically strips common title clutter like `"Minecraft "` prefixes and special symbols (`™`, `®`, `©`, `℠`) before querying Modrinth for 100% accurate search results.
- 🖼️ **High-Resolution Artwork**: Automatically retrieves maximum resolution cover images, icons, and background gallery screenshots by stripping Modrinth CDN thumbnail caps (`_305.webp`, `_128.png`, etc.).
- 📝 **Rich Description Rendering**:
  - Converts Modrinth Markdown descriptions into clean, formatted HTML.
  - Converts unsupported YouTube `<iframe>` embeds into clean, clickable video links (`▶ Watch Video on YouTube`).
  - Rewrites GitHub image links (`github.com/.../blob/...`) to direct `raw.githubusercontent.com` streams.
  - Proxies `.webp` images through `wsrv.nl` as PNGs so Playnite's MSHTML engine renders them natively without broken red X boxes.
  - Converts Shields.io SVG vector badges into formatted text links.
- 👥 **Developer & Publisher Mapping**: Fetches project team members via Modrinth's Team API to credit modpack creators accurately and attributes publisher to **Modrinth**.
- 🏷️ **Categories, Genres & Tags**: Automatically imports Modrinth categories, mod loaders (*Fabric*, *Forge*, *Quilt*, *Neoforge*), game version tags, and environment flags (*Client/Server*).
- 🔗 **Comprehensive Links**: Extracts official Modrinth project page, Source Code repository, Issue Tracker, Wiki, Discord server, and Donation links (Ko-fi, Patreon, etc.).
- 📅 **Release Date Tracking**: Captures the exact initial publication date of the modpack.

---

## 🛠️ Installation

### Option 1: Automatic via Playnite Addon Browser (Recommended)
1. Open **Playnite**.
2. Go to **Settings** (`F5`) ➔ **Add-ons** ➔ **Browse** ➔ **Metadata Providers**.
3. Search for **Modrinth Modpacks** and click **Install**.
4. Restart Playnite when prompted.

### Option 2: Manual Installation (.pext)
1. Download the latest `.pext` installer package from the [Releases Page](https://github.com/NarasimaPandiyan/Playnite-Modrinth-Metadata/releases/latest).
2. Double-click the downloaded `ModrinthModpacksMetadata_vX.X.X.pext` file or drag it directly into Playnite.
3. Click **Save** and restart Playnite.

---

## 📖 How to Use

1. Right-click any Minecraft game in your Playnite library and select **Edit** (`Ctrl+E`).
2. Go to the **Download Metadata** tab.
3. Select **Modrinth Modpacks** from the list of metadata providers.
4. Select the matching project from search results to apply metadata, high-res covers, backgrounds, tags, and formatted HTML descriptions.

---

## 🏗️ Building from Source

### Prerequisites
- [.NET Framework 4.6.2 SDK](https://dotnet.microsoft.com/download/dotnet-framework/net462)
- Visual Studio 2022 or `dotnet` CLI

### Build Steps
```bash
# Clone the repository
git clone https://github.com/NarasimaPandiyan/Playnite-Modrinth-Metadata.git
cd Playnite-Modrinth-Metadata

# Build Release Configuration
dotnet build -c Release

# Package .pext Extension
powershell -Command "Compress-Archive -Path 'ModrinthModpacksMetadata\bin\Release\net462\*' -DestinationPath 'ModrinthModpacksMetadata.zip' -Force; Copy-Item 'ModrinthModpacksMetadata.zip' 'ModrinthModpacksMetadata.pext' -Force"
```

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).

---

## 🙌 Acknowledgments

- Built for [Playnite](https://playnite.link) by Josef Nemec.
- Powered by the open [Modrinth API](https://docs.modrinth.com/api-spec).
