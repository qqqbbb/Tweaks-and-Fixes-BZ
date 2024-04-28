using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Fragment_Patch
    {
        [HarmonyPatch(typeof(ResourceTracker), "Start")]
        class ResourceTracker_Start_Patch
        {
            static void Postfix(ResourceTracker __instance)
            {
                if (ConfigToEdit.dontSpawnKnownFragments.Value
                    && __instance.techType == TechType.Fragment)
                {
                    TechType tt = CraftData.GetTechType(__instance.gameObject);
                    if (PDAScanner.complete.Contains(tt))
                    {
                        //AddDebug("ResourceTracker start known " + __instance.techType + " " + CraftData.GetTechType(__instance.gameObject));
                        __instance.Unregister();
                        //AddDebug("Destroy " + tt);
                        //if (__instance.transform.parent.name == "CellRoot(Clone)")
                            UnityEngine.Object.Destroy(__instance.gameObject);
                        //else
                        //{
                            //AddDebug("parent not CellRoot " + __instance.name);
                            //Main.Log("parent not CellRoot " + __instance.name + " parent " + __instance.transform.parent.name);
                            //UnityEngine.Object.Destroy(__instance.transform.parent.gameObject);
                        //}
                    }
                }
            }
        }
    }
}
