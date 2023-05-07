using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Object = StardewValley.Object;

namespace CombatDummy.Framework.Patches.GameLocations
{
    internal class GameLocationPatch : PatchTemplate
    {
        private readonly Type _object = typeof(GameLocation);

        internal GameLocationPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_object, nameof(GameLocation.damageMonster), new[] { typeof(Rectangle), typeof(int), typeof(int), typeof(bool), typeof(float), typeof(int), typeof(float), typeof(float), typeof(bool), typeof(Farmer) }), postfix: new HarmonyMethod(GetType(), nameof(DamageMonsterPostfix)));
            harmony.Patch(AccessTools.Method(_object, nameof(GameLocation.UpdateWhenCurrentLocation), new[] { typeof(GameTime) }), postfix: new HarmonyMethod(GetType(), nameof(UpdateWhenCurrentLocationPostfix)));
        }

        private static void UpdateWhenCurrentLocationPostfix(GameLocation __instance, GameTime time)
        {
            if (__instance is null)
            {
                return;
            }

            foreach (var tileToObject in __instance.Objects.Pairs)
            {
                if (tileToObject.Value is null || tileToObject.Value.Name != "PracticeDummy")
                {
                    continue;
                }

                bool hasMonster = false;
                foreach (var monster in __instance.characters.Where(c => c is Monster && c is not null))
                {
                    if (monster.Position / 64f == tileToObject.Key)
                    {
                        hasMonster = true;
                    }
                }

                // TODO: Implement Monster patch to use hidden monster that will transfer its "damage" to the target dummy
                if (hasMonster is false)
                {
                    var dummyMonster = new Monster("Mummy", tileToObject.Key * 64f)
                    {
                        Speed = 0,
                        DamageToFarmer = 0,
                        stunTime = Int32.MaxValue,
                        HideShadow = true
                    };
                    dummyMonster.modData["PeacefulEnd.PracticeDummy.MonsterDummy"] = true.ToString();
                    __instance.characters.Add(dummyMonster);

                    _monitor.Log($"HERE: {tileToObject.Key} | {dummyMonster.Position / 64f}", LogLevel.Debug);
                }

                var practiceDummy = tileToObject.Value;
                if (tileToObject.Value.modData.TryGetValue("PeacefulEnd.PracticeDummy.IsAnimating", out string rawIsAnimating) && Boolean.Parse(rawIsAnimating) is true)
                {
                    var animationFrame = Int32.Parse(practiceDummy.modData["PeacefulEnd.PracticeDummy.AnimationFrame"]);
                    var animationCountdown = Int32.Parse(practiceDummy.modData["PeacefulEnd.PracticeDummy.AnimationCountdown"]) - time.ElapsedGameTime.Milliseconds;
                    if (animationCountdown <= 0)
                    {
                        animationFrame += 1;
                        animationCountdown = ModEntry.ANIMATION_COOLDOWN;
                        if (animationFrame > 2)
                        {
                            animationFrame = 0;
                            practiceDummy.modData["PeacefulEnd.PracticeDummy.IsAnimating"] = false.ToString();
                        }

                        practiceDummy.modData["PeacefulEnd.PracticeDummy.AnimationFrame"] = animationFrame.ToString();
                    }

                    practiceDummy.modData["PeacefulEnd.PracticeDummy.AnimationCountdown"] = animationCountdown.ToString();
                }
                if (tileToObject.Value.modData.ContainsKey("PeacefulEnd.PracticeDummy.InvincibleCountdown") is true)
                {
                    var invincibleCountdown = Int32.Parse(practiceDummy.modData["PeacefulEnd.PracticeDummy.InvincibleCountdown"]) - time.ElapsedGameTime.Milliseconds;

                    if (invincibleCountdown <= 0)
                    {
                        practiceDummy.modData["PeacefulEnd.PracticeDummy.IsInvincible"] = false.ToString();
                    }
                    else
                    {
                        practiceDummy.modData["PeacefulEnd.PracticeDummy.InvincibleCountdown"] = invincibleCountdown.ToString();
                    }
                }
                if (tileToObject.Value.modData.ContainsKey("PeacefulEnd.PracticeDummy.DamageCountdown") is true)
                {
                    var damageCountdown = Int32.Parse(practiceDummy.modData["PeacefulEnd.PracticeDummy.DamageCountdown"]) - time.ElapsedGameTime.Milliseconds;

                    if (damageCountdown > -10000)
                    {
                        practiceDummy.modData["PeacefulEnd.PracticeDummy.DamageCountdown"] = damageCountdown.ToString();
                        //practiceDummy.modData["PeacefulEnd.PracticeDummy.CollectiveDamage"] = "0";
                        //practiceDummy.modData["PeacefulEnd.PracticeDummy.DamageCountdown"] = "1000";
                    }
                }
            }
        }

        private static void DamageMonsterPostfix(GameLocation __instance, Rectangle areaOfEffect, int minDamage, int maxDamage, bool isBomb, float knockBackModifier, int addedPrecision, float critChance, float critMultiplier, bool triggerMonsterInvincibleTimer, Farmer who)
        {
            if (__instance is null)
            {
                return;
            }
        }
    }
}
