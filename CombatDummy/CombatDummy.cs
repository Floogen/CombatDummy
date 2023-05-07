using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using CombatDummy.Framework.Managers;
using CombatDummy.Framework.Patches.Entities;
using CombatDummy.Framework.Patches.GameLocations;
using StardewValley;
using StardewValley.Objects;
using System;
using System.IO;

namespace CombatDummy
{
    public class ModEntry : Mod
    {
        // Shared static helpers
        internal static IMonitor monitor;
        internal static IModHelper modHelper;
        internal static Multiplayer multiplayer;

        // Managers
        internal static ApiManager apiManager;
        internal static AssetManager assetManager;

        // Etc.
        internal const int ANIMATION_COOLDOWN = 80;

        public override void Entry(IModHelper helper)
        {
            // Set up the monitor, helper and multiplayer
            monitor = Monitor;
            modHelper = helper;
            multiplayer = helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();

            // Load the managers
            apiManager = new ApiManager(monitor);
            assetManager = new AssetManager(modHelper);

            try
            {
                var harmony = new Harmony(this.ModManifest.UniqueID);

                // Apply patches
                new MonsterPatch(monitor, modHelper).Apply(harmony);
                new ObjectPatch(monitor, modHelper).Apply(harmony);
                new GameLocationPatch(monitor, modHelper).Apply(harmony);
            }
            catch (Exception e)
            {
                monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
                return;
            }

            // Hook into the required events
            modHelper.Events.GameLoop.GameLaunched += OnGameLaunched; ;
        }

        private void OnGameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            // Hook into DGA
            if (apiManager.HookIntoDynamicGameAssets(modHelper) && apiManager.GetDynamicGameAssetsApi() is var dgaApi && dgaApi is not null)
            {
                var contentPack = Helper.ContentPacks.CreateTemporary(
                    Path.Combine(Helper.DirectoryPath, "Framework", "Assets", "PracticeDummy"),
                    "PeacefulEnd.PracticeDummy",
                    "[DGA] Practice Dummy",
                    "Adds craftable combat practice dummy.",
                    "PeacefulEnd",
                    new SemanticVersion("1.0.0"));

                // Set DGA specific fields
                contentPack.Manifest.ExtraFields["DGA.FormatVersion"] = 2;
                contentPack.Manifest.ExtraFields["DGA.ConditionsFormatVersion"] = "1.25.0";

                dgaApi.AddEmbeddedPack(contentPack.Manifest, Path.Combine(Helper.DirectoryPath, "Framework", "Assets", "PracticeDummy"));
            }
        }
    }
}
