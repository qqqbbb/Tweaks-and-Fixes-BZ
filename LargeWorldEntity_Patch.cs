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
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void StartPostfix(LargeWorldEntity __instance)
        {
            TechType tt = CraftData.GetTechType(__instance.gameObject);
            //Main.logger.LogDebug("LargeWorldEntity start " + tt);
            if (tt == TechType.GenericJeweledDisk)
            {
                VFXSurface surface = __instance.gameObject.EnsureComponent<VFXSurface>();
                surface.surfaceType = VFXSurfaceTypes.coral;
                if (Main.config.fixCoral)
                {
                    Animator a = __instance.GetComponentInChildren<Animator>();
                    if (a)
                        a.enabled = false;

                    Vector3 rot = __instance.gameObject.transform.eulerAngles;
                    //Main.Log("fix GenericJeweledDisk " + __instance.gameObject.transform.position.x + " " + __instance.gameObject.transform.position.y + " " + __instance.gameObject.transform.position.z + " rot " + rot.x + " " + rot.y + " " + rot.z);
                    __instance.gameObject.transform.eulerAngles = new Vector3(rot.x, rot.y, 0f);
                }
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
            else if (tt == TechType.TwistyBridgeCoralLong)
            {
                VFXSurface surface = __instance.gameObject.EnsureComponent<VFXSurface>();
                surface.surfaceType = VFXSurfaceTypes.vegetation;
                // disable collision but allow scanning
                Collider collider = __instance.GetComponent<Collider>();
                if (collider)
                    UnityEngine.Object.Destroy(collider);

                Transform tr = __instance.transform.Find("GameObject");
                if (tr)
                {
                    BoxCollider bc = tr.GetComponent<BoxCollider>();
                    bc.isTrigger = true;
                }
            }
            else if (tt == TechType.TapePlant)
            {// disable collision but allow scanning
                Transform tr = __instance.transform.GetChild(0);
                CapsuleCollider cc = __instance.GetComponent<CapsuleCollider>();
                GameObject go = new GameObject("collision");
                go.transform.SetParent(__instance.transform);
                go.transform.position = tr.position;
                go.transform.localPosition = tr.localPosition;
                go.layer = 13;
                CapsuleCollider cc1 = go.AddComponent<CapsuleCollider>();
                cc1.isTrigger = true;
                cc1.center = cc.center;
                cc1.radius = cc.radius;
                cc1.height = cc.height * cc.height;
                UnityEngine.Object.Destroy(cc);
            }
            else if (tt == TechType.CyanFlower)
            {// disable collision but allow scanning
                Transform tr = __instance.transform.Find("collision");
                if (tr)
                {
                    tr.gameObject.layer = 13;
                    CapsuleCollider cc = tr.GetComponent<CapsuleCollider>();
                    cc.isTrigger = true;
                }
            }
            else if (tt == TechType.TwistyBridgesCoralShelf || tt == TechType.JeweledDiskPiece || tt == TechType.GenericJeweledDisk)
            {
                VFXSurface surface = __instance.gameObject.EnsureComponent<VFXSurface>();
                surface.surfaceType = VFXSurfaceTypes.coral;
            }
            else if (tt == TechType.OxygenPlant)
            {
                VFXSurface surface = __instance.gameObject.EnsureComponent<VFXSurface>();
                surface.surfaceType = VFXSurfaceTypes.lilypad;
            }
            else if (tt == TechType.None)
            {
                //if (__instance.name == "treespires_fissure_edge_01_b_curved_inner(Clone)")
                {
                    //int x = (int)__instance.transform.position.x;
                    //int y = (int)__instance.transform.position.y;
                    //int z = (int)__instance.transform.position.z;
                    //if (x == -133 && y == -373 && z == -1342)
                    //{
                        //AddDebug("treespires_fissure_edge_01_b_curved_inner ");
                        //__instance.transform.position = new Vector3(-88.5f, -448.4f, __instance.transform.position.z);
                        //Vector3 rot = __instance.transform.eulerAngles;
                        //__instance.transform.eulerAngles = new Vector3(rot.x, rot.y, 359f);
                    //}
                }
                if (__instance.name == "treespires_fissure_edge_01_b_straight(Clone)")
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
                else if (__instance.name == "treespires_eroded_rock_01_b(Clone)")
                {
                    int x = (int)__instance.transform.position.x;
                    int y = (int)__instance.transform.position.y;
                    int z = (int)__instance.transform.position.z;
                    if (x == -94 && y == -450 && z == -1418)
                    {// upscale to hide another hole near top
                        Vector3 scale = __instance.transform.localScale;
                        __instance.transform.localScale = new Vector3(scale.x, 0.95f, scale.z);
                        Vector3 rot = __instance.transform.eulerAngles;
                        __instance.transform.eulerAngles = new Vector3(rot.x, rot.y, 348f);
                    }
                }
                else if (__instance.name == "treespires_eroded_rock_01_d(Clone)")
                {
                    int x = (int)__instance.transform.position.x;
                    int y = (int)__instance.transform.position.y;
                    int z = (int)__instance.transform.position.z;
                    if (x == -208 && y == -410 && z == -1500)
                        __instance.gameObject.SetActive(false);
                    else if (x == -193 && y == -399 && z == -1469)
                    {
                        __instance.transform.position = new Vector3(__instance.transform.position.x, -401f, __instance.transform.position.z);
                    }
                    //else if (x == -112 && y == -444 && z == -1430)// this can't be moved down
                    //{ 
                    //}
                }
                else if (__instance.name == "treespires_eroded_rock_01_e(Clone)")
                {
                    int x = (int)__instance.transform.position.x;
                    int y = (int)__instance.transform.position.y;
                    int z = (int)__instance.transform.position.z;
                    if (x == -222 && y == -396 && z == -1394)
                    { // -222.97 -396.54 -1394.43    
                        __instance.transform.position = new Vector3(-224f, __instance.transform.position.y, __instance.transform.position.z);
                        Vector3 rot = __instance.transform.eulerAngles;
                        __instance.transform.eulerAngles = new Vector3(rot.x, 177f, 338f);
                    }
                }
                else if (__instance.name == "_DELETED_ArcticKelp_Kelp_2(Clone)")
                {
                    //AddDebug("_DELETED_ArcticKelp_Kelp_2(Clone)");
                    Transform tr = __instance.transform.Find("young_04_stem/Coral_reef_kelp_young_04_billboard");
                    if (tr) // looks too different
                        UnityEngine.Object.Destroy(tr.gameObject);
                }
                else if (__instance.name.Contains("talactite"))
                {
                    VFXSurface surface = __instance.gameObject.EnsureComponent<VFXSurface>();
                    surface.surfaceType = VFXSurfaceTypes.rock;
                }

            }
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
            if (uGUI.isLoading)
                return false;

            //AddDebug(__instance.name + " StartFading ");
            //AddDebug(" Tools_Patch.equippedTool " + Tools_Patch.equippedTool.name);
            if (Tools_Patch.equippedTool != null && Tools_Patch.equippedTool.gameObject == __instance.gameObject)
            {
                //AddDebug(__instance.name + " StartFading equippedTool");
                Tools_Patch.equippedTool = null;
                return false;
            }
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
            TechType tt = CraftData.GetTechType(__instance.gameObject);
            if (tt == TechType.JeweledDiskPiece || tt == TechType.Gold || tt == TechType.Silver || tt == TechType.Titanium || tt == TechType.Lead || tt == TechType.Lithium || tt == TechType.Diamond)
            {
                //AddDebug("resource");
                return false;
            }
            return true;
        }
   
    }


}
