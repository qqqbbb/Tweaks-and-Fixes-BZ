using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Aquarium_Fix
    {

        //[HarmonyPatch(typeof(StorageContainer), "Awake")]
        public class StorageContainer_Awake_Patch
        {
            static void Postfix(StorageContainer __instance)
            {
                AddDebug(__instance .name + " Awake");
                //__instance.UpgradeLegacyStorage();
            }
        }

        [HarmonyPatch(typeof(Aquarium), "Start")]
        public class Aquarium_Start_Patch
        {
            static void Prefix(Aquarium __instance)
            {
                //AddDebug(__instance.name + " Start");
                __instance.storageContainer.Awake();
                //__instance.UpgradeLegacyStorage();
            }
        }

    }
}
