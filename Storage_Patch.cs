using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using System.Text;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    public class Storage_Patch
    {
        static FMODAsset openSound;
        static FMODAsset closeSound;

        public class LockerDoorOpener : MonoBehaviour
        {
            public float startRotation;
            public float endRotation;
            public float t;
            public float duration = 1f;
            public float openAngle = 135f;
            public float doubleDoorOpenAngle = 90f;

            public IEnumerator Rotate(Transform door, bool playCloseSound = false, bool fridge = false)
            {
                while (t < duration)
                {
                    t += Time.deltaTime;
                    float f = t / duration;
                    float rotation = Mathf.Lerp(startRotation, endRotation, f);
                    //Main.Log("rotation " + rotation );
                    //AddDebug(" rotation " + rotation);
                    if (fridge)
                        door.localEulerAngles = new Vector3(door.localEulerAngles.x, rotation, door.localEulerAngles.z);
                    else
                        door.localEulerAngles = new Vector3(door.localEulerAngles.x, door.localEulerAngles.y, rotation);

                    if (endRotation == 0f)
                    {
                        if (playCloseSound && f > .62f && closeSound != null)
                        {
                            playCloseSound = false;
                            Utils.PlayFMODAsset(closeSound, door.transform);
                        }
                        else if (f > 1f)
                        {
                            ColoredLabel cl = door.GetComponentInChildren<ColoredLabel>();
                            Transform parent = door.transform.parent.parent.parent;
                            if (cl && parent)
                                cl.transform.SetParent(parent);
                        }
                    }
                    yield return null;
                }
            }

            public IEnumerator Rotate(Transform doorLeft, Transform doorRight, bool playCloseSound = false)
            {
                while (t < duration)
                {
                    t += Time.deltaTime;
                    float f = t / duration;
                    float rotation = Mathf.Lerp(startRotation, endRotation, f);
                    doorLeft.localEulerAngles = new Vector3(doorLeft.localEulerAngles.x, doorLeft.localEulerAngles.y, -rotation);
                    doorRight.localEulerAngles = new Vector3(doorRight.localEulerAngles.x, doorRight.localEulerAngles.y, rotation);
                    if (f > .62f && playCloseSound && closeSound != null)
                    {
                        playCloseSound = false;
                        Utils.PlayFMODAsset(closeSound, doorLeft.transform.parent);
                    }
                    yield return null;
                }
            }
        }

        static string GetKey(Transform door)
        {
            PrefabIdentifier pi = door.GetComponentInParent<PrefabIdentifier>();
            Vector3 pos = pi.transform.position;
            StringBuilder sb = new StringBuilder(Mathf.RoundToInt(pos.x).ToString());
            sb.Append("_");
            sb.Append(Mathf.RoundToInt(pos.y));
            sb.Append("_");
            sb.Append(Mathf.RoundToInt(pos.z));
            return sb.ToString();
        }

        static IEnumerator AddLabel(Transform door)
        {
            if (door.parent == null)
                yield return null;

            //AddDebug("AddLabel " + cyclops + " " + techType);
            CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(TechType.Sign);
            yield return request;
            GameObject result1 = request.GetResult();
            if (result1 != null)
            {
                GameObject go = Utils.CreatePrefab(result1);
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
                    si.stringDefaultLabel = "SmallLockerDefaultLabel";
                    si.inputField.text = Language.main.Get(si.stringDefaultLabel);
                    si.inputField.characterLimit = 58;
                    string slot = SaveLoadManager.main.currentSlot;
                    if (Main.config.lockerNames.ContainsKey(slot))
                    {
                        string key = GetKey(door);
                        if (Main.config.lockerNames[slot].ContainsKey(key))
                        {
                            SavedLabel sl = Main.config.lockerNames[slot][key];
                            si.inputField.text = sl.text;
                            si.colorIndex = sl.color;
                            si.SetBackground(sl.background);
                            si.scaleIndex = sl.scale; // range -3 3 
                        }
                    }
                }
            }
        }

        public static string GetText(ColoredLabel label, PickupableStorage ps, StorageContainer sc, GUIHand hand, Sign sign)
        {
            string text = string.Empty;
            if (sc)
            {
                text = HandReticle.main.GetText(sc.hoverText, true, GameInput.Button.LeftHand);
            }
            if (label && label.enabled)
            {
                text += "\n" + HandReticle.main.GetText(label.stringEditLabel, true, GameInput.Button.RightHand);
            }
            if (sign && sign.enabled)
            {
                //stringBuilder.Append(Language.main.Get("SmallLockerEditLabel"));
                text += "\n" + HandReticle.main.GetText("SmallLockerEditLabel", true, GameInput.Button.RightHand);
            }
            if (ps)
            {
                if (ps.storageContainer.IsEmpty() || ps.allowPickupWhenNonEmpty)
                    //ps.pickupable.OnHandHover(hand);
                    text += "\n" + OnPickupableHandHover(ps.pickupable, hand);
                else if (!string.IsNullOrEmpty(ps.cantPickupHoverText))
                {
                    text += "\n" + HandReticle.main.GetText(ps.cantPickupHoverText, true);
                    //HandReticle.main.SetText(HandReticle.TextType.Hand, ps.cantPickupHoverText, true);
                    HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, false);
                }
            }
            return text;
        }

        public static string OnPickupableHandHover(Pickupable pickupable, GUIHand hand)
        {
            HandReticle handReticle = HandReticle.main;
            if (!hand.IsFreeToInteract())
                return string.Empty;

            string text1 = string.Empty;
            string text2 = string.Empty;
            TechType techType = pickupable.GetTechType();
            GameInput.Button button = GameInput.Button.AltTool;

            if (pickupable.AllowedToPickUp())
            {
                Exosuit exosuit = Player.main.GetVehicle() as Exosuit;
                bool canPickup = exosuit == null || exosuit.HasClaw();
                if (canPickup)
                {
                    ISecondaryTooltip secTooltip = pickupable.gameObject.GetComponent<ISecondaryTooltip>();
                    if (secTooltip != null)
                        text2 = secTooltip.GetSecondaryTooltip();
                    text1 = pickupable.usePackUpIcon ? LanguageCache.GetPackUpText(techType) : LanguageCache.GetPickupText(techType);
                    //handReticle.SetIcon(pickupable.usePackUpIcon ? HandReticle.IconType.PackUp : HandReticle.IconType.Hand);
                }
                if (exosuit)
                {
                    //button = canPickup ? GameInput.Button.LeftHand : GameInput.Button.None;
                    //if (exosuit.leftArmType != TechType.ExosuitClawArmModule)
                    //    button = GameInput.Button.RightHand;
                    //HandReticle.main.SetText(HandReticle.TextType.Hand, text1, false, button);

                    HandReticle.main.SetText(HandReticle.TextType.HandSubscript, text2, false);
                }
                else
                {
                    //HandReticle.main.SetText(HandReticle.TextType.Hand, text1, false, GameInput.Button.LeftHand);
                    //button = GameInput.Button.RightHand;
                    HandReticle.main.SetText(HandReticle.TextType.HandSubscript, text2, false);
                }
            }
            else if (pickupable.isPickupable && !Player.main.HasInventoryRoom(pickupable))
            {
                //handReticle.SetText(HandReticle.TextType.Hand, techType.AsString(), true);
                handReticle.SetText(HandReticle.TextType.HandSubscript, "InventoryFull", true);
            }
            else
            {
                //handReticle.SetText(HandReticle.TextType.Hand, techType.AsString(), true);
                handReticle.SetText(HandReticle.TextType.HandSubscript, string.Empty, false);
            }
            text1 = HandReticle.main.GetText(text1, true, button);
            return text1;
        }

        public static ColoredLabel GetSeaTruckLabel(GameObject seatruck, StorageContainer container)
        {
            //AddDebug("GetSeaTruckLabel");
            ColoredLabel[] labels = Util.GetComponentsInDirectChildren<ColoredLabel>(seatruck);
            foreach (ColoredLabel l in labels)
            {
                if (l.name == "Label" && container.name == "StorageContainer (2)")
                    return l;
                else if (l.name == "Label (1)" && container.name == "StorageContainer (3)")
                    return l;
                else if (l.name == "Label (2)" && container.name == "StorageContainer")
                    return l;
                else if (l.name == "Label (3)" && container.name == "StorageContainer (4)")
                    return l;
                else if (l.name == "Label (4)" && container.name == "StorageContainer (1)")
                    return l;
            }
            return null;
        }

        public static void ProcessInput(ColoredLabel label, PickupableStorage ps, StorageContainer sc, GUIHand hand, Sign sign)
        {
            //if (GameInput.GetButtonDown(GameInput.Button.LeftHand))
            //{
                //if (sc)
                //    sc.Open(sc.transform);
                //AddDebug("LeftHand");
            //}
            if (GameInput.GetButtonDown(GameInput.Button.RightHand))
            {
                if (label && label.enabled)
                    label.signInput.Select(true);
                else if (sign && sign.enabled)
                    sign.signInput.Select(true);
                //AddDebug("RightHand");
            }
            else if (GameInput.GetButtonDown(GameInput.Button.AltTool))
            {
                if (ps.storageContainer.IsEmpty() || ps.allowPickupWhenNonEmpty)
                    ps.pickupable.OnHandClick(hand);
                //AddDebug("AltTool");
            }
        }

        [HarmonyPatch(typeof(DeployableStorage))]
        public class DeployableStorage_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Awake")]
            static void AwakePostfix(DeployableStorage __instance)
            {
                if (!ConfigToEdit.newStorageUI.Value)
                    return;

                LiveMixin lm = __instance.GetComponent<LiveMixin>();
                if (lm)
                    UnityEngine.Object.Destroy(lm);

                PickupableStorage ps = __instance.GetComponentInChildren<PickupableStorage>();
                if (ps)
                {
                    Collider collider = ps.GetComponent<Collider>();
                    if (collider)
                        UnityEngine.Object.Destroy(collider);
                }
                ColoredLabel cl = __instance.GetComponentInChildren<ColoredLabel>();
                if (cl)
                {
                    Collider collider = cl.GetComponent<Collider>();
                    if (collider)
                        UnityEngine.Object.Destroy(collider);
                }
            }

        }

        [HarmonyPatch(typeof(StorageContainer))]
        class StorageContainer_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Awake")]
            static void AwakePostfix(StorageContainer __instance)
            {
                if (openSound == null)
                {
                    openSound = ScriptableObject.CreateInstance<FMODAsset>();
                    openSound.path = "event:/sub/cyclops/locker_open";
                    openSound.id = "{c97d1fdf-ea26-4b19-8358-7f6ea77c3763}";
                    closeSound = ScriptableObject.CreateInstance<FMODAsset>();
                    closeSound.path = "event:/sub/cyclops/locker_close";
                    closeSound.id = "{16eb5589-e341-41cb-9c88-02cb4e3da44a}";
                }
                if (!ConfigToEdit.newStorageUI.Value)
                    return;

                TechTag techTag = __instance.GetComponent<TechTag>();
                if (techTag)
                {
                    //AddDebug("StorageContainer Awake " + techTag.type);
                    if (techTag.type == TechType.SmallLocker)
                    { // fix
                        ColoredLabel cl = __instance.GetComponentInChildren<ColoredLabel>();
                        if (cl)
                        {
                            Collider collider = cl.GetComponent<Collider>();
                            if (collider)
                                UnityEngine.Object.Destroy(collider);
                        }
                    }
                    else if (techTag.type == TechType.Locker && !Main.visibleLockerInteriorModLoaded)
                    {
                        LiveMixin lm = __instance.GetComponent<LiveMixin>();
                        if (lm)
                            UnityEngine.Object.Destroy(lm);

                        Transform doorRight = __instance.transform.Find("model/submarine_Storage_locker_big_01/submarine_Storage_locker_big_01_hinges_R");
                        if (doorRight)
                        { // parent is null
                            UWE.CoroutineHost.StartCoroutine(AddLabel(doorRight));
                        }
                    }
                }
                //else if (__instance.GetComponent<Fridge>())
            }

            [HarmonyPostfix]
            [HarmonyPatch("Open", new Type[] { typeof(Transform) })]
            static void OpenPostfix(StorageContainer __instance, Transform useTransform)
            {
                TechTag techTag = __instance.GetComponent<TechTag>();
                if (techTag)
                {
                    if (techTag.type == TechType.SmallLocker)
                    {
                        Transform door = __instance.transform.Find("model/submarine_locker_02/submarine_locker_02_door");
                        if (door)
                        {
                            //AddDebug("SmallLocker Open ");
                            ColoredLabel cl = __instance.GetComponentInChildren<ColoredLabel>(true);
                            if (cl)
                                cl.transform.SetParent(door.transform);
                            LockerDoorOpener rotater = __instance.gameObject.EnsureComponent<LockerDoorOpener>();
                            rotater.startRotation = door.transform.localEulerAngles.z;
                            rotater.endRotation = rotater.startRotation + rotater.openAngle;
                            rotater.t = 0f;
                            rotater.StartCoroutine(rotater.Rotate(door));
                            if (openSound != null)
                                Utils.PlayFMODAsset(openSound, __instance.transform);
                        }
                    }
                    else if (techTag.type == TechType.Locker)
                    {
                        Transform doorLeft = __instance.transform.Find("model/submarine_Storage_locker_big_01/submarine_Storage_locker_big_01_hinges_L");
                        Transform doorRight = __instance.transform.Find("model/submarine_Storage_locker_big_01/submarine_Storage_locker_big_01_hinges_R");
                        if (doorLeft && doorRight)
                        {
                            LockerDoorOpener rotater = __instance.gameObject.EnsureComponent<LockerDoorOpener>();
                            rotater.startRotation = doorLeft.transform.localEulerAngles.z;
                            rotater.endRotation = rotater.startRotation + rotater.doubleDoorOpenAngle;
                            rotater.t = 0f;
                            rotater.StartCoroutine(rotater.Rotate(doorLeft, doorRight));
                            if (openSound != null)
                                Utils.PlayFMODAsset(openSound, __instance.transform);
                        }
                    }
                }
                else if (__instance.GetComponent<Fridge>())
                {
                    Transform door = __instance.transform.Find("geo/marg_props_fridge_door");
                    if (door)
                    {
                        LockerDoorOpener rotater = __instance.gameObject.EnsureComponent<LockerDoorOpener>();
                        rotater.startRotation = door.transform.localEulerAngles.y;
                        rotater.endRotation = rotater.startRotation + rotater.openAngle;
                        rotater.t = 0f;
                        rotater.StartCoroutine(rotater.Rotate(door, false, true));
                        if (openSound != null)
                            Utils.PlayFMODAsset(openSound, __instance.transform);
                    }
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("OnClose")]
            static void OnClosePostfix(StorageContainer __instance)
            {
                TechTag techTag = __instance.GetComponent<TechTag>();
                if (techTag)
                { 
                    if (techTag.type == TechType.SmallLocker)
                    {
                        Transform door = __instance.transform.Find("model/submarine_locker_02/submarine_locker_02_door");
                        if (door)
                        {
                            //AddDebug("SmallLocker OnClose ");
                            LockerDoorOpener rotater = __instance.gameObject.EnsureComponent<LockerDoorOpener>();
                            rotater.startRotation = door.transform.localEulerAngles.z;
                            rotater.endRotation = 0f;
                            rotater.t = 0f;
                            rotater.StartCoroutine(rotater.Rotate(door, true));
                        }
                    }
                    else if (techTag.type == TechType.Locker)
                    {
                        Transform doorLeft = __instance.transform.Find("model/submarine_Storage_locker_big_01/submarine_Storage_locker_big_01_hinges_L");
                        Transform doorRight = __instance.transform.Find("model/submarine_Storage_locker_big_01/submarine_Storage_locker_big_01_hinges_R");
                        if (doorLeft && doorRight)
                        {
                            LockerDoorOpener rotater = __instance.gameObject.EnsureComponent<LockerDoorOpener>();
                            rotater.startRotation = doorRight.transform.localEulerAngles.z;
                            rotater.endRotation = 0f;
                            rotater.t = 0f;
                            rotater.StartCoroutine(rotater.Rotate(doorLeft, doorRight, true));
                        }
                    }
                
                }
                else if (__instance.GetComponent<Fridge>())
                {
                    Transform door = __instance.transform.Find("geo/marg_props_fridge_door");
                    if (door)
                    {
                        LockerDoorOpener rotater = __instance.gameObject.EnsureComponent<LockerDoorOpener>();
                        rotater.startRotation = door.transform.localEulerAngles.y;
                        rotater.endRotation = 0f;
                        rotater.t = 0f;
                        rotater.StartCoroutine(rotater.Rotate(door, true, true));
                    }
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch("OnHandHover")]
            static bool OnHandHoverPrefix(StorageContainer __instance, GUIHand hand)
            {
                if (!ConfigToEdit.newStorageUI.Value)
                    return true;
                //AddDebug("StorageContainer OnHandHover name " + __instance.name);
                //HandReticle.main.SetTextRaw(HandReticle.TextType.UseSubscript, "Subscript");
                // HandReticle.main.SetTextRaw(HandReticle.TextType.Use, "Use");
                //str = LanguageCache.GetButtonFormat("AirBladderUseTool", GameInput.Button.RightHand);
                if (!__instance.enabled || __instance.disableUseability)
                    return false;
                Constructable c = __instance.gameObject.GetComponent<Constructable>();
                if (c && !c.constructed)
                    return false;

                GameObject parent = Util.GetParent(__instance.gameObject);

                //GameObject parent = __instance.transform.parent.gameObject;
                ColoredLabel label = null;
                PickupableStorage ps = null;
                Sign sign = null;
                if (parent.name == "SeaTruckStorageModule(Clone)")
                    label = GetSeaTruckLabel(parent, __instance);
                else
                {// fix
                    label = parent.GetComponentInChildren<ColoredLabel>();
                    ps = parent.GetComponentInChildren<PickupableStorage>();
                    sign = parent.GetComponentInChildren<Sign>();
                }
                //if (label)
                //AddDebug("StorageContainer OnHandHover label");
                //if (ps)
                //AddDebug("StorageContainer OnHandHover PickupableStorage");
                string text = GetText(label, ps, __instance, hand, sign);
                //string text = HandReticle.main.GetText(__instance.hoverText, true, GameInput.Button.LeftHand);
                //HandReticle.main.SetText(HandReticle.TextType.Hand, __instance.hoverText, true, GameInput.Button.LeftHand);
                //HandReticle.main.SetText(HandReticle.TextType.HandSubscript, __instance.IsEmpty() ? "Empty" : string.Empty, true);
                HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, text);
                HandReticle.main.SetIcon(HandReticle.IconType.Hand);
                ProcessInput(label, ps, __instance, hand, sign);
                return false;
            }
        }

        [HarmonyPatch(typeof(Sign))]
        class Sign_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("UpdateCollider")]
            static bool UpdateColliderPrefix(Sign __instance)
            {
                if (__instance.boxCollider == null)
                {
                    //AddDebug("Sign boxCollider == null");
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(uGUI_SignInput))]
        class SignInput_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("OnDeselect")]
            static void OnDeselectPostfix(uGUI_SignInput __instance)
            {
                //AddDebug("uGUI_SignInput OnDeselect " + __instance.stringDefaultLabel);
                if (__instance.stringDefaultLabel == "SmallLockerDefaultLabel" && __instance.inputField.characterLimit == 58)
                {
                    //AddDebug("uGUI_SignInput OnDeselect " + __instance.stringDefaultLabel);
                    //AddDebug("uGUI_SignInput OnDeselect locker " + __instance.text);
                    //bool cyclopsLocker = __instance.transform.parent.parent.GetComponent<CyclopsLocker>();
                    //bool cyclops = __instance.transform.GetComponentInParent<SubControl>();
                    string key = GetKey(__instance.transform.parent.parent);
                    //AddDebug("key " + key);
                    string slot = SaveLoadManager.main.currentSlot;
                    if (!Main.config.lockerNames.ContainsKey(slot))
                        Main.config.lockerNames[slot] = new Dictionary<string, SavedLabel>();

                    Main.config.lockerNames[slot][key] = new SavedLabel(__instance.text, __instance.backgroundToggle.isOn, __instance.colorIndex, __instance.scaleIndex);
                }
            }
        }

        [HarmonyPatch(typeof(Constructable))]
        class Constructable_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("DeconstructAsync")]
            static void DeconstructPostfix(Constructable __instance, IOut<bool> result)
            {
                if (__instance.techType == TechType.Locker && __instance.constructedAmount == 0f)
                {
                    //AddDebug("Deconstruct " + __instance.constructedAmount);
                    string slot = SaveLoadManager.main.currentSlot;
                    if (Main.config.lockerNames.ContainsKey(slot))
                    {
                        string key = GetKey(__instance.transform);
                        if (Main.config.lockerNames[slot].ContainsKey(key))
                        {
                            //AddDebug("Deconstruct saved locker ");
                            Main.config.lockerNames[slot].Remove(key);
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(SeaTruckSegment), "Start")]
        class SeaTruckSegment_Start_patch
        {
            public static void Postfix(SeaTruckSegment __instance)
            {
                if (!ConfigToEdit.newStorageUI.Value)
                    return;

                if (__instance.name == "SeaTruckStorageModule(Clone)" || __instance.name == "SeaTruckFabricatorModule(Clone)")
                {
                    //AddDebug("StorageContainer Awake parent " + __instance.name);
                    ColoredLabel[] cls = __instance.GetComponentsInChildren<ColoredLabel>();
                    foreach (ColoredLabel cl in cls)
                    {
                        Collider collider = cl.GetComponent<Collider>();
                        if (collider)
                            UnityEngine.Object.Destroy(collider);
                    }
                }
            }
        }

        public struct SavedLabel
        {
            public string text;
            public bool background;
            public int color;
            public int scale;

            public SavedLabel(string text_, bool background_, int color_, int scale_)
            {
                text = text_;
                color = color_;
                scale = scale_;
                background = background_;
            }
        }


        //[HarmonyPatch(typeof(CyclopsLocker))]
        public class CyclopsLocker_Patch
        {
            //[HarmonyPostfix]
            //[HarmonyPatch("Start")]
            static void StartPostfix(CyclopsLocker __instance)
            { // cyclops prefab always loads
                AddDebug("CyclopsLocker Start");
                openSound = __instance.openSound;
                closeSound = __instance.closeSound;
            }
        }

        //[HarmonyPatch(typeof(ColoredLabel))]
        class ColoredLabel_Patch
        {
            //[HarmonyPrefix]
            //[HarmonyPatch("OnProtoSerialize")]
            static bool OnProtoSerializePrefix(ColoredLabel __instance)
            {
                //AddDebug("locker ColoredLabel OnProtoSerialize");
                if (__instance.gameObject.name == "submarine_Storage_locker_big_01_hinges_R")
                {
                    return false;
                }
                return true;
            }
            //[HarmonyPrefix]
            //[HarmonyPatch("OnProtoDeserialize")]
            static bool OnProtoDeserializePrefix(ColoredLabel __instance)
            {
                //AddDebug("locker ColoredLabel OnProtoDeserialize");
                if (__instance.gameObject.name == "submarine_Storage_locker_big_01_hinges_R")
                {

                    return false;
                }
                return true;
            }
            //[HarmonyPostfix]
            //[HarmonyPatch("OnProtoDeserialize")]
            static void OnProtoDeserializePostfix(ColoredLabel __instance)
            {
                //AddDebug("locker ColoredLabel OnProtoDeserialize");
                TechTag techTag = __instance.transform.parent.GetComponent<TechTag>();
                if (techTag == null)
                    return;
                //AddDebug("ColoredLabel OnProtoDeserialize " + techTag.type);
                if (techTag.type == TechType.SmallLocker)
                {
                    //Transform door = __instance.transform.Find("model/submarine_locker_02/submarine_locker_02_door");
                    //if (door)
                    //    __instance.transform.SetParent(door.transform);
                }
            }
            //[HarmonyPostfix]
            //[HarmonyPatch("OnEnable")]
            static void OnEnablePostfix(ColoredLabel __instance)
            {
                //AddDebug("locker ColoredLabel OnProtoDeserialize");
                Transform parent = __instance.transform.parent;
                TechTag techTag = parent.GetComponent<TechTag>();
                if (techTag == null)
                    return;
                //AddDebug("ColoredLabel OnEnable " + techTag.type);
                if (techTag.type == TechType.SmallLocker)
                {
                    Transform door = parent.Find("model/submarine_locker_02/submarine_locker_02_door");
                    if (door)
                        __instance.transform.SetParent(door.transform);
                }
            }
        }


    }
}
