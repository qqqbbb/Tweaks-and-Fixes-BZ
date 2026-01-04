using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UWE;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class Pings
    {
        [HarmonyPatch(typeof(uGUI_Pings), "IsVisibleNow")]
        class DamageSystem_IsVisibleNow_Patch
        {
            public static void Postfix(uGUI_Pings __instance, ref bool __result)
            {
                //AddDebug("uGUI_Pings IsVisibleNow " + __result);
                if (Player.main == null)
                    return;

                if (Player.main.currentSub || Util.IsPlayerInDropPod())
                {
                    __result = false;
                }
            }
        }


    }
}
