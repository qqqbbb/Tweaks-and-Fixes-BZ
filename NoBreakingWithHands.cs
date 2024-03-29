﻿using HarmonyLib;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(BreakableResource))]
    public static class OnHandClickPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("OnHandClick")]
        public static bool OnHandClickPrefix()
        {
            if (!Main.config.noBreakingWithHand)
                return true;

            Exosuit exosuit = Player.main.GetVehicle() as Exosuit;
            if (exosuit && exosuit.HasClaw())
                return true;

            PlayerTool tool = Inventory.main.GetHeldTool();
            if (tool && tool.GetComponent<Knife>() != null)
            {
                return true;
            }
            else
            {
                //Main.Message("no knife !");
                return false;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnHandHover")]
        public static bool OnHandHoverPrefix(BreakableResource __instance)
        {
            //AddDebug("BreakableResource OnHandHover");
            if (!Main.config.noBreakingWithHand)
                return true;

            Exosuit exosuit = Player.main.GetVehicle() as Exosuit;
            if (exosuit && exosuit.HasClaw())
                return true;

            Knife knife = Inventory.main.GetHeldTool() as Knife;
            if (knife)
            {
                HandReticle.main.SetText(HandReticle.TextType.Hand, __instance.breakText, true, GameInput.Button.LeftHand);
                //if (GameInput.GetButtonDown(GameInput.Button.RightHand))
                //{
                //    __instance.BreakIntoResources();
                //    AddDebug("RightHand");
                //}
            }
            else
                HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, Main.config.translatableStrings[12]);

            return false;
        }


    }


    [HarmonyPatch(typeof(Pickupable))]
    public static class PickupablePatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("OnHandClick")] // OnHandHover handled by GUIHand.OnUpdate
        public static bool PickupableOnHandClick(Pickupable __instance, GUIHand hand)
        {
            if (!Main.config.noBreakingWithHand)
                return true;

            if (!hand.IsFreeToInteract())
                return false;

            Exosuit exosuit = Player.main.GetVehicle() as Exosuit;
            if (exosuit && exosuit.HasClaw())
                return true;

            if (!Main.config.notPickupableResources.Contains(__instance.GetTechType()))
                return true;

            Rigidbody rb = __instance.GetComponent<Rigidbody>();

            if (rb == null)
                return true;

            if (rb.isKinematic) // attached to wall
            {
                Knife knife = Inventory.main.GetHeldTool() as Knife;
                if (knife)
                {
                    Player.main.guiHand.usedToolThisFrame = true;
                    knife.OnToolActionStart();
                    rb.isKinematic = false;
                    //return false;
                }
                return false;
            }
            return true;
        }
    }



}
