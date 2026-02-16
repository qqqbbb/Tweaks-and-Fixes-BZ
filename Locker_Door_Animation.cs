using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ErrorMessage;


namespace Tweaks_Fixes
{
    internal class Locker_Door_Animation
    {
        static FMODAsset openSound;
        static FMODAsset closeSound;

        public class LockerDoorOpener : MonoBehaviour
        {
            public float startRotation;
            public float endRotation;
            public float timeElapsed;
            public float duration = 1f;
            public float openAngle = 135f;
            public float doubleDoorOpenAngle = 90f;

            public IEnumerator Rotate(Transform door, bool playCloseSound = false, bool fridge = false, bool fixOpenedLockerDoor = false)
            {
                if (fixOpenedLockerDoor && timeElapsed == 0)
                {
                    float dist = Vector3.Distance(Player.main.transform.position, door.transform.position);
                    if (dist < 1)
                    {
                        Vector3 directionToPlayer = Player.main.transform.position - transform.position;
                        float angle = Vector3.Angle(transform.forward, directionToPlayer.normalized);
                        //AddDebug($"angle {angle}");
                        if (angle > 30 && angle < 50)
                            endRotation = 190;
                        else if (angle > 50 && angle < 70)
                            endRotation = 90;
                    }
                }
                while (timeElapsed < duration)
                {
                    timeElapsed += Time.deltaTime;
                    float f = timeElapsed / duration;
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
                while (timeElapsed < duration)
                {
                    timeElapsed += Time.deltaTime;
                    float f = timeElapsed / duration;
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
                            rotater.timeElapsed = 0f;
                            rotater.StartCoroutine(rotater.Rotate(door, false, false, true));
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
                            rotater.timeElapsed = 0f;
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
                        rotater.timeElapsed = 0f;
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
                            rotater.timeElapsed = 0f;
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
                            rotater.timeElapsed = 0f;
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
                        rotater.timeElapsed = 0f;
                        rotater.StartCoroutine(rotater.Rotate(door, true, true));
                    }
                }
            }

        }


    }
}
