using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{// -217 -394 -1420
    // 71 -97 -866
    [HarmonyPatch(typeof(LargeWorldEntity))]
    class LargeWorldEntity_
    {
        static HashSet<TechType> plantSurfaces = new HashSet<TechType>
{ TechType.PurpleVegetablePlant, TechType.Creepvine, TechType.HeatFruitPlant, TechType.IceFruitPlant, TechType.FrozenRiverPlant2, TechType.JellyPlant, TechType.LeafyFruitPlant, TechType.KelpRoot, TechType.KelpRootPustule, TechType.HangingFruitTree, TechType.MelonPlant, TechType.SnowStalkerPlant, TechType.CrashHome, TechType.DeepLilyShroom, TechType.DeepLilyPadsLanternPlant, TechType.BlueFurPlant, TechType.GlacialTree, TechType.GlowFlower, TechType.OrangePetalsPlant, TechType.HoneyCombPlant, TechType.CavePlant, TechType.GlacialPouchBulb, TechType.PurpleRattle, TechType.ThermalLily, TechType.GlacialBulb, TechType.PinkFlower, TechType.SmallMaroonPlant, TechType.DeepTwistyBridgesLargePlant, TechType.GenericShellDouble, TechType.TwistyBridgeCliffPlant, TechType.GenericCrystal, TechType.CaveFlower, TechType.TrianglePlant, TechType.PurpleBranches, TechType.GenericBigPlant2, TechType.GenericBigPlant1, TechType.GenericShellSingle, TechType.OxygenPlant, TechType.TwistyBridgeCoralLong, TechType.GenericCage, TechType.TapePlant, TechType.GenericBowl, TechType.TallShootsPlant, TechType.TreeSpireMushroom, TechType.RedBush, TechType.GenericRibbon, TechType.MohawkPlant, TechType.GenericSpiral, TechType.SpottedLeavesPlant, TechType.TornadoPlates, TechType.ThermalSpireBarnacle, TechType.TwistyBridgesLargePlant, TechType.PurpleStalk, TechType.LargeVentGarden, TechType.SmallVentGarden };

        static HashSet<TechType> LilyPadTechtypes = new HashSet<TechType> { TechType.LilyPadFallen, TechType.LilyPadMature, TechType.LilyPadResource, TechType.LilyPadRoot, TechType.LilyPadStage1, TechType.LilyPadStage2, TechType.LilyPadStage3 };
        static HashSet<TechType> coralTechtypes = new HashSet<TechType> { TechType.TwistyBridgesCoralShelf, TechType.JeweledDiskPiece, TechType.GenericJeweledDisk };

        static HashSet<string> metalWithoutTechtype = new HashSet<string> { "Precursor_ArcticSpires_Cable_LightBlock_BrokenCable_InGround" };

        static Dictionary<string, List<string>> glassRenderers = new Dictionary<string, List<string>> { {
            "Base_ResearchBase_03(Clone)", new List<string>{"Meshes/Exterior/BaseExteriorGlass", "Meshes/Greenhouse/GlassDome/BaseRoomGenericExteriorCapTopGlassExterior", "Meshes/Greenhouse/GlassDome/BaseRoomGenericExteriorCapTopGlassInterior", "Meshes/Interior/Culling/Kitchen/submarine_Storage_locker_big_01_hinges_L (1)/submarine_Storage_locker_big_01_door_L", "Meshes/Interior/Culling/Kitchen/submarine_Storage_locker_big_01_hinges_L (2)/submarine_Storage_locker_big_01_door_L", "discovery_lab_props_02/discovery_lab_props_02_glass", "Meshes/Interior/Culling/Lab/biodome_lab_containers_tube_01 (2)/biodome_lab_containers_tube_01/biodome_lab_containers_tube_01_glass", "Meshes/Interior/Culling/Lab/biodome_lab_containers_tube_01 (4)/biodome_lab_containers_tube_01/biodome_lab_containers_tube_01_glass", "Meshes/Interior/Culling/Lab/biodome_lab_containers_tube_01 (3)/biodome_lab_containers_tube_01/biodome_lab_containers_tube_01_glass", "discovery_lab_props_03/discovery_lab_props_03_glass", "Meshes/Interior/Culling/Lab/biodome_lab_containers_open_02/biodome_lab_containers_open_02/biodome_lab_containers_open_02_glass", "Meshes/Interior/Culling/Lab/biodome_lab_containers_close_01 (1)/biodome_lab_containers_close_01/biodome_lab_containers_close_01_glass", "Meshes/Interior/Culling/Lab/discovery_lab_props_03/discovery_lab_props_03_glass", "Meshes/Interior/Culling/Lab/discovery_lab_props_02/discovery_lab_props_02_glass", "Meshes/Interior/Culling/Lab/biodome_lab_containers_open_01/biodome_lab_containers_open_01/biodome_lab_containers_open_01_glass" }},

            {"HoverZone3_FrozenRiver(Clone)", null}, // -1618 24 -743
            {"PrecursorFabricatorBase_PartFabricatorBones(Clone)", null},
            {"PrecursorFabricatorBase_PartFabricatorSkin(Clone)", null},
            {"PrecursorFabricatorBase_PartFabricatorOrgans(Clone)", null},

            {"Alterra_TechSite_TwistyBridge_02(Clone)", new List<string>{"BaseCell/BaseCell/BaseCorridorIShapeWindowSide/BaseHatchModel/BaseCorridorInteriorWindowSide/BaseCorridorInteriorWindowSide_ext", "BaseCell/BaseCell/BaseCorridorIShapeWindowSide/BaseHatchModel/BaseCorridorInteriorWindowSide_LOD1/BaseCorridorInteriorWindowSide_ext_LOD1", "BaseCell/BaseCell/BaseCorridorIShapeWindowSide (1)/BaseHatchModel/BaseCorridorInteriorWindowSide/BaseCorridorInteriorWindowSide_ext", "BaseCell/BaseCell/BaseCorridorHatch/models/BaseCorridorExteriorCapHatch/hatch_end_anims/hatchGlass_geo", "BaseCell/BaseCell/BaseCorridorWindow/models/BaseCorridorExteriorCap_01/BaseCorridorExteriorCap_01_ext", "BaseCell/BaseCell/BaseCorridorWindow/models/BaseCorridorExteriorCap_01_LOD1/BaseCorridorExteriorCap_01_ext_LOD1", "BaseCell/BaseCell/BaseCorridorWindow/models/BaseCorridorExteriorCap_01/BaseCorridorExteriorCap_01_int"}},

            {"frozenriver_01_section_b(Clone)", new List<string>{ "frozenriver_01_section_b_LOD0"}}, // -8 31 423
            {"frozenriver_01_section_c(Clone)", new List<string>{ "frozenriver_01_section_c_LOD0", "Quad (1)"}}, // -4 18 397
            {"frozenriver_01_section_d(Clone)", new List<string>{ "frozenriver_01_section_d_LOD0"}}, // 27 15 356
            {"frozenriver_01_section_f(Clone)", null}, // -50 5 311
            {"frozenriver_01_section_e(Clone)", new List<string>{"frozenriver_01_section_e_LOD0", "Quad"}}, // -24 12 330

            {"frozenriver_02_glacialbasin_section_a(Clone)", null}, // -1345 4 -1104
            {"frozenriver_02_glacialbasin_section_h(Clone)", null}, // -1184 4 -1006
            {"frozenriver_02_glacialbasin_section_e(Clone)", null}, // -1197 5 -1107
            {"frozenriver_03_shieldbase_section_a 1(Clone)", new List<string>{"frozenriver_03_shieldbase_section_a/waterfall_03_LOD0" }},// -1056 8 -645
            {"frozenriver_02_glacialbasin_section_j(Clone)", null},
            {"frozenriver_02_glacialbasin_section_i(Clone)", null}, // -1079 8 -816
            {"frozenriver_02_glacialbasin_section_m(Clone)", null}, // -1522 4 -996

            {"GlacialBasin_Fissure_Ice(Clone)", null}, // -1290 25 -575
            { "PrecursorFabricatorBase(Clone)", new List<string> { "Meshes/Precursor_fabricator_room_anim/Precursor_FabricationChamber_geo/xPrecursor_Fabricator_Tank_Liquid/Precursor_Fabricator_Tank_Liquid_L", "Meshes/Precursor_fabricator_room_anim/Precursor_FabricationChamber_geo/xPrecursor_Fabricator_Tank_Liquid/Precursor_Fabricator_Tank_Liquid_R", "Meshes/Precursor_fabricator_room_anim/Precursor_FabricationChamber_geo" }},

            { "Marguerit_Base(Clone)", new List<string> { "Marguerit_base/BaseLargeRoomWindowSide/LargeRoom_lExteriorWindowSide01Glass_01", "Marguerit_base/BaseLargeRoomWindowSide/LargeRoom_InteriorWindowSide01Glass_01", "Marguerit_base/marg_base_large_room_interior_glass", "Marguerit_base/marg_base_large_room_interior_glass" }},

            { "Base_RocketBase(Clone)", new List<string> { "Base_RocketBase_snow_ground_set/Base_RocketBase_snow_ground_LOD0", "Base_RocketBase_snow_ground_set/Base_RocketBase_snow_ground_LOD1" }},

            { "Alterra_GlacialBasin_HoverBikeBase(Clone)", new List<string> { "Exterior/hoverbikebase_base_01_exterior_LOD0", "Interior/Closet/BasePartitionDoor (1)/BasePartitionDoor" }},

            { "Base_RocketBase_Emmanuel(Clone)", new List<string> { "Snow_ground/Base_RocketBase_emmanuels_set/Base_RocketBase_emmanuels_set_LOD0", "Snow_ground/Base_RocketBase_emmanuels_set/Base_RocketBase_emmanuels_set_LOD1", "ResearchBase_GenericRoom/Interior/Panels/BaseExteriorRoomGenericWindowSide01Glass", "ResearchBase_GenericRoom/Interior/Panels/BaseExteriorRoomGenericWindowSide01Glass_LOD1" }},

            { "Alterra_Base_2(Clone)", new List<string> { "Exterior/BaseCell_RoomRight_Ext/BaseGlassDome(Clone)/BaseGhost/BaseRoomGenericExteriorTopGlass/BaseRoomGenericExteriorCapTopGlassExterior", "Exterior/Hatch/hatch_end_anims/hatchGlass_geo", "Exterior/BaseCell_RoomLeft_Ext/BaseRoomWindowSide(Clone)/BaseRoomGenericInteriorWindowSide01/BaseExteriorRoomGenericWindowSide01Glass", "Interior/BaseCell_RoomLeft_Int/BaseRoomWindowSide(Clone)/BaseRoomGenericInteriorWindowSide01/BaseInteriorRoomGenericWindowSide01Glass", "Exterior/BaseCell_RoomLeft_Ext/BaseRoomWindowSide(Clone) (1)/BaseRoomGenericInteriorWindowSide01/BaseExteriorRoomGenericWindowSide01Glass", "Interior/BaseCell_RoomLeft_Int/BaseRoomWindowSide(Clone) (1)/BaseRoomGenericInteriorWindowSide01/BaseInteriorRoomGenericWindowSide01Glass", "Interior/BaseCell_RoomRight_Int/BaseRoomWindowSide(Clone)/BaseRoomGenericInteriorWindowSide01/BaseInteriorRoomGenericWindowSide01Glass", "Interior/BaseCell_RoomRight_Int/BaseRoomWindowSide(Clone) (1)/BaseRoomGenericInteriorWindowSide01/BaseInteriorRoomGenericWindowSide01Glass", "Exterior/BaseCell_RoomRight_Ext/BaseRoomWindowSide(Clone)/BaseRoomGenericInteriorWindowSide01/BaseExteriorRoomGenericWindowSide01Glass", "Exterior/BaseCell_RoomRight_Ext/BaseRoomWindowSide(Clone) (1)/BaseRoomGenericInteriorWindowSide01/BaseExteriorRoomGenericWindowSide01Glass", "Interior/BaseCell_RoomRight_Int/BaseGlassDome(Clone)/BaseGhost/BaseRoomGenericExteriorTopGlass/BaseRoomGenericExteriorCapTopGlassInterior", "Interior/BaseCell_RoomRight_Int/FiltrationMachine_Deco(Clone)/model/Water_Filtration_Machine/water_filtration_machine_geo/water_filtration_machine_glass", "Interior/Props_Int/Props_RoomRight_Int/biodome_lab_tube_01 (1)/biodome_lab_tube_01_glass", "Interior/Props_Int/Props_RoomCentral_int/discovery_lab_props_02/discovery_lab_props_02_glass", "Interior/Props_Int/Props_RoomCentral_int/biodome_lab_containers_tube_01 (1)/biodome_lab_containers_tube_01_glass", "Interior/Props_Int/Props_RoomRight_Int/biodome_lab_containers_tube_01 (6)/biodome_lab_containers_tube_01_glass", "Interior/Props_Int/Props_RoomRight_Int/biodome_lab_containers_tube_01 (8)/biodome_lab_containers_tube_01_glass", "Interior/Props_Int/Props_RoomRight_Int/biodome_lab_containers_tube_01 (2)/biodome_lab_containers_tube_01_glass", "Interior/Props_Int/Props_RoomRight_Int/biodome_lab_containers_tube_01 (4)/biodome_lab_containers_tube_01_glass", "Interior/Props_Int/Props_RoomRight_Int/biodome_lab_containers_tube_01 (5)/biodome_lab_containers_tube_01_glass", "Interior/Props_Int/Props_RoomRight_Int/biodome_lab_containers_tube_01 (7)/biodome_lab_containers_tube_01_glass", "Interior/Props_Int/Props_RoomRight_Int/discovery_lab_props_02 (1)/discovery_lab_props_02_glass", "Interior/Props_Int/Props_RoomCentral_int/biodome_lab_containers_tube_01/biodome_lab_containers_tube_01_glass", "Interior/Props_Int/Props_RoomCentral_int/biodome_lab_containers_tube_01 (1)" }},


        };

        static Dictionary<string, int> shipwrecks = new Dictionary<string, int> { { "ShipWreck1(Clone)", 9 }, { "ShipWreck2(Clone)", 6 } };

        static Dictionary<TechType, List<string>> glassRenderers_ = new Dictionary<TechType, List<string>> {
            {TechType.ExosuitFragment, new List<string>{"Exosuit_01_cabin006/Exosuit_cabin_01_glass006", "Exosuit_cabin_01_glass", "exosuit_damaged_01/Exosuit_01_cabin005/Exosuit_cabin_01_glass005", "exosuit_damaged_05/Exosuit_01_cabin007/Exosuit_cabin_01_glass007", "exosuit_damaged_02/Exosuit_01_cabin004/Exosuit_cabin_01_glass004"}},
            {TechType.Locker, new List<string>{ "model/submarine_Storage_locker_big_01/submarine_Storage_locker_big_01_hinges_L/submarine_Storage_locker_big_01_door_L", "model/submarine_Storage_locker_big_01/submarine_Storage_locker_big_01_hinges_R/submarine_Storage_locker_big_01_door_R"}},
            {TechType.BarTable, new List<string>{"descent_bar_table_01/descent_bar_table_01_glass"}},
            {TechType.BaseFiltrationMachine, new List<string>{"model/Water_Filtration_Machine/water_filtration_machine_geo/water_filtration_machine_glass"}},
            {TechType.StarshipCargoCrate, new List<string>{"Starship_cargo_damaged_opened_02/dirt_02", "Starship_cargo_damaged_opened_01/dirt_01" }},
            {TechType.Aquarium, new List<string>{"model/Aquarium_animation2/Aquarium_geo/Aquarium_glass" }},
            {TechType.LabContainer, new List<string>{"biodome_lab_containers_close_01/biodome_lab_containers_close_01_glass" }},
            {TechType.LabContainer3, new List<string>{"biodome_lab_containers_tube_01/biodome_lab_containers_tube_01_glass" }},
            {TechType.Jellyfish, null },
            {TechType.LargeVentGarden, new List<string>{"Vent_garden_anim/vent_garden_bubble" }},
            //{TechType.SmallVentGarden, new List<string>{"Vent_garden_swimming_anim/vent_garden_geo/newest_standing_geo/vent_garden_bulb_swimming" }},
            {TechType.Exosuit, new List<string>{"exosuit_01/root/Exosuit_cabin_01_glass" }},
            {TechType.SeaTruck, new List<string>{"model/seatruck_anim/Seatruck_cabin_exterior_glass_geo", "model/seatruck_anim/Seatruck_cabin_interior_glass_geo" }},
            {TechType.SeaTruckAquariumModule, new List<string>{ "seatruck_module_aquarium_anim/Seatruck_module_Aquarium_glass_exterior_geo", "seatruck_module_aquarium_anim/Seatruck_module_Aquarium_interior_glass_geo", "rearConnection/closed/Seatruck_door_int_glass", "rearConnection/closed/Seatruck_door_ext_glass" }},
            {TechType.SeaTruckDockingModule, new List<string>{ "seatruck_module_prawn_anim/Seatruck_module_PRAWN_glass_exterior_geo", "seatruck_module_prawn_anim/Seatruck_module_PRAWN_glass_interior_geo"}},
            {TechType.SeaTruckFabricatorModule, new List<string>{ "frontConnection/closed/Seatruck_door_ext_glass", "frontConnection/closed/Seatruck_door_int_glass", "rearConnection/closed/Seatruck_door_int_glass", "rearConnection/closed/Seatruck_door_ext_glass"}},
            {TechType.SeaTruckStorageModule, new List<string>{ "frontConnection/closed/Seatruck_door_ext_glass", "frontConnection/closed/Seatruck_door_int_glass", "rearConnection/closed/Seatruck_door_int_glass", "rearConnection/closed/Seatruck_door_ext_glass"}},
            {TechType.SeaTruckTeleportationModule, new List<string>{ "frontConnection/closed/Seatruck_door_ext_glass", "frontConnection/closed/Seatruck_door_int_glass", "rearConnection/closed/Seatruck_door_int_glass", "rearConnection/closed/Seatruck_door_ext_glass"}},
            {TechType.SeaTruckSleeperModule, new List<string>{ "frontConnection/closed/Seatruck_door_ext_glass", "frontConnection/closed/Seatruck_door_int_glass", "rearConnection/closed/Seatruck_door_int_glass", "rearConnection/closed/Seatruck_door_ext_glass", "model/seatruck_module_sleeper_anim/Seatruck_Sleeper_Module_interior"}},
            {TechType.BaseWaterPark, new List<string>{ "Large_Aquarium_generic_room/Large_Aquarium_generic_room_glass_01", "high/glass"}},


        };

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

            };


        [HarmonyPostfix, HarmonyPatch("Start")]
        public static void StartPostfix(LargeWorldEntity __instance)
        { // not run for items in container
            TechType techType = CraftData.GetTechType(__instance.gameObject);
            //Main.logger.LogDebug("LargeWorldEntity start " + tt);
            Vector3 posV3 = __instance.transform.position;
            Vector3Int posV3int = new Vector3Int((int)posV3.x, (int)posV3.y, (int)posV3.z);
            //techType = TechType.lily
            if (newPosRots.ContainsKey(posV3int))
            {
                foreach (var newPosRot in newPosRots[posV3int])
                    UWE.CoroutineHost.StartCoroutine(SetNewPosRot(__instance.gameObject, newPosRot));
            }
            else if (instancesToDisable.ContainsKey(posV3int) && instancesToDisable[posV3int].name == __instance.name)
            {
                __instance.gameObject.SetActive(false);
            }
            else if (posV3int.x == 66 && posV3int.y == -95 && posV3int.z == -869 && __instance.name == "thermalzone_coral_shelf_plates_01_group_c(Clone)" && __instance.transform.parent.parent.name == "EngineRoom")
            {
                Transform rooms = __instance.transform.parent.parent.parent;
                Transform servers = rooms.Find("servers");
                Transform entities = servers.Find("Entities");
                __instance.transform.SetParent(entities);
            }
            if (glassRenderers_.ContainsKey(techType))
            {
                __instance.transform.DisableShadowCasting(glassRenderers_[techType]);
            }
            if (plantSurfaces.Contains(techType))
            {
                Util.AddVFXsurfaceComponent(__instance.gameObject, VFXSurfaceTypes.vegetation);
            }
            if (LilyPadTechtypes.Contains(techType) || techType == TechType.TwistyBridgesMushroom)
            {
                Util.AddVFXsurfaceComponent(__instance.gameObject, VFXSurfaceTypes.lilypad);
            }
            if (coralTechtypes.Contains(techType))
            {
                Util.AddVFXsurfaceComponent(__instance.gameObject, VFXSurfaceTypes.coral);
            }

            if (techType == TechType.GenericJeweledDisk)
            {
                if (ConfigToEdit.fixCoral.Value)
                    FixCoral(__instance.gameObject);
            }
            else if (techType == TechType.SmallMelon)
            {
                FixGreenhouseMelons(__instance.gameObject);
            }
            else if (techType == TechType.TrianglePlant)
            {
                Collider collider = __instance.GetComponentInChildren<Collider>();
                collider.isTrigger = true;
            }
            else if (techType == TechType.IceFruitPlant || techType == TechType.Creepvine || techType == TechType.SnowStalkerPlant)
            { // bad Creepvine -680 -630
              //PickPrefab[] pickPrefabs = __instance.GetAllComponentsInChildren<PickPrefab>();
              //if (Main.config.fruitGrowTime > 0)
                Plants_Patch.AttachFruitPlant(__instance.gameObject);
            }
            else if (techType == TechType.TwistyBridgeCoralLong)
            {
                DisableCollisionPinkNarrowLeaf(__instance.gameObject);
            }
            else if (techType == TechType.TapePlant)
            {
                DisableCollisionPurpleCattail(__instance.gameObject);
            }
            else if (techType == TechType.CyanFlower)
            {
                DisableCollisionBloomingRaindrops(__instance);
            }
            else if (techType == TechType.HoneyCombPlant && ConfigToEdit.trypophobiaMode.Value)
            {
                UnityEngine.Object.Destroy(__instance.gameObject);
            }
            else if (techType == TechType.LilyPadMature || techType == TechType.LilyPadStage1 || techType == TechType.LilyPadStage2 || techType == TechType.LilyPadStage3)
            {
                __instance.gameObject.AddVFXsurfaceComponent(VFXSurfaceTypes.vegetation);
            }
            else if (techType == TechType.LilyPadRoot)
            {
                __instance.gameObject.AddVFXsurfaceComponent(VFXSurfaceTypes.lilypad);
            }


            else if (techType == TechType.None)
            {
                if (shipwrecks.ContainsKey(__instance.name))
                {
                    FixShipwreckGlass(__instance.transform);
                }
                if (glassRenderers.ContainsKey(__instance.name))
                {
                    __instance.transform.DisableShadowCasting(glassRenderers[__instance.name]);
                }

                //FixTransform(__instance);

                //if (__instance.name.Contains("talactite"))
                //{
                //    VFXSurface surface = __instance.gameObject.EnsureComponent<VFXSurface>();
                //    surface.surfaceType = VFXSurfaceTypes.rock;
                //}

            }
            if (ConfigMenu.useBestLOD.Value)
            {
                __instance.gameObject.ForceLODs();
            }
        }

        private static void FixShipwreckGlass(Transform wreck)
        {
            //Main.logger.LogDebug("FixShipwreckGlass " + wreck.name);
            int roomsIndex = shipwrecks[wreck.name];
            Transform rooms = wreck.GetChild(roomsIndex);
            foreach (Transform room in rooms)
            {
                //Main.logger.LogDebug("FixShipwreckGlass room " + room.name);
                foreach (Transform roomChild in room)
                {
                    if (roomChild.name == "Decos")
                    {
                        foreach (Transform deco in roomChild)
                        {
                            if (deco.name.StartsWith("ShipWreck_Glass"))
                                deco.DisableShadowCastingInChildren();
                        }
                    }
                }
            }
        }

        private static void DisableCollisionBloomingRaindrops(LargeWorldEntity __instance)
        {
            Transform tr = __instance.transform.Find("collision");
            if (tr)
            {
                tr.gameObject.layer = LayerID.Useable;
                CapsuleCollider cc = tr.GetComponent<CapsuleCollider>();
                cc.isTrigger = true;
            }
        }

        private static void DisableCollisionPurpleCattail(GameObject go)
        {
            go.layer = LayerID.Useable;
            CapsuleCollider cc = go.GetComponent<CapsuleCollider>();
            cc.isTrigger = true;
            cc.height *= cc.height;
        }

        private static void DisableCollisionPinkNarrowLeaf(GameObject go)
        {
            // disable collision but allow scanning
            Collider collider = go.GetComponent<Collider>();
            if (collider)
                UnityEngine.Object.Destroy(collider);

            Transform tr = go.transform.Find("GameObject");
            if (tr)
            {
                BoxCollider bc = tr.GetComponent<BoxCollider>();
                bc.isTrigger = true;
            }
        }

        private static void FixGreenhouseMelons(GameObject go)
        {
            int x = (int)go.transform.position.x;
            int y = (int)go.transform.position.y;
            int z = (int)go.transform.position.z;
            if ((x == 989 && y == 30 && z == -897) || (x == 989 && y == 29 && z == -896) || (x == 986 && y == 29 && z == -895))
            { // make melons in Marg greenhouse pickupable
                go.GetComponent<SphereCollider>().radius = .4f;
                //AddDebug("make melons in Marg greenhouse pickupable " + x +" " + y +" " + z);
            }
        }

        private static void FixCoral(GameObject go)
        {
            Vector3 rot = go.transform.eulerAngles;
            go.transform.eulerAngles = new Vector3(rot.x, rot.y, 0f);
            Animator a = go.GetComponentInChildren<Animator>();
            if (a != null)
                a.enabled = false;
        }

        private static IEnumerator SetNewPosRot(GameObject go, PosRotData data)
        {
            if (go.name != data.name)
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

        internal static bool spawning;
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
