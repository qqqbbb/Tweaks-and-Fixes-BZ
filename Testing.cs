
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using UWEXR;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Testing
    {// purpleVent -29 -79 -861
     // crypto -90 -7 -340
        static GameObject previousTarget;
        static List<string> massList = new List<string>();


        //[HarmonyPatch(typeof(SupplyCrate), "Start")]
        class SupplyCrate_Start_Patch
        {
            static void Prefix(SupplyCrate __instance)
            {
                int x = (int)__instance.transform.position.x;
                int y = (int)__instance.transform.position.y;
                int z = (int)__instance.transform.position.z;
                if (x == -1195 && y == 16 && z == -691)
                {
                    //__instance.transform.position
                }
                Main.logger.LogMessage("SupplyCrate Start " + x + " " + y + " " + z);
            }
        }

        //[HarmonyPatch(typeof(ScannerTool), "Scan")]
        class ScannerTool_Scan_Patch
        {
            static bool Prefix(ScannerTool __instance, PDAScanner.Result __result)
            {
                if (__instance.stateCurrent != ScannerTool.ScanState.None || __instance.idleTimer > 0)
                {
                    __result = PDAScanner.Result.None;
                    return false;
                }
                PDAScanner.Result result = PDAScanner.Result.None;
                if (PDAScanner.scanTarget.isValid && __instance.energyMixin.charge > 0.0)
                {
                    result = PDAScanner.Scan();
                    switch (result)
                    {
                        case PDAScanner.Result.Scan:
                            __instance.energyMixin.ConsumeEnergy(__instance.powerConsumption * Time.deltaTime);
                            __instance.stateCurrent = ScannerTool.ScanState.Scan;
                            break;
                        case PDAScanner.Result.Done:
                        case PDAScanner.Result.Researched:
                            __instance.UpdateScreen(ScannerTool.ScreenState.Default);
                            __instance.idleTimer = 0.5f;
                            if (!PDASounds.queue.HasQueued())
                                PDASounds.queue.Play(__instance.completeSound, SoundHost.PDA);

                            AddDebug("scan");
                            //if (__instance.fxControl != null)
                            //{
                            //    __instance.fxControl.Play(0);
                            //    break;
                            //}
                            break;
                    }
                }
                __result = result;
                return false;
            }
        }

        //[HarmonyPatch(typeof(Player), "Update")]
        class Player_Update_Patch
        {
            static void Postfix(Player __instance)
            {
                //AddDebug("IsPlayerInVehicle " + Util.IsPlayerInVehicle());
                //if (Player.main.currentInterior != null)
                //{
                //    AddDebug("currentInterior " + __instance.currentInterior.ToString());
                //    AddDebug("currentInterior name " + __instance.currentInterior.GetGameObject().name);
                //}
                if (__instance.currentInterior != null)
                {
                    //AddDebug("Player currentInterior GetInsideTemperature " +  __instance.currentInterior.GetInsideTemperature());
                    //AddDebug("Player currentInterior Type " + __instance.currentInterior.GetType());
                    //AddDebug("Player_Patches ambientTemperature " + Player_Patches.ambientTemperature);
                }
                //AddDebug("ambientTemperature " + (int)Player_Patches.ambientTemperature);
                //BodyTemperature bt = __instance.GetComponent<BodyTemperature>();
                //if (bt)
                {
                    //AddDebug("isExposed " + bt.isExposed);
                    //AddDebug("CalculateEffectiveAmbientTemperature " + bt.CalculateEffectiveAmbientTemperature());
                    //int temp = (int)bt.CalculateEffectiveAmbientTemperature();
                    //if (temp < 0f)
                    //    AddDebug("CalculateEffectiveAmbientTemperature " + temp);
                    //AddDebug("GetWaterTemperature " + (int)Main.bodyTemperature.GetWaterTemperature());
                }
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
                    AddDebug(" " + LargeWorld.main.GetBiome(__instance.transform.position));
                    //Inventory.main.container.Resize(8,8);   GetPlayerBiome()
                    //HandReticle.main.SetInteractText(nameof(startingFood) + " " + dict[i]);
                }

                else if (Input.GetKeyDown(KeyCode.C))
                {
                    PlayerTool tool = Inventory.main.GetHeldTool();
                    AddDebug("bloodColor " + Damage_Patch.bloodColor);
                    //PrintTerrainSurfaceType();
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
                else if (Input.GetKeyDown(KeyCode.V))
                {
                    printTarget();
                    //Survival survival = Player.main.GetComponent<Survival>();
                    //if (Input.GetKey(KeyCode.LeftShift))
                    //    __instance.liveMixin.health--;
                    //else
                    //    __instance.liveMixin.health++;
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
                    GameObject target = Player.main.guiHand.activeTarget;
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        if (previousTarget)
                        {
                            AddDebug("enable " + previousTarget.name);
                            previousTarget.SetActive(true);
                            previousTarget = null;
                            return;
                        }
                    }
                    //AddDebug(" Base_Light.bases " +);
                    //Inventory.main.quickSlots.SelectImmediate(Main.config.activeSlot);
                    //AddDebug("activeTarget parent " + target.transform.parent.name);
                    //AddDebug("activeTarget " + target.name);
                    if (!target)
                    {
                        Targeting.GetTarget(Player.main.gameObject, 5f, out target, out float targetDist);
                    }
                    if (target)
                    {

                        PrefabIdentifier pi = target.GetComponentInParent<PrefabIdentifier>();
                        if (pi)
                            target = pi.gameObject;

                        if (Input.GetKey(KeyCode.LeftShift))
                        {
                            AddDebug("disable target");
                            target.SetActive(false);
                            previousTarget = target;
                        }
                        //AddDebug("IsVehicle " + Predators_Patch.IsVehicle(target));
                        AddDebug("target " + target.name);
                        AddDebug("target TechType " + CraftData.GetTechType(target));
                        VFXSurface surface = target.GetComponent<VFXSurface>();
                        if (surface)
                            AddDebug("target surface " + surface.surfaceType);

                        VFXSurface surface1 = target.GetComponentInChildren<VFXSurface>();
                        if (surface1)
                            AddDebug("target child surface " + surface1.surfaceType);

                        int x = (int)pi.transform.position.x;
                        int y = (int)pi.transform.position.y;
                        int z = (int)pi.transform.position.z;
                        //AddDebug(x + " " + y + " " + z);
                    }
                    if (Input.GetAxis("Mouse ScrollWheel") > 0f)
                    {
                    }
                    else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
                    {
                    }
                }
                else if (Input.GetKeyDown(KeyCode.D))
                {


                }
            }

            public static void printTarget()
            {
                GameObject target = Player.main.guiHand.activeTarget;
                //if (Player.main.guiHand.activeTarget)
                //    AddDebug("activeTarget " + Player.main.guiHand.activeTarget);

                RaycastHit hitInfo = new RaycastHit();
                if (!target)
                    Util.GetPlayerTarget(111f, out hitInfo, true);
                //Targeting.GetTarget(Player.main.gameObject, 11f, out target, out float targetDist);
                if (hitInfo.collider)
                    target = hitInfo.collider.gameObject;

                if (!target)
                    return;
                //AddDebug("target " + target.name);
                VFXSurfaceTypes vfxSurfaceType = VFXSurfaceTypes.none;
                TerrainChunkPieceCollider tcpc = target.GetComponent<TerrainChunkPieceCollider>();
                if (tcpc)
                {
                    vfxSurfaceType = Utils.GetTerrainSurfaceType(hitInfo.point, hitInfo.normal);
                    AddDebug("Terrain surface type  " + vfxSurfaceType);
                    return;
                }
                if (target)
                    vfxSurfaceType = Util.GetObjectSurfaceType(target);

                LargeWorldEntity lwe = target.GetComponentInParent<LargeWorldEntity>();
                if (lwe)
                {
                    target = lwe.gameObject;
                    int posX = (int)lwe.transform.position.x;
                    int posY = (int)lwe.transform.position.y;
                    int posZ = (int)lwe.transform.position.z;
                    AddDebug(" position " + posX + " " + posY + " " + posZ);
                    //AddDebug(" cellLevel " + lwe.cellLevel);
                    if (vfxSurfaceType != VFXSurfaceTypes.none)
                        AddDebug("vfxSurfaceType  " + vfxSurfaceType);

                    LiveMixin lm = lwe.GetComponent<LiveMixin>();
                    if (lm)
                        AddDebug("max HP " + lm.data.maxHealth + " HP " + lm.health);
                }
                AddDebug(target.gameObject.name);
                //AddDebug("parent " + target.transform.parent.gameObject.name);
                //if (target.transform.parent.parent)
                //    AddDebug("parent parent " + target.transform.parent.parent.gameObject.name);
                TechType techType = CraftData.GetTechType(target);
                if (techType != TechType.None)
                    AddDebug("TechType  " + techType);

                HarvestType harvestType = TechData.GetHarvestType(techType);
                if (harvestType != HarvestType.None)
                    AddDebug("harvestType  " + harvestType);
            }

            static void PrintTerrainSurfaceType()
            {
                VFXSurfaceTypes vfxSurfaceTypes = VFXSurfaceTypes.none;
                int layerMask = 1 << LayerID.TerrainCollider | 1 << LayerID.Default;
                RaycastHit hitInfo;
                if (Physics.Raycast(MainCamera.camera.transform.position, MainCamera.camera.transform.forward, out hitInfo, 111f, layerMask) && hitInfo.collider.gameObject.layer == LayerID.TerrainCollider)
                    vfxSurfaceTypes = Utils.GetTerrainSurfaceType(hitInfo.point, hitInfo.normal);
                AddDebug("vfxSurfaceTypes " + vfxSurfaceTypes);
            }
        }

        //[HarmonyPatch(typeof(DamageVolume))]
        class SupplyCrate_Patch
        {
            //[HarmonyPrefix]
            //[HarmonyPatch("DealDamage")]
            static bool StopAttackPostfix(DamageVolume __instance, int multiplier)
            {
                //AddDebug(__instance.name + " DamageVolume DealDamage ");
                if (multiplier <= 0)
                    return false;

                Vector3 position = __instance.tr.position;
                Matrix4x4 worldToLocalMatrix = __instance.tr.worldToLocalMatrix;
                int num = UWE.Utils.OverlapSphereIntoSharedBuffer(position, DamageVolume.GetMaxRadius(__instance.tr, __instance.radius));
                for (int index = 0; index < num; ++index)
                {
                    GameObject gameObject = UWE.Utils.sharedColliderBuffer[index].gameObject;
                    if (gameObject != __instance.gameObject)
                    {
                        LiveMixin lm = Utils.FindAncestorWithComponent<LiveMixin>(gameObject);
                        if (lm != null && DamageVolume.liveMixins.Add(lm))
                        {
                            float damageScalar = DamageVolume.GetDamageScalar(worldToLocalMatrix, __instance.radius, __instance.hemisphere, lm.transform.position);
                            lm.TakeDamage(__instance.maxDamage * damageScalar * multiplier, position, __instance.type);
                            AddDebug(__instance.name + " DamageVolume Deal " + __instance.type + " Damage to " + lm.name);
                        }
                    }
                }
                DamageVolume.liveMixins.Clear();
                return false;
            }
        }

        //[HarmonyPatch(typeof(AtmosphereVolume))]
        class AtmosphereVolume_Patch
        {
            //[HarmonyPrefix]
            //[HarmonyPatch("OnTriggerEnter")]
            static void OnTriggerEnterPostfix(AtmosphereVolume __instance, Collider c)
            {
                GameObject myRoot = Util.GetEntityRoot(__instance.gameObject);
                GameObject root = Util.GetEntityRoot(c.gameObject);
                int posX = (int)myRoot.transform.position.x;
                int posY = (int)myRoot.transform.position.y;
                int posZ = (int)myRoot.transform.position.z;
                //if (posX == -25 && posY == -82 && posZ == -859)
                if (c.gameObject == Player.mainObject)
                    //AddDebug(myRoot.name + " AtmosphereVolume OnTriggerEnter " + root.name);
                    AddDebug(__instance.name + " AtmosphereVolume OnTriggerEnter ");
                //Util.Log(myRoot.name + " AtmosphereVolume OnTriggerEnter " + root.name);
            }
            //[HarmonyPrefix]
            //[HarmonyPatch("OnTriggerExit")]
            static void OnTriggerExitPostfix(AtmosphereVolume __instance, Collider c)
            {
                GameObject myRoot = Util.GetEntityRoot(__instance.gameObject);
                GameObject root = Util.GetEntityRoot(c.gameObject);
                int posX = (int)myRoot.transform.position.x;
                int posY = (int)myRoot.transform.position.y;
                int posZ = (int)myRoot.transform.position.z;
                //if (posX == -25 && posY == -82 && posZ == -859)
                if (c.gameObject == Player.mainObject)
                    //AddDebug(myRoot.name + " AtmosphereVolume OnTriggerExit " + root.name);
                    AddDebug(__instance.name + " AtmosphereVolume OnTriggerExit ");
            }
        }

        //[HarmonyPatch(typeof(CSVEntitySpawner))]
        class CSVEntitySpawner_Patch
        {
            //[HarmonyPrefix]
            //[HarmonyPatch("GetPrefabForSlot")]
            static bool GetPrefabForSlotPrefix(CSVEntitySpawner __instance)
            {
                AddDebug("CSVEntitySpawner GetPrefabForSlot");
                return true;
            }
            //[HarmonyPostfix]
            //[HarmonyPatch("GetModifiedCreatureSpawnCount")]
            static void GetModifiedCreatureSpawnCountPrefix(CSVEntitySpawner __instance)
            {
                AddDebug("CSVEntitySpawner GetModifiedCreatureSpawnCount");
                //return true;
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

        //[HarmonyPatch(typeof(SoundQueue), "Play", new Type[6] { typeof(string), typeof(SoundHost), typeof(bool), typeof(string), typeof(int), typeof})]
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
                //Util.Log("ApplySkybox " + __instance.name + " " + __instance.transform.position.x);
                for (int index = 0; index < __instance.renderers.Length; ++index)
                {
                    Renderer renderer = __instance.renderers[index];
                    if (renderer)
                    {
                        sky.ApplyFast(renderer, 0);
                        //Util.Log("ApplyFast " + __instance.name + " " + __instance.transform.position.x);
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
                    //if(__instance.customSkyPrefab)
                    //Util.Log("OnEnvironmentChanged " + __instance.name + " " + __instance.transform.position.x + " " + __instance.customSkyPrefab.name);
                    //Main.Log("OnEnvironmentChanged " + __instance.name + " " + __instance.anchorSky);
                }
                if (__instance.emissiveFromPower)
                {
                    __instance.cellLighting = __instance.GetComponentInParent<BaseCellLighting>();
                    if (__instance.cellLighting)
                    {
                        //Util.Log("RegisterSkyApplier " + __instance.name + " " + __instance.transform.position.x);
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

        static IEnumerator PrintMass(TechType techType)
        {
            CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(techType, false);
            yield return request;
            GameObject go = request.GetResult();
            if (go)
            {
                Rigidbody rb = go.GetComponent<Rigidbody>();
                if (rb)
                {
                    string name = Language.main.Get(techType);
                    string s = techType + ", " + name + ", mass " + rb.mass;
                    massList.Add(s);
                }
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
