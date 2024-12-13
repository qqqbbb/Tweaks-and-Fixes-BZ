using HarmonyLib;
using Nautilus.Handlers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UWE;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class Knife_Patch
    {
        public static bool giveResourceOnDamage;
        static Vector3 knifeTargetPos;

        [HarmonyPatch(typeof(PlayerTool))]
        public class PlayerTool_Patch
        {
            static float knifeRangeDefault = 0f;
            static float knifeDamageDefault = 0f;

            [HarmonyPostfix]
            [HarmonyPatch("OnDraw")]
            public static void OnDrawPostfix(PlayerTool __instance)
            {
                //TechType tt = CraftData.GetTechType(__instance.gameObject);
                Knife knife = __instance as Knife;
                if (knife)
                {
                    if (knifeRangeDefault == 0f)
                        knifeRangeDefault = knife.attackDist;
                    if (knifeDamageDefault == 0f)
                        knifeDamageDefault = knife.damage;

                    knife.attackDist = knifeRangeDefault * ConfigMenu.knifeRangeMult.Value;
                    knife.damage = knifeDamageDefault * ConfigMenu.knifeDamageMult.Value;
                    //AddDebug(" attackDist  " + knife.attackDist);
                    //AddDebug(" damage  " + knife.damage);
                }
            }
        }

        [HarmonyPatch(typeof(Knife))]
        class Knife_Patch_
        {
            [HarmonyPrefix]
            [HarmonyPatch("OnToolUseAnim")]
            public static bool OnToolUseAnimPrefix(Knife __instance, GUIHand hand)
            {
                Vector3 position = new Vector3();
                GameObject closestObj = null;
                Vector3 normal;
                UWE.Utils.TraceFPSTargetPosition(Player.main.gameObject, __instance.attackDist, ref closestObj, ref position, out normal);
                if (closestObj == null)
                {
                    InteractionVolumeUser ivu = Player.main.gameObject.GetComponent<InteractionVolumeUser>();
                    if (ivu != null && ivu.GetMostRecent() != null)
                        closestObj = ivu.GetMostRecent().gameObject;
                }
                if (closestObj)
                {
                    GameObject root = null;
                    LargeWorldEntity lwe = closestObj.GetComponentInParent<LargeWorldEntity>();
                    if (lwe)
                        root = lwe.gameObject;

                    //AddDebug("closestObj " + closestObj.name);
                    //AddDebug("root " + root.name);

                    LiveMixin lm = closestObj.FindAncestor<LiveMixin>();

                    if (lm && Knife.IsValidTarget(lm))
                    {
                        bool wasAlive = lm.IsAlive();
                        lm.TakeDamage(__instance.damage, position, __instance.damageType, Utils.GetLocalPlayer());
                        __instance.GiveResourceOnDamage(closestObj, lm.IsAlive(), wasAlive);
                    }
                    VFXSurface surface = closestObj.GetComponent<VFXSurface>();
                    if (surface == null && root != null)
                        surface = root.GetComponent<VFXSurface>();

                    Vector3 euler = MainCameraControl.main.transform.eulerAngles + new Vector3(300f, 90f, 0f);
                    //if (surface)
                    //    AddDebug("surface " + surface.surfaceType);

                    VFXSurfaceTypeManager.main.Play(surface, __instance.vfxEventType, position, Quaternion.Euler(euler), Player.main.transform);

                    VFXSurfaceTypes vfxSurfaceType = VFXSurfaceTypes.none;
                    if (surface)
                        vfxSurfaceType = surface.surfaceType;
                    else
                        vfxSurfaceType = Utils.GetTerrainSurfaceType(position, normal, VFXSurfaceTypes.sand);

                    FMOD.Studio.EventInstance fmodEvent = Utils.GetFMODEvent(__instance.hitSound, __instance.transform.position);
                    fmodEvent.setParameterValueByIndex(__instance.surfaceParamIndex, (int)vfxSurfaceType);
                    fmodEvent.start();
                    fmodEvent.release();
                }
                Utils.PlayFMODAsset(Player.main.IsUnderwater() ? __instance.swingWaterSound : __instance.swingSound, __instance.transform.position);
                return false;
            }

            [HarmonyPostfix]
            [HarmonyPatch("OnToolUseAnim")]
            public static void OnToolUseAnimPostfix(Knife __instance)
            {
                if (!Player.main.guiHand.activeTarget)
                    return;

                BreakableResource breakableResource = Player.main.guiHand.activeTarget.GetComponent<BreakableResource>();
                if (breakableResource)
                {
                    breakableResource.BreakIntoResources();
                    //AddDebug("BreakableResource");
                }
                Pickupable pickupable = Player.main.guiHand.activeTarget.GetComponent<Pickupable>();
                if (pickupable)
                {
                    TechType techType = pickupable.GetTechType();
                    if (PickupablePatch.notPickupableResources.Contains(techType))
                    {
                        Rigidbody rb = pickupable.GetComponent<Rigidbody>();
                        if (rb && rb.isKinematic)  // attached to wall
                            pickupable.OnHandClick(Player.main.guiHand);
                    }
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch("GiveResourceOnDamage")]
            public static void GiveResourceOnDamagePrefix(Knife __instance, GameObject target, bool isAlive, bool wasAlive)
            {
                //AddDebug("GiveResourceOnDamage ");
                knifeTargetPos = target.transform.position;
                giveResourceOnDamage = true;
            }

        }

        public static void AddToInventoryOrSpawn(TechType techType, int num)
        {
            for (int i = 0; i < num; ++i)
            {
                if (Inventory.main.HasRoomFor(techType))
                    CraftData.AddToInventory(techType);
                else
                { // spawn position from AddToInventory can be behind object
                    AddError(Language.main.Get("InventoryFull"));
                    Vector3 pos = default;
                    if (knifeTargetPos != default)
                    {
                        Transform camTr = MainCamera.camera.transform;
                        float x = Mathf.Lerp(knifeTargetPos.x, camTr.position.x, .5f);
                        float y = camTr.position.y + camTr.forward.y * 3f; // fix for creepvine 
                        float z = Mathf.Lerp(knifeTargetPos.z, camTr.position.z, .5f);
                        pos = new Vector3(x, y, z);
                        //AddDebug("spawn Pos " + pos);
                    }
                    CoroutineHost.StartCoroutine(Util.Spawn(techType, pos));
                }
            }
        }

        [HarmonyPatch(typeof(CraftData), "AddToInventory")]
        class CraftData_AddToInventory_Patch
        {
            static bool Prefix(CraftData __instance, TechType techType, int num = 1, bool noMessage = false, bool spawnIfCantAdd = true)
            {
                //AddDebug("AddToInventory Prefix giveResourceOnDamage " + techType + " " + num);
                if (giveResourceOnDamage && !spawnIfCantAdd)
                {
                    AddToInventoryOrSpawn(techType, num);
                    giveResourceOnDamage = false;
                    return false;
                }
                return true;
            }
        }

    }
}
