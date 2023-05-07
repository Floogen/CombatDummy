using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Object = StardewValley.Object;

namespace CombatDummy.Framework.Patches.Entities
{
    internal class ObjectPatch : PatchTemplate
    {
        private readonly Type _object = typeof(Object);

        internal ObjectPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal void Apply(Harmony harmony)
        {
            if (Type.GetType("DynamicGameAssets.Game.CustomBigCraftable, DynamicGameAssets") is Type dgaCraftableType && dgaCraftableType != null)
            {
                harmony.Patch(AccessTools.Method(dgaCraftableType, nameof(Object.draw), new[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }), prefix: new HarmonyMethod(GetType(), nameof(DrawPrefix)));
            }

            harmony.Patch(AccessTools.Method(_object, nameof(Object.onExplosion), new[] { typeof(Farmer), typeof(GameLocation) }), prefix: new HarmonyMethod(GetType(), nameof(OnExplosionPrefix)));
        }

        private static bool OnExplosionPrefix(Object __instance, ref bool __result, Farmer who, GameLocation location)
        {
            if (__instance is null || __instance.Name != "PracticeDummy")
            {
                return true;
            }

            __result = false;
            return __result;
        }

        private static bool DrawPrefix(Object __instance, SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
        {
            if (__instance is null || __instance.Name != "PracticeDummy")
            {
                return true;
            }
            int animationFrame = __instance.modData.TryGetValue("PeacefulEnd.PracticeDummy.AnimationFrame", out string rawAnimationFrame) is false ? 0 : Int32.Parse(rawAnimationFrame);

            Vector2 scaleFactor = __instance.getScale() * 4f;
            Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64));
            Rectangle destination = new Rectangle((int)(position.X - scaleFactor.X / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));

            alpha = __instance.modData.TryGetValue("PeacefulEnd.PracticeDummy.IsInvincible", out string rawIsInvincible) is false ? alpha : Boolean.Parse(rawIsInvincible) ? 0.75f : alpha;
            spriteBatch.Draw(ModEntry.assetManager.PracticeDummyTexture, destination, new Rectangle(animationFrame * 16, 0, 16, 32), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, __instance.getBoundingBox(new Vector2(x, y)).Bottom / 10000f);

            //Vector2 local = Game1.GlobalToLocal(new Vector2(base.getStandingX(), base.getStandingY() - this.Sprite.SpriteHeight * 4 - 64 + base.yJumpOffset));
            var collectiveDamage = __instance.modData.ContainsKey("PeacefulEnd.PracticeDummy.CollectiveDamage") ? Int32.Parse(__instance.modData["PeacefulEnd.PracticeDummy.CollectiveDamage"]) : 0;
            var damageCountdown = __instance.modData.ContainsKey("PeacefulEnd.PracticeDummy.DamageCountdown") ? Int32.Parse(__instance.modData["PeacefulEnd.PracticeDummy.DamageCountdown"]) : 0;

            float adjustedAlpha = damageCountdown <= -1000 ? 1 - (Math.Abs(damageCountdown) / 2200f) : 1f;
            if (collectiveDamage > 0 && adjustedAlpha > 0)
            {
                int yOffset = damageCountdown <= 0 ? (Math.Abs(damageCountdown) / 75) : 0;
                SpriteText.drawStringHorizontallyCenteredAt(spriteBatch, collectiveDamage.ToString(), (int)position.X + 32, (int)position.Y - 16 - (yOffset * 2), color: 5, alpha: adjustedAlpha);
            }
            return false;
        }
    }
}
