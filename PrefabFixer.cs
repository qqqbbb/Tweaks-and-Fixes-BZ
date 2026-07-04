using Oculus.Platform;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UWE;
using static ErrorMessage;
using static Tweaks_Fixes.Storage_Patch;

namespace Tweaks_Fixes
{
    internal class PrefabFixer
    {
        static public Color bloodColor;
        static public bool prefabsFixed;
        static readonly int zOffset = Shader.PropertyToID("_ZOffset");
        static bool loadedPrefabsFixed;
        readonly Dictionary<TechType, MaterialZoffsetData> glassMaterialZoffsets = new Dictionary<TechType, MaterialZoffsetData> {
            { TechType.Hoverpad, new MaterialZoffsetData("AnimatedMesh/Hoverpad_geo", 1, 10000) },
            //{ TechType.HoverpadFragment, new MaterialZoffsetData(null, 1, 10000) },
        { TechType.SeaTruckAquariumModule, new MaterialZoffsetData("seatruck_module_aquarium_anim/Seatruck_module_Aquarium_interior_geo", 2, 10000) },
        { TechType.SmallVentGarden, new MaterialZoffsetData("Vent_garden_swimming_anim/vent_garden_geo/newest_standing_geo/vent_garden_bulb_swimming", 3, 10000) }
        };
        HashSet<string> bloodPrefabs = new HashSet<string> {
            "xKnifeHit_Organic", "GenericCreatureHit", "xExoDrill_Organic"
        };
        List<string> shipWrecks = new List<string> { "f9fb8a6a-026f-4b14-8da1-92ec76c42256",// ShipWreck1 62 -91 -870
            "b9637bf7-d42c-4354-91a2-1c7f01f37a20"// ShipWreck2 265 -233 -1295
        };

        readonly Dictionary<string, MaterialZoffsetData> glassMaterialZoffsets_ = new Dictionary<string, MaterialZoffsetData> {
            { "9fccfbb8-7611-40b5-99bd-513e95993bd3", new MaterialZoffsetData(null, 0, 1000) },// ice_pool_01_a
            { "61af0fdd-f077-42b3-b8f8-c5ccef8ce3c0", new MaterialZoffsetData(null, 0, 1000) },// ice_pool_01_b
            { "0e04bfbd-0e32-4451-bed3-7955a20aea44", new MaterialZoffsetData(null, 0, 1000) },// ice_pool_01_c
            { "2168257e-2533-403f-8b3a-a3bef63adaf9",new MaterialZoffsetData(null, 1, 10000) }, // Hoverpad_Fragmment

        };

        readonly Dictionary<string, MaterialZoffsetData> fragments = new Dictionary<string, MaterialZoffsetData> {
            { "47c32ae8-b168-4ddf-bbae-7467038e3457",new MaterialZoffsetData("Thermal_reactor_damaged_03", 3, 0) },// -262 -281 -747
            { "06cc39eb-af4c-4573-866a-d92e5d4c2bf1",new MaterialZoffsetData("Thermal_reactor_damaged_02", 3, 0) },
            { "88c4c1fa-0b52-44cb-9db5-2ef18447ae5c",new MaterialZoffsetData("Thermal_reactor_damaged_01", 3, 0) },
            { "a50c91eb-f7cf-4fbf-8157-0aa8d444820c",new MaterialZoffsetData(null, 3, 0) },

        };

        static RendererData barTableGlassData = new RendererData("descent_bar_table_01/descent_bar_table_01_glass");
        static RendererData lockerGlassData = new RendererData("model/submarine_Storage_locker_big_01", new List<string> { "submarine_Storage_locker_big_01_hinges_R/submarine_Storage_locker_big_01_door_R", "submarine_Storage_locker_big_01_hinges_L/submarine_Storage_locker_big_01_door_L" });
        static RendererData filtrationMachineGlassData = new RendererData("model/Water_Filtration_Machine/water_filtration_machine_geo/water_filtration_machine_glass");
        static RendererData damagedExosuitData = new RendererData("exosuit_damaged_05/Exosuit_01_cabin007/Exosuit_cabin_01_glass007");
        static RendererData labShelfData = new RendererData("biodome_lab_shelf_01/biodome_lab_shelf_01_thing_glass");
        static RendererData cargoCrateData = new RendererData("Starship_cargo_damaged_opened_02/dirt_02");
        static RendererData seatruckFrontConnectionGlassData = new RendererData("frontConnection/closed", new List<string> { "Seatruck_door_ext_glass", "Seatruck_door_int_glass" });
        static RendererData seatruckRearConnectionGlassData = new RendererData("rearConnection/closed", new List<string> { "Seatruck_door_ext_glass", "Seatruck_door_int_glass" });

        Dictionary<TechType, RendererData> glassRenderers = new Dictionary<TechType, RendererData> {
            { TechType.Locker, lockerGlassData},
            { TechType.Aquarium, new RendererData("model/Aquarium_animation2/Aquarium_geo/Aquarium_glass")},
            { TechType.BarTable, barTableGlassData},
            { TechType.Fridge, new RendererData("geo/marg_props_fridge_door")},
            { TechType.BaseFiltrationMachine, filtrationMachineGlassData},
            {TechType.LargeVentGarden, new RendererData("Vent_garden_anim/vent_garden_bubble" ) },
            {TechType.Exosuit, new RendererData("exosuit_01/root/Exosuit_cabin_01_glass" ) },
            {TechType.SeaTruck, new RendererData("model/seatruck_anim", new List<string>{ "Seatruck_cabin_exterior_glass_geo", "Seatruck_cabin_interior_glass_geo" } ) },
            {TechType.SeaTruckDockingModule, new RendererData("seatruck_module_prawn_anim", new List<string>{ "Seatruck_module_PRAWN_glass_exterior_geo", "Seatruck_module_PRAWN_glass_interior_geo" } ) },

        };

        Dictionary<TechType, List<RendererData>> glassRenderers___ = new Dictionary<TechType, List<RendererData>> {
            {TechType.SeaTruckAquariumModule, new List<RendererData>{new RendererData("seatruck_module_aquarium_anim", new List<string> { "Seatruck_module_Aquarium_glass_exterior_geo", "Seatruck_module_Aquarium_interior_glass_geo" }), seatruckRearConnectionGlassData}},
            {TechType.SeaTruckFabricatorModule, new List<RendererData>{ seatruckFrontConnectionGlassData, seatruckRearConnectionGlassData}},
            {TechType.SeaTruckStorageModule, new List<RendererData>{ seatruckFrontConnectionGlassData, seatruckRearConnectionGlassData }},
            {TechType.SeaTruckTeleportationModule, new List<RendererData>{ seatruckFrontConnectionGlassData, seatruckRearConnectionGlassData }},
            {TechType.SeaTruckSleeperModule, new List<RendererData>{ seatruckFrontConnectionGlassData, seatruckRearConnectionGlassData, new RendererData("model/seatruck_module_sleeper_anim/Seatruck_Sleeper_Module_interior") }},
        };


        Dictionary<string, RendererData> glassRenderers_ = new Dictionary<string, RendererData>
        {
            {"dc96d06e-5d6d-450c-946b-4ac8e86f1d19", // Alterra_TechSite_TwistyBridge_02
            new RendererData("BaseCell/BaseCell", new List<string>{ "BaseCorridorIShapeWindowSide/BaseHatchModel/BaseCorridorInteriorWindowSide/BaseCorridorInteriorWindowSide_ext", "BaseCorridorIShapeWindowSide/BaseHatchModel/BaseCorridorInteriorWindowSide_LOD1/BaseCorridorInteriorWindowSide_ext_LOD1", "BaseCorridorIShapeWindowSide (1)/BaseHatchModel/BaseCorridorInteriorWindowSide/BaseCorridorInteriorWindowSide_ext", "BaseCorridorHatch/models/BaseCorridorExteriorCapHatch/hatch_end_anims/hatchGlass_geo", "BaseCorridorWindow/models/BaseCorridorExteriorCap_01/BaseCorridorExteriorCap_01_ext", "BaseCorridorWindow/models/BaseCorridorExteriorCap_01_LOD1/BaseCorridorExteriorCap_01_ext_LOD1", "BaseCorridorWindow/models/BaseCorridorExteriorCap_01/BaseCorridorExteriorCap_01_int"})},
             {"d5472f49-4eda-4c8f-a2a6-6b400dbc925a", null },// HoverZone3_FrozenRiver
             {"1b8df552-1b3e-4e96-ba1a-3d35afcb2c18", damagedExosuitData },// exosuit_damaged_05 -1678 15 -665
             {"d660e5ba-18b1-4249-b129-c41e0a3282a2", new RendererData("Exosuit_cabin_01_glass") },// OutpostZero_exofragment_2  -88 8 308
             {"d70c8458-4b19-4dbc-ba67-afa654af1999", new RendererData("exosuit_damaged_06/Exosuit_01_cabin008/Exosuit_cabin_01_glass008") },// exosuit_damaged_06  -190 -274 -739
             {"54a7d6b6-280a-43d5-8bdd-eada3dd5f6c3", new RendererData("exosuit_damaged_01/Exosuit_01_cabin005/Exosuit_cabin_01_glass005") },// exosuit_damaged_01  -193 -286 -694
             {"db54c4f1-9433-40ea-9645-58458bdd2562", damagedExosuitData},// exosuit_damaged_05_nodrill -206 -286 -686
             {"71f59e9b-701b-456c-9eae-aefbc53e4d26", new RendererData("exosuit_damaged_02/Exosuit_01_cabin004/Exosuit_cabin_01_glass004")},// exosuit_damaged_02_shipwreck 237 -244 -1264
             {"314d4f3a-b692-4ddf-8244-4dc97d8bf19b", new RendererData("Exosuit_01_cabin006/Exosuit_cabin_01_glass006") },// OutpostZero_exofragment_1  -91 9 303
             {"8c3d54c0-4330-4949-91ad-f046cfd67c7c", new RendererData("Starship_cargo_damaged_opened_01/dirt_01")},// Starship_cargo_damaged_opened_01
             {"ebc835bd-221a-4722-b1d0-becf08bd2f2c", cargoCrateData},// Starship_cargo_damaged_opened_02
             {"fb2886c4-7e03-4a47-a122-dc7242e7de5b", cargoCrateData},// Starship_cargo_damaged_opened_large_02
             {"989da7b6-d41e-4a7f-9b58-2fd4a0d4b088", new RendererData("frozenriver_01_section_b_LOD0") },// frozenriver_01_section_b  -8 31 423
             {"b682f66b-4098-4420-9b00-9f967c8e5a56", new RendererData(null, new List<string>{"Quad (1)","Quad (2)", "frozenriver_01_section_c_LOD0"}) },// frozenriver_01_section_c  -8 31 423
             {"0129c709-33e8-4bcf-8355-e14ac6647041", new RendererData("frozenriver_01_section_d_LOD0") },// frozenriver_01_section_d  27 11 370
             {"78378c09-4211-4b05-9e91-b074d5df8118", new RendererData(null, new List<string>{"frozenriver_01_section_e_LOD0", "Quad" }) },// frozenriver_01_section_e  -3 6 338
             {"74a8aa67-821e-48df-ba28-7e315152daa9", null },// frozenriver_01_section_f  -17 -8 278

            {"cdc0eaf2-f6bf-44f8-bab9-eac6dc3d8a89", barTableGlassData },// BarTable_Deco -104 12 309
            {"0d86ace0-6334-41a5-a0f0-0ebbf094ccbd", lockerGlassData },// Locker_deco -105 12 297
            {"ed3555f7-7e92-4c80-9f0a-f545956cb4a8", filtrationMachineGlassData },// FiltrationMachine_Deco -111 14 299
            {"e7c782f3-76e2-4324-a4f5-bf74a6dc0cf8", null },// frozenriver_02_glacialbasin_section_a  -1360 -4 -1097
            {"166f20cd-da83-4b0c-afb3-146b8b291bbd", null },// frozenriver_02_glacialbasin_section_c  -1395 12 -1115
            {"a4b2f1ff-75f7-4bea-853e-7f360e6bb713", null },// frozenriver_02_glacialbasin_section_d  -1383 45 -1167
            {"ca3e546d-b60f-4f1c-8ca4-9cc196117db1", null },// frozenriver_02_glacialbasin_section_e  -1197 5 -1107
            {"8c4eb1c7-8f36-4838-a3c8-f9a144b79685", null },// frozenriver_02_glacialbasin_section_f  -1157 23 -1167
            {"bb473087-f407-4ea4-b633-3b162742a7f9", null },// frozenriver_02_glacialbasin_section_h  -1184 4 -1006
            {"94e42f38-4190-4eef-b1b5-69ddf4f7a47a", null },// frozenriver_02_glacialbasin_section_g  -1584 38 -1104
            {"9c8ee782-3354-48aa-8d2f-d583f1f39e4a", null },// frozenriver_02_glacialbasin_section_i  -1079 8 -816
            {"8501598b-84fb-46f9-aba9-25285cc92305", null },// frozenriver_02_glacialbasin_section_j  -1073 5 -728
            {"6dec0b97-062e-40c4-86b0-e12187a10432", null },// frozenriver_02_glacialbasin_section_m  -1522 4 -996
            {"03465fdf-fbcc-4a4d-9194-29e8eea14e35", null },// frozenriver_02_glacialbasin_section_n  -1520 17 -1118
            {"2fffeadf-4c3b-46ff-8a68-be9ada47083a", null },// GlacialBasin_Fissure_Ice  -1296 23 -584
            {"a70bd841-214a-49cf-a477-06b1f48905ac", new RendererData("frozenriver_03_shieldbase_section_a", new List<string>{"waterfall_03_LOD0", "waterfall_03_decals_LOD0" }) },// frozenriver_03_shieldbase_section_a 1   -1056 8 -645
            {"6c5168b4-be7f-4581-9adc-4de0add69c55", new RendererData("model/Aquarium_animation2/Aquarium_geo/Aquarium_glass") },// Aquarium_Hoverbikebase  -1151 16 -723
            {"19da7da5-a864-4cee-a090-32248455936e",// Alterra_GlacialBasin_HoverBikeBase  -1296 23 -584
            new RendererData(null, new List<string>{ "Exterior/hoverbikebase_base_01_exterior_LOD0", "Interior/Closet/BasePartitionDoor (1)/BasePartitionDoor" }) },
            { "95d37d40-fb90-47d7-b2e1-9410b48d5c5f", new RendererData("model/JellyFish_anim")},// -991 -106 -494
            { "2e3a8acb-4665-4274-a65c-a02db40a91cf", new RendererData("Base_RocketBase_snow_ground_set")},// Base_RocketBase
            { "86d65ca5-68d0-4458-99a2-d30450aa9600", new RendererData("tank")},// PrecursorFabricatorBase_PartFabricatorBones 1280 -954 -310
            { "6a53f581-ea65-4e76-9b0d-2386179ff8a4", new RendererData("tank")},// PrecursorFabricatorBase_PartFabricatorSkin 1280 -954 -310
            { "019b6c8f-b289-48e1-aba3-8c34fc83937a", new RendererData("tank")},// PrecursorFabricatorBase_PartFabricatorOrgans 1280 -954 -310
            { "bccab6fa-7d5f-437f-86ba-0b2879764657",// Marguerit_Base 75 -376 -911
            new RendererData("Marguerit_base", new List<string>{"Marguerite_room_rectroom_exterior_and_interior", "BaseLargeRoomWindowSide/LargeRoom_lExteriorWindowSide01Glass_01", "BaseLargeRoomWindowSide/LargeRoom_InteriorWindowSide01Glass_01", "marg_base_large_room_interior_glass" })},
            { "94cddf73-f4c7-41b0-9fd8-a0c5247fc476",// ShipWreck_LargeAquarium 260 -233 -1252
            new RendererData(null, new List<string>{ "high/glass", "Large_Aquarium_generic_room/Large_Aquarium_generic_room_glass_01" })},
            { "ddd8d991-2180-4467-95ea-343896b581af", filtrationMachineGlassData},// ShipWreck_FiltrationMachine 266 -250 -1295 
            { "cb000fd6-a31c-4a3a-97cd-d60a37eb8237", barTableGlassData},// ShipWreck_BarTable 237 -258 -1290
            { "4c8852cb-2b5f-4acc-9494-ecf3b1b72093", labShelfData},// ShipWreck_Biodome_Lab_Shelf 267 -255 -1306
            { "33acd899-72fe-4a98-85f9-b6811974fbeb", labShelfData},// biodome_lab_shelf_01 -1201 21 -717
            { "2f2a6eeb-2239-4105-ab60-a6f5129f8a38", new RendererData("model/Seatruck_AquariumModule_Fragment_02")},// seatruck_aquariummodule_fragment_02
                
        };

        static string roomIntWindowGlassPath1 = "BaseRoomWindowSide(Clone) (1)/BaseRoomGenericInteriorWindowSide01/BaseInteriorRoomGenericWindowSide01Glass";
        static string roomExtWindowGlassPath1 = "BaseRoomWindowSide(Clone) (1)/BaseRoomGenericInteriorWindowSide01/BaseExteriorRoomGenericWindowSide01Glass";
        static string roomIntWindowGlassPath = "BaseRoomWindowSide(Clone)/BaseRoomGenericInteriorWindowSide01/BaseInteriorRoomGenericWindowSide01Glass";
        static string roomExtWindowGlassPath = "BaseRoomWindowSide(Clone)/BaseRoomGenericInteriorWindowSide01/BaseExteriorRoomGenericWindowSide01Glass";

        Dictionary<string, List<RendererData>> glassRenderers__ = new Dictionary<string, List<RendererData>>
        {
            {"b39aff99-17e5-4430-8261-f60ff22a1a0f",// Base_ResearchBase_03  -86 7 300
            new List<RendererData>{
                {new RendererData("Meshes/Greenhouse/GlassDome", new List<string>{ "BaseRoomGenericExteriorCapTopGlassInterior",  "BaseRoomGenericExteriorCapTopGlassExterior"}) },
                {new RendererData("Meshes/Exterior/BaseExteriorGlass") },
                {new RendererData("Meshes/Interior/Culling", new List<string>{ "Lab/biodome_lab_containers_tube_01 (2)/biodome_lab_containers_tube_01/biodome_lab_containers_tube_01_glass", "Lab/biodome_lab_containers_tube_01 (4)/biodome_lab_containers_tube_01/biodome_lab_containers_tube_01_glass", "Lab/biodome_lab_containers_tube_01 (3)/biodome_lab_containers_tube_01/biodome_lab_containers_tube_01_glass", "Kitchen/submarine_Storage_locker_big_01_hinges_L (1)/submarine_Storage_locker_big_01_door_L", "Kitchen/submarine_Storage_locker_big_01_hinges_L (2)/submarine_Storage_locker_big_01_door_L", "Lab/biodome_lab_containers_open_02/biodome_lab_containers_open_02/biodome_lab_containers_open_02_glass", "Lab/biodome_lab_containers_close_01 (1)/biodome_lab_containers_close_01/biodome_lab_containers_close_01_glass","Lab/discovery_lab_props_03/discovery_lab_props_03_glass", "Lab/discovery_lab_props_02/discovery_lab_props_02_glass", "Lab/biodome_lab_containers_open_01/biodome_lab_containers_open_01/biodome_lab_containers_open_01_glass"}) },
                {new RendererData(null, new List<string>{ "discovery_lab_props_02/discovery_lab_props_02_glass", "discovery_lab_props_03/discovery_lab_props_03_glass", }) },
            } },
            {"482bf0ee-047c-4c4b-8be0-96128f7837e1",// Base_RocketBase_Emmanuel
            new List<RendererData>{
                {new RendererData("Snow_ground/Base_RocketBase_emmanuels_set") },
                {new RendererData("ResearchBase_GenericRoom/Interior/Panels", new List<string>{ "BaseExteriorRoomGenericWindowSide01Glass", "BaseExteriorRoomGenericWindowSide01Glass_LOD1", }) },
            } },
            {"dd1bd28d-9de9-4c81-b198-7c827e3a94f4",// Alterra_Base_2
            new List<RendererData>{
                {new RendererData("Exterior/BaseCell_RoomRight_Ext" , new List<string>{"BaseGlassDome(Clone)/BaseGhost/BaseRoomGenericExteriorTopGlass/BaseRoomGenericExteriorCapTopGlassExterior", roomExtWindowGlassPath1, roomExtWindowGlassPath }) },
                {new RendererData("Interior/BaseCell_RoomRight_Int", new List<string>{"BaseGlassDome(Clone)/BaseGhost/BaseRoomGenericExteriorTopGlass/BaseRoomGenericExteriorCapTopGlassInterior", roomIntWindowGlassPath, roomIntWindowGlassPath1, "FiltrationMachine_Deco(Clone)/model/Water_Filtration_Machine/water_filtration_machine_geo/water_filtration_machine_glass"}) },
                {new RendererData("Interior/Props_Int/Props_RoomRight_Int", new List<string>{ "biodome_lab_tube_01 (1)/biodome_lab_tube_01_glass", "discovery_lab_props_02 (1)/discovery_lab_props_02_glass"}) },
                {new RendererData("Exterior/Hatch/hatch_end_anims/hatchGlass_geo") },
                {new RendererData("Exterior/BaseCell_RoomLeft_Ext", new List<string>{roomExtWindowGlassPath, roomExtWindowGlassPath1 }) },
                {new RendererData("Interior/BaseCell_RoomLeft_Int", new List<string>{roomIntWindowGlassPath, roomIntWindowGlassPath1 }) },
                {new RendererData("Interior/Props_Int/Props_RoomCentral_int", new List<string>{ "discovery_lab_props_02/discovery_lab_props_02_glass" }) },
                {new RendererData("Interior/Props_Int/Props_RoomCentral_int/biodome_lab_containers_tube_01 (1)") },
                {new RendererData("Interior/Props_Int/Props_RoomCentral_int/biodome_lab_containers_tube_01") },
                {new RendererData("Interior/Props_Int/Props_RoomRight_Int/biodome_lab_containers_tube_01 (2)") },
                {new RendererData("Interior/Props_Int/Props_RoomRight_Int/biodome_lab_containers_tube_01 (4)") },
                {new RendererData("Interior/Props_Int/Props_RoomRight_Int/biodome_lab_containers_tube_01 (5)") },
                {new RendererData("Interior/Props_Int/Props_RoomRight_Int/biodome_lab_containers_tube_01 (6)") },
                {new RendererData("Interior/Props_Int/Props_RoomRight_Int/biodome_lab_containers_tube_01 (7)") },
                {new RendererData("Interior/Props_Int/Props_RoomRight_Int/biodome_lab_containers_tube_01 (8)") },
            } },
        };

        List<string> fruitPlants = new List<string> {
            "7329db6b-7385-4e77-8afa-71830ead9350",// coral_reef_kelp_arctic_01_bulb_b
            "a17ef178-6952-4a91-8f66-44e1d8ca0575",// coral_reef_kelp_arctic_01_bulb_a
            "702e2057-e964-4792-8433-2abfe7e9b680",// generic_fruit_ice_plant_short
            "002749e5-db0a-4d2b-bb0d-aa0725f781a2",// generic_fruit_ice_plant_tall
            //"",// generic_fruit_ice_plant_peak_01
            //"",// generic_fruit_ice_plant_peak_01
        };


        public void IterateRootGameObjects()
        {
            if (loadedPrefabsFixed)
                return;

            //Main.logger.LogDebug("FindAllRootGameObjects");
            foreach (GameObject go in Util.FindAllRootGameObjects())
            {
                //if (go.name == "CellRoot(Clone)" || go.name == "ChunkCollider(Clone)" || go.name == "ChunkGrass(Clone)" || go.name == "ChunkLayer(Clone)" || go.name == "Chunk(Clone)" || go.name.StartsWith("Batch"))
                //{
                //    continue;
                //}
                //UniqueIdentifier pi = go.GetComponentInChildren<UniqueIdentifier>();
                //if (pi != null)
                //    logger.LogDebug($"{go.name} {pi.classId}");
                //else
                //    logger.LogDebug($"{go.name} no UniqueIdentifier");

                //if (go.name.Contains("stone"))
                //    logger.LogInfo("prefab " + go.name);
                if (ConfigToEdit.grassCastShadow.Value && go.name == "ChunkGrass(Clone)")
                {
                    Renderer renderer = go.GetComponent<Renderer>();
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                }
                if (bloodPrefabs.Contains(go.name))
                {
                    SetBloodColor(go);
                }
            }
            //loadedPrefabsFixed = true;
        }

        public void SetBloodColor(GameObject go)
        {
            //0.784f, 1f, 0.157f
            if (bloodColor == default)
                return;

            //AddDebug($"SetBloodColor {go.name} {bloodColor}");
            ParticleSystem[] pss = go.GetAllComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in pss)
            {
                ParticleSystem.MainModule psMain = ps.main;
                psMain.startColor = new ParticleSystem.MinMaxGradient(bloodColor);
            }
        }

        public void FixPrefabs()
        {
            foreach (string classID in fruitPlants)
            {
                UWE.CoroutineHost.StartCoroutine(AddFruitPlant(classID));
            }
            foreach (var kv in glassRenderers)
            {
                UWE.CoroutineHost.StartCoroutine(DisableShadowCasting(kv.Key, kv.Value));
            }
            foreach (var kv in glassRenderers_)
            {
                UWE.CoroutineHost.StartCoroutine(DisableShadowCasting(kv.Key, kv.Value));
            }
            foreach (var kv in glassRenderers__)
            {
                UWE.CoroutineHost.StartCoroutine(DisableShadowCasting(kv.Key, kv.Value));
            }
            foreach (var kv in glassRenderers___)
            {
                UWE.CoroutineHost.StartCoroutine(DisableShadowCasting(kv.Key, kv.Value));
            }
            foreach (var kv in glassMaterialZoffsets)
            {
                UWE.CoroutineHost.StartCoroutine(ChangeMaterialZoffsetAsync(kv.Key, kv.Value));
            }
            foreach (var kv in glassMaterialZoffsets_)
            {
                UWE.CoroutineHost.StartCoroutine(ChangeMaterialZoffsetAsync(kv.Key, kv.Value));
            }
            foreach (var kv in fragments)
            {
                UWE.CoroutineHost.StartCoroutine(FixFragment(kv.Key, kv.Value));
            }
            foreach (string classID in shipWrecks)
            {
                UWE.CoroutineHost.StartCoroutine(FixShipwreck(classID));
            }
            UWE.CoroutineHost.StartCoroutine(FixMargGreenhouseMelons());
            UWE.CoroutineHost.StartCoroutine(RemoveCollisionTwistyBridgeCoralLong());
            UWE.CoroutineHost.StartCoroutine(RemoveCollisionTrianglePlant());
            UWE.CoroutineHost.StartCoroutine(RemoveCollisionCyanFlower());
            UWE.CoroutineHost.StartCoroutine(RemoveCollisionTapePlant());
            if (ConfigToEdit.trypophobiaMode.Value)
                UWE.CoroutineHost.StartCoroutine(DisableHoneyCombPlants());
        }

        private IEnumerator FixMargGreenhouseMelons()
        {
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync("62292143-8a6c-459e-9196-cf780628ad41");// Marguerite_GreenHouse 990 31 -890
            yield return request;
            GameObject prefab;
            request.TryGetPrefab(out prefab);
            Transform scannables = prefab.transform.Find("Scannable");
            for (int i = 26; i < 30; i++)
            {
                if (i == 27)
                    continue;

                Transform scannable = scannables.GetChild(i);
                SphereCollider collider = scannable.GetComponentInChildren<SphereCollider>();
                collider.radius = 0.25f; // fix: melons could not be picked up
            }
        }

        private IEnumerator DisableHoneyCombPlants()
        {
            List<string> classIDs = new List<string> { "8e29762d-e18b-4304-8c24-c43534b737d1", "82e8005f-be3b-4fcb-a2aa-b9c159bcef0c", "71c45285-5170-4704-93e4-cb668015a5b1" };
            foreach (string classID in classIDs)
            {
                IPrefabRequest request = PrefabDatabase.GetPrefabAsync(classID);
                yield return request;
                GameObject prefab;
                if (request.TryGetPrefab(out prefab) == false)
                {
                    Main.logger.LogError("DisableHoneyCombPlants No prefab for " + classID);
                    continue;
                }
                foreach (Transform child in prefab.transform)
                    child.gameObject.SetActive(false);
            }
        }

        private IEnumerator FixShipwreck(string classID)
        {
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync(classID);
            yield return request;
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogError("FixShipwrecks No prefab for " + classID);
                yield break;
            }
            FixShipwreckGlass(prefab.transform);
            if (prefab.name == "ShipWreck1")
                FixThatShroom(prefab);
        }

        private void FixThatShroom(GameObject wreck)
        {// 66 -95 -869 thermalzone_coral_shelf_plates_01_group_c
            Transform rooms = wreck.transform.Find("Rooms");
            Transform entities = rooms.Find("EngineRoom/Entities");
            Transform serversEntities = rooms.Find("servers/Entities");
            Transform shroom = entities.GetChild(65);
            shroom.SetParent(serversEntities);
        }

        private IEnumerator FixFragment(string classID, MaterialZoffsetData data)
        {
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync(classID);
            yield return request;
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogError("FixFragment No prefab for " + classID);
                yield break;
            }
            Renderer renderer;
            if (data.rendererPath == null)
            {
                renderer = prefab.transform.GetComponentInChildren<Renderer>();
            }
            else
            {
                Transform rendererT = prefab.transform.Find(data.rendererPath);
                renderer = rendererT.GetComponent<Renderer>();
            }
            //Main.logger.LogDebug($"ChangeMaterialZoffset {techType} {renderer.name} materials");
            if (data.materialIndex >= renderer.materials.Length)
            {
                //Main.logger.LogDebug("ChangeMaterialZoffsetAsync wrong materialIndex");
                yield break;
            }
            Material material = renderer.materials[data.materialIndex];
            //Material material = renderer.sharedMaterials[data.materialIndex];
            //Main.logger.LogDebug("Set offset " + material.name);
            material.SetFloat(zOffset, data.offsetValue);
            Pickupable pickupable = prefab.GetComponent<Pickupable>();
            if (pickupable != null)
                UnityEngine.Object.Destroy(pickupable);
        }

        private IEnumerator DisableShadowCasting(TechType techType, RendererData data)
        {
            CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(techType);
            yield return request;
            GameObject prefab = request.GetResult();
            if (prefab == null)
            {
                Main.logger.LogError($"DisableShadowCasting no prefab for {techType}");
                yield break;
            }
            //PrefabIdentifier identifier = prefab.GetComponent<PrefabIdentifier>();
            //Main.logger.LogError($"DisableShadowCasting techType {techType} {identifier.classId}");
            prefab.transform.DisableShadowCasting(data);
        }

        private IEnumerator DisableShadowCasting(TechType techType, List<RendererData> datas)
        {
            CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(techType);
            yield return request;
            GameObject prefab = request.GetResult();
            if (prefab == null)
            {
                Main.logger.LogError($"DisableShadowCasting no prefab for {techType}");
                yield break;
            }
            //PrefabIdentifier identifier = prefab.GetComponent<PrefabIdentifier>();
            //Main.logger.LogError($"DisableShadowCasting prefab classId");
            foreach (RendererData data in datas)
                prefab.transform.DisableShadowCasting(data);
        }

        private IEnumerator DisableShadowCasting(string classID, List<RendererData> datas)
        {
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync(classID);
            yield return request;
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogError("DisableShadowCasting No prefab for " + classID);
                yield break;
            }
            PrefabIdentifier identifier = prefab.GetComponent<PrefabIdentifier>();
            //Main.logger.LogError($"DisableShadowCasting prefab classId");
            foreach (RendererData data in datas)
                prefab.transform.DisableShadowCasting(data);
        }

        private IEnumerator DisableShadowCasting(string classID, RendererData data)
        {
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync(classID);
            yield return request;
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogError("DisableShadowCasting No prefab for " + classID);
                yield break;
            }
            //Main.logger.LogError($"DisableShadowCasting(string classID, RendererData data) {classID} ");
            prefab.transform.DisableShadowCasting(data);
        }

        IEnumerator ChangeMaterialZoffsetAsync(TechType techType, MaterialZoffsetData data)
        {
            //Main.logger.LogDebug("ChangeMaterialZoffsetAsync techType " + techType);
            CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(techType);
            yield return request;
            GameObject prefab = request.GetResult();
            if (prefab == null)
            {
                Main.logger.LogError($"ChangeMaterialZoffsetAsync no prefab for {techType}");
                yield break;
            }
            ChangeMaterialZoffset(prefab, data);
        }

        void ChangeMaterialZoffset(GameObject go, MaterialZoffsetData data)
        {
            //Main.logger.LogDebug($"ChangeMaterialZoffset {go.name} ");
            //PrefabIdentifier identifier = go.GetComponent<PrefabIdentifier>();
            //Main.logger.LogDebug($"ChangeMaterialZoffset {go.name} {identifier.classId}");
            Renderer renderer;
            if (data.rendererPath == null)
                renderer = go.GetComponentInChildren<Renderer>();
            else
            {
                Transform rendererT = go.transform.Find(data.rendererPath);
                renderer = rendererT.GetComponent<Renderer>();
            }
            if (renderer == null)
            {
                Main.logger.LogError($"ChangeMaterialZoffset {go.name} renderer null");
                return;
            }
            //Main.logger.LogDebug($"ChangeMaterialZoffset renderer {renderer.name}");
            //Main.logger.LogDebug($"ChangeMaterialZoffset {go.name} materialIndex {data.materialIndex} renderer.materials.Length {renderer.materials.Length}");
            if (data.materialIndex >= renderer.materials.Length)
            {
                Main.logger.LogError("ChangeMaterialZoffset wrong materialIndex");
                return;
            }
            Material material = renderer.materials[data.materialIndex];
            //Main.logger.LogDebug("Set offset " + material.name);
            material.SetFloat(zOffset, data.offsetValue);
        }

        IEnumerator ChangeMaterialZoffsetAsync(string classID, MaterialZoffsetData data)
        {
            //Main.logger.LogDebug("ChangeMaterialZoffsetAsync " + classID);
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync(classID);
            yield return request;
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogError("ChangeMaterialZoffsetAsync No prefab for " + classID);
                yield break;
            }
            ChangeMaterialZoffset(prefab, data);
        }

        IEnumerator RemoveCollisionTwistyBridgeCoralLong()
        {
            HashSet<string> twistyBridgeLongCorals = new HashSet<string> { "e6708774-d20c-4f0f-a4fe-6fdc35d0b512", "d792dd71-eac2-4208-b291-ef6e771126e3", "7e6f0b45-59b8-4383-befb-f680643b7248", "14cc520f-c417-46ad-a678-daee76210d15", "19223fd5-9dfd-45ae-b1b9-c046fb6d5509", "56ef4e45-2b9c-487a-aa12-6c4d37eec98a", "524c456a-7a6d-4c75-92fb-9f6cef8e51fa" };

            foreach (string classID in twistyBridgeLongCorals)
            {
                IPrefabRequest request = PrefabDatabase.GetPrefabAsync(classID);
                yield return request;
                GameObject prefab;
                if (request.TryGetPrefab(out prefab) == false)
                {
                    Main.logger.LogError("RemoveCollisionTwistyBridgeCoralLong No prefab for " + classID);
                    continue;
                }
                Collider collider = prefab.GetComponent<Collider>();
                if (collider)
                    UnityEngine.Object.Destroy(collider);

                Transform tr = prefab.transform.Find("GameObject");
                if (tr)
                {
                    BoxCollider bc = tr.GetComponent<BoxCollider>();
                    bc.isTrigger = true;
                }
            }
        }

        IEnumerator RemoveCollisionTrianglePlant()
        {
            List<string> trianglePlants = new List<string> { "22e0569d-1983-4c5f-953b-e546e999d916", "9b3fedc4-e8df-4f17-9bdb-d8e9b5062fd6", "7a675e26-b75f-4575-b712-1a9593be15f5" };

            foreach (string classID in trianglePlants)
            {
                IPrefabRequest request = PrefabDatabase.GetPrefabAsync(classID);
                yield return request;
                GameObject prefab;
                if (request.TryGetPrefab(out prefab) == false)
                {
                    Main.logger.LogError("RemoveCollisionTrianglePlant No prefab for " + classID);
                    continue;
                }
                Collider collider = prefab.GetComponentInChildren<Collider>();
                collider.isTrigger = true;
            }
        }

        IEnumerator RemoveCollisionCyanFlower()
        {
            List<string> cyanFlowers = new List<string> { "7e133c95-e0b5-463b-9a87-230402707cec", "c90f0639-e22c-43cf-aa80-3abaa75a0629" };

            foreach (string classID in cyanFlowers)
            {
                IPrefabRequest request = PrefabDatabase.GetPrefabAsync(classID);
                yield return request;
                GameObject prefab;
                if (request.TryGetPrefab(out prefab) == false)
                {
                    Main.logger.LogError("RemoveCollisionCyanFlower No prefab for " + classID);
                    continue;
                }
                Transform collision = prefab.transform.Find("collision");
                if (collision)
                {
                    collision.gameObject.layer = LayerID.Useable;
                    CapsuleCollider cc = collision.GetComponent<CapsuleCollider>();
                    cc.isTrigger = true;
                }
            }
        }

        IEnumerator RemoveCollisionTapePlant()
        {
            CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(TechType.TapePlant);
            yield return request;
            GameObject prefab = request.GetResult();
            if (prefab == null)
            {
                Main.logger.LogError($"RemoveCollisionTapePlant prefab null");
                yield break;
            }
            CapsuleCollider oldCollider = prefab.GetComponent<CapsuleCollider>();
            GameObject col = new GameObject("Collision");
            col.layer = LayerID.Useable;
            col.transform.SetParent(prefab.transform);
            col.transform.localPosition = Vector3.zero;
            CapsuleCollider c = col.AddComponent<CapsuleCollider>();
            c.isTrigger = true;
            c.center = new Vector3(0, 5.5f, 0);
            c.height = 11;
            c.radius = oldCollider.radius;
            //Main.logger.LogError($"RemoveCollisionTapePlant {prefab.name} c.height {c.height}");
            UnityEngine.Object.Destroy(oldCollider);
        }

        private void FixShipwreckGlass(Transform wreck)
        {
            //Main.logger.LogDebug("FixShipwreckGlass " + wreck.name);
            Transform rooms = wreck.Find("Rooms");
            if (rooms == null)
                rooms = wreck.Find("Rooms (1)");

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

        IEnumerator AddFruitPlant(string classID)
        {
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync(classID);
            yield return request;
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogError("AddFruitPlant No prefab for " + classID);
                yield break;
            }
            PickPrefab[] pickPrefabs = prefab.GetComponentsInChildren<PickPrefab>(true);
            if (pickPrefabs.Length == 0)
            {
                Main.logger.LogError("AddFruitPlant No pickPrefabs on " + classID);
                yield break;
            }
            FruitPlant fruitPlant = prefab.GetComponent<FruitPlant>();
            if (fruitPlant == null)
            {
                Main.logger.LogDebug("AddFruitPlant No FruitPlant on " + prefab.name);
                fruitPlant = prefab.AddComponent<FruitPlant>();
            }
            fruitPlant.allowFruitSpawnByDefault = true;
            fruitPlant.fruitSpawnEnabled = true;

            if (ConfigToEdit.fruitGrowTime.Value == 0)
                fruitPlant.fruitSpawnInterval = 300;
            else
            {
                yield return new WaitUntil(() => DayNightCycle.main != null);
                fruitPlant.fruitSpawnInterval = ConfigToEdit.fruitGrowTime.Value * DayNightCycle.main.dayLengthSeconds;
            }
            Main.logger.LogDebug($"AddFruitPlant {prefab.name} fruitSpawnInterval {fruitPlant.fruitSpawnInterval} fruitSpawnEnabled {fruitPlant.fruitSpawnEnabled}");
            fruitPlant.fruits = pickPrefabs;
            foreach (PickPrefab pp in pickPrefabs)
            {
                if (!pp.gameObject.activeSelf && !fruitPlant.inactiveFruits.Contains(pp))
                    fruitPlant.inactiveFruits.Add(pp);
            }
        }

        static IEnumerator AddLabel(Transform door)
        {
            //while (door.parent == null)
            //    yield return null;

            //AddDebug("AddLabel " + cyclops + " " + techType);
            CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(TechType.Sign);
            yield return request;
            GameObject go = request.GetResult();
            if (go == null)
            {
                Main.logger.LogError($"AddFruitPlant AddLabel prefab null");
                yield break;
            }
            //GameObject go = Utils.CreatePrefab(result1);
            go.transform.position = door.transform.position;
            go.transform.SetParent(door);
            go.transform.localPosition = new Vector3(.32f, -.58f, .26f);
            go.transform.localEulerAngles = new Vector3(0f, 90f, 90f);
            Transform tr = go.transform.Find("Trigger");
            UnityEngine.Object.Destroy(tr.gameObject);
            tr = go.transform.Find("UI/Base/Up");
            UnityEngine.Object.Destroy(tr.gameObject);
            tr = go.transform.Find("UI/Base/Down");
            UnityEngine.Object.Destroy(tr.gameObject);
            tr = go.transform.Find("UI/Base/Left");
            UnityEngine.Object.Destroy(tr.gameObject);
            tr = go.transform.Find("UI/Base/Right");
            UnityEngine.Object.Destroy(tr.gameObject);
            tr = go.transform.Find("ConsturctableModel");
            UnityEngine.Object.Destroy(tr.gameObject);
            //tr = go.transform.Find("UI/Base/BackgroundToggle");
            tr = go.transform.Find("UI/Base/Minus");
            tr.localPosition = new Vector3(tr.localPosition.x - 130f, tr.localPosition.y - 320f, tr.localPosition.z);
            tr = go.transform.Find("UI/Base/Plus");
            tr.localPosition = new Vector3(tr.localPosition.x + 130f, tr.localPosition.y - 320f, tr.localPosition.z);
            Constructable c = go.GetComponent<Constructable>();
            UnityEngine.Object.Destroy(c);
            TechTag tt = go.GetComponent<TechTag>();
            UnityEngine.Object.Destroy(tt);
            ConstructableBounds cb = go.GetComponent<ConstructableBounds>();
            UnityEngine.Object.Destroy(cb);
            PrefabIdentifier pi = go.GetComponent<PrefabIdentifier>();
            UnityEngine.Object.Destroy(pi);

            uGUI_SignInput si = go.GetComponentInChildren<uGUI_SignInput>(true);
            if (si)
            {
                //si.stringDefaultLabel = "SmallLockerDefaultLabel";
                //si.inputField.text = Language.main.Get(si.stringDefaultLabel);
                //si.inputField.characterLimit = 58;
                //string slot = SaveLoadManager.main.currentSlot;
                //if (Main.configMain.lockerNames.ContainsKey(slot))
                //{
                //    string key = GetKey(door);
                //    if (Main.configMain.lockerNames[slot].ContainsKey(key))
                //    {
                //        SavedLabel sl = Main.configMain.lockerNames[slot][key];
                //        si.inputField.text = sl.text;
                //        si.colorIndex = sl.color;
                //        si.SetBackground(sl.background);
                //        si.scaleIndex = sl.scale; // range -3 3 
                //    }
                //}
            }
            Main.logger.LogError($"AddFruitPlant AddLabel !!!");
        }

    }

    public class RendererData
    {
        public string parentPath;
        public List<string> renderers;

        public RendererData(string parentPath, List<string> renderers)
        {
            this.parentPath = parentPath;
            this.renderers = renderers;
        }

        public RendererData(string parentPath)
        {
            this.parentPath = parentPath;
        }
    }

    class MaterialZoffsetData
    {
        public string rendererPath;
        public int materialIndex;
        public int offsetValue;

        public MaterialZoffsetData(string rendererPath, int materialIndex, int offsetValue)
        {
            this.rendererPath = rendererPath;
            this.materialIndex = materialIndex;
            this.offsetValue = offsetValue;
        }
    }

}
