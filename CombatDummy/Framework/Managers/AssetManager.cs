using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombatDummy.Framework.Managers
{
    internal class AssetManager
    {
        internal Texture2D PracticeDummyTexture { get; }
        internal Texture2D KnockbackDummyTexture { get; }

        public AssetManager(IModHelper helper)
        {
            // Get the asset folder path
            var assetFolderPath = helper.ModContent.GetInternalAssetName(Path.Combine("Framework", "Assets")).Name;

            // Load in the asset tilesheet
            PracticeDummyTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "PracticeDummy", "Sprites", "target_dummy.png"));
            KnockbackDummyTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "PracticeDummy", "Sprites", "knockback_target_dummy.png"));
        }
    }
}