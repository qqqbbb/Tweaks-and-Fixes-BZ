using HarmonyLib;
using ProtoBuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UWE;
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
                bool movingUnderwater = !ConfigMenu.useRealTempForPlayerTemp.Value && underwater && (__instance.player.movementSpeed > Mathf.Epsilon || __instance.player.IsRidingCreature());
                //float temp = Main.bodyTemperature.CalculateEffectiveAmbientTemperature();
                bool heat = !ConfigMenu.useRealTempForPlayerTemp.Value && (HeatSource.GetHeatImpactAtPosition(__instance.transform.position) > 0f || __instance.player.GetCurrentHeatVolume());
                bool immune = movingUnderwater || heat || __instance.player.cinematicModeActive || __instance.CalculateEffectiveAmbientTemperature() > ConfigToEdit.warmTemp.Value;
                bool piloting = __instance.player.IsPiloting();

                if (piloting && ConfigMenu.useRealTempForPlayerTemp.Value)
                {
                    if (__instance.player.inHovercraft)
                        piloting = false;
                    else if (Player.main.inExosuit)
                        piloting = Player.main.currentMountedVehicle.IsPowered();
                    else if (Player.main._currentInterior != null && Player.main._currentInterior is SeaTruckSegment)
                    {
                        SeaTruckSegment sts = Player.main._currentInterior as SeaTruckSegment;
                        //AddDebug("SeaTruck IsPowered " + sts.relay.IsPowered());
                        piloting = sts.relay.IsPowered();
                    }
                }
                bool interior = !ConfigMenu.useRealTempForPlayerTemp.Value && __instance.player.currentInterior != null;
                __result = !immune && !piloting && !interior;
                //AddDebug("GetHeatImpactAtPosition " + HeatSource.GetHeatImpactAtPosition(__instance.transform.position));
                //AddDebug("GetCurrentHeatVolume " + __instance.player.GetCurrentHeatVolume());
                //AddDebug("immune " + immune + " interior " + interior);
                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("AddCold")]
            static bool AddColdPrefix(BodyTemperature __instance, float cold)
            {
                __instance.currentColdMeterValue = Mathf.Clamp(__instance.currentColdMeterValue + cold * ConfigMenu.coldMult.Value, 0f, __instance.coldMeterMaxValue);

                return false;
            }

            //[HarmonyPrefix]
            //[HarmonyPatch("GetAmbientTemperatureFromEnvironment")]
            static bool GetAmbientTemperatureFromEnvironmentPrefix(BodyTemperature __instance, ref float __result)
            {
                if (__instance.player.frozenMixin.IsFrozen())
                {
                    __result = -4f;
                    return false;
                }
                if (__instance.player.IsUnderwaterForSwimming())
                {
                    __result = __instance.GetWaterTemperature();
                    return false;
                }
                IInteriorSpace currentInterior = __instance.player.currentInterior;
                if (currentInterior != null)
                {
                    __result = currentInterior.GetInsideTemperature();
                    AddDebug("GetAmbientTemperatureFromEnvironment currentInterior " + currentInterior.GetType());
                    //AddDebug("GetAmbientTemperatureFromEnvironment GetInsideTemperature " + (int)__result);
                    return false;
                }
                __result = __instance.player.transform.position.y < Ocean.GetOceanLevel() ? __instance.GetWaterTemperature() : WeatherManager.main.GetFeelsLikeTemperature();

                return false;
            }

            //[HarmonyPrefix]
            //[HarmonyPatch("GetAmbientTemperature")]
            static bool UpdateColdMeterPrefix(BodyTemperature __instance, ref float __result)
            {
                //if (!Main.config.useRealTempForColdMeter)
                //    return true;
                float temperatureFromEnvironment = __instance.GetAmbientTemperatureFromEnvironment();
                //AddDebug("GetAmbientTemperature temperatureFromEnvironment " + (int)temperatureFromEnvironment);
                HeatVolume currentHeatVolume = __instance.player.GetCurrentHeatVolume();
                if (currentHeatVolume != null)
                {
                    float temperatureOverride = currentHeatVolume.temperatureOverride;
                    //AddDebug("GetAmbientTemperature currentHeatVolume.temperatureOverride " + (int)currentHeatVolume.temperatureOverride);
                    if (temperatureOverride > temperatureFromEnvironment)
                    {
                        __result = temperatureOverride;
                        return false;
                    }
                }
                __result = temperatureFromEnvironment;
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
                if (!Main.gameLoaded)
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
                    __instance.crushDepth = ConfigMenu.crushDepth.Value;
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
                if (Main.configMain.activeSlot != -1)
                    Inventory.main.quickSlots.SelectImmediate(Main.configMain.activeSlot);
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
                if (ConfigMenu.dropItemsOnDeath.Value == ConfigMenu.DropItemsOnDeath.Vanilla)
                    return true;
                else if (ConfigMenu.dropItemsOnDeath.Value == ConfigMenu.DropItemsOnDeath.Do_not_drop_anything)
                    return false;
                else if (ConfigMenu.dropItemsOnDeath.Value == ConfigMenu.DropItemsOnDeath.Drop_everything)
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
                else if (usingWPT == 1 && tt == TechType.WaterPurificationTablet)
                    usingWPT = 2;
                else
                    usingWPT = 0;
            }
        }

        [HarmonyPatch(typeof(WaterTemperatureSimulation), "GetTemperature", new Type[] { typeof(Vector3), typeof(float) }, new[] { ArgumentType.Normal, ArgumentType.Out })]
        class WaterTemperatureSimulation_GetTemperature_PrefixPatch
        {
            private const float defaultWaterTemp = 6f;

            public static bool Prefix(WaterTemperatureSimulation __instance, ref float __result, Vector3 wsPos, ref float posBaseTemperature)
            {
                //AddDebug(" Targeting GetTarget  " + result.name);
                float baseTemperature = defaultWaterTemp;
                WaterBiomeManager waterBiomeManager = WaterBiomeManager.main;
                WaterscapeVolume.Settings settings;
                if (!ConfigToEdit.warmKelpWater.Value && waterBiomeManager && waterBiomeManager.GetSettings(wsPos, false, out settings))
                {
                    baseTemperature = settings.temperature;
                    int biomeIndex = -1;
                    if (LargeWorld.main)
                    {
                        biomeIndex = waterBiomeManager.GetBiomeIndex(waterBiomeManager.GetBiome(wsPos, false));
                        if (biomeIndex >= 0 && biomeIndex < waterBiomeManager.biomeSettings.Count)
                        {
                            WaterBiomeManager.BiomeSettings biomeSettings = waterBiomeManager.biomeSettings[biomeIndex];
                            //AddDebug("GetTemperature biomeSettings " + biomeSettings.name);
                            if (biomeSettings.name == "arcticKelp")
                                baseTemperature = defaultWaterTemp;
                        }
                    }
                    //AddDebug("GetTemperature waterBiomeManager settings.temperature " + (int)settings.temperature);
                }
                EcoRegionManager ecoRegionManager = EcoRegionManager.main;
                if (ecoRegionManager != null)
                {
                    float distance;
                    IEcoTarget nearestTarget = ecoRegionManager.FindNearestTarget(EcoTargetType.HeatArea, wsPos, out distance, null, 3);
                    if (nearestTarget != null)
                    {
                        float num = Mathf.Clamp(60f - distance, 0f, 60f);
                        baseTemperature += num;
                        //AddDebug("GetTemperature HeatArea temperature " + (int)num);
                        //Debug.DrawLine(wsPos, nearestTarget.GetPosition(), Color.red, 5f);
                    }
                }
                posBaseTemperature = baseTemperature;
                __result = __instance.GetFinalTemperature(baseTemperature, wsPos);
                //AddDebug("GetTemperature waterBiomeManager final temperature " + (int)__result);
                return false;
            }
        }

        [HarmonyPatch(typeof(PlayerBreathBubbles), "MakeBubbles")]
        class PlayerBreathBubbles_MakeBubbles_Patch
        {
            public static bool Prefix(PlayerBreathBubbles __instance)
            {
                if (ConfigToEdit.playerBreathBubbles.Value && ConfigToEdit.playerBreathBubblesSoundFX.Value)
                    return true;

                if (!__instance.enabled)
                    return false;

                if (ConfigToEdit.playerBreathBubblesSoundFX.Value)
                    __instance.bubbleSound.Play();

                if (ConfigToEdit.playerBreathBubbles.Value)
                    __instance.bubbles.Play();

                return false;
            }
        }


    }
}
