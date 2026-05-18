using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ErrorMessage;


namespace Tweaks_Fixes
{
    internal class Food
    {
        public static HashSet<TechType> decayingFood = new HashSet<TechType>();
        public static void PauseDecayIfOutside(Eatable eatable)
        {
            //AddDebug(" CheckFood " + eatable.name);
            float temp = Util.GetTemperature(eatable.gameObject);
            if (temp < 0f)
                eatable.PauseDecay();
            else
                eatable.UnpauseDecay();
        }


        [HarmonyPatch(typeof(Eatable))]
        class Eatable_patch
        {
            [HarmonyPrefix, HarmonyPatch("Awake")]
            static void AwakePrefix(Eatable __instance)
            {
                TechType tt = CraftData.GetTechType(__instance.gameObject);
                if (decayingFood.Contains(tt))
                {
                    //AddDebug("decayingFood " + tt);
                    __instance.decomposes = true;
                    __instance.despawns = true;
                    __instance.kDecayRate = 0.005f;
                }
            }

            [HarmonyPostfix, HarmonyPatch("Awake")]
            public static void AwakePostfix(Eatable __instance)
            {
                //AddDebug("Eatable awake " + __instance.gameObject.name);
                //Main.Log("Eatable awake " + __instance.gameObject.name + " decomposes "+ __instance.decomposes);
                //__instance.kDecayRate *= .5f;
                //string tt = CraftData.GetTechType(__instance.gameObject).AsString();
                //Main.Log("Eatable awake " + tt );
                //Main.Log("kDecayRate " + __instance.kDecayRate);
                //Main.Log("waterValue " + __instance.waterValue);
                //Creature creature = __instance.GetComponent<Creature>();

                if (Util.IsFood(__instance))
                {
                    //AddDebug(__instance.name + " kDecayRate " + __instance.kDecayRate);
                    __instance.kDecayRate *= ConfigMenu.foodDecayRateMult.Value;
                }
                if (ConfigMenu.fishFoodWaterRatio.Value > 0)
                {
                    if (Util.IsRawFish(__instance.gameObject) && __instance.foodValue > 0)
                        __instance.waterValue = __instance.foodValue * ConfigMenu.fishFoodWaterRatio.Value * .01f;
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch("SetDecomposes")]
            public static void SetDecomposesPrefix(Eatable __instance, ref bool value)
            { // SetDecomposes runs when fish killed
                if (Util.IsFood(__instance) && value && ConfigMenu.foodDecayRateMult.Value == 0)
                    value = false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("IterateDespawn")]
            static bool IterateDespawnPrefix(Eatable __instance)
            {
                if (!Main.gameLoaded)
                    return false;
                //AddDebug($" IterateDespawn {__instance.name} {__instance.GetDecayValue()}");
                if (__instance.decomposes && __instance.foodValue > 0f)
                {
                    PauseDecayIfOutside(__instance);
                }
                if (__instance.gameObject.activeSelf && __instance.IsRotten() && DayNightCycle.main.timePassedAsFloat - __instance.timeDespawnStart > __instance.despawnDelay)
                { // fix bug: fish in player hand despawns
                    PlayerTool tool = Inventory.main.GetHeldTool();
                    if (tool)
                    {
                        Eatable eatable = tool.GetComponent<Eatable>();
                        if (eatable && eatable == __instance)
                            return false;
                    }
                }
                return true;
            }

            [HarmonyPostfix, HarmonyPatch("GetHealthValue")]
            static void GetHealthValuePostfix(Eatable __instance, ref float __result)
            {
                TechType tt = CraftData.GetTechType(__instance.gameObject);
                if (Pickupable_.eatableHealth.ContainsKey(tt))
                {
                    int healthValue = Pickupable_.eatableHealth[tt];
                    //AddDebug($"{__instance.name} HealthValue {healthValue} old {__result}");
                    if (healthValue < 0 && healthValue < __result)
                        __result = healthValue;
                }
            }

            [HarmonyPostfix, HarmonyPatch("GetWaterValue")]
            static void GetWaterValuePostfix(Eatable __instance, ref float __result)
            {
                TechType tt = CraftData.GetTechType(__instance.gameObject);
                if (Pickupable_.eatableWater.ContainsKey(tt))
                {
                    int value = Pickupable_.eatableWater[tt];
                    if (value < 0 && value < __result)
                        __result = value;
                }
            }

            [HarmonyPostfix, HarmonyPatch("GetFoodValue")]
            static void GetFoodValuePostfix(Eatable __instance, ref float __result)
            {
                TechType tt = CraftData.GetTechType(__instance.gameObject);
                if (Pickupable_.eatableFood.ContainsKey(tt))
                {
                    int value = Pickupable_.eatableFood[tt];
                    if (value < 0 && value < __result)
                        __result = value;
                }
            }

        }

    }
}
