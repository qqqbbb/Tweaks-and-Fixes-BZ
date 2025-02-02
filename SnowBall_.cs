using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Tweaks_Fixes
{
    internal class SnowBall_
    {
        static float snowBallMeltRate = 0.05f;
        public static void CheckSnowball(Eatable eatable)
        {
            InventoryItem inventoryItem = eatable.GetComponent<Pickupable>().inventoryItem;
            ItemsContainer container = null;
            if (inventoryItem != null)
            {
                container = inventoryItem.container as ItemsContainer;
                if (Main.fridges.Contains(container))
                //if (container != null && container.tr.parent && container.tr.parent.GetComponent<Fridge>())
                {
                    //AddDebug("snowball in fridge " );
                    return;
                }
            }
            else
            {
                float dist = Vector3.Distance(Player.main.transform.position, eatable.transform.position);
                //AddDebug( " dist " + dist);
                if (dist > 33f)
                {
                    eatable.CancelInvoke();
                    UnityEngine.Object.Destroy(eatable.gameObject);
                    return;
                }
            }
            if (eatable.GetWaterValue() <= 0f)
            {
                if (container != null)
                    container.RemoveItem(inventoryItem.item);
                //AddDebug("Destroy snowball ");
                eatable.CancelInvoke();
                UnityEngine.Object.Destroy(eatable.gameObject);
                return;
            }
            float temp = Util.GetTemperature(eatable.gameObject);
            //AddDebug(eatable.name + " temperature " + temp);
            if (temp > 0)
            {
                //eatable.kDecayRate = decayRate * temp ;
                eatable.UnpauseDecay();
            }
            else if (temp < 0)
                eatable.PauseDecay();
            //AddDebug(" GetWaterValue " + eatable.GetWaterValue());
            //AddDebug("timePassedAsFloat " + DayNightCycle.main.timePassedAsFloat);
            //AddDebug("timeDecayStart " + eatable.timeDecayStart);
            //AddDebug("DecayValue " + eatable.GetDecayValue());
            //AddDebug("snowball GetDecayValue " + eatable.GetDecayValue());
        }

        [HarmonyPatch(typeof(SnowBall))]
        class SnowBall_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Awake")]
            static void AwakePostfix(SnowBall __instance)
            {
                //AddDebug("SnowBall Awake");
                if (ConfigMenu.snowballWater.Value > 0)
                {
                    Eatable eatable = __instance.gameObject.EnsureComponent<Eatable>();
                    eatable.kDecayRate = snowBallMeltRate;
                    eatable.decomposes = true;
                    eatable.waterValue = ConfigMenu.snowballWater.Value;
                    eatable.coldMeterValue = ConfigMenu.snowballWater.Value;
                    //AddDebug("SnowBall Awake waterValue " + eatable.waterValue);
                    __instance.GetComponent<WorldForces>().underwaterGravity = .5f;
                }
                LiveMixin lm = __instance.gameObject.AddComponent<LiveMixin>();
                lm.data = ScriptableObject.CreateInstance<LiveMixinData>();
                lm.data.maxHealth = 1;
                lm.data.destroyOnDeath = true;
                //lm.data.explodeOnDestroy = false;
                lm.data.knifeable = true;
                Util.AddVFXsurfaceComponent(__instance.gameObject, VFXSurfaceTypes.snow); // vanilla is metal
                //SnowBallChecker snowBallChecker = __instance.gameObject.EnsureComponent<SnowBallChecker>();
                //snowBallChecker.InvokeRepeating("CheckSnowball", 1f, checkInterval);
            }

            [HarmonyPrefix]
            [HarmonyPatch("Update")]
            static bool UpdatePrefix(SnowBall __instance)
            {
                if (__instance.throwing)
                    __instance.sequence.Update();

                return false;
            }
        }

        [HarmonyPatch(typeof(Eatable))]
        class Eatable_patch
        {
            [HarmonyPrefix, HarmonyPatch("Awake")]
            static void AwakePrefix(Eatable __instance)
            {
                //TechType tt = CraftData.GetTechType(__instance.gameObject);
                //Main.logger.LogDebug("Eatable Awake " + tt);
                if (__instance.GetComponent<SnowBall>())
                    __instance.decomposes = true;
            }

            [HarmonyPrefix, HarmonyPatch("IterateDespawn")]
            static void IterateDespawnPrefix(Eatable __instance)
            {
                if (!Main.gameLoaded)
                    return;
                //AddDebug(" IterateDespawn " + __instance.name);
                if (__instance.GetComponent<SnowBall>())
                {
                    CheckSnowball(__instance);
                }
            }

            //[HarmonyPostfix, HarmonyPatch("GetHealthValue")]
            public static void GetHealthValuePostfix(Eatable __instance, ref float __result)
            {
                if (__instance.GetComponent<SnowBall>())
                    __result = 0f;
            }

            //[HarmonyPostfix, HarmonyPatch("GetFoodValue")]
            public static void GetFoodValuePostfix(Eatable __instance, ref float __result)
            {
                if (__instance.GetComponent<SnowBall>())
                    __result = 0f;
            }



        }
    }
}
