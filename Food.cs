using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tweaks_Fixes
{
    internal class Food
    {
        public static HashSet<TechType> decayingFood = new HashSet<TechType>();
        public static void CheckFood(Eatable eatable)
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
            [HarmonyPrefix]
            [HarmonyPatch("Awake")]
            static void AwakePrefix(Eatable __instance)
            {
                //TechType tt = CraftData.GetTechType(__instance.gameObject);
                //Main.logger.LogDebug("Eatable Awake " + tt);
                if (decayingFood.Contains(CraftData.GetTechType(__instance.gameObject)))
                {
                    __instance.decomposes = true;
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("Awake")]
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
                    if (Util.IsEatableFish(__instance.gameObject) && __instance.foodValue > 0)
                        __instance.waterValue = __instance.foodValue * ConfigMenu.fishFoodWaterRatio.Value;
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
                //AddDebug(" IterateDespawn " + __instance.name);
                if (__instance.decomposes && __instance.foodValue > 0f)
                {
                    CheckFood(__instance);
                    //return false;
                }
                return true;
            }

        }

    }
}
