using HarmonyLib;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(BreakableResource), nameof(BreakableResource.OnHandClick))]
    public static class OnHandClickPatch
    {
        public static bool Prefix()
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
    }

    [HarmonyPatch(typeof(BreakableResource), nameof(BreakableResource.OnHandHover))]
    public static class OnHandHoverPatch
    {
        public static bool Prefix(BreakableResource __instance)
        {
            //AddDebug("BreakableResource OnHandHover");
            //if (Player.main.inExosuit)
            //{
            //    HandReticle.main.SetInteractText(__instance.breakText);
            //    return false;
            //}
            //if (__instance.GetComponent<LiveMixin>() != null)
            //    AddDebug("BreakableResource LiveMixin");
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
                HandReticle.main.SetText(HandReticle.TextType.Hand, Main.config.translatableStrings[12], true, GameInput.Button.LeftHand);

            return false;
        }
    }

    [HarmonyPatch(typeof(Pickupable))]
    public static class PickupablePatch
    {
        public static bool CanCollect(Pickupable instance, TechType techType)
        {
            if (!Main.config.noBreakingWithHand)
                return true;

            Exosuit exosuit = Player.main.GetVehicle() as Exosuit;
            if (exosuit && exosuit.HasClaw())
                return true;

            if (Main.config.notPickupableResources.Contains(techType))
            {
                Rigidbody rb = instance.GetComponent<Rigidbody>();
                if (rb == null)
                    return true;

                if (rb.isKinematic)  // attached to terrain
                {
                    Knife knife = Inventory.main.GetHeldTool() as Knife;
                    if (knife)
                    {
                        return true;
                    }
                    HandReticle.main.SetText(HandReticle.TextType.Hand, Main.config.translatableStrings[13], false);

                    return false;
                }
            }
            return true;
        }

        [HarmonyPatch(nameof(Pickupable.OnHandHover))]
        [HarmonyPrefix]
        public static bool PickupableOnHandHover(Pickupable __instance)
        {
            //AddDebug("Can Collect " + CanCollect(__instance, __instance.GetTechType()));
            //AddDebug("attached " +  __instance.attached);
            return CanCollect(__instance, __instance.GetTechType());
        }

        [HarmonyPatch(nameof(Pickupable.OnHandClick))]
        [HarmonyPrefix]
        public static bool PickupableOnHandClick(Pickupable __instance)
        {
            if (!Main.config.noBreakingWithHand)
                return true;

            Exosuit exosuit = Player.main.GetVehicle() as Exosuit;
            if (exosuit && exosuit.HasClaw())
                return true;

            if (!Main.config.notPickupableResources.Contains(__instance.GetTechType()))
                return true;

            Rigidbody rb = __instance.GetComponent<Rigidbody>();
            Knife knife = Inventory.main.GetHeldTool() as Knife;
            if (rb == null)
                return true;

            if (rb.isKinematic) // attached to wall
            {
                if (knife)
                {
                    Main.guiHand.usedToolThisFrame = true;
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
