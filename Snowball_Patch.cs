using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    public class Snowball_Patch
    {

        //[HarmonyPatch(typeof(Eatable))]
        class Eatable_Patch
        { // Food_Patch
            //[HarmonyPrefix]
            //[HarmonyPatch("Awake")]
            static void AwakePrefix(Eatable __instance)
            {
                if (__instance.GetComponent<SnowBall>())
                {
                    __instance.decomposes = true;
                }
            }
            //[HarmonyPostfix]
            //[HarmonyPatch( "SetDecomposes")]
            static void PSetDecomposesostfix(Eatable __instance, bool value)
            {
                //AddDebug(__instance.name + " SetDecomposes " + value);
                if (__instance.GetComponent<SnowBall>())
                {
                    //AddDebug( " SetDecomposes " + value);
                }
            }
            //[HarmonyPrefix]
            //[HarmonyPatch("IterateDespawn")]
            static bool IterateDespawnPrefix(Eatable __instance)
            {
                if (__instance.GetComponent<SnowBall>())
                {
                    //AddDebug(" IterateDespawn ");
                    //CheckSnowball(__instance);
                    return false;
                }
                return true;
            }
            //[HarmonyPostfix]
            //[HarmonyPatch("GetDecayValue")]
            static bool GetDecayValuePostfix(Eatable __instance, ref float __result)
            {
                if (__instance.GetComponent<SnowBall>())
                {
                    float temp = Main.bodyTemperature.CalculateEffectiveAmbientTemperature();
                    if (temp > 0f)
                    {
                        //AddDebug(" GetDecayValue ");
                        //__result *=
                    return false;
                    }
                }
                return true;
            }
        }

        //[HarmonyPatch(typeof(ItemsContainer), "NotifyAddItem")]
        class ItemsContainer_NotifyAddItem_Patch
        {
            static void Postfix(ItemsContainer __instance, InventoryItem item)
            {
                //AddDebug("NotifyAddItem " + item.item.GetTechName());
                if (item.item.GetTechType() == TechType.SnowBall)
                {
                    BodyTemperature bt = Player.main.GetComponent<BodyTemperature>();
                    if (!bt)
                        return;

                    bool melt = bt.CalculateEffectiveAmbientTemperature() > 0f;
                    if (melt)
                    {
                        //AddDebug("NotifyAddItem EnsureComponent SnowBallChecker");
                        Eatable eatable = item.item.GetComponent<Eatable>();
                        eatable.SetDecomposes(true);
                        //__instance.tr.gameObject.EnsureComponent<SnowBallChecker>();
                    }
                    else
                    {
                        Eatable eatable = item.item.GetComponent<Eatable>();
                        eatable.SetDecomposes(false);
                    }
                }
            }
        }



    }
}
