
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
    {// -29 -79 -861

        //[HarmonyPatch(typeof(uGUI_PDA), "OnOpenPDA")]
        class uGUI_PDA_Patch
        {
            //[HarmonyPrefix]
            //[HarmonyPatch("Update")]
            static bool Prefix(uGUI_PDA __instance)
            {
                AddDebug("uGUI_PDA OnOpenPDA");
                return false;
            }
        }

        //[HarmonyPatch(typeof(Player), "Update")]
        class Player_Update_Patch
        {
            static void Postfix(Player __instance)
            {
                //AddDebug("_dayNightSpeed " + DayNightCycle.main._dayNightSpeed);
                BodyTemperature bt = __instance.GetComponent<BodyTemperature>();
                if (bt)
                {
                    //AddDebug("isExposed " + bt.isExposed);
                    //int temp = (int)bt.CalculateEffectiveAmbientTemperature();
                    //if (temp < 0f)
                    //    AddDebug("CalculateEffectiveAmbientTemperature " + temp);
                    //AddDebug("GetWaterTemperature " + (int)Main.bodyTemperature.GetWaterTemperature());
                }
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
                if (Input.GetKeyDown(KeyCode.B))
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

                else if (Input.GetKeyDown(KeyCode.C))
                {
                    //VFXSurfaceTypes vfxSurfaceTypes = VFXSurfaceTypes.none;
                    //int layerMask = 1 << LayerID.TerrainCollider | 1 << LayerID.Default;
                    //RaycastHit hitInfo;
                    //if (Physics.Raycast(MainCamera.camera.transform.position, MainCamera.camera.transform.forward, out hitInfo, 3f, layerMask) && hitInfo.collider.gameObject.layer == LayerID.TerrainCollider)
                    //    vfxSurfaceTypes = Utils.GetTerrainSurfaceType(hitInfo.point, hitInfo.normal);
                    //AddDebug("vfxSurfaceTypes " + vfxSurfaceTypes);

                    //TechType tt = TechType.IceBubble;
                    //string classid = CraftData.GetClassIdForTechType(tt);
                    //CoroutineTask<GameObject> result = AddressablesUtility.InstantiateAsync("PrefabInstance/Bubble", position: Player.main.transform.position);
                    //GameObject bubble = result.GetResult();
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

                else if (Input.GetKeyDown(KeyCode.X))
                {
                    //Survival survival = Player.main.GetComponent<Survival>();
                    //if (Input.GetKey(KeyCode.LeftShift))
                    //    __instance.liveMixin.health--;
                    //else
                    //    __instance.liveMixin.health++;
                }

                else if (Input.GetKeyDown(KeyCode.Z))
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
                            Brinicle brinicle = pi.GetComponent<Brinicle>();
                            if (brinicle)
                                AddDebug("Brinicle " + brinicle.state );
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
                    __instance.selector.enabled = false;
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

        //[HarmonyPatch(typeof(Plantable), "ValidateTechType")]
        class Plantable_Play_Patch
        {
            public static void Postfix(Plantable __instance)
            {
                AddDebug("Plantable ValidateTechType " + __instance.plantTechType);
                //if (!Main.loadingDone)
                //    return false;

                //return true;
            }
        }

        //[HarmonyPatch(typeof(SoundQueue), "Play", new Type[6] { typeof(string), typeof(SoundHost), typeof(bool), typeof(string), typeof(int), typeof(int) })]
        class SoundQueue_PlayQueued_Patch
        {
            public static bool Prefix(SoundQueue __instance, string sound)
            {
                AddDebug(" PlayQueued  " + sound);
                //if (!Main.loadingDone)
                //    return false;

                return true;
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

        //[HarmonyPatch(typeof(Targeting), "GetTarget", new Type[] { typeof(float), typeof(GameObject), typeof(float), typeof(Targeting.FilterRaycast) }, new[] { ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out, ArgumentType.Normal })]
        class Targeting_GetTarget_PostfixPatch
        {
            public static void Postfix(ref GameObject result)
            {
                //AddDebug(" Targeting GetTarget  " + result.name);
            }
        }


    }
}
