using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{   // not tested with more than 1 grav trap
    [HarmonyPatch(typeof(Gravsphere))]
    public class Gravsphere_Patch
    {
        static public Gravsphere gravSphere;
        static public HashSet<Pickupable> gravSphereFish = new HashSet<Pickupable>();
        //static public HashSet<GasPod> gasPods = new HashSet<GasPod>();
        //static public HashSet<Pickupable> gravSphereFish = new HashSet<Pickupable>();
        static public HashSet<TechType> gravTrappable = new HashSet<TechType>();

        [HarmonyPostfix]
        [HarmonyPatch("IsValidTarget")]
        public static void OnPickedUp(Gravsphere __instance, GameObject obj, ref bool __result)
        {
            if (__result)
                return;

            TechType t = CraftData.GetTechType(obj);

            if (t != TechType.None && gravTrappable.Contains(t))
            {
                __result = true;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("AddAttractable")]
        public static void AddAttractable(Gravsphere __instance, Rigidbody r)
        {
            gravSphere = __instance;
            Pickupable p = r.GetComponent<Pickupable>();
            //AddDebug("AddAttractable ");
            if (p)
            {
                gravSphereFish.Add(p);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("ClearAll")]
        public static void ClearAll(Gravsphere __instance)
        {
            //AddDebug("ClearAll ");
            gravSphereFish.Clear();
        }

        //[HarmonyPrefix]
        //[HarmonyPatch("OnDropped")]
        public static bool OnTriggerEnter(Gravsphere __instance, Pickupable p)
        {

            AddDebug(" OnDropped ");
            //__instance.animator.SetBool("deployed", true);
            //__instance.trigger.enabled = false;
            //__instance.StartCoroutine(__instance.ActivateAsync());
            return false;
        }

        [HarmonyPatch(typeof(Pickupable), "Pickup")]
        internal class Pickupable_Pickup_Patch
        {
            public static void Postfix(Pickupable __instance)
            {
                if (gravSphereFish.Contains(__instance))
                {
                    int num = gravSphere.attractableList.IndexOf(__instance.GetComponent<Rigidbody>());
                    if (num == -1)
                        return;
                    //AddDebug("Pick up gravSphere");
                    gravSphere.removeList.Add(num);
                }
            }
        }
    }
}
