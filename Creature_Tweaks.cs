using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Creature_Tweaks
    {
        public static HashSet<TechType> silentCreatures = new HashSet<TechType> { };


        [HarmonyPatch(typeof(FleeOnDamage), "OnTakeDamage")]
        class FleeOnDamage_OnTakeDamage_Postfix_Patch
        {
            public static bool Prefix(FleeOnDamage __instance, DamageInfo damageInfo)
            {
                if (Main.config.CreatureFleeChance == 100 && !Main.config.CreatureFleeChanceBasedOnHealth && Main.config.CreatureFleeUseDamageThreshold)
                    return true;

                if (!__instance.enabled || __instance.frozen)
                    return false;

                float damage = damageInfo.damage;
                bool doFlee = false;
                LiveMixin liveMixin = __instance.creature.liveMixin;
                if (Main.config.CreatureFleeChanceBasedOnHealth && liveMixin && liveMixin.IsAlive())
                {
                    int maxHealth = Mathf.RoundToInt(liveMixin.maxHealth);
                    int rnd1 = Main.rndm.Next(0, maxHealth + 1);
                    int health = Mathf.RoundToInt(liveMixin.health);
                    //if (__instance.gameObject == Testing.goToTest)
                    //AddDebug(__instance.name + " max Health " + maxHealth + " Health " + health);
                    if (health < rnd1)
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
                    if (Main.config.CreatureFleeUseDamageThreshold && __instance.accumulatedDamage <= __instance.damageThreshold)
                        return false;

                    int rnd = Main.rndm.Next(1, 101);
                    if (Main.config.CreatureFleeChance >= rnd)
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
            [HarmonyPrefix]
            [HarmonyPatch("IsInFieldOfView")]
            public static void IsInFieldOfViewPrefix(Creature __instance, GameObject go, ref bool __result)
            {
                __result = false;
                if (go != null)
                { // when casting ray from creature to player terrain may not be loaded. Cast from player instead
                    Vector3 dir = go.transform.position - __instance.transform.position;
                    Vector3 rhs = __instance.eyesOnTop ? __instance.transform.up : __instance.transform.forward;
                    if ((Mathf.Approximately(__instance.eyeFOV, -1f) || Vector3.Dot(dir.normalized, rhs) >= __instance.eyeFOV) && !Physics.Linecast(go.transform.position, __instance.transform.position,  Voxeland.GetTerrainLayerMask()))
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

        [HarmonyPatch(typeof(CreatureDeath))]
        class CreatureDeath_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            static void StartPostfix(CreatureDeath __instance)
            {
                if (__instance.GetComponent<Pickupable>()) // fish
                {
                    __instance.respawn = Main.config.fishRespawn;
                    __instance.respawnOnlyIfKilledByCreature = !Main.config.fishRespawnIfKilledByPlayer;
                    if (Main.config.fishRespawnTime > 0)
                        __instance.respawnInterval = Main.config.fishRespawnTime * 1200f;
                }
                else
                {
                    LiveMixin liveMixin = __instance.GetComponent<LiveMixin>();

                    if (!liveMixin)
                        return;

                    if (liveMixin.maxHealth >= 5000f) // Leviathan
                    {
                        __instance.respawn = Main.config.leviathansRespawn;
                        __instance.respawnOnlyIfKilledByCreature = !Main.config.leviathansRespawnIfKilledByPlayer;
                        if (Main.config.leviathanRespawnTime > 0)
                            __instance.respawnInterval = Main.config.leviathanRespawnTime * 1200f;
                    }
                    else
                    {
                        __instance.respawn = Main.config.creaturesRespawn;
                        __instance.respawnOnlyIfKilledByCreature = !Main.config.creaturesRespawnIfKilledByPlayer;
                        if (Main.config.creatureRespawnTime > 0)
                             __instance.respawnInterval = Main.config.creatureRespawnTime * 1200f;
                    }
                }
            }
            [HarmonyPostfix]
            [HarmonyPatch("OnTakeDamage")]
            static void OnTakeDamagePostfix(CreatureDeath __instance, DamageInfo damageInfo)
            {
                //AddDebug("OnTakeDamage " + damageInfo.dealer.name);
                if (!Main.config.heatBladeCooks && damageInfo.type == DamageType.Heat && damageInfo.dealer == Player.mainObject)
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
                if (Main.config.noFishCatching && Util.IsCreatureAlive(__instance.gameObject) && Util.IsEatableFish(__instance.gameObject))
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
                if (Util.IsEatableFish(__instance.gameObject))
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
