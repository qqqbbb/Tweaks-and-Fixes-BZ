using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UWE;
using HarmonyLib;
using ProtoBuf;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Player_Patches
    {
        //static Survival survival;
        //static LiveMixin liveMixin;
        //public static GUIHand gUIHand;
        //public static float exitWaterOffset = 0.8f; // 0.8f
        public static float crushPeriod = 3f;
        public static float ambientTemperature;

        [HarmonyPatch(typeof(BodyTemperature))]
        class BodyTemperature_Patch
        { 
            [HarmonyPrefix]
            [HarmonyPatch("isExposed", MethodType.Getter)]
            public static bool isExposedPrefix(BodyTemperature __instance, ref bool __result)
            {// fix : player gets cold when ambient temp is high
                if (__instance.player.frozenMixin.IsFrozen())
                {
                    __result = true;
                    return false;
                }
                bool underwater = __instance.player.transform.position.y < Ocean.GetOceanLevel() || __instance.player.IsUnderwaterForSwimming();
                bool movinUnderwater = !Main.config.useRealTempForColdMeter && underwater && (__instance.player.movementSpeed > Mathf.Epsilon || __instance.player.IsRidingCreature());
                //float temp = Main.bodyTemperature.CalculateEffectiveAmbientTemperature();
                bool heat = !Main.config.useRealTempForColdMeter && (HeatSource.GetHeatImpactAtPosition(__instance.transform.position) > 0f || __instance.player.GetCurrentHeatVolume());
                bool immune = movinUnderwater || heat || __instance.player.cinematicModeActive || Main.bodyTemperature.CalculateEffectiveAmbientTemperature() > Main.config.getWarmTemp;
                bool piloting = __instance.player.IsPiloting();
                if (Main.config.useRealTempForColdMeter && __instance.player.inHovercraft)
                    piloting = false;

                bool interior = !Main.config.useRealTempForColdMeter && __instance.player.currentInterior != null;
                __result = !immune && !piloting && !interior;
                //AddDebug("GetHeatImpactAtPosition " + HeatSource.GetHeatImpactAtPosition(__instance.transform.position));
                //AddDebug("GetCurrentHeatVolume " + __instance.player.GetCurrentHeatVolume());
                //AddDebug("isExposed " + __result + " " + (int)temp);
                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("AddCold")]
            static bool AddColdPrefix(BodyTemperature __instance, float cold)
            {
                __instance.currentColdMeterValue = Mathf.Clamp(__instance.currentColdMeterValue + cold * Main.config.coldMult, 0f, __instance.coldMeterMaxValue);

                return false;
            }

            //[HarmonyPrefix]
            //[HarmonyPatch("UpdateColdMeter")]
            static bool UpdateColdMeterPrefix(BodyTemperature __instance, bool exposedToWeather, float dt)
            {
                if (!Main.config.useRealTempForColdMeter)
                    return true;

                float num = exposedToWeather ? __instance.exposedColdMeterImpactPerSecond.Evaluate(__instance.effectiveAmbientTemperature) : __instance.shelteredColdMeterImpactPerSecond.Evaluate(__instance.effectiveAmbientTemperature);
                if (exposedToWeather)
                {
                    float resistanceAmount = __instance.GetColdResistanceAmount();
                    //if (__instance.currentColdMeterValue > __instance.coldMeterMaxWithFullBuff & (!Player.emergencyMode || resistanceAmount >= 1f))
                    if (__instance.currentColdMeterValue > __instance.coldMeterMaxWithFullBuff & (IntroVignette.isIntroActive || resistanceAmount >= 1f))
                        num = __instance.shelteredColdMeterImpactPerSecond.Evaluate(0f);
                    if (num > 0f)
                        num -= num * resistanceAmount;
                }
                __instance.AddCold(num * dt);
                __instance.wboit.frostScalar = __instance.coldMeterAlphaToFrostEffectAlpha.Evaluate(__instance.GetColdMeterAlpha());
                if (__instance.shouldReactToWarming || __instance.currentColdMeterValue <= __instance.warmingColdThreshold)
                    return false;
                __instance.shouldReactToWarming = true;

                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("GetColdResistanceAmount")]
            static bool GetColdResistanceAmountPrefix(BodyTemperature __instance, ref float __result)
            {
                __result = Mathf.Clamp01(__instance.coldResistEquipmentBuff * .01f);
                //AddDebug("GetColdResistanceAmount " + __result);
                return false;
            }

            [HarmonyPostfix]
            [HarmonyPatch("UpdateEffectiveAmbientTemperature")]
            static void UpdateEffectiveAmbientTemperaturePostfix(BodyTemperature __instance)
            {
                ambientTemperature = __instance.effectiveAmbientTemperature;
                //AddDebug("effectiveAmbientTemperature " + (int)__instance.effectiveAmbientTemperature);
            }
        }

        //[HarmonyPatch(typeof(Player), "Start")]
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

            //static void Postfix(Player __instance)
            //{
            //    gUIHand = Player.main.GetComponent<GUIHand>();
            //if (Main.config.cantScanExosuitClawArm)
            //    DisableExosuitClawArmScan();

            //__instance.StartCoroutine(Test());
            //}
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
                {
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

        //[HarmonyPatch(typeof(MainCameraControl), "Awake")]
        internal class MainCameraControl_Awake_Patch
        {
            public static void Postfix(MainCameraControl __instance)
            {
                //if (Main.config.playerCamRot != -1f)
                //    __instance.rotationX = Main.config.playerCamRot;
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

        [HarmonyPatch(typeof(Inventory))]
        class Inventory_Patch
        {
            static int usingWPT = 0;
            static float snowballWater = 0f;

            [HarmonyPrefix]
            [HarmonyPatch("LoseItems")]
            public static bool LoseItemPrefixs(Inventory __instance, ref bool __result)
            {
                //AddDebug("LoseItems");
                __result = false;
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
                        if (__instance.InternalDropItem(item.item, false, true))
                            __result = true;

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

            [HarmonyPostfix]
            [HarmonyPatch("ExecuteItemAction", new Type[] { typeof(ItemAction), typeof(InventoryItem) })]
            public static void ExecuteItemActionPostfix(Inventory __instance, ItemAction action, InventoryItem item)
            {
                //AddDebug("ExecuteItemAction " + item.item.GetTechName() + " " + action);
                if (usingWPT == 2 && action == ItemAction.Use && item.item.GetTechType() == TechType.WaterPurificationTablet)
                    usingWPT = 3;
                else
                    usingWPT = 0;
            }

            [HarmonyPostfix]
            [HarmonyPatch("OnAddItem")]
            public static void OnAddItemPostfix(Inventory __instance, InventoryItem item)
            {
                //AddDebug("OnAddItem " + item.item.GetTechName());
                if (usingWPT == 3 && snowballWater < 1f && item.item.GetTechType() == TechType.BigFilteredWater)
                {
                    Eatable eatable = item.item.GetComponent<Eatable>();
                    eatable.waterValue *= snowballWater; // resets after reload
                    //AddDebug("BigFilteredWater  " + eatable.waterValue);
                }
                usingWPT = 0;
            }

            [HarmonyPostfix]
            [HarmonyPatch("OnRemoveItem")]
            public static void OnRemoveItemPostfix(Inventory __instance, InventoryItem item)
            {
                //AddDebug("OnRemoveItem " + item.item.GetTechName());
                TechType tt = item.item.GetTechType();
                if (usingWPT == 0 && tt == TechType.SnowBall)
                {              
                    Eatable eatable = item.item.GetComponent<Eatable>();
                    if (eatable)
                    {
                        usingWPT = 1;
                        snowballWater = eatable.GetWaterValue() / eatable.waterValue;
                        //AddDebug("SnowBall GetWaterValue " + snowballWater);
                    }
                    else
                        usingWPT = 0;
                }
                else if(usingWPT == 1 && tt == TechType.WaterPurificationTablet)
                    usingWPT = 2;
                else
                    usingWPT = 0;
            }
        }


        //[HarmonyPatch(typeof(ItemsContainer))]
        class ItemsContainer_Patch
        {
            //[HarmonyPostfix]
            //[HarmonyPatch("NotifyRemoveItem")]
            public static void NotifyRemoveItemPostfix(ItemsContainer __instance, InventoryItem item)
            {
                AddDebug("ItemsContainer NotifyRemoveItem " + item.item.GetTechName());
            }
            //[HarmonyPostfix]
            //[HarmonyPatch("NotifyAddItem")]
            public static void NotifyAddItemPostfix(ItemsContainer __instance, InventoryItem item)
            {
                AddDebug("ItemsContainer NotifyAddItem " + item.item.GetTechName());
            }
        }



    }
}
