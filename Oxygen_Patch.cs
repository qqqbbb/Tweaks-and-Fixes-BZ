﻿using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Oxygen_Patch
    {
        static GameObject bubble;
        public static float extraBreathPeriod = 0f;
        public static float bubbleEndTime = 0f;


        static void SpawnBubble(Vector3 position)
        {
            //UWE.PrefabDatabase.TryGetPrefabForFilename("WorldEntities/VFX/x_BubblesPlane_01", out bubble);
            //bubble = Resources.Load<GameObject>("PrefabInstance/Bubble");
            //bubble = AssetBundle.LoadFromFileAsync(AssetBundleUtils.GetLoadPath("PrefabInstance/Bubble")));
            CoroutineTask<GameObject> result = AddressablesUtility.InstantiateAsync("PrefabInstance/Bubble", position: Player.main.transform.position);
            bubble = result.GetResult();

            if (bubble)
            {
                //AddDebug("bubble ");
                //GameObject ent = UWE.Utils.InstantiateWrap(bubble, position, Quaternion.identity);
                UnityEngine.Object.Instantiate<GameObject>(bubble, position, Quaternion.identity);
            }
        }

        //[HarmonyPatch(typeof(Bubble), "Pop")]
        class Bubble_Pop_Patch
        {
            public static void Postfix(Bubble __instance, bool hitPlayer)
            {
                if (hitPlayer)
                {
                    //extraBreathPeriod += extraBreathPeriodDefault;
                    bubbleEndTime = Time.time + extraBreathPeriod;
                    //Main.canBreathe = true;
                    //AddDebug("Bubble hit player " );
                }
            }
        }

        //[HarmonyPatch(typeof(IntermittentInstantiate), "OnEnable")]
        class IntermittentInstantiate_OnEnable_Patch
        {
            public static void Postfix(IntermittentInstantiate __instance)
            {
                if (__instance.GetComponent<BrainCoral>())
                {
                    __instance.baseIntervalTime /= __instance.numToInstantiate;
                    __instance.randomIntervalTime /= __instance.numToInstantiate;
                    __instance.numToInstantiate = 1;
                    extraBreathPeriod = __instance.baseIntervalTime + __instance.randomIntervalTime;
                    //AddDebug("extraBreathPeriodDefault " + extraBreathPeriodDefault);
                }
            }
        }

        //[HarmonyPatch(typeof(OxygenManager), "RemoveOxygen")]
        class OxygenManager_RemoveOxygen_Patch
        {
            public static void Prefix(OxygenManager __instance, ref float amountToRemove)
            {
                //amountToRemove *= Main.config.oxygenMult;
                //if (extraBreathPeriod > 0)
                //{
                //extraBreathPeriod -= extraBreathPeriod;
                //extraBreathPeriod -= extraBreathPeriod;
                //Mathf.Clamp(extraBreathPeriod, 0f, extraBreathPeriod);
                //AddDebug("RemoveOxygen extra " );
                //}
                //AddDebug("RemoveOxygen " + amountToRemove);
            }
        }
        //[HarmonyPatch(typeof(OxygenManager), "Update")]
        class OxygenManager_Update_Patch
        {
            public static void Prefix(OxygenManager __instance)
            {
                //amountToRemove *= Main.config.oxygenMult;
                if (__instance.GetComponent<Player>())
                {
                    //AddDebug("Player OxygenManager ");
                }
                else
                {
                    //AddDebug("Oxygen Available " + __instance.GetOxygenAvailable());
                    //AddDebug("Oxygen Capacity " + __instance.GetOxygenCapacity());
                }

            }
        }

        //[HarmonyPatch(typeof(OxygenArea), "OnTriggerStay")]
        class OxygenArea_OnTriggerStay_Patch
        { // OnTriggerExit does not fire when you pick up pipe
            public static void Postfix(OxygenArea __instance, Collider other)
            {
                if (other && other.gameObject.GetComponentInParent<Player>())
                {
                    //Main.canBreathe = true;
                    //AddDebug("OnTriggerStay ");
                }
            }
        }

        //[HarmonyPatch(typeof(Player), "CanBreathe")]
        class Player_CanBreathe_Patch
        {
            public static void Postfix(Player __instance, ref bool __result)
            {

                //if (Main.canBreathe)
                {
                    if (bubbleEndTime > 0 && Time.time > bubbleEndTime)
                    {
                        bubbleEndTime = 0f;
                        //Main.canBreathe = false;
                        //AddDebug("bubbleEndTime ");
                    }
                    //__result = Main.canBreathe;
                }
            }
        }

        [HarmonyPatch(typeof(Player))]
        internal class Player_Patch
        {
            private const float breathPeriodMax = 3f;

            [HarmonyPostfix, HarmonyPatch("GetOxygenPerBreath")]
            internal static void GetOxygenPerBreathPostfix(Player __instance, ref float __result, float breathingInterval, int depthClass)
            {// vanilla script returns wrong value at depth 200 to 100
                __result = 0f;
                if (GameModeManager.GetOption<bool>(GameOption.OxygenDepletes))
                    __result = ConfigMenu.oxygenPerBreath.Value;
                //AddDebug("GetOxygenPerBreath breathingInterval " + breathingInterval);
                //AddDebug("GetOxygenPerBreath  " + __result);
            }

            [HarmonyPostfix, HarmonyPatch("GetBreathPeriod")]
            static void GetBreathPeriodPostfix(Player __instance, ref float __result)
            {
                //AddDebug("depthLevel " + (int)__instance.depthLevel);
                //AddDebug("depthOf " + (int)Ocean.main.GetDepthOf(__instance.gameObject);
                if (!ConfigMenu.realOxygenCons.Value)
                    return;

                if (Player.main._currentInterior != null || __instance.currentMountedVehicle || __instance.currentWaterPark || Inventory.main.equipment.GetCount(TechType.Rebreather) > 0)
                {
                    //AddDebug("safe BreathPeriod " );
                    __result = breathPeriodMax;
                    return;
                }
                float depth = Mathf.Abs(__instance.depthLevel);
                float mult = 1.5f / ConfigMenu.crushDepth.Value;
                __result = breathPeriodMax - depth * mult;
                // __result is negative when depth is 2x deeper than crushDepth
                __result = Mathf.Clamp(__result, 0.1f, breathPeriodMax);
            }
        }

        //[HarmonyPatch(typeof(LiveMixin), "TakeDamage")]
        class LiveMixin_TakeDamage_Patch
        {
            public static void Postfix(LiveMixin __instance, float originalDamage, Vector3 position, DamageType type, GameObject dealer)
            {
                if (originalDamage > 1f && __instance.GetComponent<BrainCoral>())
                {
                    //AddDebug("BrainCoral " );
                    IntermittentInstantiate ii = __instance.GetComponent<IntermittentInstantiate>();
                    //GameObject ent = UWE.Utils.InstantiateWrap(ii.prefab, position, Quaternion.identity);
                    UnityEngine.Object.Instantiate<GameObject>(ii.prefab, position, Quaternion.identity);
                    //if (ii.registerToWorldStreamer && LargeWorldStreamer.main)
                    //    LargeWorldStreamer.main.cellManager.RegisterEntity(ent);
                    //if (ii.onCreateSound)
                    //    Utils.PlayEnvSound(ii.onCreateSound, ent.transform.position, 1f);
                }
            }
        }

        //[HarmonyPatch(typeof(Player), "Start")]
        class Player_Start_Patch
        {// cant use Resources.Load , bubble prefab is not in resources folder
            public static void Postfix()
            {

                //GameObject brainCoral = CraftData.GetPrefabForTechType(TechType.PurpleBrainCoral);
                //brainCoral = Utils.CreatePrefab(brainCoral);
                //IntermittentInstantiate ii = brainCoral.GetComponent<IntermittentInstantiate>();
                //bubble = ii.prefab;
                //UnityEngine.Object.Destroy(brainCoral);
                //LargeWorldEntity.Register(prefab);

            }
        }

        //[HarmonyPatch(typeof(LiveMixin), "Kill")]
        class LiveMixin_Kill_Patch
        {
            public static void Postfix(LiveMixin __instance)
            {
                if (__instance.GetComponent<Bladderfish>() || __instance.GetComponent<Floater>())
                {
                    AddDebug("Kill ");
                    SpawnBubble(__instance.gameObject.transform.position);
                }

            }
        }

    }
}
