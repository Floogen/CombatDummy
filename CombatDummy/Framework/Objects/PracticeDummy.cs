using CombatDummy.Framework.Utilities;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Object = StardewValley.Object;

namespace CombatDummy.Framework.Objects
{
    internal class PracticeDummy
    {
        public static bool IsValid(Object instance)
        {
            if (instance is null || instance.Name != ModDataKeys.PRACTICE_DUMMY_NAME)
            {
                return false;
            }

            return true;
        }

        public static void Update(Object instance, GameTime time, GameLocation location)
        {
            bool hasMonster = false;

            foreach (var monster in location.characters.Where(c => c is Monster && c is not null))
            {
                if (monster.Position / 64f == instance.TileLocation)
                {
                    hasMonster = true;
                }
            }

            // TODO: Implement Monster patch to use hidden monster that will transfer its "damage" to the target dummy
            if (hasMonster is false)
            {
                var dummyMonster = new Monster("Mummy", instance.TileLocation * 64f)
                {
                    Speed = 0,
                    DamageToFarmer = 0,
                    stunTime = Int32.MaxValue,
                    HideShadow = true
                };
                dummyMonster.modData[ModDataKeys.MONSTER_DUMMY_FLAG] = true.ToString();
                location.characters.Add(dummyMonster);

                CombatDummy.monitor.Log($"HERE: {instance.TileLocation} | {dummyMonster.Position / 64f}", LogLevel.Debug);
            }

            var practiceDummy = instance;
            if (practiceDummy.modData.TryGetValue(ModDataKeys.IS_DUMMY_ANIMATING, out string rawIsAnimating) && Boolean.Parse(rawIsAnimating) is true)
            {
                var animationFrame = Int32.Parse(practiceDummy.modData[ModDataKeys.DUMMY_ANIMATION_FRAME]);
                var animationCountdown = Int32.Parse(practiceDummy.modData[ModDataKeys.DUMMY_ANIMATION_COUNTDOWN]) - time.ElapsedGameTime.Milliseconds;
                if (animationCountdown <= 0)
                {
                    animationFrame += 1;
                    animationCountdown = CombatDummy.ANIMATION_COOLDOWN;
                    if (animationFrame > 2)
                    {
                        animationFrame = 0;
                        practiceDummy.modData[ModDataKeys.IS_DUMMY_ANIMATING] = false.ToString();
                    }

                    practiceDummy.modData[ModDataKeys.DUMMY_ANIMATION_FRAME] = animationFrame.ToString();
                }

                practiceDummy.modData[ModDataKeys.DUMMY_ANIMATION_COUNTDOWN] = animationCountdown.ToString();
            }

            if (practiceDummy.modData.ContainsKey(ModDataKeys.DUMMY_INVINCIBLE_COUNTDOWN) is true)
            {
                var invincibleCountdown = Int32.Parse(practiceDummy.modData[ModDataKeys.DUMMY_INVINCIBLE_COUNTDOWN]) - time.ElapsedGameTime.Milliseconds;

                if (invincibleCountdown <= 0)
                {
                    practiceDummy.modData[ModDataKeys.IS_DUMMY_INVINCIBLE] = false.ToString();
                }
                else
                {
                    practiceDummy.modData[ModDataKeys.DUMMY_INVINCIBLE_COUNTDOWN] = invincibleCountdown.ToString();
                }
            }

            if (practiceDummy.modData.ContainsKey(ModDataKeys.DUMMY_DAMAGE_COUNTDOWN) is true)
            {
                var damageCountdown = Int32.Parse(practiceDummy.modData[ModDataKeys.DUMMY_DAMAGE_COUNTDOWN]) - time.ElapsedGameTime.Milliseconds;

                if (damageCountdown > -10000)
                {
                    practiceDummy.modData[ModDataKeys.DUMMY_DAMAGE_COUNTDOWN] = damageCountdown.ToString();
                    //practiceDummy.modData[ModDataKeys.DUMMY_COLLECTIVE_DAMAGE] = "0";
                    //practiceDummy.modData[ModDataKeys.DUMMY_DAMAGE_COUNTDOWN] = "1000";
                }
            }
        }
    }
}
