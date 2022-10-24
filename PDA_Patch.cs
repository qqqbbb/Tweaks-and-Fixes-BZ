using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using System.Text;
using RootMotion.FinalIK;
using UWE;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class PDA_Patch
    { 

        static void showPDA(PDA pda)
        {
            //AddDebug("showPDA");
            Transform cameraTr = Camera.main.transform;
            //Transform cameraTr = pda.cameraOffsetTransform;
            pda.transform.SetParent(cameraTr);
            pda.transform.forward = cameraTr.forward;
            pda.transform.Rotate(new Vector3(0f, 180f, 0f));
            Vector3 pos = cameraTr.position;
            if (cameraTr.forward.y > 0f)
            {
                pda.transform.position = pos + .156f * cameraTr.forward;
                Vector3 pdaLocalPos = pda.transform.localPosition;
                float posZ = Main.NormalizeToRange(cameraTr.forward.y, 0f, 1f, .16f, .22f);
                //float aValue;
                //float normal = Mathf.InverseLerp(0f, 1f, cameraTr.forward.y);
                Vector3 pdaLookForwardPos = new Vector3(pdaLocalPos.x, pdaLocalPos.y, .16f);
                Vector3 pdaUpLookPos = new Vector3(pdaLocalPos.x, pdaLocalPos.y, .2f);
                Vector3 v3 = Vector3.Slerp(pda.transform.forward, pda.transform.up, cameraTr.forward.y);
                AddDebug("cameraTr.forward.y " + cameraTr.forward.y + " " + v3.z);
                //float bValue = Mathf.Slerp(bLow, bHigh, normal);
                //transform = __instance.cameraOffsetTransform;
                //pda.transform.localPosition = new Vector3(pdaLocalPos.x, pdaLocalPos.y, v3.z);
                //pda.transform.localPosition = new Vector3(pdaLocalP os.x, pdaLocalPos.y, v3.z);
                //pda.transform.localPosition.z *= cameraTr.forward.y;
            }
            else
            {
                //pda.transform.position = pos + .156f * cameraTr.forward;
                pda.transform.position = pos + .17f * cameraTr.forward;
                //MainCameraControl.main.ForceUpdatePDAOffset();
            }

            //pda.transform.Translate(-.2f * Camera.main.transform.right);
            pda.transform.position = pda.transform.position - .2f * cameraTr.right;
        }

        //[HarmonyPatch(typeof(MainCameraControl), "Update")]
        class MainCameraControl_Update_Patch
        {
            static bool Prefix(MainCameraControl __instance)
            {
                float deltaTime = Time.deltaTime;
                __instance.swimCameraAnimation = !Player.main.IsUnderwater() ? Mathf.Clamp01(__instance.swimCameraAnimation - deltaTime) : Mathf.Clamp01(__instance.swimCameraAnimation + deltaTime);
                Vector3 velocity = __instance.playerController.velocity;
                bool flag1 = false;
                bool flag2 = false;
                bool flag3 = false;
                bool inExosuit = Player.main.inExosuit;
                bool flag4 = false;
                bool flag5 = uGUI_BuilderMenu.IsOpen();
                bool flag6 = false;
                if (Player.main != null)
                {
                    flag1 = Player.main.GetPDA().isInUse;
                    flag3 = Player.main.motorMode == Player.MotorMode.Vehicle;
                    flag2 = flag1 | flag3 || __instance.cinematicMode;
                    flag6 = Player.main.inHovercraft;
                    if (UWEXR.XRSettings.enabled && VROptions.gazeBasedCursor)
                        flag2 |= flag5;
                }
                if (flag2 != __instance.wasInLockedMode || __instance.lookAroundMode != __instance.wasInLookAroundMode)
                {
                    __instance.camRotationX = 0f;
                    __instance.camRotationY = 0f;
                    __instance.wasInLockedMode = flag2;
                    __instance.wasInLookAroundMode = __instance.lookAroundMode;
                }
                bool flag7 = (!__instance.cinematicMode || __instance.lookAroundMode) && (!flag1 && __instance.mouseLookEnabled) && (flag3 || AvatarInputHandler.main == null || AvatarInputHandler.main.IsEnabled() || Builder.isPlacing);
                if (flag3 && !UWEXR.XRSettings.enabled && (!inExosuit && !flag4))
                    flag7 = false;
                if (__instance.deathSequence)
                    flag7 = false;
                Transform transform = __instance.transform;
                float num1 = flag1 || __instance.lookAroundMode || Player.main.GetMode() == Player.Mode.LockedPiloting ? 1f : -1f;
                if (!flag2 || __instance.cinematicMode && !__instance.lookAroundMode)
                {
                    __instance.cameraOffsetTransform.localEulerAngles = UWE.Utils.LerpEuler(__instance.cameraOffsetTransform.localEulerAngles, Vector3.zero, deltaTime * 5f);
                }
                else if (!Main.config.instantPDA)
                {
                    transform = __instance.cameraOffsetTransform;
                    __instance.rotationY = Mathf.LerpAngle(__instance.rotationY, 0f, PDA.deltaTime * 15f);
                    __instance.transform.localEulerAngles = new Vector3(Mathf.LerpAngle(__instance.transform.localEulerAngles.x, 0f, PDA.deltaTime * 15f), __instance.transform.localEulerAngles.y, 0f);
                    __instance.cameraUPTransform.localEulerAngles = UWE.Utils.LerpEuler(__instance.cameraUPTransform.localEulerAngles, Vector3.zero, PDA.deltaTime * 15f);
                }
                if (!UWEXR.XRSettings.enabled)
                {
                    float num2 = __instance.camPDAZOffset * num1 * PDA.deltaTime / __instance.cameraPDAZoomDuration;
                    Vector3 localPosition = __instance.cameraOffsetTransform.localPosition;
                    localPosition.z = Mathf.Clamp(localPosition.z + num2, 0.0f, __instance.camPDAZOffset);
                    __instance.cameraOffsetTransform.localPosition = localPosition;
                }
                else
                    __instance.animator.SetFloat(MainCameraControl.pdaDistanceParamId, VROptions.pdaDistance);
                Vector2 vector2 = Vector2.zero;
                if (flag7 && FPSInputModule.current.lastGroup == null)
                {
                    Vector2 lookDelta = GameInput.GetLookDelta();
                    if (UWEXR.XRSettings.enabled && VROptions.disableInputPitch)
                        lookDelta.y = 0.0f;
                    if (inExosuit | flag4)
                        lookDelta.x = 0.0f;
                    vector2 = lookDelta * Player.main.mesmerizedSpeedMultiplier;
                    if (Player.main.frozenMixin.IsFrozen())
                        vector2 *= Player.main.frozenMixin.cameraSpeedMultiplier;
                }
                if (__instance.deathSequence)
                    vector2 = new Vector2(Mathf.Cos(Time.time * 8.5f) * 25f * Time.deltaTime, (-50f * Time.deltaTime + Mathf.Cos(Time.time * 8f) * 9f * Time.deltaTime));
                __instance.UpdateCamShake();
                if (__instance.cinematicMode && !__instance.lookAroundMode)
                {
                    if (__instance.cinematicOverrideRotation)
                    {
                        __instance.camRotationX = __instance.transform.localEulerAngles.y;
                        __instance.camRotationY = -__instance.transform.localEulerAngles.x;
                    }
                    else
                    {
                        __instance.camRotationX = Mathf.LerpAngle(__instance.camRotationX, 0f, deltaTime * 2f);
                        __instance.camRotationY = Mathf.LerpAngle(__instance.camRotationY, 0f, deltaTime * 2f);
                        __instance.transform.localEulerAngles = new Vector3(-__instance.camRotationY, __instance.camRotationX, 0f);
                    }
                }
                else if (flag2)
                {
                    if (!UWEXR.XRSettings.enabled)
                    {
                        bool flag8 = ((__instance.lookAroundMode || inExosuit ? 0 : (!flag4 ? 1 : 0)) | (flag1 ? 1 : 0)) != 0;
                        bool flag9 = !__instance.lookAroundMode | flag1;
                        __instance.camRotationX += vector2.x;
                        __instance.camRotationY += vector2.y;
                        __instance.camRotationX = Mathf.Clamp(__instance.camRotationX, -60f, 60f);
                        __instance.camRotationY = Mathf.Clamp(__instance.camRotationY, -60f, 60f);
                        if (flag6)
                        {
                            __instance.cameraOffsetTransform.eulerAngles = __instance.vehicleOverrideHeadRot;
                        }
                        else
                        {
                            if (flag9)
                                __instance.camRotationX = Mathf.LerpAngle(__instance.camRotationX, 0f, PDA.deltaTime * 10f);
                            if (flag8)
                                __instance.camRotationY = Mathf.LerpAngle(__instance.camRotationY, 0f, PDA.deltaTime * 10f);
                            __instance.cameraOffsetTransform.localEulerAngles = new Vector3(-__instance.camRotationY, __instance.camRotationX, 0f);
                        }
                    }
                }
                else
                {
                    __instance.rotationX += vector2.x;
                    __instance.rotationY += vector2.y;
                    __instance.rotationY = Mathf.Clamp(__instance.rotationY, __instance.minimumY, __instance.maximumY);
                    __instance.cameraUPTransform.localEulerAngles = new Vector3(Mathf.Min(0f, -__instance.rotationY), 0f, 0f);
                    transform.localEulerAngles = new Vector3(Mathf.Max(0.0f, -__instance.rotationY), __instance.rotationX, 0f);
                }
                __instance.UpdateStrafeTilt();
                Vector3 vector3_1 = __instance.transform.localEulerAngles + new Vector3(0f, 0f, (__instance.cameraAngleMotion.y * __instance.cameraTiltMod + __instance.strafeTilt * 0.5f));
                float num3 = 0.0f - __instance.skin;
                if (!flag2 && __instance.GetCameraBob())
                {
                    __instance.smoothedSpeed = Mathf.MoveTowards(__instance.smoothedSpeed, Mathf.Min(1f, velocity.magnitude / 5f), deltaTime);
                    num3 += ((Mathf.Sin(Time.time * 6f) - 1f) * (0.02f + __instance.smoothedSpeed * 0.15f)) * __instance.swimCameraAnimation;
                }
                if (__instance.impactForce > 0f)
                {
                    __instance.impactBob = Mathf.Min(0.9f, __instance.impactBob + __instance.impactForce * deltaTime);
                    __instance.impactForce -= (Mathf.Max(1f, __instance.impactForce) * deltaTime * 5f);
                }
                float y = num3 - __instance.impactBob - __instance.stepAmount;
                if (__instance.impactBob > 0.0)
                    __instance.impactBob = Mathf.Max(0f, __instance.impactBob - (Mathf.Pow(__instance.impactBob, 0.5f) * deltaTime * 3f));
                __instance.stepAmount = Mathf.Lerp(__instance.stepAmount, 0f, deltaTime * Mathf.Abs(__instance.stepAmount));
                float max = __instance.shakeAmount / 20f;
                __instance.shakeOffset = Vector3.Lerp(__instance.shakeOffset, __instance.initialOffset + new Vector3(UnityEngine.Random.Range(-max, max), UnityEngine.Random.Range(-max, max), UnityEngine.Random.Range(-max, max)) * __instance.camShake, deltaTime * 20f);
                Vector3 vector3_2 = flag6 ? __instance.vehicleOverrideHeadPos : Vector3.zero;
                __instance.transform.localPosition = new Vector3(0f, y, 0f) + vector3_2 + __instance.shakeOffset;
                __instance.transform.localEulerAngles = vector3_1;
                if (Player.main.motorMode == Player.MotorMode.Vehicle)
                    __instance.transform.localEulerAngles = Vector3.zero;
                Vector3 vector3_3 = new Vector3(Mathf.LerpAngle(__instance.viewModel.localEulerAngles.x, 0.0f, deltaTime * 5f), __instance.transform.localEulerAngles.y, 0f);
                Vector3 vector3_4 = __instance.transform.localPosition;
                if (UWEXR.XRSettings.enabled)
                {
                    vector3_3.y = !flag2 || flag3 ? 0f : __instance.viewModelLockedYaw;
                    if (!flag3 && !__instance.cinematicMode)
                    {
                        if (!flag2)
                        {
                            Quaternion rotation = __instance.playerController.forwardReference.rotation;
                            Quaternion quaternion = __instance.gameObject.transform.parent.rotation.GetInverse() * rotation;
                            vector3_3.y = quaternion.eulerAngles.y;
                        }
                        vector3_4 = __instance.gameObject.transform.parent.worldToLocalMatrix.MultiplyPoint(__instance.playerController.forwardReference.position);
                    }
                }
                __instance.viewModel.transform.localEulerAngles = vector3_3;
                __instance.viewModel.transform.localPosition = vector3_4;
                return false;
            }

        }


        //[HarmonyPatch(typeof(ArmsController), "InstallAnimationRules")]
        class ArmsController_Patch
        {
            static bool Prefix(ArmsController __instance)
            {
                if (!Main.config.instantPDA)
                    __instance.GetComponent<ConditionRules>().AddCondition((ConditionRules.ConditionFunction)(() => __instance.player.GetPDA().isInUse)).WhenChanges((ConditionRules.BoolHandlerFunction)(newValue => SafeAnimator.SetBool(__instance.animator, "using_pda", newValue)));
                __instance.GetComponent<ConditionRules>().AddCondition((ConditionRules.ConditionFunction)(() => (Inventory.main.GetHeldTool() as Welder) != null)).WhenChanges((ConditionRules.BoolHandlerFunction)(newValue => SafeAnimator.SetBool(__instance.animator, "holding_welder", newValue)));
                __instance.GetComponent<ConditionRules>().AddCondition((ConditionRules.ConditionFunction)(() =>
                {
                    float y = __instance.player.gameObject.transform.position.y;
                    return y > Ocean.GetOceanLevel() - 1.0 && y < Ocean.GetOceanLevel() + 1.0 && !Player.main.IsInside() && !Player.main.forceWalkMotorMode;
                })).WhenChanges((ConditionRules.BoolHandlerFunction)(newValue => SafeAnimator.SetBool(__instance.animator, "on_surface", newValue)));
                __instance.GetComponent<ConditionRules>().AddCondition((ConditionRules.ConditionFunction)(() => __instance.player.GetInMechMode())).WhenChanges((ConditionRules.BoolHandlerFunction)(newValue => SafeAnimator.SetBool(__instance.animator, "using_mechsuit", newValue)));
                return false;
            }
        }

        //[HarmonyPatch(typeof(SNCameraRoot), "SetFov")]
        class CameraToPlayerManager_Update_Patch
        {
            static bool Prefix(SNCameraRoot __instance, float fov)
            {
                AddDebug("SNCameraRoot SetFov fov " + fov);
                //AddDebug("SNCameraRoot SetFov isInUse " + Main.pda.isInUse);
                return false;
            }
        }

        //[HarmonyPatch(typeof(PDAOffsetHelper), "SetFOV")]
        class PDAOffsetHelper_SyncFieldOfView_Patch
        {
            static bool Prefix(PDAOffsetHelper __instance)
            {
                AddDebug("PDAOffsetHelper SetFOV ");
                //AddDebug("SNCameraRoot SetFov isInUse " + Main.pda.isInUse);
                return false;
            }
        }


        //[HarmonyPatch(typeof(uGUI_PDA), "RevealContentRoutine")]
        class uGUI_PDA_RevealContentRoutine_Patch
        {
            static void Postfix(uGUI_PDA __instance)
            {
                if (__instance.revealContentRoutine != null)
                {
                    AddDebug("uGUI_PDA RevealContentRoutine revealContentRoutine");
                }
                else
                    AddDebug("uGUI_PDA RevealContentRoutine revealContentRoutine null");
                //AddDebug("SNCameraRoot SetFov isInUse " + Main.pda.isInUse);
                //return false;
            }
        }

    }
}
