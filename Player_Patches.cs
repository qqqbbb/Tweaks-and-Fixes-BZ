using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UWE;
using HarmonyLib;
using ProtoBuf;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Player_Patches
    {
        //static Survival survival;
        //static LiveMixin liveMixin;
        public static GUIHand gUIHand;
        //public static float exitWaterOffset = 0.8f; // 0.8f
        public static float crushPeriod = 3f;

        //[HarmonyPatch(typeof(Survival), nameof(Survival.Reset))]
        internal class Survival_Reset_Patch
        {
            [HarmonyPostfix]
            public static void Postfix(Survival __instance)
            {
                //survival = Player.main.GetComponent<Survival>();
                //liveMixin = Player.main.GetComponent<LiveMixin>();
                //Main.Log("1.40129846432482E-45  " + (int)1.40129846432482E-45);
                //Main.Message("Survival_Reset_Patch "); 
                //__instance.food = 11f;
                //__instance.water = 11f;
                //Player.main.liveMixin.health -= 40f;
            }
        }

        [HarmonyPatch(typeof(BodyTemperature), "AddCold")]
        class BodyTemperature_UpdateColdMeter_Patch
        {
            static bool Prefix(BodyTemperature __instance, float cold)
            {
                if (cold > 0f)
                    __instance.currentColdMeterValue = Mathf.Clamp(__instance.currentColdMeterValue + cold * Main.config.coldMult, 0f, __instance.coldMeterMaxValue);
                else
                    __instance.currentColdMeterValue = Mathf.Clamp(__instance.currentColdMeterValue + cold, 0f, __instance.coldMeterMaxValue);

                return false;
            }
        }

        [HarmonyPatch(typeof(Player), "Start")]
        class Player_Start_Patch
        {
            static IEnumerator Test()
            {
                //AddDebug("Test start ");
                //Main.Log("Test start ");
                while (!uGUI.main.hud.active)
                    yield return null;
                AddDebug("Test end ");
            }

            static void Postfix(Player __instance)
            {
                gUIHand = Player.main.GetComponent<GUIHand>();
                //if (Main.config.cantScanExosuitClawArm)
                //    DisableExosuitClawArmScan();

                //__instance.StartCoroutine(Test());

            }
        }

        //[HarmonyPatch(typeof(CrushDamage), "GetDepth")]
        internal class CrushDamage_GetDepth_Patch
        {
            public static void Prefix(CrushDamage __instance)
            {
                if (__instance.depthCache == null)
                {
                    AddDebug("__instance.depthCache == null");
                }
                else
                    AddDebug("depthCache" + __instance.depthCache.Get());
            }
        }

        [HarmonyPatch(typeof(Player), "GetDepthClass")]
        internal class Player_GetDepthClass_Patch
        {
            public static bool Prefix(Player __instance, ref Ocean.DepthClass __result)
            {
                //AddDebug("GetDepthClass");
                Ocean.DepthClass depthClass = Ocean.DepthClass.Surface;
                if (!Main.loadingDone)
                { // avoid null reference exception when loading game inside cyclops
                    __result = depthClass;
                    return false;
                }
                CrushDamage crushDamage = null;
                if (__instance.currentSub != null && !__instance.currentSub.isBase || __instance.mode == Player.Mode.LockedPiloting)
                    crushDamage = __instance.currentSub == null ? __instance.gameObject.GetComponentInParent<CrushDamage>() : __instance.currentSub.gameObject.GetComponent<CrushDamage>();

                if (crushDamage != null)
                {
                    depthClass = crushDamage.GetDepthClass();
                    __instance.crushDepth = crushDamage.crushDepth;
                }
                else
                {
                    __instance.crushDepth = Main.config.crushDepth;
                    float depth = Ocean.GetDepthOf(__instance.gameObject);
                    if (depth > __instance.crushDepth)
                        depthClass = Ocean.DepthClass.Crush;
                    else if (depth > __instance.crushDepth * .5f)
                        depthClass = Ocean.DepthClass.Unsafe;
                    else if (depth > __instance.GetSurfaceDepth())
                        depthClass = Ocean.DepthClass.Safe;
                }
                __result = depthClass;
                return false;
            }
        }

        [HarmonyPatch(typeof(MainCameraControl), "Awake")]
        internal class MainCameraControl_Awake_Patch
        {
            public static void Postfix(MainCameraControl __instance)
            {
                if (Main.config.playerCamRot != -1f)
                    __instance.rotationX = Main.config.playerCamRot;
            }
        }

        //[HarmonyPatch(typeof(Inventory), "OnProtoDeserialize")]
        internal class Inventory_OnProtoDeserialize_Patch
        {
            public static void Postfix(Inventory __instance)
            { // does not work
                //AddDebug("OnProtoDeserialize " + Main.config.activeSlot);
                if (Main.config.activeSlot != -1) 
                    Inventory.main.quickSlots.SelectImmediate(Main.config.activeSlot);
            }
        }

        [HarmonyPatch(typeof(Inventory), "LoseItems")]
        internal class Inventory_LoseItems_Patch
        {
            public static bool Prefix(Inventory __instance)
            {
                //AddDebug("LoseItems");
                if (Main.config.loseItemsOnDeath == Config.LoseItemsOnDeath.Vanilla)
                    return true;
                else if (Main.config.loseItemsOnDeath == Config.LoseItemsOnDeath.None)
                    return false;
                else if (Main.config.loseItemsOnDeath == Config.LoseItemsOnDeath.All)
                {
                    List<InventoryItem> itemsToDrop = new List<InventoryItem>();
                    foreach (InventoryItem inventoryItem in Inventory.main.container)
                    {
                        itemsToDrop.Add(inventoryItem);
                    }
                    foreach (InventoryItem inventoryItem in (IItemsContainer)Inventory.main.equipment)
                    {
                        //AddDebug("equipment " + inventoryItem.item.GetTechName());
                        itemsToDrop.Add(inventoryItem);
                    }
                    foreach (InventoryItem item in itemsToDrop)
                    {
                        //AddDebug("DROP " + item.item.GetTechName());
                        __instance.InternalDropItem(item.item, false);
                        if (item.item.GetTechType() == TechType.Beacon)
                        {
                            Beacon component = item.item.GetComponent<Beacon>();
                            if (component)
                            {
                                if (!Player.main.IsUnderwater())
                                {
                                    component.SetDeployedOnLand();
                                    component.OnDroppedLand(true);
                                }
                                string label = Language.main.Get("DroppedBeacon");
                                component.beaconLabel.SetLabel(label);
                                component.beaconLabel.pingInstance.AddNotification();
                                component.label = label;
                            }
                        }
                    }
                }
                return false;

            }
        }



    }
}
