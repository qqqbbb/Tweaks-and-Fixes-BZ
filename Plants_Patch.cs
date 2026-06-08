using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Plants_Patch
    {// fruit test -583 -30 -212   -520 -85 -80     -573 -34 -110
        [HarmonyPatch(typeof(GrowingPlant))]
        class GrowingPlant_Patch
        {
            //[HarmonyPrefix]
            //[HarmonyPatch("GetGrowthDuration")]
            public static bool GetGrowthDurationPrefix(GrowingPlant __instance, ref float __result)
            {
                //__result = __instance.growthDuration * Main.config.plantGrowthTimeMult * (NoCostConsoleCommand.main.fastGrowCheat ? 0.01f : 1f);
                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("SetScale")]
            static bool SetScalePrefix(GrowingPlant __instance, Transform tr, float progress)
            {
                if (!ConfigToEdit.fixMelon.Value)
                    return true;

                TechType tt = __instance.plantTechType;

                if (tt == TechType.SnowStalkerPlant || tt == TechType.MelonPlant)
                {
                    float mult = 1.7f;
                    if (tt == TechType.MelonPlant)
                        mult = 1.2f;

                    float num = __instance.isIndoor ? __instance.growthWidthIndoor.Evaluate(progress) : __instance.growthWidth.Evaluate(progress);
                    float y = __instance.isIndoor ? __instance.growthHeightIndoor.Evaluate(progress) : __instance.growthHeight.Evaluate(progress);
                    num *= mult;
                    tr.localScale = new Vector3(num, y * mult, num);
                    if (__instance.passYbounds != null)
                        __instance.passYbounds.UpdateWavingScale(tr.localScale);
                    else
                    {
                        if (__instance.wavingScaler != null)
                            __instance.wavingScaler.UpdateWavingScale(tr.localScale);
                    }
                    //AddDebug("SnowStalkerPlant maxProgress " + __instance.maxProgress);
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Inventory), "OnAddItem")]
        class Inventory_OnAddItem_Patch
        {
            public static void Postfix(Inventory __instance, InventoryItem item)
            {
                if (!ConfigToEdit.fixMelon.Value)
                    return;

                if (item._techType == TechType.MelonSeed || item._techType == TechType.SnowStalkerFruit)
                {
                    if (item.item)
                    {
                        Plantable p = item.item.GetComponent<Plantable>();
                        if (p)
                            p.size = Plantable.PlantSize.Large;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Plantable))]
        class Plantable_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("OnProtoDeserialize")]
            static void OnProtoDeserializePostfix(Plantable __instance)
            {
                if (!ConfigToEdit.fixMelon.Value)
                    return;

                TechType tt = __instance.plantTechType;
                if (tt == TechType.SnowStalkerPlant || tt == TechType.MelonPlant)
                {
                    //AddDebug("Plantable OnProtoDeserialize " + __instance.plantTechType);
                    //AddDebug("Planter AddItem fix " + p.plantTechType);
                    __instance.size = Plantable.PlantSize.Large;
                }
                if (!ConfigToEdit.canReplantMelon.Value)
                {
                    if (tt == TechType.Melon || tt == TechType.SmallMelon || tt == TechType.JellyPlant)
                        UnityEngine.Object.Destroy(__instance);
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("Spawn")]
            public static void SpawnPostfix(ref GameObject __result, Plantable __instance)
            {
                if (ConfigToEdit.randomPlantRotation.Value && __instance.plantTechType != TechType.HeatFruitPlant)
                {
                    //AddDebug("Plantable Spawn " + __result.name);
                    Vector3 rot = __result.transform.eulerAngles;
                    float y = UnityEngine.Random.Range(0, 360);
                    __result.transform.eulerAngles = new Vector3(rot.x, y, rot.z);
                }
            }
        }

        [HarmonyPatch(typeof(TechData), "GetItemSize")]
        class TechData_GetItemSize_Patch
        {
            static void Postfix(TechType techType, ref Vector2int __result)
            {
                if (!ConfigToEdit.fixMelon.Value)
                    return;

                if (techType == TechType.MelonPlant || techType == TechType.SnowStalkerPlant)
                {
                    __result = new Vector2int(2, 2);
                }
            }
        }

        [HarmonyPatch(typeof(Eatable), "Awake")]
        class Eatable_Awake_Patch
        {
            static void Postfix(Eatable __instance)
            {
                TechType tt = CraftData.GetTechType(__instance.gameObject);
                if (tt == TechType.TwistyBridgesMushroomChunk)
                {
                    Rigidbody rb = __instance.GetComponent<Rigidbody>();
                    if (rb)
                    {
                        //AddDebug("Eatable Awake TwistyBridgesMushroomChunk " + rb.isKinematic);
                        rb.isKinematic = false;
                    }
                }
            }
        }

        //[HarmonyPatch(typeof(FruitPlant), "OnFruitHarvest")]
        class FruitPlant_OnFruitHarvest_Patch
        {
            public static void Postfix(FruitPlant __instance, PickPrefab fruit)
            {
                //AddDebug("FruitPlant OnFruitHarvest " + fruit.pickTech);
            }
        }

        //[HarmonyPatch(typeof(Pickupable), "OnHandClick")]
        class Pickupable_OnHandClick_Patch
        {
            public static void Postfix(Pickupable __instance)
            {
                if (__instance.GetTechType() == TechType.KelpRootPustule)
                {
                    //AddDebug("KelpRootPustule Pickupable OnHandClick ");
                }
            }
        }

        //[HarmonyPatch(typeof(PickPrefab), "OnHandClick")]
        class PickPrefab_OnHandClick_Patch
        {
            public static void Postfix(Pickupable __instance)
            {
                if (__instance.GetTechType() == TechType.KelpRootPustule)
                {
                    //AddDebug("KelpRootPustule PickPrefab OnHandClick ");
                }
            }
        }


    }
}
