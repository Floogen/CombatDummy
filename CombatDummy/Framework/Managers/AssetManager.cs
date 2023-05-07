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
        internal string PracticeDummyTexturePath { get; }
        internal Texture2D PracticeDummyTexture { get; }

        public AssetManager(IModHelper helper)
        {
            // Get the asset folder path
            var assetFolderPath = helper.ModContent.GetInternalAssetName(Path.Combine("Framework", "Assets")).Name;

            // Load in the asset tilesheet
            PracticeDummyTexturePath = Path.Combine(assetFolderPath, "PracticeDummy", "Sprites", "target_dummy.png");
            PracticeDummyTexture = helper.ModContent.Load<Texture2D>(PracticeDummyTexturePath);
        }
    }
}