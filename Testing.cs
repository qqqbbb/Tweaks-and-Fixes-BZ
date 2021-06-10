
using HarmonyLib;
using QModManager.API.ModLoading;
using System.Reflection;
using System;
using SMLHelper.V2.Handlers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using System.Text;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Testing
    {
        private Vector3 ClipWithTerrain(GameObject go)
        {
            Vector3 origin = go.transform.position;
            //origin.y = go.transform.position.y + 5f;
            //RaycastHit hitInfo;
            //if (!Physics.Raycast(new Ray(origin, Vector3.down), out hitInfo, 10f, Voxeland.GetTerrainLayerMask(), QueryTriggerInteraction.Ignore))
            //    return;
            //go.transform.position.y = Mathf.Max(go.transform.position.y, hitInfo.point.y + 0.3f);
            return origin;
        }

        //[HarmonyPatch(typeof(Player), "Update")]
        class Player_Update_Patch
        {
            static void Postfix(Player __instance)
            {
                //BodyTemperature bt = __instance.GetComponent<BodyTemperature>();
                //if (bt)
                //{
                //    float effectiveAmbientTemperature = bt.CalculateEffectiveAmbientTemperature();
                //    AddDebug("Effective Ambient Temp " + (int)effectiveAmbientTemperature);
                //    AddDebug("isExposed " + bt.isExposed);
                //}

                //AddDebug("heldItem " + Inventory.main.quickSlots.heldItem.ToString());
                //if (Player.main.currentMountedVehicle)
                //    AddDebug("currentMountedVehicle " + Player.main.currentMountedVehicle);
                //if (Player.main._currentInterior != null) 
                //    AddDebug("_currentInterior " + Player.main._currentInterior);
                //if (Player.main._currentSub)
                //    AddDebug("_currentSub " + Player.main._currentSub);
                //if (Player.main.inExosuit)
                //    AddDebug("inExosuit ");
                //float movementSpeed = (float)System.Math.Round(__instance.movementSpeed * 10f) / 10f;
                if (Input.GetKey(KeyCode.B))
                {
                    //AddDebug("currentSlot " + Main.config.escapePodSmokeOut[SaveLoadManager.main.currentSlot]);
                    //if (Player.main.IsInBase())
                    //    AddDebug("IsInBase");
                    //else if (Player.main.IsInSubmarine())
                    //    AddDebug("IsInSubmarine");
                    //else if (Player.main.inExosuit)
                    //    AddDebug("GetInMechMode");
                    //else if (Player.main.inSeamoth)
                    //    AddDebug("inSeamoth");
                    int x = Mathf.RoundToInt(Player.main.transform.position.x);
                    int y = Mathf.RoundToInt(Player.main.transform.position.y);
                    int z = Mathf.RoundToInt(Player.main.transform.position.z);
                    AddDebug(x + " " + y + " " + z);
                    AddDebug("" + Player.main.GetBiomeString());
                    //Inventory.main.container.Resize(8,8);   GetPlayerBiome()
                    //HandReticle.main.SetInteractText(nameof(startingFood) + " " + dict[i]);
                }

                if (Input.GetKey(KeyCode.C))
                {
                    AddDebug("SeaTruckUpgrades.slotIDs.Length " + SeaTruckUpgrades.slotIDs.Length);
                    //TechType tt = TechType.IceBubble;
                    //string classid = CraftData.GetClassIdForTechType(tt);
                    //CoroutineTask<GameObject> result = AddressablesUtility.InstantiateAsync("PrefabInstance/Bubble", position: Player.main.transform.position);
                    //GameObject bubble = result.GetResult();
                    //if (bubble)
                    //{
                    //    AddDebug("BUBBLE");
                    //}

                    //if (UWE.PrefabDatabase.TryGetPrefabFilename(classid, out string filename))
                    {
                        //AddDebug("spawn IceBubble");
                        //TaskResult<GameObject> taskResult = new TaskResult<GameObject>();
                        //CoroutineTask <GameObject> result = AddressablesUtility.InstantiateAsync(filename, position: Player.main.transform.position);
                        //AddressablesUtility.InstantiateAsync(filename, (IOut<GameObject>)taskResult, position: Vector3.zero, rotation: Quaternion.identity, awake: false);
                        //GameObject go = result.GetResult();
                    }
                    //Survival survival = Player.main.GetComponent<Survival>();

                    //if (Input.GetKey(KeyCode.LeftShift))
                    //    survival.water++;
                    //else
                    //    survival.food++;
                }

                if (Input.GetKey(KeyCode.X))
                {
                    //Survival survival = Player.main.GetComponent<Survival>();
                    //if (Input.GetKey(KeyCode.LeftShift))
                    //    __instance.liveMixin.health--;
                    //else
                    //    __instance.liveMixin.health++;
                }

                if (Input.GetKey(KeyCode.Z))
                {
                    //AddDebug(" Base_Light.bases " +);
                    //Inventory.main.quickSlots.SelectImmediate(Main.config.activeSlot);
                    Targeting.GetTarget(Player.main.gameObject, 5f, out GameObject target, out float targetDist);
                    if (target)
                    {


                    }
                    if (Main.guiHand.activeTarget)
                    {
                        //VFXSurface[] vFXSurfaces = __instance.GetAllComponentsInChildren<VFXSurface>();
                        //if (vFXSurfaces.Length == 0)
                        //    AddDebug(" " + Main.guiHand.activeTarget.name + " no VFXSurface");
                        //else
                        //    AddDebug(" " + Main.guiHand.activeTarget.name);

                        //AddDebug("TechType " + CraftData.GetTechType(Main.guiHand.activeTarget));
                    }
                    if (Input.GetAxis("Mouse ScrollWheel") > 0f)
                    {
                    }
                    else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
                    {
                    }

                    //else

                    //Inventory.main.DropHeldItem(true);
                    //Player.main.liveMixin.TakeDamage(99);
                    //Pickupable held = Inventory.main.GetHeld();
                    //AddDebug("isUnderwaterForSwimming " + Player.main.isUnderwaterForSwimming.value);
                    //AddDebug("isUnderwater " + Player.main.isUnderwater.value);
                    //LaserCutObject laserCutObject = 
                    //Inventory.main.quickSlots.Select(1);

                    if (Main.guiHand.activeTarget)
                    {
                        //AddDebug("activeTarget " + Main.guiHand.activeTarget.name);
                        //AddDebug(" " + CraftData.GetTechType(Main.guiHand.activeTarget));
                        //RadiatePlayerInRange radiatePlayerInRange = Main.guiHand.activeTarget.GetComponent<RadiatePlayerInRange>();
                        //if (radiatePlayerInRange)
                        {

                        }
                        //else
                        //    AddDebug("no radiatePlayerInRange " );

                    }
                    //if (target)
                    //    Main.Message(" target " + target.name);
                    //else
                    //{
                    //TechType techType = CraftData.GetTechType(target);
                    //HarvestType harvestTypeFromTech = CraftData.GetHarvestTypeFromTech(techType);
                    //TechType harvest = CraftData.GetHarvestOutputData(techType);
                    //Main.Message("techType " + techType.AsString() );
                    //Main.Message("name " + target.name);
                    //}
                }
            }
        }

        //[HarmonyPatch(typeof(uGUI_InventoryTab), "OnPointerClick")]
        class uGUI_InventoryTab_OnPointerClick_Patch
        {
            public static void Prefix(InventoryItem item, int button)
            {
                AddDebug("OnPointerClick " + button);
            }
        }
    }
}
