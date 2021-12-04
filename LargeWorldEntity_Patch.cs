using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(LargeWorldEntity))]
    class LargeWorldEntity_Patch
    {
        public static bool rock_01_d_disabled = false;

        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        public static void StartPrefix(LargeWorldEntity __instance)
        {
            //if (Vector3.Distance(Player.main.transform.position, __instance.transform.position) < 6)
            //{
            //    AddDebug(__instance.name + " close to player");
            //    Main.Log(__instance.name + " close to player");
            //}
            TechType tt = CraftData.GetTechType(__instance.gameObject);
            //if (tt == TechType.None && __instance.name.StartsWith("kelpcave_root"))
            {
                //AttachFruitPlantToKelpRoot(__instance.gameObject);
                //PrefabPlaceholder[] pphs = __instance.GetComponentsInChildren<PrefabPlaceholder>();
                //Pickupable[] ps = __instance.GetComponentsInChildren<Pickupable>();
                //AddDebug(__instance.name + " LargeWorldEntity Start PrefabPlaceholder " + pphs.Length + " Pickupable " + ps.Length);
                //Main.Log(__instance.name +" LargeWorldEntity Start PrefabPlaceholder " + pphs.Length + " Pickupable " + ps.Length);
            }
            if (tt == TechType.GenericJeweledDisk)
            {
                Animator a = __instance.GetComponentInChildren<Animator>();
                if (a)
                    a.enabled = false;
                Vector3 rot = __instance.gameObject.transform.eulerAngles;
                //Main.Log("fix GenericJeweledDisk " + __instance.gameObject.transform.position.x + " " + __instance.gameObject.transform.position.y + " " + __instance.gameObject.transform.position.z + " rot " + rot.x + " " + rot.y + " " + rot.z);
                __instance.gameObject.transform.eulerAngles = new Vector3(rot.x, rot.y, 0f);
            }
            else if (tt == TechType.PurpleVegetablePlant || tt == TechType.LeafyFruitPlant || tt == TechType.HeatFruitPlant || tt == TechType.HangingFruitTree || tt == TechType.MelonPlant)
            {
                LODGroup lODGroup = __instance.GetComponent<LODGroup>();
                if (lODGroup)
                    //UnityEngine.Object.Destroy(lODGroup);
                    lODGroup.enabled = false;
            }
            else if (tt == TechType.SmallMelon)
            {
                int x = (int)__instance.transform.position.x;
                int y = (int)__instance.transform.position.y;
                int z = (int)__instance.transform.position.z;
                if ((x == 989 && y == 30 && z == -897) || (x == 989 && y == 29 && z == -896) || (x == 986 && y == 29 && z == -895))
                { // make melons in Marg greenhouse pickupable
                    __instance.GetComponent<SphereCollider>().radius = .4f;
                    //AddDebug("make melons in Marg greenhouse pickupable " + x +" " + y +" " + z);
                }
            }
            else if (tt == TechType.IceFruitPlant || tt == TechType.Creepvine || tt == TechType.SnowStalkerPlant)
            {
                //PickPrefab[] pickPrefabs = __instance.GetAllComponentsInChildren<PickPrefab>();
                Plants_Patch.AttachFruitPlant(__instance.gameObject);
            }
            else if (tt == TechType.None)
            {
                if (__instance.name == "treespires_fissure_edge_01_b_curved_inner(Clone)")
                {
                    //int x = (int)__instance.transform.position.x;
                    //int y = (int)__instance.transform.position.y;
                    //int z = (int)__instance.transform.position.z;
                    //if (x == -133 && y == -373 && z == -1342)
                    {
                        //AddDebug("treespires_fissure_edge_01_b_curved_inner ");
                        //__instance.transform.position = new Vector3(-88.5f, -448.4f, __instance.transform.position.z);
                        //Vector3 rot = __instance.transform.eulerAngles;
                        //__instance.transform.eulerAngles = new Vector3(rot.x, rot.y, 359f);
                    }
                }
                else if(__instance.name == "treespires_fissure_edge_01_b_straight(Clone)")
                {
                    int x = (int)__instance.transform.position.x;
                    int y = (int)__instance.transform.position.y;
                    int z = (int)__instance.transform.position.z;
                    //Main.Log("treespires_fissure_edge_01_b_straight " + x + " " + y + " " + z);
                    if (x == -88 && y == -448 && z == -1416)
                    {
                        __instance.transform.position = new Vector3(-88.5f, -448.4f, __instance.transform.position.z);
                        Vector3 rot = __instance.transform.eulerAngles;
                        __instance.transform.eulerAngles = new Vector3(rot.x, rot.y, 359f);
                    }
                }
                else if(__instance.name == "treespires_eroded_rock_01_b(Clone)")
                {
                    int x = (int)__instance.transform.position.x;
                    int y = (int)__instance.transform.position.y;
                    int z = (int)__instance.transform.position.z;
                    if (x == -94 && y == -450 && z == -1418)
                    {// up scale to hide another hole near top
                        Vector3 scale = __instance.transform.localScale; 
                        __instance.transform.localScale = new Vector3(scale.x, 0.95f, scale.z);
                       Vector3 rot = __instance.transform.eulerAngles;
                        __instance.transform.eulerAngles = new Vector3(rot.x, rot.y, 348f);
                    }
                }
                else if(__instance.name == "treespires_eroded_rock_01_d(Clone)")
                {
                    int x = (int)__instance.transform.position.x;
                    int y = (int)__instance.transform.position.y;
                    int z = (int)__instance.transform.position.z;
                    if (x == -208 && y == -410 && z == -1500)
                        __instance.gameObject.SetActive(false);
                    else if(x == -193 && y == -399 && z == -1469)
                    {
                        __instance.transform.position = new Vector3(__instance.transform.position.x, -401f, __instance.transform.position.z);
                    }
                    else if (x == -112 && y == -444 && z == -1430)// this can't be moved down
                    { 
                    }
                    else if (x == -208 && y == -376 && z == -1332)
                    { // there is 2 of them! reported to UWE, should be fixed
                        if (rock_01_d_disabled)
                            __instance.transform.position = new Vector3(-209f, -377.4f, -1331.5f);
                        else
                        {
                            __instance.gameObject.SetActive(false);
                            rock_01_d_disabled = true;
                        }
                    }
                }
                else if (__instance.name == "treespires_eroded_rock_01_e(Clone)")
                {
                    int x = (int)__instance.transform.position.x;
                    int y = (int)__instance.transform.position.y;
                    int z = (int)__instance.transform.position.z;
                    if (x == -222 && y == -396 && z == -1394)
                    { // -222.97 -396.54 -1394.43        0  166.82 341.89
                        __instance.transform.position = new Vector3(-224f, __instance.transform.position.y, __instance.transform.position.z);
                        Vector3 rot = __instance.transform.eulerAngles;
                        __instance.transform.eulerAngles = new Vector3(rot.x, 177f, 338f);
                    }
                }
            }
            //else if (tt == TechType.KelpRootPustule) // KelpRoot tt is none
            //{ //  at awake parent may be null
            //GameObject parent = __instance.transform.parent.gameObject;
            //if (parent && parent.GetComponent<LargeWorldEntity>())
            //{
            //PrefabPlaceholder[] pphs = parent.GetComponentsInChildren<PrefabPlaceholder>();
            //Pickupable[] ps = parent.GetComponentsInChildren<Pickupable>();
            //if (pphs.Length > 0 && pphs.Length == ps.Length)
            //    AttachFruitPlantToKelpRoot(parent);
            //AddDebug("KelpRootPustule LargeWorldEntity Start PrefabPlaceholder " + pphs.Length + " Pickupable " + ps.Length);
            //Main.Log("KelpRootPustule LargeWorldEntity Start PrefabPlaceholder " + pphs.Length + " Pickupable " + ps.Length);
            //}
            //}
            //else if (tt == TechType.Boomerang || tt == TechType.CookedBoomerang)
        }
     
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void StartPostfix(LargeWorldEntity __instance)
        {
            //TechType tt = CraftData.GetTechType(__instance.gameObject);
            if (Main.config.alwaysBestLOD)
            {
                //TechType tt = CraftData.GetTechType(__instance.gameObject);
                LODGroup[] lodGroups = __instance.GetComponentsInChildren<LODGroup>();
                if (lodGroups.Length == 0)
                    return;

                List<LODGroup> toCheck = new List<LODGroup>();
                foreach (LODGroup lodG in lodGroups)
                {
                    //Main.Log(__instance.name + " lodGroup " + lodG.name);
                    if (!lodG.enabled)
                        continue;
                    if (lodG.lodCount == 1)
                    {
                        lodG.enabled = false;
                        continue;
                    }
                    LargeWorldEntity lwe = lodG.GetComponent<LargeWorldEntity>();
                    if (lwe && lwe != __instance)
                        continue;

                    toCheck.Add(lodG);
                }
                //Main.Log(__instance.name + " LODGroup Count " + toCheck.Count);
                if (toCheck.Count != 1)
                { // dont touch those. Abandoned bases will miss pieces
                    //AddDebug(__instance.name + " lodGroups bad Count " + toCheck.Count);
                    //Main.Log(__instance.name + " lodGroups bad Count " + toCheck.Count);
                    return;
                }
                LODGroup lodGroup = toCheck[0];
                LOD[] lods = lodGroup.GetLODs();
                lodGroup.enabled = false;
                LOD best = new LOD();
                float highest = 0f;
                foreach (LOD lod in lods)
                {
                    if (lod.screenRelativeTransitionHeight > highest)
                    {
                        highest = lod.screenRelativeTransitionHeight;
                        best = lod;
                    }
                }
                //Main.Log(__instance.name + " best lod "+ best.renderers[0].name);
                foreach (LOD lod in lods)
                {
                    if (!lod.Equals(best))
                    {
                        foreach (Renderer r in lod.renderers)
                                r.enabled = false;
                    }
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("StartFading")]
        public static bool StartFadingPrefix(LargeWorldEntity __instance)
        {
            if (!Main.loadingDone)
                return false;
            else if(Tools_Patch.releasingGrabbedObject)
            {
                Tools_Patch.releasingGrabbedObject = false;
                //AddDebug("StartFading releasingGrabbedObject " + __instance.name);
                return false;
            }
            else if (Tools_Patch.repCannonGOs.Contains(__instance.gameObject))
            {
                //AddDebug("StartFading rep Cannon go " + __instance.name);
                Tools_Patch.repCannonGOs.Remove(__instance.gameObject);
                return false;
            }
            return true;
        }
   
    }


}
