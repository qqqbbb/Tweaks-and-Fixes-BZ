using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class Table_Coral
    {
        [HarmonyPatch(typeof(ResourceTracker), "Start")]
        class ResourceTracker_Start_Patch
        {
            public static void Postfix(ResourceTracker __instance)
            {
                //AddDebug("ResourceTracker Start");
                if (ConfigToEdit.fixTableCoral.Value && __instance.techType == TechType.GenericJeweledDisk)
                {
                    Vector3 rot = __instance.transform.eulerAngles;
                    __instance.transform.eulerAngles = new Vector3(rot.x, rot.y, 0f);
                    Animator a = __instance.GetComponentInChildren<Animator>();
                    if (a != null)
                    {
                        //AddDebug("playbackTime " + a.playbackTime);
                        a.enabled = false;
                    }
                }
            }


        }
    }
}
