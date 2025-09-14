using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static ErrorMessage;


namespace Tweaks_Fixes
{
    internal class Exosuit_movement
    {
        public static Vector3 moveDir;

        [HarmonyPatch(typeof(GameInput), "GetMoveDirection")]
        class GameInput_GetMoveDirection_Patch
        {
            static void Postfix(GameInput __instance, ref Vector3 __result)
            {
                if (!Main.gameLoaded || __result == Vector3.zero || moveDir == __result)
                    return;

                if (!ConfigToEdit.disableExosuitSidestep.Value && ConfigMenu.exosuitSpeedMult.Value == 1)
                    return;

                if (Player.main.currentMountedVehicle is Exosuit)
                {
                    //AddDebug("Exosuit z " + z);
                    if (ConfigToEdit.disableExosuitSidestep.Value)
                        __result.x = 0;

                    __result *= ConfigMenu.exosuitSpeedMult.Value;
                    moveDir = __result;
                }
            }
        }

        [HarmonyPatch(typeof(Exosuit))]
        class Exosuit_Patch
        {
            [HarmonyPostfix, HarmonyPatch("Update")]
            public static void UpdatePostfix(Exosuit __instance)
            {
                if (!Main.gameLoaded || !ConfigToEdit.exosuitThrusterWithoutLimit.Value || !__instance.GetPilotingMode())
                    return;

                Vector3 input = AvatarInputHandler.main.IsEnabled() ? GameInput.GetMoveDirection() : Vector3.zero;
                bool thrusterOn = input.y > 0;
                bool hasPower = __instance.IsPowered() && __instance.liveMixin.IsAlive();
                bool boosting = GameInput.GetButtonHeld(GameInput.Button.Sprint);
                bool consumeMorePower = thrusterOn || boosting;
                __instance.GetEnergyValues(out float charge, out float capacity);
                __instance.thrustPower = Util.NormalizeTo01range(charge, 0, capacity);
                if (consumeMorePower && hasPower && GameModeManager.GetOption<bool>(GameOption.TechnologyRequiresPower))
                {
                    float energyCost = __instance.thrustConsumption * Time.deltaTime;
                    //AddDebug("thrustConsumption " + __instance.thrustConsumption);
                    __instance.ConsumeEngineEnergy(energyCost);
                }
            }

            [HarmonyPrefix, HarmonyPatch("ApplyJumpForce")]
            static bool ApplyJumpForcePrefix(Exosuit __instance)
            {
                if (!ConfigToEdit.fixExosuitJumpParticleFX.Value)
                    return true;

                if (__instance.timeLastJumped + 1f > Time.time)
                    return false;

                if (__instance.onGround)
                {
                    Utils.PlayFMODAsset(__instance.jumpSound, __instance.transform);
                    if (__instance.IsUnderwater())
                    {
                        if (Physics.Raycast(new Ray(__instance.transform.position, Vector3.down), out RaycastHit hitInfo, 10f))
                        {
                            TerrainChunkPieceCollider tcpc = hitInfo.collider.GetComponent<TerrainChunkPieceCollider>();
                            if (tcpc)
                            {
                                __instance.fxcontrol.Play(2);
                                //AddDebug("jumped on terrain ");
                            }
                            else
                                __instance.fxcontrol.Play(1);
                        }
                    }
                }
                __instance.ConsumeEngineEnergy(1.2f);
                float jumpForce = 5f;
                if (__instance.jumpJetsUpgraded)
                    jumpForce = 7f;

                __instance.useRigidbody.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
                __instance.timeLastJumped = Time.time;
                __instance.timeOnGround = 0f;
                __instance.onGround = false;
                return false;
            }

            [HarmonyPrefix, HarmonyPatch("OnLand")]
            static bool OnLandPrefix(Exosuit __instance)
            {
                if (!ConfigToEdit.fixExosuitJumpParticleFX.Value)
                    return true;

                Utils.PlayFMODAsset(__instance.landSound, __instance.bottomTransform);
                if (__instance.IsUnderwater())
                {
                    RaycastHit hitInfo;
                    if (Physics.Raycast(new Ray(__instance.transform.position, Vector3.down), out hitInfo, 10f))
                    {
                        //if (hitInfo.transform && hitInfo.transform.gameObject)
                        //{
                        //AddDebug("Landed on  " + hitInfo.transform.gameObject.name);
                        //VFXSurface surface = hitInfo.transform.gameObject.GetComponent<VFXSurface>();
                        TerrainChunkPieceCollider tcpc = hitInfo.collider.GetComponent<TerrainChunkPieceCollider>();
                        //if (surface)
                        //    AddDebug("surfaceType  " + surface.surfaceType);
                        if (tcpc)
                        {
                            __instance.fxcontrol.Play(4);
                            //AddDebug("Landed on terrain ");
                        }
                        else
                            __instance.fxcontrol.Play(3);
                    }
                }
                return false;
            }

            [HarmonyPostfix, HarmonyPatch("GetHUDValues")]
            [HarmonyPatch("GetHUDValues", new Type[] { typeof(float), typeof(float), typeof(float) }, new[] { ArgumentType.Out, ArgumentType.Out, ArgumentType.Out })]
            public static void GetHUDValuesPostfix(Exosuit __instance, float health, ref float power, ref float thrust)
            {
                if (Main.gameLoaded && ConfigToEdit.exosuitThrusterWithoutLimit.Value && __instance.GetPilotingMode())
                {
                    thrust = power;
                    //thrust = .4f;
                }
                //AddDebug($"GetHUDValues power {power} thrust {thrust}");
            }
        }


    }
}
