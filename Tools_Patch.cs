using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Tools_Patch
    {
        static float originalIntensity = -1f;
        [HarmonyPatch(typeof(Knife), nameof(Knife.OnToolUseAnim))]
        class Knife_OnToolUseAnim_Postfix_Patch
        {
            public static void Postfix(Knife __instance)
            {
                if (!Main.guiHand.activeTarget)
                    return;

                BreakableResource breakableResource = Main.guiHand.activeTarget.GetComponent<BreakableResource>();
                if (breakableResource)
                {
                    breakableResource.BreakIntoResources();
                    //AddDebug("BreakableResource");
                }
                Pickupable pickupable = Main.guiHand.activeTarget.GetComponent<Pickupable>();
                if (pickupable)
                {
                    TechType techType = pickupable.GetTechType();
                    if (Main.config.notPickupableResources.Contains(techType))
                    {
                        Rigidbody rb = pickupable.GetComponent<Rigidbody>();
                        if (rb && rb.isKinematic)  // attached to wall
                            pickupable.OnHandClick(Main.guiHand);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(PlayerTool), nameof(PlayerTool.OnDraw))]
        class Knife_Awake_Patch
        {
            static float knifeRangeDefault = 0f;
            static float knifeDamageDefault = 0f;

            public static void Postfix(PlayerTool __instance)
            {
                Knife knife = __instance as Knife;
                if (knife)
                {
                    if (knifeRangeDefault == 0f)
                        knifeRangeDefault = knife.attackDist;
                    if (knifeDamageDefault == 0f)
                        knifeDamageDefault = knife.damage;

                    knife.attackDist = knifeRangeDefault * Main.config.knifeRangeMult;
                    knife.damage = knifeDamageDefault * Main.config.knifeDamageMult;
                    //AddDebug(" attackDist  " + knife.attackDist);
                    //AddDebug(" damage  " + knife.damage);
                }

            }
        }


        //[HarmonyPatch(typeof(ScannerTool), "Update")]
        class ScannerTool_Update_Patch
        {// SHOW power when equipped
            private static bool Prefix(ScannerTool __instance)
            {
                //PlayerTool playerTool = 
                //bool isDrawn = (bool)PlayerTool_get_isDrawn.Invoke(__instance, new object[] { });
                if (__instance.isDrawn)
                {
                    //float idleTimer = (float)ScannerTool_idleTimer.GetValue(__instance);
                    //AddDebug("useText1 " + HandReticle.main.useText1);
                    //AddDebug("useText2 " + HandReticle.main.useText2);
                    if (__instance.idleTimer > 0f)
                    {
                        __instance.idleTimer = Mathf.Max(0f, __instance.idleTimer - Time.deltaTime);
                        //string buttonFormat = LanguageCache.GetButtonFormat("ScannerSelfScanFormat", GameInput.Button.AltTool);
                        //               HandReticle.main.SetUseTextRaw(buttonFormat, null);
                    }
                }
                return false;
            }
        }

        //[HarmonyPatch(nameof(Flare.OnDraw))]
        [HarmonyPostfix]
        internal static void OnDraw(Flare __instance)
        {
            if (originalIntensity == -1f)
                originalIntensity = __instance.originalIntensity;

            //AddDebug("throwDuration " + __instance.throwDuration);
            //__instance.originalIntensity = originalIntensity * Main.config.flareIntensity;
        }

        //[HarmonyPatch(typeof(Flare), "OnDrop")]
        class Flare_OnDrop_Patch
        {
            public static bool Prefix(Flare __instance)
            {
                if (__instance.isThrowing)
                {
                    //__instance.GetComponent<Rigidbody>().AddForce(MainCamera.camera.transform.forward * __instance.dropForceAmount);
                    //__instance.GetComponent<Rigidbody>().AddTorque(__instance.transform.right * __instance.dropTorqueAmount);
                    //__instance.isThrowing = false;
                }
                //AddDebug("energyLeft " + __instance.energyLeft);
                if (__instance.energyLeft < 1800f)
                {
                    if (__instance.fxControl && !__instance.fxIsPlaying)
                        __instance.fxControl.Play(1);
                    __instance.fxIsPlaying = true;
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(Constructor), "OnEnable")]
        class Constructor_OnEnable_Patch
        {
            static void Postfix(Constructor __instance)
            {

                ImmuneToPropulsioncannon itpc = __instance.GetComponent<ImmuneToPropulsioncannon>();
                if (itpc)
                {
                    //AddDebug("OnEnable Constructor ");
                    UnityEngine.Object.Destroy(itpc);
                }
                //itpc.enabled = false;

            }
        }

        //[HarmonyPatch(typeof(RepulsionCannon), "ShootObject")]
        class RepulsionCannon_ShootObject_Patch
        {
            static void Prefix(RepulsionCannon __instance, Rigidbody rb, Vector3 velocity)
            {
                rb.constraints = RigidbodyConstraints.None;
                AddDebug("ShootObject " + rb.gameObject.name + " " + velocity);
                //AddDebug("constraints " + rb.constraints);
            }
        }

    }
}
