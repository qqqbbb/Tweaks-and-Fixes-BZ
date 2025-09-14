﻿using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(LargeWorldEntity))]
    class LargeWorldEntity_
    {

        static HashSet<TechType> plantSurfaces = new HashSet<TechType> { TechType.PurpleVegetablePlant, TechType.Creepvine, TechType.HeatFruitPlant, TechType.IceFruitPlant, TechType.FrozenRiverPlant2, TechType.JellyPlant, TechType.LeafyFruitPlant, TechType.KelpRoot, TechType.KelpRootPustule, TechType.HangingFruitTree, TechType.MelonPlant, TechType.SnowStalkerPlant, TechType.CrashHome, TechType.DeepLilyShroom, TechType.DeepLilyPadsLanternPlant, TechType.BlueFurPlant, TechType.GlacialTree, TechType.GlowFlower, TechType.OrangePetalsPlant, TechType.HoneyCombPlant, TechType.CavePlant, TechType.GlacialPouchBulb, TechType.PurpleRattle, TechType.ThermalLily, TechType.GlacialBulb, TechType.PinkFlower, TechType.SmallMaroonPlant, TechType.DeepTwistyBridgesLargePlant, TechType.GenericShellDouble, TechType.TwistyBridgeCliffPlant, TechType.GenericCrystal, TechType.CaveFlower, TechType.TrianglePlant, TechType.PurpleBranches, TechType.GenericBigPlant2, TechType.GenericBigPlant1, TechType.GenericShellSingle, TechType.OxygenPlant, TechType.TwistyBridgeCoralLong, TechType.GenericCage, TechType.TapePlant, TechType.GenericBowl, TechType.TallShootsPlant, TechType.TreeSpireMushroom, TechType.RedBush, TechType.GenericRibbon, TechType.MohawkPlant, TechType.GenericSpiral, TechType.SpottedLeavesPlant, TechType.TornadoPlates, TechType.ThermalSpireBarnacle, TechType.TwistyBridgesLargePlant, TechType.PurpleStalk, TechType.LargeVentGarden, TechType.SmallVentGarden };

        static HashSet<TechType> LilyPadTechtypes = new HashSet<TechType> { TechType.LilyPadFallen, TechType.LilyPadMature, TechType.LilyPadResource, TechType.LilyPadRoot, TechType.LilyPadStage1, TechType.LilyPadStage2, TechType.LilyPadStage3 };
        static HashSet<TechType> coralTechtypes = new HashSet<TechType> { TechType.TwistyBridgesCoralShelf, TechType.JeweledDiskPiece, TechType.GenericJeweledDisk };

        static HashSet<string> plantsWithoutTechtype = new HashSet<string> { "treespires_cliff_lanterns_01_" };
        static HashSet<string> lilypadWithoutTechtype = new HashSet<string> { "lilypad_roots_", "lilypad_base_", "lilypad_middle_", "lilypad_stage_", "lilypad_fallen_" };
        static HashSet<string> metalWithoutTechtype = new HashSet<string> { "Precursor_ArcticSpires_Cable_LightBlock_BrokenCable_InGround" };


        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void StartPostfix(LargeWorldEntity __instance)
        { // not run for items in container
            TechType tt = CraftData.GetTechType(__instance.gameObject);
            //Main.logger.LogDebug("LargeWorldEntity start " + tt);
            if (plantSurfaces.Contains(tt))
            {
                Util.AddVFXsurfaceComponent(__instance.gameObject, VFXSurfaceTypes.vegetation);
            }
            if (LilyPadTechtypes.Contains(tt) || tt == TechType.TwistyBridgesMushroom)
            {
                Util.AddVFXsurfaceComponent(__instance.gameObject, VFXSurfaceTypes.lilypad);
            }
            if (coralTechtypes.Contains(tt))
            {
                Util.AddVFXsurfaceComponent(__instance.gameObject, VFXSurfaceTypes.coral);
            }
            if (tt == TechType.GenericJeweledDisk)
            {
                if (ConfigToEdit.fixCoral.Value)
                {
                    Vector3 rot = __instance.gameObject.transform.eulerAngles;
                    //Main.Log("fix GenericJeweledDisk " + __instance.gameObject.transform.position.x + " " + __instance.gameObject.transform.position.y + " " + __instance.gameObject.transform.position.z + " rot " + rot.x + " " + rot.y + " " + rot.z);
                    __instance.gameObject.transform.eulerAngles = new Vector3(rot.x, rot.y, 0f);
                    Animator a = __instance.GetComponentInChildren<Animator>();
                    if (a)
                        a.enabled = false;
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
            { // bad Creepvine -680 -630
              //PickPrefab[] pickPrefabs = __instance.GetAllComponentsInChildren<PickPrefab>();
              //if (Main.config.fruitGrowTime > 0)
                Plants_Patch.AttachFruitPlant(__instance.gameObject);
            }
            else if (tt == TechType.TwistyBridgeCoralLong)
            {
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
                CapsuleCollider cc = __instance.GetComponent<CapsuleCollider>();
                __instance.gameObject.layer = LayerID.Useable;
                cc.isTrigger = true;
                cc.height *= cc.height;
            }
            else if (tt == TechType.CyanFlower)
            {// disable collision but allow scanning
                Transform tr = __instance.transform.Find("collision");
                if (tr)
                {
                    tr.gameObject.layer = LayerID.Useable;
                    CapsuleCollider cc = tr.GetComponent<CapsuleCollider>();
                    cc.isTrigger = true;
                }
            }
            else if (tt == TechType.HoneyCombPlant && ConfigToEdit.trypophobiaMode.Value)
            {
                UnityEngine.Object.Destroy(__instance.gameObject);
            }
            else if (tt == TechType.None)
            {
                foreach (string s in lilypadWithoutTechtype)
                {
                    if (__instance.name.StartsWith(s))
                    {
                        Util.AddVFXsurfaceComponent(__instance.gameObject, VFXSurfaceTypes.lilypad);
                    }
                }
                foreach (string s in plantsWithoutTechtype)
                {
                    if (__instance.name.StartsWith(s))
                        Util.AddVFXsurfaceComponent(__instance.gameObject, VFXSurfaceTypes.vegetation);
                }
                if (__instance.name == "Alterra_Walkway_Stairs_01(Clone)")
                { // you get stuck at top of this
                    Vector3 pos = __instance.transform.position;
                    int x = (int)pos.x;
                    int y = (int)pos.y;
                    int z = (int)pos.z;
                    if (x == -164 && y == 6 && z == -652)
                    {
                        __instance.transform.position = new Vector3(pos.x, pos.y + .15f, pos.z);
                    }
                }
                //if (__instance.name == "treespires_fissure_edge_01_b_straight(Clone)")
                //{
                //    int x = (int)__instance.transform.position.x;
                //    int y = (int)__instance.transform.position.y;
                //    int z = (int)__instance.transform.position.z;
                //    //Main.Log("treespires_fissure_edge_01_b_straight " + x + " " + y + " " + z);
                //    if (x == -88 && y == -448 && z == -1416)
                //    {
                //        __instance.transform.position = new Vector3(-88.5f, -448.4f, __instance.transform.position.z);
                //        Vector3 rot = __instance.transform.eulerAngles;
                //        __instance.transform.eulerAngles = new Vector3(rot.x, rot.y, 359f);
                //    }
                //}
                //else if (__instance.name == "treespires_eroded_rock_01_b(Clone)")
                //{
                //    int x = (int)__instance.transform.position.x;
                //    int y = (int)__instance.transform.position.y;
                //    int z = (int)__instance.transform.position.z;
                //    if (x == -94 && y == -450 && z == -1418)
                //    {// upscale to hide another hole near top
                //        Vector3 scale = __instance.transform.localScale;
                //        __instance.transform.localScale = new Vector3(scale.x, 0.95f, scale.z);
                //        Vector3 rot = __instance.transform.eulerAngles;
                //        __instance.transform.eulerAngles = new Vector3(rot.x, rot.y, 348f);
                //    }
                //}
                //else if (__instance.name == "treespires_eroded_rock_01_d(Clone)")
                //{
                //    int x = (int)__instance.transform.position.x;
                //    int y = (int)__instance.transform.position.y;
                //    int z = (int)__instance.transform.position.z;
                //    if (x == -208 && y == -410 && z == -1500)
                //        __instance.gameObject.SetActive(false);
                //    else if (x == -193 && y == -399 && z == -1469)
                //    {
                //        __instance.transform.position = new Vector3(__instance.transform.position.x, -401f, __instance.transform.position.z);
                //    }
                //    //else if (x == -112 && y == -444 && z == -1430)// this can't be moved down
                //    //{ 
                //    //}
                //}
                //else if (__instance.name == "treespires_eroded_rock_01_e(Clone)")
                //{
                //    int x = (int)__instance.transform.position.x;
                //    int y = (int)__instance.transform.position.y;
                //    int z = (int)__instance.transform.position.z;
                //    if (x == -222 && y == -396 && z == -1394)
                //    { // "treespires_eroded_rock_01_e(Clone)"   -222 -396 -1394
                //        __instance.transform.position = new Vector3(-224f, __instance.transform.position.y, __instance.transform.position.z);
                //        Vector3 rot = __instance.transform.eulerAngles;
                //        __instance.transform.eulerAngles = new Vector3(rot.x, 177f, 338f);
                //    }
                //}
                FixTransform(__instance);
                if (__instance.name == "_DELETED_ArcticKelp_Kelp_2(Clone)")
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
            if (ConfigMenu.useBestLOD.Value)
            {
                ForceBestLOD(__instance);
            }
        }

        private static void FixTransform(LargeWorldEntity __instance)
        {
            int x = (int)__instance.transform.position.x;
            int y = (int)__instance.transform.position.y;
            int z = (int)__instance.transform.position.z;
            InstanceToFix instanceToFix = new InstanceToFix(__instance.name, x, y, z);
            if (!instancesToFix.ContainsKey(instanceToFix))
                return;

            TransformData data = instancesToFix[instanceToFix];
            if (data.disabled)
            {
                __instance.gameObject.SetActive(false);
                return;
            }
            Vector3 newPos = data.position;
            if (newPos != default)
            {
                float xNew = newPos.x == float.NaN ? __instance.transform.position.x : newPos.x;
                float yNew = newPos.y == float.NaN ? __instance.transform.position.y : newPos.y;
                float zNew = newPos.z == float.NaN ? __instance.transform.position.z : newPos.z;
                __instance.transform.position = new Vector3(xNew, yNew, zNew);
            }
            Vector3 newRot = data.rotation;
            if (newRot != default)
            {
                Vector3 rot = __instance.transform.eulerAngles;
                float xNew = newRot.x == float.NaN ? rot.x : newRot.x;
                float yNew = newRot.y == float.NaN ? rot.y : newRot.y;
                float zNew = newRot.z == float.NaN ? rot.z : newRot.z;
                __instance.transform.eulerAngles = new Vector3(xNew, yNew, zNew);
            }
            Vector3 newScale = data.scale;
            if (newScale != default)
            {
                Vector3 scale = __instance.transform.localScale;
                float xNew = newScale.x == float.NaN ? scale.x : newScale.x;
                float yNew = newScale.y == float.NaN ? scale.y : newScale.y;
                float zNew = newScale.z == float.NaN ? scale.z : newScale.z;
                __instance.transform.localScale = new Vector3(xNew, yNew, zNew);
            }
        }

        static void ForceBestLOD(LargeWorldEntity lwe)
        {
            //TechType tt = CraftData.GetTechType(__instance.gameObject);
            LODGroup[] lodGroups = lwe.GetComponentsInChildren<LODGroup>();
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
                LargeWorldEntity lwe_ = lodG.GetComponent<LargeWorldEntity>();
                if (lwe_ && lwe != lwe_)
                    continue;

                toCheck.Add(lodG);
            }
            //Main.Log(__instance.name + " LODGroup Count " + toCheck.Count);
            if (toCheck.Count != 1)
            { // dont touch those. Abandoned bases will miss stuff
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

        [HarmonyPrefix]
        [HarmonyPatch("StartFading")]
        public static bool StartFadingPrefix(LargeWorldEntity __instance)
        {
            if (!Main.gameLoaded)
                return false;

            //AddDebug(__instance.name + " StartFading ");
            //AddDebug(" Tools_Patch.equippedTool " + Tools_Patch.equippedTool.name);
            if (spawning)
            {
                //AddDebug(" spawning " + __instance.name);
                spawning = false;
                return false;
            }
            if (__instance.gameObject == Drop_items_anywhere.droppedObject)
            {
                return false;
            }
            else if (Tools_Patch.equippedTool != null && Tools_Patch.equippedTool.gameObject == __instance.gameObject)
            {
                //AddDebug(__instance.name + " StartFading equippedTool");
                Tools_Patch.equippedTool = null;
                return false;
            }
            else if (PropulsionCannon_Patch.releasingGrabbedObject)
            {
                PropulsionCannon_Patch.releasingGrabbedObject = false;
                //AddDebug("StartFading releasingGrabbedObject " + __instance.name);
                return false;
            }
            //else if (Tools_Patch.repCannonGOs.Contains(__instance.gameObject))
            //{
            //AddDebug("StartFading rep Cannon go " + __instance.name);
            //    Tools_Patch.repCannonGOs.Remove(__instance.gameObject);
            //    return false;
            //}
            TechType tt = CraftData.GetTechType(__instance.gameObject);
            if (tt == TechType.JeweledDiskPiece || tt == TechType.Gold || tt == TechType.Silver || tt == TechType.Titanium || tt == TechType.Lead || tt == TechType.Lithium || tt == TechType.Diamond)
            {
                //AddDebug("resource");
                return false;
            }
            return true;
        }

        static TransformData treespires_fissure_edge_01_b_straight = new TransformData(new Vector3(-88.5f, -448.4f, float.NaN), new Vector3(float.NaN, float.NaN, 359f));
        static TransformData treespires_eroded_rock_01_b = new TransformData(new Vector3(-88.5f, -448.4f, float.NaN), new Vector3(float.NaN, float.NaN, 348f), new Vector3(float.NaN, 0.95f, float.NaN));
        static TransformData treespires_eroded_rock_01_e = new TransformData(new Vector3(-224f, float.NaN, float.NaN), new Vector3(float.NaN, 177f, 338f));

        static Dictionary<InstanceToFix, TransformData> instancesToFix = new Dictionary<InstanceToFix, TransformData>
        {
            { new InstanceToFix("treespires_fissure_edge_01_b_straight(Clone)", -88, -448,-1416), treespires_fissure_edge_01_b_straight},
            { new InstanceToFix("treespires_eroded_rock_01_b(Clone)", -94, -450, -1418), treespires_eroded_rock_01_b},
            { new InstanceToFix("treespires_eroded_rock_01_d(Clone)", -208, -410, -1500), new TransformData(default, default, default, true)},
            { new InstanceToFix("treespires_eroded_rock_01_d(Clone)", -193, -399, -1469), new TransformData(new Vector3(float.NaN, -401f, float.NaN))},
            { new InstanceToFix("treespires_eroded_rock_01_e(Clone)", -222, -396, -1394), treespires_eroded_rock_01_e}
        };
        internal static bool spawning;
    }

    public struct InstanceToFix : IEquatable<InstanceToFix>
    {
        string name;
        int x;
        int y;
        int z;

        public InstanceToFix(string name, int x, int y, int z)
        {
            this.name = name;
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public override bool Equals(object obj)
        {
            return obj is InstanceToFix fix && Equals(fix);
        }

        public bool Equals(InstanceToFix other)
        {
            return name == other.name &&
                   x == other.x &&
                   y == other.y &&
                   z == other.z;
        }

        public override int GetHashCode()
        {
            int hashCode = 1755449710;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(name);
            hashCode = hashCode * -1521134295 + x.GetHashCode();
            hashCode = hashCode * -1521134295 + y.GetHashCode();
            hashCode = hashCode * -1521134295 + z.GetHashCode();
            return hashCode;
        }
    }

    public struct TransformData
    {
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        public bool disabled;

        public TransformData(Vector3 position, Vector3 rotation = default, Vector3 scale = default, bool disabled = false)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
            this.disabled = disabled;
        }
    }
}
