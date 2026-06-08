using BepInEx;
using HarmonyLib;
using Nautilus.Handlers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UWE;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal static class Util
    {

        public static IEnumerator SpawnAsync(TechType techType, Vector3 pos = default, bool fadeIn = false)
        {
            //AddDebug("Spawn " + techType + " " + pos);
            GameObject prefab;
            TaskResult<GameObject> result = new TaskResult<GameObject>();
            yield return CraftData.GetPrefabForTechTypeAsync(techType, false, result);
            prefab = result.Get();
            if (!fadeIn)
                LargeWorldEntity_.spawningNearPlayer = true;

            GameObject go = prefab == null ? Utils.CreateGenericLoot(techType) : Utils.SpawnFromPrefab(prefab, null);
            if (go != null)
            {
                if (pos == default)
                {
                    Transform camTr = MainCamera.camera.transform;
                    go.transform.position = camTr.position + camTr.forward * 3f;
                }
                else
                    go.transform.position = pos;
                //AddDebug("Spawn " + techType + " " + pos);
                //AttachPing(go);
                CrafterLogic.NotifyCraftEnd(go, techType);
            }
            LargeWorldEntity_.spawningNearPlayer = false;
        }

        public static IEnumerator AddToContainerAsync(TechType techType, ItemsContainer container, bool pickupSound)
        {
            TaskResult<GameObject> prefabResult = new TaskResult<GameObject>();
            yield return CraftData.InstantiateFromPrefabAsync(techType, prefabResult);
            GameObject gameObject = prefabResult.Get();
            if (gameObject == null)
                yield break;

            Pickupable pickupable = gameObject.GetComponent<Pickupable>();
            if (!pickupable)
            {
                UnityEngine.Object.Destroy(gameObject);
                yield break;
            }
            if (!container.HasRoomFor(pickupable))
            {
                UnityEngine.Object.Destroy(gameObject);
                yield break;
            }
            pickupable.Initialize();
            if (pickupSound)
                pickupable.PlayPickupSound();

            InventoryItem item = new InventoryItem(pickupable);
            container.UnsafeAdd(item);
        }

        public static bool CanPlayerEat()
        {
            bool canEat = GameModeManager.GetOption<bool>(GameOption.Hunger) || GameModeManager.GetOption<bool>(GameOption.Thirst);
            bool cantEat = ConfigMenu.cantEatUnderwater.Value && Player.main.isUnderwater.value;
            return canEat && !cantEat;
        }

        static public IEnumerator AddToInventory(TechType techType)
        {
            GameObject gameObject = null;
            //AddDebug("AddToInventory " + techType);
            TaskResult<GameObject> result = new TaskResult<GameObject>();
            yield return CraftData.InstantiateFromPrefabAsync(techType, (IOut<GameObject>)result);
            gameObject = result.Get();
            if (gameObject != null)
            {
                //addedToInv = gameObject;
                //Eatable eatable = gameObject.GetComponent<Eatable>();
                //if (eatable != null)
                //    eatable.SetDecomposes(true); gameObject.EnsureComponent<EcoTarget>().SetTargetType(EcoTargetType.DeadMeat);
                Pickupable pickupable = gameObject.GetComponent<Pickupable>();
                if (pickupable)
                {
                    Inventory.main.ForcePickup(pickupable);
                    //AddDebug("ForcePickup " + pickupable.name);
                }
            }
        }

        public static void CookFish(GameObject go)
        {
            //int currentSlot = Inventory.main.quickSlots.desiredSlot;
            //AddDebug("currentSlot " + currentSlot);
            Inventory.main.quickSlots.DeselectImmediate();
            TechType processed = TechData.GetProcessed(CraftData.GetTechType(go));
            if (processed != TechType.None)
            { // cooked fish cant be in quickslot
              //AddDebug("CookFish " + processed);
                CraftData.AddToInventory(processed);
                UnityEngine.Object.Destroy(go);
            }
        }

        public static void AddVFXsurfaceComponent(this GameObject go, VFXSurfaceTypes type)
        {
            VFXSurface vFXSurface = go.GetComponentInChildren<VFXSurface>();
            if (vFXSurface == null)
                vFXSurface = go.AddComponent<VFXSurface>();

            vFXSurface.surfaceType = type;
        }

        public static bool IsWater(Eatable eatable)
        {
            return eatable.waterValue > 0f && eatable.foodValue <= 0f && eatable.TryGetComponent<SnowBall>(out _) == false;
        }

        public static bool IsFood(Eatable eatable)
        {
            return eatable.foodValue > 0f;
        }

        public static void FreezeObject(GameObject go, bool state)
        {
            WorldForces wf = go.GetComponent<WorldForces>();
            Rigidbody rb = go.GetComponent<Rigidbody>();
            if (wf && rb)
            {
                wf.enabled = !state;
                rb.isKinematic = state;
            }
        }

        public static Component CopyComponent(Component original, GameObject destination)
        {
            System.Type type = original.GetType();
            Component copy = destination.AddComponent(type);
            // Copied fields can be restricted with BindingFlags
            System.Reflection.FieldInfo[] fields = type.GetFields();
            foreach (System.Reflection.FieldInfo field in fields)
            {
                field.SetValue(copy, field.GetValue(original));
            }
            return copy;
        }

        public static T CopyComponent<T>(T original, GameObject destination) where T : Component
        {
            System.Type type = original.GetType();
            var dst = destination.GetComponent(type) as T;
            if (!dst) dst = destination.AddComponent(type) as T;
            var fields = type.GetFields();
            foreach (var field in fields)
            {
                if (field.IsStatic) continue;
                field.SetValue(dst, field.GetValue(original));
            }
            var props = type.GetProperties();
            foreach (var prop in props)
            {
                if (!prop.CanWrite || !prop.CanWrite || prop.Name == "name") continue;
                prop.SetValue(dst, prop.GetValue(original, null), null);
            }
            return dst as T;
        }

        public static T[] GetComponentsInDirectChildren<T>(Component parent, bool includeInactive = false) where T : Component
        {
            List<T> tmpList = new List<T>();
            foreach (Transform transform in parent.transform)
            {
                if (includeInactive || transform.gameObject.activeInHierarchy)
                    tmpList.AddRange(transform.GetComponents<T>());
            }
            return tmpList.ToArray();
        }

        public static T[] GetComponentsInDirectChildren<T>(GameObject parent, bool includeInactive = false) where T : Component
        {
            List<T> tmpList = new List<T>();
            foreach (Transform transform in parent.transform)
            {
                if (includeInactive || transform.gameObject.activeInHierarchy)
                {
                    T[] components = transform.GetComponents<T>();
                    if (components.Length > 0)
                        tmpList.AddRange(components);
                }
            }
            return tmpList.ToArray();
        }

        public static GameObject GetEntityRoot(GameObject go)
        {
            UniqueIdentifier ui = go.GetComponentInParent<UniqueIdentifier>();
            return ui != null ? ui.gameObject : null;
        }

        public static float GetPlayerTemperature()
        {
            //AddDebug("GetPlayerTemperature ");
            //IInteriorSpace currentInterior = Player.main.GetComponentInParent<IInteriorSpace>();
            //if (currentInterior != null)
            //    return currentInterior.GetInsideTemperature();
            if (Player.main.currentMountedVehicle)
            {
                if (ConfigMenu.useRealTempForPlayer.Value && Player.main.currentMountedVehicle.IsPowered())
                    return ConfigToEdit.insideBaseTemp.Value;
                else if (!ConfigMenu.useRealTempForPlayer.Value)
                    return ConfigToEdit.insideBaseTemp.Value;
            }
            else if (Player.main.inHovercraft && !ConfigMenu.useRealTempForPlayer.Value)
            {
                return ConfigToEdit.insideBaseTemp.Value;
            }
            else if (Player.main._currentInterior is SeaTruckSegment)
            {
                SeaTruckSegment sts = Player.main._currentInterior as SeaTruckSegment;
                //AddDebug("SeaTruck IsPowered " + sts.relay.IsPowered());
                if (sts.relay.IsPowered())
                    return ConfigToEdit.insideBaseTemp.Value;
            }
            return Player_.ambientTemperature;
        }

        public static bool IsPlayerInTruck()
        {
            return Player.main._currentInterior != null && Player.main._currentInterior is SeaTruckSegment;
        }

        public static bool IsPlayerInPrecursor_()
        {
            if (PrecursorMoonPoolTrigger.inMoonpool || PrisonManager.IsInsideAquarium(Player.main.transform.position))
                return true;

            return false;
        }

        public static bool IsPlayerInPrecursor()
        {
            string biomeString = Player.main.GetBiomeString();
            if (biomeString.StartsWith("precursor", StringComparison.OrdinalIgnoreCase) || biomeString.StartsWith("prison", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        public static bool IsVehicle(GameObject go)
        {
            return go.TryGetComponent<Vehicle>(out _) || go.TryGetComponent<SeaTruckSegment>(out _) || go.TryGetComponent<Hoverbike>(out _);
        }

        public static bool IsInPoweredFridge(GameObject go)
        {
            if (go.transform.parent == null || go.transform.parent.parent == null)
                return false;

            Fridge fridge = go.transform.parent.parent.GetComponent<Fridge>();
            if (fridge && fridge.powerConsumer.IsPowered())
                return true;

            return false;
        }

        public static float GetTemperature(GameObject go)
        {
            //AddDebug("GetTemperature " + go.name);
            if (go.GetComponentInParent<Player>()) // in inventory
                return GetPlayerTemperature();

            if (IsInPoweredFridge(go))
            {
                return -1f;
            }
            IInteriorSpace currentInterior = go.GetComponentInParent<IInteriorSpace>();
            if (currentInterior != null)
                return currentInterior.GetInsideTemperature();

            if (go.transform.position.y < Ocean.GetOceanLevel())
                return WaterTemperatureSimulation.main.GetTemperature(go.transform.position);
            else
                return WeatherManager.main.GetFeelsLikeTemperature();
        }

        public static bool IsCreatureAlive(GameObject go)
        {
            if (go.TryGetComponent<Creature>(out _) == false)
                return false;

            if (go.TryGetComponent(out LiveMixin liveMixin) == false)
                return false;

            return liveMixin.IsAlive();
        }

        public static bool IsRawFish(GameObject go)
        {
            return go.TryGetComponent<Creature>(out _) && go.TryGetComponent<Eatable>(out _);
        }

        public static bool IsPlayerInVehicle()
        {
            if (Player.main.currentMountedVehicle || Player.main.inHovercraft || Player.main._currentInterior is SeaTruckSegment)
                return true;

            return false;
        }

        public static void Message(string str)
        {
            int count = main.messages.Count;

            if (count == 0)
            {
                AddDebug(str);
            }
            else
            {
                _Message message = main.messages[main.messages.Count - 1];
                message.messageText = str;
                message.entry.text = str;
            }
        }

        public static float NormalizeTo01range(int value, int min, int max)
        {
            float fl;
            int oldRange = max - min;

            if (oldRange == 0)
                fl = 0f;
            else
                fl = ((float)value - (float)min) / (float)oldRange;

            return Mathf.Clamp01(fl);
        }

        public static float NormalizeTo01range(float value, float min, float max)
        {
            float fl;
            float oldRange = max - min;

            if (oldRange == 0)
                fl = 0f;
            else
                fl = ((float)value - (float)min) / (float)oldRange;

            return Mathf.Clamp01(fl);
        }

        public static float NormalizeToRange(float value, float oldMin, float oldMax, float newMin, float newMax)
        {
            float oldRange = oldMax - oldMin;
            float newValue;

            if (oldRange == 0)
                newValue = newMin;
            else
            {
                float newRange = newMax - newMin;
                newValue = ((value - oldMin) * newRange) / oldRange + newMin;
            }
            return newValue;
        }

        public static IEnumerator PlaySound(FMODAsset sound, float delay)
        {
            yield return new WaitForSeconds(delay);
            //AddDebug("PlaySound " + sound.name);
            Utils.PlayFMODAsset(sound, Player.main.transform);
        }

        public static IEnumerator SelectEquippedItem()
        { // need this for seaglide
            while (!uGUI.main.hud.active)
                yield return null;

            yield return new WaitForSeconds(.5f);
            if (Main.configMain.activeSlot != -1 && Player.main.mode == Player.Mode.Normal)
            {
                //Inventory.main.quickSlots.SelectImmediate(config.activeSlot);
                //Inventory.main.quickSlots.DeselectImmediate();
                Inventory.main.quickSlots.Select(Main.configMain.activeSlot);
            }
        }

        public static Bounds GetAABB(GameObject go)
        {
            FixedBounds fb = go.GetComponent<FixedBounds>();
            Bounds bounds = fb == null ? UWE.Utils.GetEncapsulatedAABB(go) : fb.bounds;
            return bounds;
        }

        public static bool GetTarget(Vector3 startPos, Vector3 dir, float distance, out RaycastHit hitInfo)
        {
            //return Physics.Raycast(startPos, dir, out hitInfo, distance, Voxeland.GetTerrainLayerMask(), QueryTriggerInteraction.Ignore);
            return Physics.Raycast(startPos, dir, out hitInfo, distance);
        }

        public static bool GetPlayerTarget(float distance, out RaycastHit hitInfo, bool getTriggers = false)
        {
            Vector3 startPos = Player.mainObject.transform.position;
            Vector3 dir = MainCamera.camera.transform.forward;
            int layerMask = ~(1 << LayerMask.NameToLayer("Player"));
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore;
            if (getTriggers)
                queryTriggerInteraction = QueryTriggerInteraction.Collide;

            RaycastHit[] results = new RaycastHit[1];
            int hits = Physics.RaycastNonAlloc(startPos, dir, results, distance, layerMask, queryTriggerInteraction);
            //AddDebug("GetPlayerTarget hits " + hits + " results " + results.Length);
            if (hits > 0)
            {
                hitInfo = results[0];
                return true;
            }
            else
                hitInfo = new RaycastHit();

            return false;
        }

        public static void MakeEatable(GameObject go, float food, float water = float.NaN, float health = float.NaN, float cold = float.NaN)
        {
            //AddDebug($"MakeEatable {go.name} food {food} water {water} health {health} cold {cold}");
            //Main.logger.LogDebug($"MakeEatable {go.name} food {food} water {water} health {health} cold {cold}");

            if (float.IsNaN(food) && float.IsNaN(water) && float.IsNaN(health) && float.IsNaN(cold))
                return;

            Eatable eatable = go.EnsureComponent<Eatable>();
            if (float.IsNaN(food) == false)
                eatable.foodValue = food;

            if (float.IsNaN(water) == false)
                eatable.waterValue = water;

            if (float.IsNaN(health) == false)
                eatable.healthValue = health;

            if (float.IsNaN(cold) == false)
                eatable.coldMeterValue = cold;
        }

        public static float CelciusToFahrenhiet(float celcius)
        {
            return celcius * 1.8f + 32f;
        }

        public static IEnumerable<GameObject> FindAllRootGameObjects()
        {
            return Resources.FindObjectsOfTypeAll<Transform>()
                .Where(t => t.parent == null)
                .Select(x => x.gameObject);
        }

        public static bool IsEquipped(TechType tt)
        {
            if (tt == TechType.None)
                return false;

            foreach (var kv in Inventory.main.equipment.equipment)
            {
                if (kv.Value == null)
                    continue;

                if (kv.Value._techType == tt)
                    return true;
            }
            return false;
        }

        public static bool IsOneHanded(PlayerTool playerTool)
        {
            //TechType tt = CraftData.GetTechType(playerTool.gameObject);
            //AddDebug("IsOneHanded " + tt);
            if (playerTool.TryGetComponent<PenguinBaby>(out _))
                return false;

            switch (playerTool)
            {
                case DiveReel _:
                case SpyPenguinRemote _:
                case SnowBall _:
                case PlaceTool _:
                case CreatureTool _:
                    return true;
            }
            return playerTool.hasBashAnimation;
        }

        public static bool IsPlayerInDropPod()
        {
            return Player.main.currentInterior is LifepodDrop;
        }

        public static bool CanEatFish()
        {
            return GameModeManager.GetOption<bool>(GameOption.VegetarianDiet) == false && GameModeManager.GetOption<bool>(GameOption.Hunger) || GameModeManager.GetOption<bool>(GameOption.Thirst); ;
        }

        public static void PrintOpcodes(List<CodeInstruction> codes, string methodName)
        {
            Main.logger.LogMessage($"{methodName} opcodes");
            for (int i = 0; i < codes.Count; i++)
            {
                string operand = "";
                if (codes[i].operand != null)
                    operand = "operand " + codes[i].operand.ToString();

                Main.logger.LogMessage($"opcode {codes[i].opcode}  {operand}");
            }
        }

        public static string UppercaseFirstCharacter(string s)
        {
            return s[0].ToString().ToUpper() + s.Substring(1);
        }

        public static bool IsAnimationPlaying(Animator animator, int layerIndex = 0)
        {
            if (animator == null || !animator.enabled || !animator.gameObject.activeInHierarchy)
                return false;

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
            return stateInfo.length > 0 && stateInfo.normalizedTime < 1.0f;
        }

        public static Transform GetExosuitLightsTransform(Exosuit __instance)
        {
            Transform t = __instance.leftArmAttach.transform.Find("lights_parent");
            if (t == null)
                return __instance.transform.Find("lights_parent");

            return t;
        }

        public static IItemsContainer GetOpenContainer()
        {
            IItemsContainer itemsContainer = null;
            for (int i = 0; i < Inventory.main.usedStorage.Count; i++)
            {
                itemsContainer = Inventory.main.GetUsedStorage(i);
                if (itemsContainer != null)
                    break;
            }
            return itemsContainer;
        }

        public static void DisableShadowCasting(this Transform root, RendererData data)
        {
            //Main.logger.LogDebug($"DisableShadowCasting {root.name}");
            if (data == null)
            {
                root.DisableShadowCastingInChildren();
                return;
            }
            Transform parent = root;
            if (data.parentPath.IsNullOrWhiteSpace() == false)
            {
                parent = root.Find(data.parentPath);
                if (parent == null)
                {
                    Main.logger.LogError($"DisableShadowCasting {root.name} RendererData parent null " + data.parentPath);
                    return;
                }
            }
            if (data.renderers == null)
            {
                //Main.logger.LogDebug($"DisableShadowCasting {root.name} {parent.name} renderers null");
                parent.DisableShadowCastingInChildren();
                return;
            }
            foreach (string rendererName in data.renderers)
            {
                //Main.logger.LogDebug($"DisableShadowCasting parent {parent.name} rendererName {rendererName}");
                Transform rendererT = parent.Find(rendererName);
                if (rendererT == null)
                    continue;

                rendererT.DisableShadowCasting();
            }
        }

        public static void DisableShadowCasting(this Transform renderer)
        {
            //Main.logger.LogDebug("DisableShadowCasting Transform " + renderer.name);
            Renderer r = renderer.GetComponent<Renderer>();
            if (r == null)
            {
                Main.logger.LogError($"DisableShadowCasting {renderer.name} has no renderer");
                return;
            }
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        public static void DisableShadowCastingInChildren(this Transform transform)
        {
            //Main.logger.LogDebug("DisableShadowCastingInChildren " + t.name);
            Renderer[] rs = transform.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in rs)
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        public static void DisableShadowCasting(this Transform parent, string path)
        {
            //Main.logger.LogDebug($"DisableShadowCasting {parent.name}  {path}");
            Transform t = parent.Find(path);
            if (t == null)
            {
                //Main.logger.LogError($"DisableShadowCasting {parent.name} has no child {path}");
                return;
            }
            Renderer r = t.GetComponent<Renderer>();
            if (r == null)
            {
                //Main.logger.LogError($"DisableShadowCasting {parent.name} has no renderer on go {path}");
                return;
            }
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        public static void DisableShadowCasting(this Transform parent, List<string> paths)
        {
            //Main.logger.LogDebug("DisableShadowCasting " + parent.name);
            if (paths == null)
            {
                DisableShadowCastingInChildren(parent);
                return;
            }
            foreach (string path in paths)
            {
                Transform t = parent.Find(path);
                if (t == null)
                {
                    //Main.logger.LogError($"DisableShadowCasting {parent.name} has no child {path}");
                    continue;
                }
                Renderer r = t.GetComponent<Renderer>();
                if (r == null)
                {
                    //Main.logger.LogError($"DisableShadowCasting {parent.name} has no renderer on go {path}");
                    continue;
                }
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }

        public static List<Transform> FindAllChildren(this Transform parent, string name, bool recursive = false)
        {
            List<Transform> results = new List<Transform>();
            //Main.logger.LogDebug($"{parent.name} FindAllChildren childCount {parent.childCount}");
            foreach (Transform child in parent)
            {
                //Main.logger.LogDebug($"{parent.name} child {child.name}");
                if (child.name == name)
                    results.Add(child);

                if (recursive && child.childCount > 0)
                    results.AddRange(child.FindAllChildren(name, true));
            }
            return results;
        }

        public static void ForceLODs(this GameObject go, int index = 0)
        {
            //AddDebug("ForceLODs " + go.name);
            //Main.logger.LogDebug("ForceLODs " + go.name);
            LODGroup[] lods = go.GetComponentsInChildren<LODGroup>();

            foreach (LODGroup lod in lods)
                lod.ForceLOD(index);
        }

        public static void DisableGlowShader(this GameObject gameObject)
        {
            foreach (MeshRenderer mr in gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                foreach (Material m in mr.materials)
                    m.DisableKeyword("MARMO_EMISSION");
            }
        }

        public static List<GameObject> FindObjectsInRadius(Vector3 center, float radius, GameObject[] exclusions = null)
        {
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            List<GameObject> objectsInRange = new List<GameObject>();
            //HashSet<GameObject> exclusions_ = new HashSet<GameObject>(exclusions);
            foreach (GameObject obj in allObjects)
            {
                //if (exclusions != null && exclusions_.Contains(obj))
                //    continue;

                //if (targetTags != null && targetTags.Length > 0)
                //{
                //    bool hasValidTag = false;
                //    foreach (string tag in targetTags)
                //    {
                //        if (obj.CompareTag(tag))
                //        {
                //            hasValidTag = true;
                //            break;
                //        }
                //    }
                //    if (!hasValidTag) continue;
                //}
                float distance = Vector3.Distance(center, obj.transform.position);
                if (distance <= radius)
                    objectsInRange.Add(obj);
            }
            return objectsInRange;
        }


        public static VFXSurfaceTypes GetObjectSurfaceType(GameObject obj)
        {
            //AddDebug("GetObjectSurfaceType " + obj.name);
            VFXSurfaceTypes result = VFXSurfaceTypes.none;
            if (obj)
            {
                VFXSurface vfxSurface = obj.GetComponent<VFXSurface>();
                if (vfxSurface)
                {
                    result = vfxSurface.surfaceType;
                }
                else
                {
                    vfxSurface = obj.FindAncestor<VFXSurface>();
                    if (vfxSurface == null)
                        vfxSurface = obj.GetComponentInChildren<VFXSurface>();

                    if (vfxSurface)
                        result = vfxSurface.surfaceType;
                }
            }
            return result;
        }

        public static Vector3Int Vecto3ToVecto3int(Vector3 pos)
        {
            int x = Mathf.RoundToInt(pos.x);
            int y = Mathf.RoundToInt(pos.y);
            int z = Mathf.RoundToInt(pos.z);
            return new Vector3Int(x, y, z);
        }


    }
}