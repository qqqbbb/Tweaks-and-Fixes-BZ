using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(Bed))]
    internal class Bed_
    {
        static bool space;
        static Vector3 playerPos;

        //[HarmonyPostfix, HarmonyPatch("GetCanSleep")]
        public static void GetCanSleepPostfix(Bed __instance, Player player, bool notify, ref bool __result)
        {
            __result = true;
        }
        [HarmonyPostfix, HarmonyPatch("CheckForSpace")]
        public static void CheckForSpacePostfix(Bed __instance, ref bool __result)
        {
            if (__result == true)
                space = true;
            //AddDebug("CheckForSpace " + __result);
            __result = true;

        }
        [HarmonyPrefix, HarmonyPatch("OnHandClick")]
        public static void OnHandClickPrefix(Bed __instance)
        {
            space = false;
            playerPos = Player.main.transform.position;
        }

        [HarmonyPostfix, HarmonyPatch("ExitInUseMode")]
        public static void ExitInUseModePostfix(Bed __instance, Player player)
        {
            if (space == false)
                __instance.StartCoroutine(RestorePlayerPos(player));
        }

        public static IEnumerator RestorePlayerPos(Player player)
        {
            yield return new WaitUntil(() => player.cinematicModeActive == false);
            //AddDebug("RestorePlayerPos ");
            if (playerPos != default)
                player.transform.position = playerPos;
        }



    }
}
