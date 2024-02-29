using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal static class Util
    {

        static public IEnumerator AddToInventory(TechType techType)
        {
            GameObject gameObject = null;
            //AddDebug("AddToInventory " + techType);
            TaskResult<GameObject> result = new TaskResult<GameObject>();
            yield return CraftData.InstantiateFromPrefabAsync(techType, (IOut<GameObject>)result);
            gameObject = result.Get();
            result = (TaskResult<GameObject>)null;
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
            //Inventory.main._container.DestroyItem(tt);
            //Inventory.main.ConsumeResourcesForRecipe(tt);
            TechType processed = TechData.GetProcessed(CraftData.GetTechType(go));
            if (processed != TechType.None)
            { // cooked fish cant be in quickslot
              //AddDebug("CookFish " + processed);
              //UWE.CoroutineHost.StartCoroutine(Main.AddToInventory(processed));
                CraftData.AddToInventory(processed);
                //Inventory.main.quickSlots.desiredSlot
                UnityEngine.Object.Destroy(go);
                //Inventory.main.quickSlots.SelectInternal(int slotID);
            }
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

        public static GameObject GetParent(GameObject go)
        {
            //if (go.name.Contains("(Clone)"))
            if (go.GetComponent<PrefabIdentifier>())
            {
                //AddDebug("name " + go.name);
                return go;
            }
            Transform t = go.transform;
            while (t.parent != null)
            {
                //if (t.parent.name.Contains("(Clone)"))
                if (t.parent.GetComponent<PrefabIdentifier>())
                {
                    //AddDebug("parent.name " + t.parent.name);
                    return t.parent.gameObject;
                }
                t = t.parent.transform;
            }
            return null;
        }

        public static float GetPlayerTemperature()
        {
            //AddDebug("GetPlayerTemperature ");
            //IInteriorSpace currentInterior = Player.main.GetComponentInParent<IInteriorSpace>();
            //if (currentInterior != null)
            //    return currentInterior.GetInsideTemperature();
            if (Player.main.inExosuit)
            {
                if (Main.config.useRealTempForColdMeter && Player.main.currentMountedVehicle.IsPowered())
                    return Main.config.vehicleTemp;
                else if (!Main.config.useRealTempForColdMeter)
                    return Main.config.vehicleTemp;
            }
            else if (Player.main.inHovercraft && !Main.config.useRealTempForColdMeter)
            {
                return Main.config.vehicleTemp;
            }
            else if (Player.main._currentInterior != null && !Player.main._currentInterior.Equals(null) && Player.main._currentInterior is SeaTruckSegment)
            {
                SeaTruckSegment sts = Player.main._currentInterior as SeaTruckSegment;
                //AddDebug("SeaTruck IsPowered " + sts.relay.IsPowered());
                if (sts.relay.IsPowered())
                    return Main.config.vehicleTemp;
            }
            return Player_Patches.ambientTemperature;
        }

        public static bool IsVehicle(GameObject go)
        {
            return go.GetComponent<Vehicle>() || go.GetComponent<SeaTruckSegment>() || go.GetComponent<Hoverbike>();
        }

        public static float GetTemperature(GameObject go)
        {
            //AddDebug("GetTemperature " + go.name);
            if (go.GetComponentInParent<Player>())
                return GetPlayerTemperature();

            if (go.transform.parent && go.transform.parent.parent)
            {
                Fridge fridge = go.transform.parent.parent.GetComponent<Fridge>();
                if (fridge && fridge.powerConsumer.IsPowered())
                {
                    //AddDebug("GetTemperature " + go.name + " in fridge");
                    return -1f;
                }
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
            LiveMixin liveMixin = go.GetComponent<LiveMixin>();
            Creature creature = go.GetComponent<Creature>();
            if (creature && liveMixin && liveMixin.IsAlive())
                return true;
            else
                return false;
        }

        public static bool IsEatableFish(GameObject go)
        {
            Creature creature = go.GetComponent<Creature>();
            if (!creature)
                return false;

            Eatable eatable = go.GetComponent<Eatable>();
            if (eatable)
                return true;
            else
                return false;
        }

        public static bool IsPlayerInVehicle()
        {
            if (Player.main._currentInterior != null && !Player.main._currentInterior.Equals(null) && Player.main._currentInterior is SeaTruckSegment)
                return true;

            return Player.main.inExosuit || Player.main.inHovercraft;
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

            return fl;
        }

        public static float NormalizeTo01range(float value, float min, float max)
        {
            float fl;
            float oldRange = max - min;

            if (oldRange == 0)
                fl = 0f;
            else
                fl = ((float)value - (float)min) / (float)oldRange;

            return fl;
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
            if (Main.config.activeSlot != -1)
            {
                //Inventory.main.quickSlots.SelectImmediate(config.activeSlot);
                //Inventory.main.quickSlots.DeselectImmediate();
                Inventory.main.quickSlots.Select(Main.config.activeSlot);
            }
        }

        public static VFXSurfaceTypes GetObjectSurfaceType(GameObject obj)
        {
            VFXSurfaceTypes result = VFXSurfaceTypes.none;
            if (obj)
            {
                VFXSurface vfxSurface = obj.GetComponent<VFXSurface>();
                if (vfxSurface)
                {
                    result = vfxSurface.surfaceType;
                    //AddDebug(" VFXSurface " + component.name);
                    //AddDebug(" VFXSurface parent " + component.transform.parent.name);
                    //AddDebug(" VFXSurface parent parent " + component.transform.parent.parent.name);
                }
                else
                    vfxSurface = obj.FindAncestor<VFXSurface>();

                if (vfxSurface)
                    result = vfxSurface.surfaceType;
            }
            return result;
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

        public static bool GetPlayerTarget(float distance, out RaycastHit hitInfo)
        {
            Vector3 startPos = Player.mainObject.transform.position;
            Vector3 dir = MainCamera.camera.transform.forward;
            int layerMask = ~(1 << LayerMask.NameToLayer("Player"));
            return Physics.Raycast(startPos, dir, out hitInfo, distance, layerMask, QueryTriggerInteraction.Ignore);
            //return Physics.Raycast(startPos, dir, out hitInfo, distance);
        }

        public static GameObject GetEntityRoot(GameObject go)
        {
            PrefabIdentifier prefabIdentifier = go.GetComponent<PrefabIdentifier>();
            if (prefabIdentifier == null)
                prefabIdentifier = go.GetComponentInParent<PrefabIdentifier>();
            return prefabIdentifier != null ? prefabIdentifier.gameObject : null;
        }

        public static bool Approximately(float a, float b, float tolerance = 0.00001f)
        { // Mathf.Approximately does not work when compare to 0
            return (Mathf.Abs(a - b) < tolerance);
        }

    }
}