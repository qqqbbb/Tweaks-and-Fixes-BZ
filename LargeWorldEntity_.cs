using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static ErrorMessage;

namespace Tweaks_Fixes
{// -217 -394 -1420
    [HarmonyPatch(typeof(LargeWorldEntity))]
    class LargeWorldEntity_
    {
        internal static bool spawningNearPlayer;

        HashSet<TechType> plantSurfaces = new HashSet<TechType>
{ TechType.PurpleVegetablePlant, TechType.Creepvine, TechType.HeatFruitPlant, TechType.IceFruitPlant, TechType.FrozenRiverPlant2, TechType.JellyPlant, TechType.LeafyFruitPlant, TechType.KelpRoot, TechType.KelpRootPustule, TechType.HangingFruitTree, TechType.MelonPlant, TechType.SnowStalkerPlant, TechType.CrashHome, TechType.DeepLilyShroom, TechType.DeepLilyPadsLanternPlant, TechType.BlueFurPlant, TechType.GlacialTree, TechType.GlowFlower, TechType.OrangePetalsPlant, TechType.HoneyCombPlant, TechType.CavePlant, TechType.GlacialPouchBulb, TechType.PurpleRattle, TechType.ThermalLily, TechType.GlacialBulb, TechType.PinkFlower, TechType.SmallMaroonPlant, TechType.DeepTwistyBridgesLargePlant, TechType.GenericShellDouble, TechType.TwistyBridgeCliffPlant, TechType.GenericCrystal, TechType.CaveFlower, TechType.TrianglePlant, TechType.PurpleBranches, TechType.GenericBigPlant2, TechType.GenericBigPlant1, TechType.GenericShellSingle, TechType.OxygenPlant, TechType.TwistyBridgeCoralLong, TechType.GenericCage, TechType.TapePlant, TechType.GenericBowl, TechType.TallShootsPlant, TechType.TreeSpireMushroom, TechType.RedBush, TechType.GenericRibbon, TechType.MohawkPlant, TechType.GenericSpiral, TechType.SpottedLeavesPlant, TechType.TornadoPlates, TechType.ThermalSpireBarnacle, TechType.TwistyBridgesLargePlant, TechType.PurpleStalk, TechType.LargeVentGarden, TechType.SmallVentGarden };

        HashSet<TechType> LilyPadTechtypes = new HashSet<TechType> { TechType.LilyPadFallen, TechType.LilyPadMature, TechType.LilyPadResource, TechType.LilyPadRoot, TechType.LilyPadStage1, TechType.LilyPadStage2, TechType.LilyPadStage3 };

        HashSet<TechType> coralTechtypes = new HashSet<TechType> { TechType.TwistyBridgesCoralShelf, TechType.JeweledDiskPiece, TechType.GenericJeweledDisk };


        static Dictionary<Vector3Int, PosRotData> instancesToDisable = new Dictionary<Vector3Int, PosRotData> {
            {new Vector3Int(-208, -410, -1500), new PosRotData("treespires_eroded_rock_01_d(Clone)", default, default) }
        };

        static Dictionary<Vector3Int, List<PosRotData>> newPosRots = new Dictionary<Vector3Int, List<PosRotData>>  {
            {new Vector3Int(543, -203, -1063), new List<PosRotData>{
            new PosRotData("bed_covers_omega_vinh(Clone)", new Vector3(float.NaN, -204.2f, float.NaN), default) }},
            {new Vector3Int(542, -203, -1066), new List<PosRotData>{
            new PosRotData("bed_covers_omega_danielle(Clone)", new Vector3(float.NaN, -204.2f, float.NaN), default) }},
            {new Vector3Int(-1179, 16, -713), new List<PosRotData>{
            new PosRotData("Starship_work_chair_02(Clone)", new Vector3(-1177.5f, float.NaN, -712.35f), new Vector3(350f, 330f, 90f)),
            new PosRotData("BarTable_Deco(Clone)", new Vector3(-1179.9f, float.NaN, -714.5f), default) } },
            {new Vector3Int(-638, -18, 16), new List<PosRotData>{
            new PosRotData("underwater_ice_brinicle_large_01_animated_b(Clone)", new Vector3(float.NaN, -17.4f, float.NaN), default) }},
            {new Vector3Int(-164, 6, -652), new List<PosRotData>{
            new PosRotData("Alterra_Walkway_Stairs_01(Clone)", new Vector3(float.NaN, 6.6f, float.NaN), default) }},
            {new Vector3Int(-88,-448, -1416), new List<PosRotData>{
            new PosRotData("treespires_fissure_edge_01_b_straight(Clone)", new Vector3(-88.5f, -448.55f, float.NaN), default) }},
            {new Vector3Int(-94, -450, -1418), new List<PosRotData>{
            new PosRotData("treespires_eroded_rock_01_b(Clone)", new Vector3(-93.5f, float.NaN, float.NaN), new Vector3(float.NaN, float.NaN, 348f)) }},
            {new Vector3Int(-193, -399, -1469), new List<PosRotData>{
            new PosRotData("treespires_eroded_rock_01_d(Clone)", new Vector3(float.NaN, -401f, float.NaN), default) }},
            {new Vector3Int(-222, -396, -1394), new List<PosRotData>{
            new PosRotData("treespires_eroded_rock_01_e(Clone)", new Vector3(-224f, float.NaN, float.NaN), new Vector3(float.NaN, 177f, 338f)) }},
            {new Vector3Int(-1032, 5, -724), new List<PosRotData>{
            new PosRotData("ice_pool_01_a(Clone)", new Vector3(float.NaN, 4.93f, float.NaN), default) }},
            {new Vector3Int(-1169, 2, -976), new List<PosRotData>{
            new PosRotData("glacier_cliffs_medium_01_d_straight(Clone)", new Vector3(float.NaN, float.NaN, -975.8f), new Vector3(float.NaN, 60, float.NaN)) }}, // cover z-fighting at -1166 8 -975
            {new Vector3Int(-1168, 5, -976), new List<PosRotData>{
            new PosRotData("rock_medium_01_single_b_ice(Clone)", new Vector3(-1168.2f, 6.2f, -975.8f), new Vector3(float.NaN, 20, 0)) }}, // cover z-fighting at -1166 8 -975
            {new Vector3Int(36, -131, -485), new List<PosRotData>{
            new PosRotData("rock_medium_01_cluster_c(Clone)", new Vector3(float.NaN, -131.8f, float.NaN), default) }},
            {new Vector3Int(-262, -281, -747), new List<PosRotData>{
            new PosRotData(null, new Vector3(float.NaN, -283.8f, float.NaN), default) }},
            //{new Vector3Int(-1264, 34, -563), new List<PosRotData>{
            //new PosRotData("glacier_cliffs_rock_01_a(Clone)", new Vector3(float.NaN, -131.8f, float.NaN), default) }}, // bad collider
           
            };

        static HashSet<string> corals = new HashSet<string>();

        [HarmonyPostfix, HarmonyPatch("Start")]
        public static void StartPostfix(LargeWorldEntity __instance)
        {
            //TechType techType = CraftData.GetTechType(__instance.gameObject);
            //Main.logger.LogDebug("LargeWorldEntity start " + tt);
            Vector3 posV3 = __instance.transform.position;
            Vector3Int posV3int = new Vector3Int((int)posV3.x, (int)posV3.y, (int)posV3.z);
            if (newPosRots.ContainsKey(posV3int))
            {
                foreach (var newPosRot in newPosRots[posV3int])
                    UWE.CoroutineHost.StartCoroutine(SetNewPosRot(__instance.gameObject, newPosRot));
            }
            else if (instancesToDisable.ContainsKey(posV3int) && instancesToDisable[posV3int].name == __instance.name)
            {
                __instance.gameObject.SetActive(false);
            }
            //if (plantSurfaces.Contains(techType))
            //{
            //    Util.AddVFXsurfaceComponent(__instance.gameObject, VFXSurfaceTypes.vegetation);
            //}
            //if (LilyPadTechtypes.Contains(techType) || techType == TechType.TwistyBridgesMushroom)
            //{
            //    Util.AddVFXsurfaceComponent(__instance.gameObject, VFXSurfaceTypes.lilypad);
            //}
            //if (__instance.TryGetComponent(out FruitPlant fruitPlant))
            {
                //PickPrefab pickPrefab = __instance.GetComponentInChildren<PickPrefab>();
                //if (pickPrefab)
                {
                    //PrefabIdentifier identifier = __instance.GetComponent<PrefabIdentifier>();
                    //if (corals.Contains(identifier.classId) == false)
                    //{
                    //    Main.logger.LogDebug($"fruit {__instance.name} {identifier.classId}");
                    //AddDebug($"fruit {__instance.name}");
                    //corals.Add(identifier.classId);
                    //}
                    //Plants_Patch.AttachFruitPlant(__instance.gameObject);
                }
            }
            //if (coralTechtypes.Contains(techType))
            //{
            //    Util.AddVFXsurfaceComponent(__instance.gameObject, VFXSurfaceTypes.coral);
            //}
            //else if (techType == TechType.LilyPadMature || techType == TechType.LilyPadStage1 || techType == TechType.LilyPadStage2 || techType == TechType.LilyPadStage3)
            //{
            //    __instance.gameObject.AddVFXsurfaceComponent(VFXSurfaceTypes.vegetation);
            //}
            //else if (techType == TechType.LilyPadRoot)
            //{
            //    __instance.gameObject.AddVFXsurfaceComponent(VFXSurfaceTypes.lilypad);
            //}
            if (ConfigMenu.useBestLOD.Value)
            {
                __instance.gameObject.ForceLODs();
            }
        }

        private static IEnumerator SetNewPosRot(GameObject go, PosRotData data)
        {
            if (data.name != null && go.name != data.name)
                yield break;

            yield return new WaitForFrames(1);

            Vector3 currentPos = go.transform.position;
            float x = float.IsNaN(data.newPos.x) ? currentPos.x : data.newPos.x;
            float y = float.IsNaN(data.newPos.y) ? currentPos.y : data.newPos.y;
            float z = float.IsNaN(data.newPos.z) ? currentPos.z : data.newPos.z;
            Vector3 newPos = new Vector3(x, y, z);

            Vector3 currentRot = go.transform.eulerAngles;
            float xRot = float.IsNaN(data.newRot.x) ? currentRot.x : data.newRot.x;
            float yRot = float.IsNaN(data.newRot.y) ? currentRot.y : data.newRot.y;
            float zRot = float.IsNaN(data.newRot.z) ? currentRot.z : data.newRot.z;
            Vector3 newRot = new Vector3(xRot, yRot, zRot);

            //AddDebug($"SetNewPosRot {go.name} currentPos {currentPos} currentRot {currentRot} newRot {newRot} newPos {newPos}");
            //Main.logger.LogDebug($"SetNewPosRot {go.name} currentPos {currentPos} currentRot {currentRot} newRot {newRot} newPos {newPos}");

            if (data.newPos != default && data.newRot != default && currentPos != newPos && currentRot != data.newRot)
            {
                //Main.logger.LogDebug($"SetNewPosRot {go.name} SetPositionAndRotation");
                go.transform.SetPositionAndRotation(newPos, Quaternion.Euler(newRot));
            }
            else if (data.newPos != default && currentPos != newPos)
            {
                //Main.logger.LogDebug($"SetNewPosRot {go.name} newPos {newPos}");
                go.transform.position = newPos;
            }
            else if (newRot != default && currentRot != newRot)
            {
                //Main.logger.LogDebug($"SetNewPosRot {go.name} newRot {data.newRot}");
                go.transform.eulerAngles = newRot;
            }
            if (go.TryGetComponent(out CrashHome crashHome) && crashHome.crash)
            {
                crashHome.crash.transform.forward = go.transform.forward;
                crashHome.crash.transform.Rotate(-90, 0, 0);
                Vector3 posDif = currentPos - newPos;
                crashHome.crash.transform.position -= posDif;
                //AddDebug("move crash " + posDif);
            }
        }

        [HarmonyPrefix, HarmonyPatch("StartFading")]
        public static bool StartFadingPrefix(LargeWorldEntity __instance)
        {
            //AddDebug(__instance.name + " StartFading ");
            if (spawningNearPlayer)
            {
                //AddDebug(" spawningNearPlayer " + __instance.name);
                spawningNearPlayer = false;
                return false;
            }
            return true;
        }
    }


    [HarmonyPatch(typeof(BreakableResource), "SpawnResourceFromPrefab", new Type[] { typeof(AssetReferenceGameObject) })]
    class BreakableResource_SpawnResourceFromPrefab_Patch
    {
        public static void Prefix(BreakableResource __instance)
        {
            //AddDebug("BreakableResource SpawnResourceFromPrefab");
            LargeWorldEntity_.spawningNearPlayer = true;
        }
    }

    [HarmonyPatch(typeof(SpawnOnKill), "OnKill")]
    class SpawnOnKill_OnKill_Patch
    {
        public static void Prefix(SpawnOnKill __instance)
        {
            //AddDebug("SpawnOnKill OnKill");
            LargeWorldEntity_.spawningNearPlayer = true;
        }
    }

    class PosRotData
    {
        public string name;
        public Vector3 newPos;
        public Vector3 newRot;

        public PosRotData(string name, Vector3 newPos, Vector3 newRot)
        {
            this.name = name;
            this.newPos = newPos;
            this.newRot = newRot;
        }
    }


}
