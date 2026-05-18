using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static ErrorMessage;


namespace Tweaks_Fixes
{
    internal class BaseGlassFix
    {
        [HarmonyPatch(typeof(BaseDeconstructable))]
        class BaseDeconstructable_Patch
        {
            static readonly Dictionary<TechType, List<string>> glassRenderers = new Dictionary<TechType, List<string>> {
                { TechType.BaseWindow, new List<string> { "model/BaseLargeRoomWindowSide/LargeRoom_lExteriorWindowSide01Glass_01", "model/BaseLargeRoomWindowSide/LargeRoom_InteriorWindowSide01Glass_01", "model/BaseLargeRoomWindowSideShort/LargeRoom_lExteriorWindowSide01Glass_002", "model/BaseLargeRoomWindowSideShort/LargeRoom_InteriorWindowSide01Glass_002", "BaseRoomGenericInteriorWindowSide01/BaseExteriorRoomGenericWindowSide01Glass", "BaseRoomGenericInteriorWindowSide01/BaseExteriorRoomGenericWindowSide01Glass_LOD1", "BaseRoomGenericInteriorWindowSide01/BaseInteriorRoomGenericWindowSide01Glass", "BaseRoomGenericInteriorWindowSide01/BaseInteriorRoomGenericWindowSide01Glass_LOD1", "models/BaseCorridorExteriorCap_01/BaseCorridorExteriorCap_01_ext", "models/BaseCorridorExteriorCap_01_LOD1/BaseCorridorExteriorCap_01_ext_LOD1", "BaseHatchModel/BaseCorridorInteriorWindowSide/BaseCorridorInteriorWindowSide_ext", "BaseHatchModel/BaseCorridorInteriorWindowSide_LOD1/BaseCorridorInteriorWindowSide_ext_LOD1", "models/BaseCorridorInteriorWindowTop/BaseCorridorInteriorWindowTop_ext", "models/BaseCorridorInteriorWindowTop_LOD1/BaseCorridorInteriorWindowTop_ext_LOD1", "models/BaseCorridorXShapeExteriorWindowTop/BaseCorridorXShapeExteriorWindowTop_ext", "models/BaseCorridorXShapeExteriorWindowTop_LOD1/BaseCorridorXShapeExteriorWindowTop_ext_LOD1", "BaseMapRoomInteriorWindowSide/BaseMapRoomInteriorWindowSideGlass_ext", "BaseMapRoomInteriorWindowSide/BaseMapRoomInteriorWindowSideGlass_int", "model/BaseRoomMoonPoolExteriorWindowSide01Glass_01", "model/BaseRoomMoonPoolExteriorWindowSide01Glass_01_LOD1", "model/BaseRoomMoonPoolInteriorWindowSide01Glass_01", "model/BaseRoomMoonPoolInteriorWindowSide01Glass_01_LOD1", "model/BaseRoomMoonPoolInteriorWindowSide01Glass_02", "model/BaseRoomMoonPoolInteriorWindowSide01Glass_02_LOD1", "model/BaseRoomMoonPoolExteriorWindowSide01Glass_02", "model/BaseRoomMoonPoolExteriorWindowSide01Glass_02_LOD1" } },
                { TechType.BaseGlassDome, new List<string> { "model/BaseRoomGenericExteriorCapTopGlassExterior", "model/BaseRoomGenericExteriorCapTopGlassInterior" } },
                { TechType.BaseWaterPark, new List<string> { "model/Large_Aquarium_generic_room_glass_01", "model/Large_Aquarium_02_glass" } },
                { TechType.BaseLargeGlassDome, new List<string> { "model/LargeRoomExteriorTop_01/LargeRoomExteriorTop_01_glass", "model/LargeRoomExteriorTop_01/LargeRoomInteriorTop_01_glass" } }, // this works somehow
                { TechType.BaseFiltrationMachine, new List<string> {
                    "model/Water_Filtration_Machine/Water_Filtration_Machine_flat/water_filtration_machine_glass",
                "model/Water_Filtration_Machine/water_filtration_machine_geo/water_filtration_machine_glass"} },
                {TechType.BaseHatch, new List<string> {"BaseCorridorHatch/underWater/model/BaseCorridorExteriorCapHatch/hatch_end_anim/hatchGlass_geo", "underWater/model/BaseCorridorExteriorCapHatch/hatch_end_anim/hatchGlass_geo", "models/BaseExteriorHatchTop/BaseExteriorHatchTop 1/hatch_top_anim/hatchGlass_geo", "model/hatch_side_anim/hatchGlass_geo", "models/hatch_bottom_anims/hatchGlass_geo", "models/hatch_bottom_anim/hatchGlass_geo", "models/BaseExteriorHatchTop/BaseExteriorHatchTop 1/hatch_top_anims/hatchGlass_geo", "BaseCorridorHatch/models/hatch_alienContainment_anim/hatchGlass_geo", "BaseCorridorHatch/models/hatch_alienContainment_anims/hatchGlass_geo" } },
                };

            public static void FixWaterParkGlassRoofOLD(Transform wp)
            {
                string[] wpFoofRenderers = new string[] { "model/BaseLargeWaterParkCeilingGlassDome/BaseWaterParkCeilingGlassDome_glass_ext", "model/BaseLargeWaterParkCeilingGlassDome/BaseWaterParkCeilingGlassDome_glass_int", "model/BaseWaterParkCeilingGlassGlass/BaseWaterParkCeilingGlass_geo" };
                foreach (Transform child in wp.parent)
                {
                    if (child.name.StartsWith("BaseLargeWaterParkCeilingGlass")) // has no BaseDeconstructable
                    {
                        foreach (string rendererName in wpFoofRenderers)
                        {
                            Transform t = child.Find(rendererName);
                            if (t != null)
                                t.DisableShadowCasting();
                        }
                    }
                }
            }
            public static void FixLargeWaterParkGlassRoof(Transform wp)
            {
                string[] wpGlassFoofRenderers = new string[] { "model/BaseLargeWaterParkCeilingGlassDome/BaseWaterParkCeilingGlassDome_glass_ext", "model/BaseLargeWaterParkCeilingGlassDome/BaseWaterParkCeilingGlassDome_glass_int" };

                List<Transform> glassRoofs = wp.parent.FindAllChildren("BaseLargeWaterParkCeilingGlassDome(Clone)");

                foreach (Transform glassRoof in glassRoofs)
                {
                    foreach (string rendererPath in wpGlassFoofRenderers)
                    {
                        Transform t = glassRoof.Find(rendererPath);
                        t.DisableShadowCasting();
                    }
                }
                foreach (Transform child in wp.parent.transform)
                {
                    if (child.name.StartsWith("BaseLargeWaterParkCeilingGlass"))
                    {
                        Transform t = child.Find("model/BaseWaterParkCeilingGlassGlass/BaseWaterParkCeilingGlass_geo"); // glass floor you get when you build wp in room below
                        t?.DisableShadowCasting();
                    }
                }
            }

            public static IEnumerator FixWaterParkGlassRoof(Transform wp)
            {
                yield return new WaitForFrames(1);
                string[] wpFoofRenderers = new string[] {
                    "BaseWaterParkCeilingGlassDome_glass_ext",
                    "BaseWaterParkCeilingGlassDome_glass_int" };
                Transform glassDome = wp.parent.Find("BaseWaterParkCeilingGlassDome(Clone)/BaseWaterParkCeilingGlass/model/BaseWaterParkCeilingGlassDome");
                if (glassDome)
                {
                    foreach (string rendererName in wpFoofRenderers)
                    {
                        Transform t = glassDome.Find(rendererName);
                        t?.DisableShadowCasting();
                    }
                }
                Transform glassFloor = wp.parent.Find("BaseWaterParkCeilingGlass(Clone)/BaseWaterParkCeilingGlass/model");
                if (glassFloor)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Transform r = glassFloor.GetChild(i);
                        r.DisableShadowCasting();
                    }
                }
            }

            //[HarmonyPostfix, HarmonyPatch("Awake")]
            static void AwakePostfix(BaseDeconstructable __instance)
            {
                //AddDebug("BaseDeconstructable Awake " + __instance.recipe);
            }

            [HarmonyPostfix, HarmonyPatch("Init")]
            static void InitPostfix(BaseDeconstructable __instance)
            {
                //AddDebug("BaseDeconstructable Init " + __instance.recipe);
                if (__instance.recipe == TechType.BaseWaterPark)
                {
                    //AddDebug("BaseDeconstructable Init BaseWaterPark " + __instance.name);
                    if (__instance.name.StartsWith("BaseLargeWaterParkWalls"))
                        FixLargeWaterParkGlassRoof(__instance.transform);
                    else if (__instance.name.StartsWith("BaseWaterParkBottom"))
                        UWE.CoroutineHost.StartCoroutine(FixWaterParkGlassRoof(__instance.transform));
                }
                if (glassRenderers.ContainsKey(__instance.recipe))
                    __instance.transform.DisableShadowCasting(glassRenderers[__instance.recipe]);
            }
        }

        [HarmonyPatch(typeof(Leakable), "Start")]
        class Leakable_Start_Patch
        {
            static Dictionary<TechType, List<string>> glassRenderers = new Dictionary<TechType, List<string>> {
             { TechType.BaseCorridorGlassL, new List<string> { "models/BaseCorridorLShapeGlassExterior/BaseCorridorLShapeGlassExteriorGlass", "models/BaseCorridorLShapeGlassExterior_LOD1/BaseCorridorLShapeGlassExteriorGlass_LOD1" } },
            { TechType.BaseCorridorGlassI, new List<string> { "models/BaseCorridorhIShapeGlass01Exterior/BaseCorridorhIShapeGlass01ExteriorGlass", "models/BaseCorridorhIShapeGlass01Exterior/BaseCorridorhIShapeGlass01ExteriorGlass_LOD1" } },
            { TechType.BaseObservatory, new List<string> { "Room_Observatory/BaseRoomObservatory_glass", "Room_Observatory/BaseRoomObservatory_glass_LOD1" } },
            { TechType.BaseMoonpoolExpansion, new List<string> { "Expansion/Launchbay_cinematic/Reboot Art/MoonPool_Seatruck_Anim/SeatruckMoonPool_EntranceTube_Glass", "Expansion/Launchbay_cinematic/Reboot Art/MoonPool_Seatruck_Anim/SeatruckMoonPool_Cab_DockingRoom_PaneWindowGlass", "Expansion/Launchbay_cinematic/Reboot Art/MoonPool_Seatruck_Anim/SeatruckMoonPool_Cab_DockingRoom_BubbleWindowGlass" } },
                };
            static void Postfix(Leakable __instance)
            {

                BaseDeconstructable[] bds = __instance.transform.GetComponentsInChildren<BaseDeconstructable>();
                foreach (var bd in bds)
                {
                    //AddDebug("Leakable Start " + bd.recipe);
                    if (glassRenderers.ContainsKey(bd.recipe))
                    {
                        Main.logger.LogDebug("Leakable Start " + bd.recipe);
                        bd.transform.DisableShadowCasting(glassRenderers[bd.recipe]);
                    }
                }
            }
        }


        [HarmonyPatch(typeof(Constructable))]
        class Constructable_Patch
        {
            static Dictionary<TechType, List<string>> glassRenderers = new Dictionary<TechType, List<string>> {
            {TechType.Aquarium, new List < string > { "model/Aquarium_animation2/Aquarium_geo/Aquarium_glass" } },
            {TechType.BarTable, new List < string > { "descent_bar_table_01/descent_bar_table_01_glass" } },
            {TechType.Fridge, new List < string > { "geo/marg_props_fridge_door" } },
            {TechType.Locker, new List < string > { "model/submarine_Storage_locker_big_01/submarine_Storage_locker_big_01_hinges_R/submarine_Storage_locker_big_01_door_R", "model/submarine_Storage_locker_big_01/submarine_Storage_locker_big_01_hinges_L/submarine_Storage_locker_big_01_door_L" } }};

            [HarmonyPostfix, HarmonyPatch("NotifyConstructedChanged")]
            public static void NotifyConstructedChangedPostfix(Constructable __instance, bool constructed)
            {
                if (!constructed)
                    return;

                //AddDebug("Constructable NotifyConstructedChanged " + __instance.techType);
                if (glassRenderers.ContainsKey(__instance.techType))
                    __instance.transform.DisableShadowCasting(glassRenderers[__instance.techType]);
            }
        }

    }
}
