using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tweaks_Fixes
{
    internal class Water_Freeze
    {
        public static void CheckWater(Eatable eatable)
        {   // __instance.timeDecayStart stores decay value
            float temp = Util.GetTemperature(eatable.gameObject);
            //TechType tt = CraftData.GetTechType(eatable.gameObject);
            //if (tt == TechType.BigFilteredWater)
            //AddDebug(eatable.name + " CheckWater " + temp);
            //AddDebug(eatable.name + " CheckWater " + eatable.timeDecayStart);
            if (temp <= 0)
            {
                //AddDebug(" freeze " + eatable.name);
                //eatable.UnpauseDecay();
                if (eatable.timeDecayStart < eatable.waterValue)
                    eatable.timeDecayStart += ConfigMenu.waterFreezeRate.Value * DayNightCycle.main._dayNightSpeed;
                else if (eatable.timeDecayStart > eatable.waterValue)
                    eatable.timeDecayStart = eatable.waterValue;
            }
            else if (temp > 0)
            {
                if (eatable.timeDecayStart > 0)
                    eatable.timeDecayStart -= ConfigMenu.waterFreezeRate.Value * DayNightCycle.main._dayNightSpeed;
                else if (eatable.timeDecayStart < 0)
                    eatable.timeDecayStart = 0;
                //AddDebug(" thaw " + eatable.name);
                //eatable.timeDecayStart += eatable.kDecayRate;
                //if (eatable.GetWaterValue() < eatable.waterValue && eatable.timeDecayPause < DayNightCycle.main.timePassedAsFloat)
                //{
                //    AddDebug(" thaw " + eatable.name);
                //    eatable.timeDecayPause -= waterFreezeRate * 33.33f * DayNightCycle.main._dayNightSpeed;
                //}
                //eatable.PauseDecay();
            }
            //AddDebug(eatable.name + " CheckWater done " + eatable.timeDecayStart);
            //AddDebug(" GetWaterValue " + eatable.GetWaterValue());
            //AddDebug("timePassedAsFloat " + DayNightCycle.main.timePassedAsFloat);
            //AddDebug("timeDecayStart " + eatable.timeDecayStart);
            //AddDebug("DecayValue " + eatable.GetDecayValue());
            //AddDebug("snowball GetDecayValue " + eatable.GetDecayValue());
        }

        [HarmonyPatch(typeof(Eatable))]
        class Eatable_patch
        {
            [HarmonyPrefix, HarmonyPatch("Awake")]
            static bool AwakePrefix(Eatable __instance)
            {
                //TechType tt = CraftData.GetTechType(__instance.gameObject);
                //Main.logger.LogDebug("Eatable Awake " + tt);
                if (ConfigMenu.waterFreezeRate.Value > 0f && Util.IsWater(__instance))
                {
                    if (__instance.timeDecayPause > 0f)
                    {
                        __instance.waterValue = __instance.timeDecayPause;
                        //AddDebug(__instance.name + " used water " + __instance.waterValue);
                    }
                    return false;
                }
                return true;
            }

            [HarmonyPostfix, HarmonyPatch("Awake")]
            public static void AwakePostfix(Eatable __instance)
            {
                if (ConfigMenu.waterFreezeRate.Value > 0f && Util.IsWater(__instance))
                {
                    __instance.decomposes = true;
                    //__instance.kDecayRate = waterFreezeRate;
                    //__instance.SetDecomposes(true);
                    //__instance.PauseDecay();
                    __instance.StartDespawnInvoke();
                }
                if (ConfigMenu.fishFoodWaterRatio.Value > 0)
                {
                    if (Util.IsEatableFish(__instance.gameObject) && __instance.foodValue > 0)
                        __instance.waterValue = __instance.foodValue * ConfigMenu.fishFoodWaterRatio.Value;
                }
            }

            [HarmonyPostfix, HarmonyPatch("GetFoodValue")]
            public static void GetFoodValuePostfix(Eatable __instance, ref float __result)
            {
                if (ConfigMenu.waterFreezeRate.Value > 0f && Util.IsWater(__instance))
                {
                    if (__instance.GetWaterValue() > 0)
                        __result = __instance.foodValue;
                    else
                        __result = 0f;
                }
            }

            [HarmonyPrefix, HarmonyPatch("IterateDespawn")]
            static bool IterateDespawnPrefix(Eatable __instance)
            {
                if (!Main.gameLoaded)
                    return false;
                //AddDebug(" IterateDespawn " + __instance.name);
                if (ConfigMenu.waterFreezeRate.Value > 0f && Util.IsWater(__instance))
                {
                    CheckWater(__instance);
                    return false;
                }
                return true;
            }

            [HarmonyPostfix, HarmonyPatch("GetSecondaryTooltip")]
            public static void GetSecondaryTooltipPostfix(Eatable __instance, ref string __result)
            {
                if (ConfigMenu.waterFreezeRate.Value > 0f && Util.IsWater(__instance))
                    __result = "";
            }

            [HarmonyPostfix, HarmonyPatch("GetDecayValue")]
            public static void GetDecayValuePostfix(Eatable __instance, ref float __result)
            {
                //AddDebug(__instance.name + " GetDecayValue " );
                if (ConfigMenu.waterFreezeRate.Value > 0f && Util.IsWater(__instance))
                {
                    //AddDebug(__instance.name + " Water GetDecayValue ");
                    __result = __instance.timeDecayStart;
                    //AddDebug(__instance.name + " GetDecayValue " + __result);
                }
            }
        }

        [HarmonyPatch(typeof(Fridge))]
        class Fridge_patch
        {
            [HarmonyPrefix, HarmonyPatch("AddItem")]
            static bool AddItemPrefix(Fridge __instance, InventoryItem item)
            { // dont touch timeDecayStart if water
                if (item == null || item.item == null)
                    return false;

                Eatable eatable = item.item.GetComponent<Eatable>();
                if (eatable == null)
                    return false;

                bool water = ConfigMenu.waterFreezeRate.Value > 0f && Util.IsWater(eatable);
                if (water || !eatable.decomposes || !__instance.powerConsumer.IsPowered())
                    return false;

                eatable.PauseDecay();
                return false;
            }
            [HarmonyPrefix, HarmonyPatch("RemoveItem")]
            static bool RemoveItemPrefix(Fridge __instance, InventoryItem item)
            { // dont touch timeDecayStart if water
                if (item == null || item.item == null)
                    return false;

                Eatable eatable = item.item.GetComponent<Eatable>();
                bool water = ConfigMenu.waterFreezeRate.Value > 0f && Util.IsWater(eatable);
                if (eatable == null || water || !eatable.decomposes)
                    return false;

                eatable.UnpauseDecay();
                return false;
            }
        }

    }
}
