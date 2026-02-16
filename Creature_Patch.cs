using HarmonyLib;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Creature_Patch
    {
        //public static HashSet<TechType> silentCreatures = new HashSet<TechType> { };
        public static HashSet<TechType> creatureTT = new HashSet<TechType> { };

        public static ConditionalWeakTable<SwimBehaviour, string> fishSBs = new ConditionalWeakTable<SwimBehaviour, string>();

        [HarmonyPatch(typeof(FleeOnDamage), "OnTakeDamage")]
        class FleeOnDamage_OnTakeDamage_Postfix_Patch
        {
            public static bool Prefix(FleeOnDamage __instance, DamageInfo damageInfo)
            {
                if (ConfigMenu.CreatureFleeChance.Value == 100 && !ConfigMenu.creatureFleeChanceBasedOnHealth.Value && ConfigMenu.creatureFleeUseDamageThreshold.Value)
                    return true;

                if (!__instance.enabled || __instance.frozen)
                    return false;

                float damage = damageInfo.damage;
                bool doFlee = false;
                LiveMixin liveMixin = __instance.creature.liveMixin;
                if (ConfigMenu.creatureFleeChanceBasedOnHealth.Value && liveMixin && liveMixin.IsAlive())
                {
                    //AddDebug(__instance.name + " max Health " + maxHealth + " Health " + health);
                    if (liveMixin.health < Random.Range(0, liveMixin.maxHealth))
                    {
                        //if (__instance.gameObject == Testing.goToTest)
                        //    AddDebug(__instance.name + " health low ");

                        doFlee = true;
                    }
                }
                else
                {
                    if (damageInfo.type == DamageType.Electrical)
                        damage *= 35f;
                    __instance.accumulatedDamage += damage;
                    //if (__instance.gameObject == Testing.goToTest)
                    //AddDebug(__instance.name + " accumulatedDamage " + __instance.accumulatedDamage + " damageThreshold " + __instance.damageThreshold);

                    __instance.lastDamagePosition = damageInfo.position;
                    if (ConfigMenu.creatureFleeUseDamageThreshold.Value && __instance.accumulatedDamage <= __instance.damageThreshold)
                        return false;

                    int rnd = Random.Range(1, 101);
                    if (ConfigMenu.CreatureFleeChance.Value >= rnd)
                        doFlee = true;
                }
                if (doFlee)
                {
                    //if (__instance.gameObject == Testing.goToTest)
                    //AddDebug(__instance.name + " Flee " + __instance.fleeDuration);

                    __instance.timeToFlee = Time.time + __instance.fleeDuration;
                    __instance.creature.Scared.Add(1f);
                    __instance.creature.TryStartAction(__instance);
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(Creature), "Start")]
        public static class Creature_Start_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            public static void StartPostfix(Creature __instance)
            {
                TechType tt = CraftData.GetTechType(__instance.gameObject);
                creatureTT.Add(tt);
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
            }
            [HarmonyPrefix]
            [HarmonyPatch("IsInFieldOfView")]
            public static void IsInFieldOfViewPrefix(Creature __instance, GameObject go, ref bool __result)
            {
                __result = false;
                if (go != null)
                { // ray does not hit terrain if cast from underneath. Cast from player to avoid it.
                    Vector3 dir = go.transform.position - __instance.transform.position;
                    Vector3 rhs = __instance.eyesOnTop ? __instance.transform.up : __instance.transform.forward;
                    if ((Mathf.Approximately(__instance.eyeFOV, -1f) || Vector3.Dot(dir.normalized, rhs) >= __instance.eyeFOV) && !Physics.Linecast(go.transform.position, __instance.transform.position, Voxeland.GetTerrainLayerMask()))
                        __result = true;
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

        [HarmonyPatch(typeof(Pickupable), "AllowedToPickUp")]
        class Pickupable_AllowedToPickUp_Patch
        {
            public static void Postfix(Pickupable __instance, ref bool __result)
            {
                //__result = __instance.isPickupable && Time.time - __instance.timeDropped > 1.0 && Player.main.HasInventoryRoom(__instance);
                if (ConfigMenu.noFishCatching.Value && Util.IsCreatureAlive(__instance.gameObject) && Util.IsRawFish(__instance.gameObject))
                {
                    __result = false;
                    if (Player.main._currentWaterPark)
                    {
                        __result = true;
                        //AddDebug("WaterPark ");
                        return;
                    }
                    PropulsionCannonWeapon pc = Inventory.main.GetHeldTool() as PropulsionCannonWeapon;
                    if (pc && pc.propulsionCannon.grabbedObject == __instance.gameObject)
                    {
                        //AddDebug("PropulsionCannonWeapon ");
                        __result = true;
                        return;
                    }
                    if (Gravsphere_Patch.gravSphereFish.Contains(__instance))
                    {
                        //AddDebug("Gravsphere ");
                        __result = true;
                        return;
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
                if (fishSBs.TryGetValue(__instance, out string s) || Util.IsRawFish(__instance.gameObject))
                {
                    velocity *= ConfigMenu.fishSpeedMult.Value;
                    if (s == null)
                        fishSBs.Add(__instance, "");
                }
                else
                {
                    velocity *= ConfigMenu.creatureSpeedMult.Value;
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

        [HarmonyPatch(typeof(SeaMonkey), "IsGiftBehaviorEnabled")]
        class SeaMonkey_IsGiftBehaviorEnabled_patch
        {
            public static void Postfix(SeaMonkey __instance, ref bool __result)
            {
                if (__result && !ConfigToEdit.seaMonkeyBringGift.Value)
                    __result = false;
            }
        }

        [HarmonyPatch(typeof(SeaMonkeyStealShiny), "Evaluate")]
        class SeaMonkeyStealShiny_Evaluate_patch
        {
            public static void Postfix(SeaMonkeyStealShiny __instance, ref float __result)
            {
                //AddDebug("SeaMonkeyStealShiny Evaluate " + __result);
                if (!ConfigToEdit.seaMonkeyGrabTool.Value)
                    __result = 0;
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

        [HarmonyPatch(typeof(WaterParkCreature), "GetCanBreed")]
        class GWaterParkCreature_GetCanBreed_patch
        {
            public static void Postfix(WaterParkCreature __instance, ref bool __result)
            {
                if (!ConfigMenu.waterparkCreaturesBreed.Value)
                    __result = false;
            }
        }

        [HarmonyPatch(typeof(RockPuncherTreasureHunt), "PlayKnockAnimation")]
        class RockPuncherTreasureHunt_PlayKnockAnimation_patch
        {
            public static void Prefix(RockPuncherTreasureHunt __instance)
            {
                __instance.successChance = ConfigToEdit.rockPuncherChanceToFindRock.Value * .01f;
                //AddDebug("PlayKnockAnimation " + __instance.successChance);
            }
        }

    }
}
