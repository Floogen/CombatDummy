using CombatDummy.Framework.Objects;
using CombatDummy.Framework.Utilities;
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
            if (__instance is null || __instance.modData.ContainsKey(ModDataKeys.MONSTER_DUMMY_FLAG) is false)
            {
                return true;
            }

            // Verify that the Target Dummy exists on the same tile
            // If it does not, delete this monster
            var tilePosition = __instance.getTileLocationPoint();
            var practiceDummy = __instance.currentLocation.getObjectAtTile(tilePosition.X, tilePosition.Y);
            if (practiceDummy is null || practiceDummy.Name != "PracticeDummy")
            {
                __instance.currentLocation.characters.Remove(__instance);
                return false;
            }

            if (___invincibleCountdown > 0)
            {
                ___invincibleCountdown -= time.ElapsedGameTime.Milliseconds;
                if (___invincibleCountdown <= 0)
                {
                    __instance.stopGlowing();
                }
            }

            __instance.stunTime = Int32.MaxValue;
            __instance.MaxHealth = Int32.MaxValue;
            __instance.Health = Int32.MaxValue;
            __instance.Slipperiness = 1;

            return false;
        }

        private static void GetBoundingBoxPostfix(Character __instance, ref Rectangle __result)
        {
            if (__instance is null || __instance.modData.ContainsKey(ModDataKeys.MONSTER_DUMMY_FLAG) is false)
            {
                return;
            }

            // Adjust the bounding box to include the whole dummy sprite
            __result.Y -= 48;
            __result.Height = 96;
        }

        private static bool ShedChunksPrefix(Monster __instance, int number, float scale)
        {
            if (__instance is null || __instance.modData.ContainsKey(ModDataKeys.MONSTER_DUMMY_FLAG) is false)
            {
                return true;
            }

            return false;
        }

        private static void TakeDamagePostfix(Monster __instance, int ___invincibleCountdown, int __result, int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, string hitSound)
        {
            if (__instance is null || __instance.modData.ContainsKey(ModDataKeys.MONSTER_DUMMY_FLAG) is false)
            {
                return;
            }

            // Verify that the Target Dummy exists on the same tile
            // If it does not, delete this monster
            var tilePosition = __instance.getTileLocationPoint();
            var practiceDummy = __instance.currentLocation.getObjectAtTile(tilePosition.X, tilePosition.Y);
            if (PracticeDummy.IsValid(practiceDummy) is false)
            {
                __instance.currentLocation.characters.Remove(__instance);
                return;
            }

            _monitor.LogOnce($"[{DateTime.Now.ToString("T")}] {__instance.xVelocity} vs {yTrajectory}", LogLevel.Debug);

            // Label the damage amount
            int damageAmount = __result;

            // Cache the hit data to the practice dummy
            bool isInvincible = practiceDummy.modData.TryGetValue(ModDataKeys.IS_DUMMY_INVINCIBLE, out string rawIsInvincible) is true ? Boolean.Parse(rawIsInvincible) : false;
            if (isInvincible is false)
            {
                if (damageAmount > 0)
                {
                    var collectiveDamage = practiceDummy.modData.ContainsKey(ModDataKeys.DUMMY_COLLECTIVE_DAMAGE) ? Int32.Parse(practiceDummy.modData[ModDataKeys.DUMMY_COLLECTIVE_DAMAGE]) : 0;
                    var damageCountdown = practiceDummy.modData.ContainsKey(ModDataKeys.DUMMY_DAMAGE_COUNTDOWN) ? Int32.Parse(practiceDummy.modData[ModDataKeys.DUMMY_DAMAGE_COUNTDOWN]) : 0;
                    if (damageCountdown <= 0)
                    {
                        collectiveDamage = 0;
                    }
                    damageCountdown = 1000;

                    practiceDummy.modData[ModDataKeys.DUMMY_COLLECTIVE_DAMAGE] = (collectiveDamage + damageAmount).ToString();
                    practiceDummy.modData[ModDataKeys.DUMMY_DAMAGE_COUNTDOWN] = damageCountdown.ToString();
                    //__instance.debris.Add(new Debris(damageAmount, new Vector2(practiceDummyBox.Center.X + 16, practiceDummyBox.Center.Y), false ? Color.Yellow : new Color(255, 130, 0), false ? (1f + (float)damageAmount / 300f) : 1f, Game1.player));
                }
                if (___invincibleCountdown > 0)
                {
                    practiceDummy.modData[ModDataKeys.IS_DUMMY_INVINCIBLE] = true.ToString();
                    practiceDummy.modData[ModDataKeys.DUMMY_INVINCIBLE_COUNTDOWN] = ___invincibleCountdown.ToString();
                }

                bool isAnimating = practiceDummy.modData.TryGetValue(ModDataKeys.IS_DUMMY_ANIMATING, out string rawIsAnimating) is true ? Boolean.Parse(rawIsAnimating) : false;
                if (isAnimating is false)
                {
                    practiceDummy.modData[ModDataKeys.IS_DUMMY_ANIMATING] = true.ToString();
                    practiceDummy.modData[ModDataKeys.DUMMY_ANIMATION_COUNTDOWN] = CombatDummy.ANIMATION_COOLDOWN.ToString();
                    practiceDummy.modData[ModDataKeys.DUMMY_ANIMATION_FRAME] = "1";
                }
            }
        }

        [HarmonyPriority(Priority.High)]
        private static bool DrawPrefix(Monster __instance, SpriteBatch b)
        {
            if (__instance is null || __instance.modData.ContainsKey(ModDataKeys.MONSTER_DUMMY_FLAG) is false)
            {
                return true;
            }

            return false;
        }
    }
}
