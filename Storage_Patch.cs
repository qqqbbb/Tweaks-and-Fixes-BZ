using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Storage_Patch
    {


        //[HarmonyPatch(typeof(StorageContainer), "OnHandHover")]
        class StorageContainer_OnHandHover_patch
        {
            public static bool Prefix(StorageContainer __instance, GUIHand hand)
            {
                //HandReticle.main.SetTextRaw(HandReticle.TextType.UseSubscript, "Subscript");
                // HandReticle.main.SetTextRaw(HandReticle.TextType.Use, "Use");
                //str = LanguageCache.GetButtonFormat("AirBladderUseTool", GameInput.Button.RightHand);
                //string buttonFormat = LanguageCache.GetButtonFormat("AirBladderConsumeOxygen", GameInput.Button.AltTool);
                if (!__instance.enabled || __instance.disableUseability)
                    return false;
                Constructable c = __instance.gameObject.GetComponent<Constructable>();
                if (c && !c.constructed)
                    return false;
                string text = HandReticle.main.GetText(__instance.hoverText, true, GameInput.Button.LeftHand);
                //HandReticle.main.SetText(HandReticle.TextType.Hand, __instance.hoverText, true, GameInput.Button.LeftHand);
                //HandReticle.main.SetText(HandReticle.TextType.HandSubscript, __instance.IsEmpty() ? "Empty" : string.Empty, true);
                ColoredLabel label = __instance.transform.parent.GetComponentInChildren<ColoredLabel>();
                if (label)
                {
                    //AddDebug("StorageContainer label ");
                    //HandReticle.main.SetText(HandReticle.TextType.Hand, label.stringEditLabel, true, GameInput.Button.AltTool);
                    //HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, false);

                    text = text + HandReticle.main.GetText(label.stringEditLabel, true, GameInput.Button.AltTool);
                }
                PickupableStorage storage = __instance.transform.parent.GetComponentInChildren<PickupableStorage>();
                if (storage)
                {
                    AddDebug("StorageContainer PickupableStorage ");
                }
                HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, text);
                HandReticle.main.SetIcon(HandReticle.IconType.Hand);
                return false;
            }
        }

        //[HarmonyPatch(typeof(ColoredLabel), "OnHandHover")]
        class ColoredLabel_OnHandHover_patch
        {
            public static bool Prefix(ColoredLabel __instance, GUIHand hand)
            {
                //AddDebug("ColoredLabel OnHandHover");
                StorageContainer container = __instance.transform.parent.transform.parent.GetComponentInChildren<StorageContainer>();
                if (container)
                {
                    AddDebug("ColoredLabel container ");
                }
                PickupableStorage storage = __instance.transform.parent.transform.parent.GetComponentInChildren<PickupableStorage>();
                if (storage)
                {
                    AddDebug("ColoredLabel storage ");
                }
                return true;
            }
        }


        //[HarmonyPatch(typeof(PickupableStorage), "OnHandHover")]
        class PickupableStorage_OnHandHover_patch
        {
            public static bool Prefix(PickupableStorage __instance, GUIHand hand)
            {
                //AddDebug("PickupableStorage OnHandHover");
                StorageContainer container = __instance.transform.parent.GetComponentInChildren<StorageContainer>();
                if (container)
                {
                    AddDebug("PickupableStorage container ");
                }
                PickupableStorage storage = __instance.transform.parent.GetComponentInChildren<PickupableStorage>();
                if (storage)
                {
                    AddDebug("PickupableStorage storage ");
                }
                return true;
            }
        }

    }
}
