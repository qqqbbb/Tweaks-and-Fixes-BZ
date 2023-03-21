﻿using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Creature_Tweaks
    {
        public static HashSet<TechType> silentCreatures = new HashSet<TechType> { };

        //[HarmonyPatch(typeof(FleeOnDamage), "OnTakeDamage")]
        class FleeOnDamage_OnTakeDamage_Postfix_Patch
        {
            public static void Postfix(FleeOnDamage __instance, DamageInfo damageInfo)
            { //
                if (damageInfo.dealer.Equals(Player.main.gameObject))
                { // these 2 are the same
                    AddDebug(" moveTo " + __instance.moveTo);
                    AddDebug(" originalTargetPosition " + __instance.swimBehaviour.originalTargetPosition);
                }

                __instance.moveTo = __instance.swimBehaviour.originalTargetPosition * damageInfo.damage;

                //__instance.timeToFlee = Time.time;
                //if (damageInfo.type == DamageType.Heat)
                //{
                //TechType techType = CraftData.GetTechType(__instance.gameObject);
                //string name = Language.main.Get(techType);
                //float magnitude = (__instance.transform.position - Player.main.transform.position).magnitude;
                //if (damageInfo.damage == 0 && magnitude < 5)
                //{
                //    LiveMixin liveMixin = __instance.creature.liveMixin;
                //AddDebug(name + " maxHealth " + liveMixin.maxHealth + " Health " + liveMixin.health);
                //}
            }
        }

        [HarmonyPatch(typeof(FleeOnDamage), "OnTakeDamage")]
        internal class FleeOnDamage_OnTakeDamage_Prefix_Patch
        {
            private static bool Prefix(FleeOnDamage __instance, DamageInfo damageInfo)
            {
                LiveMixin liveMixin = __instance.creature.liveMixin;
                AggressiveWhenSeeTarget awst = __instance.GetComponent<AggressiveWhenSeeTarget>();
                if (liveMixin && awst && liveMixin.IsAlive())
                { //  && damageInfo.dealer == Player.main
                    //if (damageInfo.dealer)
                    //  Main.Message("damage dealer " + damageInfo.dealer.name);
                    int maxHealth = Mathf.RoundToInt(liveMixin.maxHealth);
                    //int halfMaxHealth = Mathf.RoundToInt(liveMixin.maxHealth * .5f);
                    int rnd = Main.rndm.Next(1, maxHealth);
                    float aggrMult = GameModeManager.GetCreatureAggressionModifier();
                    int health = Mathf.RoundToInt(liveMixin.health * aggrMult);
                    //if (health > halfMaxHealth || rnd < health)
                    if (health > rnd)
                    {
                        damageInfo.damage = 0f;
                        //Main.Message("health " + liveMixin.health + " rnd100 " + rnd100);
                    }
                    if (aggrMult == 0f)
                        damageInfo.damage = 0f;

                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Creature), "Start")]
        public static class Creature_Start_Patch
        {
            public static void Postfix(Creature __instance)
            {
                VFXSurface vFXSurface = __instance.GetComponent<VFXSurface>();
                if (vFXSurface == null)
                {
                    EcoTarget ecoTarget = __instance.GetComponent<EcoTarget>();
                    if (ecoTarget && ecoTarget.type == EcoTargetType.FishSchool)
                    { }
                    else
                    {
                        vFXSurface = __instance.gameObject.EnsureComponent<VFXSurface>();
                        vFXSurface.surfaceType = VFXSurfaceTypes.organic;
                        //AddDebug(__instance.name + " no VFXSurface");
                    }
                }
                if (__instance is SpinnerFish || __instance is RockGrub)
                {
                    CreatureDeath cd = __instance.GetComponent<CreatureDeath>();
                    if (cd)
                        cd.respawnOnlyIfKilledByCreature = false;
                }
            }
        }

        [HarmonyPatch(typeof(CreatureEgg), "Start")]
        class CreatureEgg_Start_Patch
        {
            public static void Postfix(CreatureEgg __instance)
            {
                VFXSurface surface = __instance.gameObject.EnsureComponent<VFXSurface>();
                surface.surfaceType = VFXSurfaceTypes.organic;
            }
        }

        [HarmonyPatch(typeof(CreatureDeath))]
        class CreatureDeath_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            static void StartPostfix(CreatureDeath __instance)
            {
                if (__instance.GetComponent<Pickupable>()) // fish
                {
                    if (Main.config.fishRespawnTime > 0)
                        __instance.respawnInterval = Main.config.fishRespawnTime * 1200f;
                }
                else
                {
                    LiveMixin liveMixin = __instance.GetComponent<LiveMixin>();
                    if (liveMixin)
                    {
                        if (liveMixin.maxHealth >= 5000f) // Leviathan
                        {
                            if (Main.config.leviathanRespawnTime > 0)
                                __instance.respawnInterval = Main.config.leviathanRespawnTime * 1200f;

                            if (Main.config.creatureRespawn == Config.CreatureRespawn.Leviathans_only || Main.config.creatureRespawn == Config.CreatureRespawn.Big_creatures_and_leviathans)
                                __instance.respawnOnlyIfKilledByCreature = false;
                        }
                        else
                        {
                            if (Main.config.creatureRespawnTime > 0)
                                __instance.respawnInterval = Main.config.creatureRespawnTime * 1200f;

                            if (Main.config.creatureRespawn == Config.CreatureRespawn.Big_creatures_and_leviathans || Main.config.creatureRespawn == Config.CreatureRespawn.Big_creatures_only)
                                __instance.respawnOnlyIfKilledByCreature = false;
                        }
                    }
                }
            }
            [HarmonyPostfix]
            [HarmonyPatch("OnTakeDamage")]
            static void OnTakeDamagePostfix(CreatureDeath __instance)
            {
                if (!Main.config.heatBladeCooks)
                    __instance.lastDamageWasHeat = false;
            }
            //[HarmonyPrefix]
            //[HarmonyPatch("OnKill")]
            static void OnKillPrefix(CreatureDeath __instance)
            {
                //AddDebug(__instance.name + " OnKill");
            }
            //[HarmonyPostfix]
            //[HarmonyPatch("OnKill")]
            static void OnKillPostfix(CreatureDeath __instance)
            {
                AddDebug("OnKill ");
                AquariumFish aquariumFish = __instance.GetComponent<AquariumFish>();
                if (aquariumFish)
                    UnityEngine.Object.Destroy(aquariumFish);
            }
        }

        [HarmonyPatch(typeof(Pickupable), "AllowedToPickUp")]
        class Pickupable_AllowedToPickUp_Patch
        {
            public static void Postfix(Pickupable __instance, ref bool __result)
            {
                //__result = __instance.isPickupable && Time.time - __instance.timeDropped > 1.0 && Player.main.HasInventoryRoom(__instance);
                if (Main.config.noFishCatching && Main.IsEatableFishAlive(__instance.gameObject))
                {
                    __result = false;
                    if (Player.main._currentWaterPark)
                    {
                        __result = true;
                        //AddDebug("WaterPark ");
                        return;
                    }
                    PropulsionCannonWeapon pc = Inventory.main.GetHeldTool() as PropulsionCannonWeapon;
                    if (pc && pc.propulsionCannon.grabbedObject.Equals(__instance.gameObject))
                    {
                        //AddDebug("PropulsionCannonWeapon ");
                        __result = true;
                        return;
                    }
                    foreach (Pickupable p in Gravsphere_Patch.gravSphereFish)
                    {
                        if (p.Equals(__instance))
                        {
                            //AddDebug("Gravsphere ");
                            __result = true;
                            return;
                        }
                    }
                }

            }
        }

        [HarmonyPatch(typeof(SwimBehaviour))]
        class SwimBehaviour_SwimToInternal_patch
        {
            [HarmonyPatch("SwimToInternal")]
            public static void Prefix(SwimBehaviour __instance, ref float velocity)
            {
                if (Main.IsEatableFish(__instance.gameObject))
                {
                    velocity *= Main.config.fishSpeedMult;
                }
                else
                {
                    velocity *= Main.config.creatureSpeedMult;
                }
            }
        }

        [HarmonyPatch(typeof(SeaMonkeyBringGift), "Start")]
        class SeaMonkeyBringGift_Start_patch
        {
            public static void Postfix(SeaMonkeyBringGift __instance)
            {
                Transform tr = __instance.transform.Find("heldToolHandTarget");
                if (tr)
                {
                    VFXSurface sfxs = tr.gameObject.AddComponent<VFXSurface>();
                    sfxs.surfaceType = VFXSurfaceTypes.organic;
                }
            }
        }



        //[HarmonyPatch(typeof(CreatureDeath), nameof(CreatureDeath.OnKill))]
        class CreatureDeath_OnKill_Patch
        {
            public static void Postfix(CreatureDeath __instance)
            {
                //AddDebug("respawnOnlyIfKilledByCreature " + __instance.respawnOnlyIfKilledByCreature);
                Stalker stalker = __instance.GetComponent<Stalker>();
                //ReaperLeviathan reaper = __instance.GetComponent<ReaperLeviathan>();
                //SandShark sandShark = __instance.GetComponent<SandShark>();
                //if (sandShark)
                //{
                //Animator animator = __instance.GetComponentInChildren<Animator>();
                //animator.GetCurrentAnimatorStateInfo(animator.layerCount -1);
                //if (animator != null)
                //    animator.enabled = false;
                //}
                //if (reaper != null)
                //{
                //Animator animator = __instance.GetComponentInChildren<Animator>();
                //if (animator != null)
                //    animator.enabled = false;
                //}
                if (stalker != null)
                {
                    //Main.Log("Stalker kill");
                    Animator animator = __instance.GetComponentInChildren<Animator>();
                    //AnimateByVelocity animByVelocity = __instance.GetComponentInChildren<AnimateByVelocity>();
                    if (animator != null)
                    {
                        animator.enabled = false;
                        //animator.enabled = true;
                        //animator.SetFloat(AnimateByVelocity.animSpeed, 0.0f);
                        //animator.SetFloat(AnimateByVelocity.animPitch, 0.0f);
                        //animator.SetFloat(AnimateByVelocity.animTilt, 0.0f);
                        //SafeAnimator.SetBool(animator, "dead", true);
                    }
                    CollectShiny collectShiny = __instance.GetComponent<CollectShiny>();
                    collectShiny?.DropShinyTarget();
                }
            }
        }

        //[HarmonyPatch(typeof(FleeOnDamage), "Evaluate")]
        class FleeOnDamage_Evaluate_Prefix_Patch
        {
            public static bool Prefix(FleeOnDamage __instance, Creature creature)
            {
                //__instance.GetEvaluatePriority();
                //__instance.StartPerform(creature);
                //Main.Message(" FleeOnDamage_Evaluate_Prefix_Patch ");
                return false;

            }
        }

        //[HarmonyPatch(typeof(FleeOnDamage), "Evaluate")]
        class FleeOnDamage_Evaluate_Patch
        {
            public static void Postfix(FleeOnDamage __instance, Creature creature)
            {
                TechType techType = CraftData.GetTechType(__instance.gameObject);
                string name = Language.main.Get(techType);
                //Creature_Loot_Drop.Creature_Loot Crloot = __instance.GetComponent<Creature_Loot_Drop.Creature_Loot>();
                //if (Crloot)
                //{
                //Main.Message("Stalker DropShinyTarget");
                //if (Time.time < __instance.timeToFlee)
                //{
                //Main.Message(name + " FleeOnDamage GetEvaluatePriority " + __instance.GetEvaluatePriority());
                //}
                //else
                //    Main.Message(name + " FleeOnDamage Evaluate " + 0);
                //}
            }
        }

        //[HarmonyPatch(typeof(FleeOnDamage), "StopPerform")]
        class FleeOnDamage_StopPerform_Patch
        {
            public static void Postfix(FleeOnDamage __instance, Creature creature)
            {

                TechType techType = CraftData.GetTechType(__instance.gameObject);
                string name = Language.main.Get(techType);
                //Main.Message(name + " Stop Perform ");
            }
        }
    }
}
