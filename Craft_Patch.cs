using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Craft_Patch
    {
        static bool crafting = false;
        static float hoverBikeBuildTime = 0f;
        static float timeDecayStart = 0f;

        [HarmonyPatch(typeof(Crafter), "Craft")]
        class Crafter_Craft_Patch
        {
            static void Prefix(Crafter __instance, TechType techType, ref float duration)
            {
                //AddDebug("Craft " + techType);
                duration *= Main.config.craftTimeMult;
                //return true;
            }
        }

        [HarmonyPatch(typeof(HoverpadConstructor), "TryStartConstructBike")]
        internal class HoverpadConstructor_Patch
        {
            public static void Prefix(HoverpadConstructor __instance)
            {
                if (hoverBikeBuildTime == 0f)
                    hoverBikeBuildTime = __instance.timeToConstruct;

                __instance.timeToConstruct = hoverBikeBuildTime * Main.config.craftTimeMult;
                //AddDebug("TryStartConstructBike " + __instance.timeToConstruct);
            }
        }

        [HarmonyPatch(typeof(Constructable), "GetConstructInterval")]
        class Constructable_GetConstructInterval_Patch
        {
            static void Postfix(ref float __result)
            {
                if (NoCostConsoleCommand.main.fastBuildCheat)
                    return;
                //AddDebug("GetConstructInterval " );
                __result *= Main.config.buildTimeMult;
            }
        }

        //[HarmonyPatch(typeof(CraftTree), "FabricatorScheme")]
        class CraftTree_FabricatorScheme_Patch
        {
            public static bool Prefix(CraftTree __instance, ref CraftNode __result)
            {
                //AddDebug("CraftTree FabricatorScheme");
                if (Main.config.craftVehicleUpgradesOnlyInMoonpool)
                {
                    __result = new CraftNode("Root").AddNode(new CraftNode[4]
                    {
                    new CraftNode("Resources", TreeAction.Expand).AddNode(new CraftNode[3]
                    {
                      new CraftNode("BasicMaterials", TreeAction.Expand).AddNode(new CraftNode[8]
                      {
                        new CraftNode("Titanium", TreeAction.Craft, TechType.Titanium),
                        new CraftNode("TitaniumIngot", TreeAction.Craft, TechType.TitaniumIngot),
                        new CraftNode("FiberMesh", TreeAction.Craft, TechType.FiberMesh),
                        new CraftNode("Silicone", TreeAction.Craft, TechType.Silicone),
                        new CraftNode("Glass", TreeAction.Craft, TechType.Glass),
                        new CraftNode("Lubricant", TreeAction.Craft, TechType.Lubricant),
                        new CraftNode("EnameledGlass", TreeAction.Craft, TechType.EnameledGlass),
                        new CraftNode("PlasteelIngot", TreeAction.Craft, TechType.PlasteelIngot)
                      }),
                      new CraftNode("AdvancedMaterials", TreeAction.Expand).AddNode(new CraftNode[7]
                      {
                        new CraftNode("HydrochloricAcid", TreeAction.Craft, TechType.HydrochloricAcid),
                        new CraftNode("Benzene", TreeAction.Craft, TechType.Benzene),
                        new CraftNode("AramidFibers", TreeAction.Craft, TechType.AramidFibers),
                        new CraftNode("Aerogel", TreeAction.Craft, TechType.Aerogel),
                        new CraftNode("Polyaniline", TreeAction.Craft, TechType.Polyaniline),
                        new CraftNode("HydraulicFluid", TreeAction.Craft, TechType.HydraulicFluid),
                        new CraftNode("FrozenCreatureAntidote", TreeAction.Craft, TechType.FrozenCreatureAntidote)
                      }),
                      new CraftNode("Electronics", TreeAction.Expand).AddNode(new CraftNode[11]
                      {
                        new CraftNode("CopperWire", TreeAction.Craft, TechType.CopperWire),
                        new CraftNode("Battery", TreeAction.Craft, TechType.Battery),
                        new CraftNode("PrecursorIonBattery", TreeAction.Craft, TechType.PrecursorIonBattery),
                        new CraftNode("PowerCell", TreeAction.Craft, TechType.PowerCell),
                        new CraftNode("PrecursorIonPowerCell", TreeAction.Craft, TechType.PrecursorIonPowerCell),
                        new CraftNode("ComputerChip", TreeAction.Craft, TechType.ComputerChip),
                        new CraftNode("WiringKit", TreeAction.Craft, TechType.WiringKit),
                        new CraftNode("AdvancedWiringKit", TreeAction.Craft, TechType.AdvancedWiringKit),
                        new CraftNode("ReactorRod", TreeAction.Craft, TechType.ReactorRod),
                        new CraftNode("RadioTowerPPU", TreeAction.Craft, TechType.RadioTowerPPU),
                        new CraftNode("RadioTowerTOM", TreeAction.Craft, TechType.RadioTowerTOM)
                      })
                    }),
                    new CraftNode("Survival", TreeAction.Expand).AddNode(new CraftNode[3]
                    {
                      new CraftNode("Water", TreeAction.Expand).AddNode(new CraftNode[2]
                      {
                        new CraftNode("FilteredWater", TreeAction.Craft, TechType.FilteredWater),
                        new CraftNode("WaterPurificationTablet", TreeAction.Craft, TechType.WaterPurificationTablet)
                      }),
                      new CraftNode("CookedFood", TreeAction.Expand).AddNode(new CraftNode[14]
                      {
                        new CraftNode("CookedBladderfish", TreeAction.Craft, TechType.CookedBladderfish),
                        new CraftNode("CookedBoomerang", TreeAction.Craft, TechType.CookedBoomerang),
                        new CraftNode("CookedHoopfish", TreeAction.Craft, TechType.CookedHoopfish),
                        new CraftNode("CookedSpinefish", TreeAction.Craft, TechType.CookedSpinefish),
                        new CraftNode("CookedSpinnerfish", TreeAction.Craft, TechType.CookedSpinnerfish),
                        new CraftNode("CookedArcticPeeper", TreeAction.Craft, TechType.CookedArcticPeeper),
                        new CraftNode("CookedArrowRay", TreeAction.Craft, TechType.CookedArrowRay),
                        new CraftNode("CookedSymbiote", TreeAction.Craft, TechType.CookedSymbiote),
                        new CraftNode("CookedNootFish", TreeAction.Craft, TechType.CookedNootFish),
                        new CraftNode("CookedTriops", TreeAction.Craft, TechType.CookedTriops),
                        new CraftNode("CookedFeatherFish", TreeAction.Craft, TechType.CookedFeatherFish),
                        new CraftNode("CookedFeatherFishRed", TreeAction.Craft, TechType.CookedFeatherFishRed),
                        new CraftNode("CookedDiscusFish", TreeAction.Craft, TechType.CookedDiscusFish),
                        new CraftNode("SpicyFruitSalad", TreeAction.Craft, TechType.SpicyFruitSalad)
                      }),
                      new CraftNode("CuredFood", TreeAction.Expand).AddNode(new CraftNode[13]
                      {
                        new CraftNode("CuredBladderfish", TreeAction.Craft, TechType.CuredBladderfish),
                        new CraftNode("CuredBoomerang", TreeAction.Craft, TechType.CuredBoomerang),
                        new CraftNode("CuredHoopfish", TreeAction.Craft, TechType.CuredHoopfish),
                        new CraftNode("CuredSpinefish", TreeAction.Craft, TechType.CuredSpinefish),
                        new CraftNode("CuredSpinnerfish", TreeAction.Craft, TechType.CuredSpinnerfish),
                        new CraftNode("CuredArcticPeeper", TreeAction.Craft, TechType.CuredArcticPeeper),
                        new CraftNode("CuredArrowRay", TreeAction.Craft, TechType.CuredArrowRay),
                        new CraftNode("CuredSymbiote", TreeAction.Craft, TechType.CuredSymbiote),
                        new CraftNode("CuredNootFish", TreeAction.Craft, TechType.CuredNootFish),
                        new CraftNode("CuredTriops", TreeAction.Craft, TechType.CuredTriops),
                        new CraftNode("CuredFeatherFish", TreeAction.Craft, TechType.CuredFeatherFish),
                        new CraftNode("CuredFeatherFishRed", TreeAction.Craft, TechType.CuredFeatherFishRed),
                        new CraftNode("CuredDiscusFish", TreeAction.Craft, TechType.CuredDiscusFish)
                      })
                    }),
                    new CraftNode("Personal", TreeAction.Expand).AddNode(new CraftNode[2]
                    {
                      new CraftNode("Equipment", TreeAction.Expand).AddNode(new CraftNode[16]
                      {
                        new CraftNode("Tank", TreeAction.Craft, TechType.Tank),
                        new CraftNode("DoubleTank", TreeAction.Craft, TechType.DoubleTank),
                        new CraftNode("SuitBoosterTank", TreeAction.Craft, TechType.SuitBoosterTank),
                        new CraftNode("Fins", TreeAction.Craft, TechType.Fins),
                        new CraftNode("ReinforcedDiveSuit", TreeAction.Craft, TechType.ReinforcedDiveSuit),
                        new CraftNode("Stillsuit", TreeAction.Craft, TechType.Stillsuit),
                        new CraftNode("ColdSuit", TreeAction.Craft, TechType.ColdSuit),
                        new CraftNode("ColdSuitGloves", TreeAction.Craft, TechType.ColdSuitGloves),
                        new CraftNode("ColdSuitHelmet", TreeAction.Craft, TechType.ColdSuitHelmet),
                        new CraftNode("FirstAidKit", TreeAction.Craft, TechType.FirstAidKit),
                        new CraftNode("Rebreather", TreeAction.Craft, TechType.Rebreather),
                        new CraftNode("Compass", TreeAction.Craft, TechType.Compass),
                        new CraftNode("Pipe", TreeAction.Craft, TechType.Pipe),
                        new CraftNode("PipeSurfaceFloater", TreeAction.Craft, TechType.PipeSurfaceFloater),
                        new CraftNode("FlashlightHelmet", TreeAction.Craft, TechType.FlashlightHelmet),
                        new CraftNode("Coffee", TreeAction.Craft, TechType.Coffee)
                      }),
                      new CraftNode("Tools", TreeAction.Expand).AddNode(new CraftNode[14]
                      {
                        new CraftNode("Scanner", TreeAction.Craft, TechType.Scanner),
                        new CraftNode("Welder", TreeAction.Craft, TechType.Welder),
                        new CraftNode("Flashlight", TreeAction.Craft, TechType.Flashlight),
                        new CraftNode("Knife", TreeAction.Craft, TechType.Knife),
                        new CraftNode("DiveReel", TreeAction.Craft, TechType.DiveReel),
                        new CraftNode("AirBladder", TreeAction.Craft, TechType.AirBladder),
                        new CraftNode("Flare", TreeAction.Craft, TechType.Flare),
                        new CraftNode("Builder", TreeAction.Craft, TechType.Builder),
                        new CraftNode("LaserCutter", TreeAction.Craft, TechType.LaserCutter),
                        new CraftNode("PropulsionCannon", TreeAction.Craft, TechType.PropulsionCannon),
                        new CraftNode("LEDLight", TreeAction.Craft, TechType.LEDLight),
                        new CraftNode("Thumper", TreeAction.Craft, TechType.Thumper),
                        new CraftNode("MetalDetector", TreeAction.Craft, TechType.MetalDetector),
                        new CraftNode("Tether Tool", TreeAction.Craft, TechType.TeleportationTool)
                      })
                    }),
                    new CraftNode("Machines", TreeAction.Expand).AddNode(new CraftNode[10]
                    {
                      new CraftNode("Seaglide", TreeAction.Craft, TechType.Seaglide),
                      new CraftNode("Constructor", TreeAction.Craft, TechType.Constructor),
                      new CraftNode("Beacon", TreeAction.Craft, TechType.Beacon),
                      new CraftNode("SmallStorage", TreeAction.Craft, TechType.SmallStorage),
                      new CraftNode("QuantumLocker", TreeAction.Craft, TechType.QuantumLocker),
                      new CraftNode("Gravsphere", TreeAction.Craft, TechType.Gravsphere),
                      new CraftNode("SpyPenguinRemote", TreeAction.Craft, TechType.SpyPenguinRemote),
                      new CraftNode("SpyPenguin", TreeAction.Craft, TechType.SpyPenguin),
                      new CraftNode("HoverbikeJumpModule", TreeAction.Craft, TechType.HoverbikeJumpModule),
                      new CraftNode("HoverbikeSilentModule", TreeAction.Craft, TechType.HoverbikeIceWormReductionModule)
                    }),
                        });
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(TreeNode), "AddNode", new Type[] { typeof(TreeNode) })]
        class TreeNode_Addnode_Prefix_Patch
        {
            public static bool Prefix(TreeNode __instance, TreeNode node)
            {
                //Main.Log("AddNode " + node.id);
                //AddDebug("AddNode " + node.id);
                if (Main.config.craftVehicleUpgradesOnlyInMoonpool)
                {
                    if (node.id == "Upgrades")
                    {
                        //Main.Log("AddNode Upgrades !!! " + node.id + " parent " + __instance.id);
                        //AddDebug("AddNode Upgrades !!!");
                        return false;
                    }
                    else if (__instance.id == "Root")
                    { // upgrades form senna mods will be added to root if Upgrades node removed from fabricator
                        if ( node.id == "SeaTruckSpeedMK1" || node.id == "SeaTruckSpeedMK2" || node.id == "SeaTruckSpeedMK3" || node.id == "SeaTruckArmorMK1" || node.id == "SeaTruckArmorMK2" || node.id == "SeaTruckArmorMK3" || node.id == "SeaTruckDepthMK4" || node.id == "SeaTruckDepthMK5" || node.id == "SeaTruckDepthMK6")
                            return false;
                    }
                }
                return true;
            }
        }
        
        [HarmonyPatch(typeof(CrafterLogic), "NotifyCraftEnd")]
        class CrafterLogic_NotifyCraftEnd_Patch
        {
            static void Postfix(CrafterLogic __instance, GameObject target, TechType techType)
            {
                //AddDebug("CrafterLogic NotifyCraftEnd timeDecayStart " + timeDecayStart);
                if (Main.config.foodTweaks && timeDecayStart > 0)
                {
                    //AddDebug("CrafterLogic NotifyCraftEnd timeDecayStart" + timeDecayStart);
                    Eatable eatable = target.GetComponent<Eatable>();
                    if (eatable)
                        eatable.timeDecayStart = timeDecayStart;
                }
                Battery battery = target.GetComponent<Battery>();
                if (battery)
                {
                    //AddDebug("crafterOpen");
                    float mult = Main.config.craftedBatteryCharge * .01f;
                    battery._charge = battery._capacity * mult;
                }
                timeDecayStart = 0f;
                crafting = false;
            }
        }

        [HarmonyPatch(typeof(Inventory))]
        class Inventory_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("ConsumeResourcesForRecipe")]
            static void Prefix(Inventory __instance, TechType techType, uGUI_IconNotifier.AnimationDone endFunc = null)
            {
                crafting = true;
                //AddDebug("ConsumeResourcesForRecipe");
            }

            [HarmonyPostfix]
            [HarmonyPatch("OnRemoveItem")]
            static void OnRemoveItemPostfix(Inventory __instance, InventoryItem item)
            {
                //AddDebug("OnRemoveItem " + item.item.GetTechName());
                if (crafting)
                {
                    if (Main.config.foodTweaks && Main.IsEatableFish(item.item.gameObject))
                    {
                        Eatable eatable = item.item.GetComponent<Eatable>();
                        timeDecayStart = eatable.timeDecayStart;
                    }
                    //else
                    //    timeDecayStart = 0f;
                }
            }
        }


    }
}
