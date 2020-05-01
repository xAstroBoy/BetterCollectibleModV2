using System.Resources;
using System.Reflection;
using System.Runtime.InteropServices;
using MelonLoader;

[assembly: AssemblyTitle(Collectible_finder_v2.BuildInfo.Name)]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany(Collectible_finder_v2.BuildInfo.Company)]
[assembly: AssemblyProduct(Collectible_finder_v2.BuildInfo.Name)]
[assembly: AssemblyCopyright("Created by " + Collectible_finder_v2.BuildInfo.Author)]
[assembly: AssemblyTrademark(Collectible_finder_v2.BuildInfo.Company)]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
//[assembly: Guid("")]
[assembly: AssemblyVersion(Collectible_finder_v2.BuildInfo.Version)]
[assembly: AssemblyFileVersion(Collectible_finder_v2.BuildInfo.Version)]
[assembly: NeutralResourcesLanguage("en")]
[assembly: MelonModInfo(typeof(Collectible_finder_v2.CollectibleFinderV2), Collectible_finder_v2.BuildInfo.Name, Collectible_finder_v2.BuildInfo.Version, Collectible_finder_v2.BuildInfo.Author, Collectible_finder_v2.BuildInfo.DownloadLink)]


// Create and Setup a MelonModGame to mark a Mod as Universal or Compatible with specific Games.
// If no MelonModGameAttribute is found or any of the Values for any MelonModGame on the Mod is null or empty it will be assumed the Mod is Universal.
// Values for MelonModGame can be found in the Game's app.info file or printed at the top of every log directly beneath the Unity version.
[assembly: MelonModGame(null, null)]