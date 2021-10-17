
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
        //static HashSet<SeaTruckSegment> segments = new HashSet<SeaTruckSegment>();
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

        //[HarmonyPatch(typeof(uGUI_QuickSlots), "OnSelect")]
        class uGUI_QuickSlots_OnSelect_Patch
        {
            static bool Prefix(uGUI_QuickSlots __instance, int slotID)
            {
                //AddDebug("uGUI_QuickSlots OnSelect");
                if (__instance.selector == null)
                    AddDebug("selector == null");
                if (__instance.target == null || __instance.selector == null)
                    return false;
                if (slotID < 0)
                {
                    __instance.selector.enabled = false;
                }
                else
                {
                    if (__instance.selector.rectTransform == null)
                    {
                        AddDebug("selector.rectTransform == null");
                        return false;
                    }
                    __instance.selector.rectTransform.anchoredPosition = __instance.GetPosition(slotID);
                    __instance.selector.enabled = true;
                }
                return false;
            }
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
                //bool inTruck = Player.main._currentInterior != null && Player.main._currentInterior is SeaTruckSegment;
                //AddDebug("inTruck " + inTruck);
                //AddDebug("IsPilotingSeatruck " + Player.main.IsPilotingSeatruck());
                //AddDebug("inExosuit " + Player.main.inExosuit);
                if (Player.main._currentInterior != null && Player.main._currentInterior is SeaTruckSegment)
                {
                    //SeaTruckSegment sts = (SeaTruckSegment)Player.main._currentInterior;
                    //Rigidbody rb = sts.GetComponent<Rigidbody>();
                    //if (rb)
                    //{
                    //    AddDebug("vel " + rb.velocity.x);
                    //}
                }
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
                    GameObject target = Main.guiHand.activeTarget;
                    //AddDebug("activeTarget parent " + target.transform.parent.name);
                    //AddDebug("activeTarget parent parent " + target.transform.parent.parent.name);
                    if (!target)
                    { 
                        Targeting.GetTarget(Player.main.gameObject, 5f, out target, out float targetDist);
                    }
                    if (target)
                    {
                        PrefabIdentifier pi = target.GetComponentInParent<PrefabIdentifier>();
                        if (pi)
                        {
                            AddDebug("target " + pi.gameObject.name);
                            AddDebug("target TechType " + CraftData.GetTechType(pi.gameObject));
                            int x = (int)pi.transform.position.x;
                            int y = (int)pi.transform.position.y;
                            int z = (int)pi.transform.position.z;
                            AddDebug(x + " " + y + " " + z);
                        }
                        else
                        {
                            AddDebug("target " + target.name);
                            AddDebug("target TechType " + CraftData.GetTechType(target));
                        }
                    }
                    if (Input.GetAxis("Mouse ScrollWheel") > 0f)
                    {
                    }
                    else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
                    {
                    }
                }
            }
        }

        //[HarmonyPatch(typeof(SkyApplier))]
        internal class SkyApplier_Patch
        {
            //[HarmonyPrefix]
            //[HarmonyPatch("ApplySkybox")]
            public static bool ApplySkyboxPrefix(SkyApplier __instance)
            {
                TechType tt = CraftData.GetTechType(__instance.gameObject);
                if (tt != TechType.ThermalPlantFragment)
                    return true;
                //Main.Log("ApplySkybox " + __instance.name + " " + __instance.transform.position.x);
                __instance.applyPosition = __instance.transform.position;
                mset.Sky sky = __instance.environmentSky;
                if (sky == null)
                {
                    sky = WaterBiomeManager.main.GetBiomeEnvironment(__instance.applyPosition);
                    //Main.Log("GetBiomeEnvironment " + __instance.name + " " + __instance.transform.position.x);
                }
                if (sky == __instance.skyApplied)
                    return false;

                if (__instance.skyApplied != null)
                {
                    __instance.skyApplied.UnregisterSkyApplier(__instance);
                    //Main.Log("UnregisterSkyApplier " + __instance.name + " " + __instance.transform.position.x);
                }
                __instance.skyApplied = sky;
                if (sky == null)
                    return false;
                Main.Log("ApplySkybox " + __instance.name + " " + __instance.transform.position.x);
                for (int index = 0; index < __instance.renderers.Length; ++index)
                {
                    Renderer renderer = __instance.renderers[index];
                    if (renderer)
                    {
                        sky.ApplyFast(renderer, 0);
                        Main.Log("ApplyFast " + __instance.name + " " + __instance.transform.position.x);
                    }
                }
                sky.RegisterSkyApplier(__instance);
                return false;
            }
            //[HarmonyPrefix]
            //[HarmonyPatch("Initialize")]
            public static bool InitializePrefix(SkyApplier __instance)
            {
                TechType tt = CraftData.GetTechType(__instance.gameObject);
                if (tt != TechType.ThermalPlantFragment)
                    return true;

                if (!SkyApplier.IsWorldReady())
                    return false;

                //AddDebug("Initialize " + __instance.name + " " + __instance.transform.position.x);
                if (!__instance.environmentSky)
                {
                    __instance.OnEnvironmentChanged(SkyApplier.GetEnvironment(__instance.gameObject, __instance.anchorSky));
                    if(__instance.customSkyPrefab)
                    Main.Log("OnEnvironmentChanged " + __instance.name + " " + __instance.transform.position.x + " " + __instance.customSkyPrefab.name);
                    //Main.Log("OnEnvironmentChanged " + __instance.name + " " + __instance.anchorSky);
                }
                if (__instance.emissiveFromPower)
                {
                    __instance.cellLighting = __instance.GetComponentInParent<BaseCellLighting>();
                    if (__instance.cellLighting)
                    {
                        Main.Log("RegisterSkyApplier " + __instance.name + " " + __instance.transform.position.x);
                        __instance.cellLighting.RegisterSkyApplier(__instance);
                    }
                }
                if (!__instance.dynamic)
                    __instance.enabled = false;
                __instance.initialized = true;
                return false;
            }
            //[HarmonyPostfix]
            //[HarmonyPatch("Initialize")]
            public static void InitializePostfix(SkyApplier __instance)
            {
                //AddDebug("Initialize Postfix " + __instance.name);
                //__instance.OnEnvironmentChanged(null);
                //__instance.OnEnvironmentChanged(__instance.sky);
                //__instance.OnEnvironmentChanged(SkyApplier.GetEnvironment(__instance.gameObject, __instance.anchorSky));
                //__instance.ApplySkybox();
            }
        }
    }
}
