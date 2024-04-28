using HarmonyLib;
using System.Collections.Generic;
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
            if (!ConfigMenu.noBreakingWithHand.Value)
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
            //AddDebug("BreakableResource OnHandHover " + __instance.breakText);
            Exosuit exosuit = Player.main.currentMountedVehicle as Exosuit;
            //if (exosuit == null)
            //    return true;

            if (exosuit && !exosuit.HasClaw())
                return false;

            if (exosuit)
            {
                //AddDebug("leftArmType " + exosuit.leftArmType);
                //AddDebug("rightArmType " + exosuit.rightArmType);
                GameInput.Button button;
                if (exosuit.leftArmType == TechType.ExosuitClawArmModule)
                    button = GameInput.Button.LeftHand;
                else
                    button = GameInput.Button.RightHand;

                HandReticle.main.SetText(HandReticle.TextType.Hand, __instance.breakText, true, button);
                //HandReticle.main.SetIcon(HandReticle.IconType.Hand);
                return false;
            }
            if (!ConfigMenu.noBreakingWithHand.Value)
                return true;

            Knife knife = Inventory.main.GetHeldTool() as Knife;
            if (knife)
            {
                HandReticle.main.SetText(HandReticle.TextType.Hand, __instance.breakText, true, GameInput.Button.LeftHand);
            }
            else
                HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, Language.main.Get("TF_need_knife_to_break_outcrop"));
            
            return false;
        }


    }


    [HarmonyPatch(typeof(Pickupable))]
    public static class PickupablePatch
    {
        public static HashSet<TechType> notPickupableResources = new HashSet<TechType>
        {{TechType.Salt}, {TechType.Quartz}, {TechType.AluminumOxide}, {TechType.Lithium} , {TechType.Sulphur}, {TechType.Diamond}, {TechType.Kyanite}, {TechType.Magnetite}, {TechType.Nickel}, {TechType.UraniniteCrystal}  };

        [HarmonyPrefix]
        [HarmonyPatch("OnHandClick")] // OnHandHover handled by GUIHand.OnUpdate
        public static bool PickupableOnHandClick(Pickupable __instance, GUIHand hand)
        {
            if (!ConfigMenu.noBreakingWithHand.Value)
                return true;

            if (!hand.IsFreeToInteract())
                return false;

            Exosuit exosuit = Player.main.GetVehicle() as Exosuit;
            if (exosuit && exosuit.HasClaw())
                return true;

            if (!notPickupableResources.Contains(__instance.GetTechType()))
                return true;

            Rigidbody rb = __instance.GetComponent<Rigidbody>();

            if (rb == null)
                return true;

            if (rb.isKinematic) // attached to seabed
            {
                Knife knife = Inventory.main.GetHeldTool() as Knife;
                if (knife)
                {
                    Player.main.guiHand.usedToolThisFrame = true;
                    knife.OnToolActionStart();
                    rb.isKinematic = false;
                }
                return false;
            }
            return true;
        }
    }



}
