using CombatDummy.Framework.Objects;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;
using System;

namespace CombatDummy.Framework.Patches.Entities
{
    internal class MonsterPatch : PatchTemplate
    {
        private readonly Type _object = typeof(Monster);

        internal MonsterPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_object, nameof(Monster.update), new[] { typeof(GameTime), typeof(GameLocation) }), prefix: new HarmonyMethod(GetType(), nameof(UpdatePrefix)));
            harmony.Patch(AccessTools.Method(typeof(Character), nameof(Character.GetBoundingBox), null), postfix: new HarmonyMethod(GetType(), nameof(GetBoundingBoxPostfix)));
            harmony.Patch(AccessTools.Method(_object, nameof(Monster.shedChunks), new[] { typeof(int), typeof(float) }), prefix: new HarmonyMethod(GetType(), nameof(ShedChunksPrefix)));
            harmony.Patch(AccessTools.Method(_object, nameof(Monster.takeDamage), new[] { typeof(int), typeof(int), typeof(int), typeof(bool), typeof(double), typeof(string) }), postfix: new HarmonyMethod(GetType(), nameof(TakeDamagePostfix)));

            harmony.Patch(AccessTools.Method(_object, nameof(Monster.draw), new[] { typeof(SpriteBatch) }), prefix: new HarmonyMethod(GetType(), nameof(DrawPrefix)));
        }

        private static bool UpdatePrefix(Monster __instance, ref int ___invincibleCountdown, GameTime time, GameLocation location)
        {
            if (MonsterDummy.IsValid(__instance))
            {
                MonsterDummy.Update(__instance, ref ___invincibleCountdown, time, location);
                return false;
            }

            return true;
        }

        private static void GetBoundingBoxPostfix(Character __instance, ref Rectangle __result)
        {
            if (MonsterDummy.IsValid(__instance))
            {
                // Adjust the bounding box to include the whole dummy sprite
                __result.Y -= 48;
                __result.Height = 96;
            }
        }

        private static bool ShedChunksPrefix(Monster __instance, int number, float scale)
        {
            if (MonsterDummy.IsValid(__instance))
            {
                return false;
            }

            return true;
        }

        private static void TakeDamagePostfix(Monster __instance, int ___invincibleCountdown, int __result, int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, string hitSound)
        {
            if (MonsterDummy.IsValid(__instance))
            {
                MonsterDummy.TakeDamage(__instance, ___invincibleCountdown, __result, damage, xTrajectory, yTrajectory, isBomb, addedPrecision, hitSound);
            }
        }

        [HarmonyPriority(Priority.High)]
        private static bool DrawPrefix(Monster __instance, SpriteBatch b)
        {
            if (MonsterDummy.IsValid(__instance))
            {
                MonsterDummy.Draw(__instance, b);
                return false;
            }

            return true;
        }
    }
}
