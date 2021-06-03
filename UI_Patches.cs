using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using HarmonyLib;
using System.Text;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class UI_Patches
    {
        private static IEnumerator IntroSequence(ExpansionIntroManager introManager, uGUI_ExpansionIntro __instance)
        {
            IntroVignette.isIntroActive = true;
            if (FPSInputModule.current)
                FPSInputModule.current.lockPauseMenu = true;
            __instance.fader.SetState(true);
            Player.main.playerController.inputEnabled = false;
            __instance.PauseGameTime();
            yield return new WaitForSecondsRealtime(0.5f);
            MainMenuMusic.Stop();
            introManager.TriggerStartScreenAudio();
            __instance.mainText.SetText("");
            yield return new WaitForSecondsRealtime(2f);
            while (!LargeWorldStreamer.main.IsReady() || !LargeWorldStreamer.main.IsWorldSettled())
                yield return new WaitForSecondsRealtime(1f);
            __instance.mainText.SetText(Language.main.Get("PressAnyButton"));
            __instance.mainText.SetState(true);
            VRLoadingOverlay.Hide();
            if (!QuickLaunchHelper.IsQuickLaunching())
            {
                while (!UnityEngine.Input.anyKeyDown)
                    yield return null;
            }
            if (FPSInputModule.current)
                FPSInputModule.current.lockPauseMenu = false;
            yield return introManager.Play(Player.main, __instance);
            IntroVignette.isIntroActive = false;
            __instance.ResumeGameTime();
            __instance.StopCoroutine(__instance.coroutine);
            __instance.coroutine = (Coroutine)null;
            //gui.StartCoroutine(gui.ControlsHints());
        }

        [HarmonyPatch(typeof(uGUI_ExpansionIntro), "Play")]
        class uGUI_ExpansionIntro_Plays_Patch
        { 
            static bool Prefix(uGUI_ExpansionIntro __instance, ExpansionIntroManager introManager)
            {
                //AddDebug("uGUI_ExpansionIntro Play");
                if (!Main.config.disableHints)
                    return true;

                if (__instance.showing)
                    return false;
                //AddDebug("StartCoroutine IntroSequence");
                __instance.coroutine = __instance.StartCoroutine(IntroSequence(introManager, __instance));
                InputHandlerStack.main.Push((IInputHandler)__instance);
                return false;
            }
        }

        //[HarmonyPatch(typeof(GUIHand), "UpdateActiveTarget")]
        class GUIHand_UpdateActiveTarget_Patch
        {
            public static void Postfix(GUIHand __instance)
            {
                //if (Main.config.eatFishOnRelease && action == ItemAction.Drop && Main.IsEatableFish(pickupable.gameObject))
                //{
                AddDebug("activeTarget " + __instance.activeTarget.name);
                //    __result = "ItemActionEat";
                //}
            }
        }

        [HarmonyPatch(typeof(GUIHand), "GetActionString")]
        class GUIHand_GetActionString_Patch
        { 
            public static void Postfix(GUIHand __instance, ItemAction action, Pickupable pickupable, ref string __result)
            {
                if (Main.config.eatFishOnRelease && action == ItemAction.Drop && Main.IsEatableFish(pickupable.gameObject))
                {
                    //AddDebug("GetActionString " + action);
                    __result = "ItemActionEat";
                }
            }
        }

        //[HarmonyPatch(typeof(GUIHand), "OnUpdate")]
        class GUIHand_OnUpdate_Prefix_Patch
        { // UI tells you if looking at dead fish 
            public static bool Prefix(GUIHand __instance)
            {
                __instance.usedToolThisFrame = false;
                __instance.usedAltAttackThisFrame = false;
                __instance.suppressTooltip = false;
                GameInput.Button button1 = GameInput.Button.LeftHand;
                GameInput.Button button2 = GameInput.Button.RightHand;
                GameInput.Button button3 = GameInput.Button.Reload;
                GameInput.Button button4 = GameInput.Button.Exit;
                GameInput.Button button5 = GameInput.Button.AltTool;
                GameInput.Button button6 = GameInput.Button.AutoMove;
                GameInput.Button button7 = GameInput.Button.PDA;
                __instance.UpdateInput(button1);
                __instance.UpdateInput(button2);
                __instance.UpdateInput(button3);
                __instance.UpdateInput(button4);
                __instance.UpdateInput(button5);
                __instance.UpdateInput(GameInput.Button.Answer);
                __instance.UpdateInput(GameInput.Button.Exit);
                __instance.UpdateInput(button6);
                __instance.UpdateInput(button7);
                if (AvatarInputHandler.main.IsEnabled() && !uGUI.isIntro && !uGUI.isLoading)
                {
                    uGUI_PopupNotification main = uGUI_PopupNotification.main;
                    if (main != null && main.id == "Call")
                    {
                        if (__instance.GetInput(GameInput.Button.Answer, GUIHand.InputState.Down))
                        {
                            __instance.UseInput(GameInput.Button.Answer, GUIHand.InputState.Down | GUIHand.InputState.Held | GUIHand.InputState.Up);
                            main.Answer();
                            GameInput.ClearInput();
                        }
                        else if (__instance.GetInput(GameInput.Button.Exit, GUIHand.InputState.Down))
                        {
                            __instance.UseInput(GameInput.Button.Answer, GUIHand.InputState.Down | GUIHand.InputState.Held | GUIHand.InputState.Up);
                            main.Decline();
                            GameInput.ClearInput();
                        }
                    }
                }
                if (__instance.player.IsFreeToInteract() && (AvatarInputHandler.main.IsEnabled() || Builder.inputHandlerActive))
                {
                    string text = string.Empty;
                    InventoryItem heldItem = Inventory.main.quickSlots.heldItem;
                    Pickupable pickupable = heldItem?.item;
                    PlayerTool playerTool = pickupable != null ? pickupable.GetComponent<PlayerTool>() : (PlayerTool)null;
                    bool flag = playerTool != null && playerTool is DropTool;
                    EnergyMixin energyMixin = (EnergyMixin)null;
                    if (playerTool != null)
                    {
                        text = playerTool.GetCustomUseText();
                        energyMixin = playerTool.GetComponent<EnergyMixin>();
                    }
                    ItemAction action = ItemAction.None;
                    if (playerTool == null | flag && heldItem != null)
                    {
                        ItemAction allItemActions = Inventory.main.GetAllItemActions(heldItem);
                        if ((allItemActions & ItemAction.Eat) != ItemAction.None)
                            action = ItemAction.Eat;
                        else if ((allItemActions & ItemAction.Use) != ItemAction.None)
                            action = ItemAction.Use;
                        if (action == ItemAction.Eat)
                        {
                            Plantable component1 = pickupable.GetComponent<Plantable>();
                            LiveMixin component2 = pickupable.GetComponent<LiveMixin>();
                            if (component1 == null && component2 != null)
                                action = ItemAction.None;
                        }
                        if (action == ItemAction.None && (allItemActions & ItemAction.Drop) != ItemAction.None)
                            action = ItemAction.Drop;
                        if (action != ItemAction.None)
                            HandReticle.main.SetText(HandReticle.TextType.Use, GUIHand.GetActionString(action, pickupable), true, GameInput.Button.RightHand);
                    }
                    if (energyMixin != null && energyMixin.allowBatteryReplacement)
                    {
                        int num = Mathf.FloorToInt(energyMixin.GetEnergyScalar() * 100f);
                        if (__instance.cachedTextEnergyScalar != num)
                        {
                            __instance.cachedEnergyHudText = num > 0 ? Language.main.GetFormat<float>("PowerPercent", energyMixin.GetEnergyScalar()) : LanguageCache.GetButtonFormat("ExchangePowerSource", GameInput.Button.Reload);
                            __instance.cachedTextEnergyScalar = num;
                        }
                        HandReticle.main.SetTextRaw(HandReticle.TextType.Use, text);
                        HandReticle.main.SetTextRaw(HandReticle.TextType.UseSubscript, __instance.cachedEnergyHudText);
                    }
                    else if (!string.IsNullOrEmpty(text))
                        HandReticle.main.SetTextRaw(HandReticle.TextType.Use, text);

                    if (AvatarInputHandler.main.IsEnabled())
                    {
                        if (__instance.grabMode == GUIHand.GrabMode.None)
                            __instance.UpdateActiveTarget();
                        HandReticle.main.SetTargetDistance(__instance.activeHitDistance);
                        if (__instance.activeTarget != null && !__instance.suppressTooltip)
                        {
                            TechType techType = CraftData.GetTechType(__instance.activeTarget);
                            if (techType != TechType.None)
                            {
                                AddDebug(" techType " + techType);
                                HandReticle.main.SetText(HandReticle.TextType.Hand, "555", true);
                            }

                            GUIHand.Send(__instance.activeTarget, HandTargetEventType.Hover, __instance);
                        }
                        if (Inventory.main.container.Contains(TechType.Scanner))
                        {
                            PDAScanner.UpdateTarget(8f);
                            PDAScanner.ScanTarget scanTarget = PDAScanner.scanTarget;
                            if (scanTarget.isValid && PDAScanner.CanScan(scanTarget) == PDAScanner.Result.Scan)
                                uGUI_ScannerIcon.main.Show();
                        }
                        if (playerTool != null && (!flag || action == ItemAction.Drop || action == ItemAction.None))
                        {
                            if (__instance.GetInput(button2, GUIHand.InputState.Down))
                            {
                                if (playerTool.OnRightHandDown())
                                {
                                    __instance.UseInput(button2, GUIHand.InputState.Down | GUIHand.InputState.Held | GUIHand.InputState.Up);
                                    __instance.usedToolThisFrame = true;
                                    playerTool.OnToolActionStart();
                                }
                            }
                            else if (__instance.GetInput(button2, GUIHand.InputState.Held))
                            {
                                if (playerTool.OnRightHandHeld())
                                    __instance.UseInput(button2, GUIHand.InputState.Down | GUIHand.InputState.Held);
                            }
                            else if (__instance.GetInput(button2, GUIHand.InputState.Up) && playerTool.OnRightHandUp())
                                __instance.UseInput(button2, GUIHand.InputState.Up);
                            if (__instance.GetInput(button1, GUIHand.InputState.Down))
                            {
                                if (playerTool.OnLeftHandDown())
                                {
                                    __instance.UseInput(button1, GUIHand.InputState.Down | GUIHand.InputState.Held | GUIHand.InputState.Up);
                                    playerTool.OnToolActionStart();
                                }
                            }
                            else if (__instance.GetInput(button1, GUIHand.InputState.Held))
                            {
                                if (playerTool.OnLeftHandHeld())
                                    __instance.UseInput(button1, GUIHand.InputState.Down | GUIHand.InputState.Held);
                            }
                            else if (__instance.GetInput(button1, GUIHand.InputState.Up) && playerTool.OnLeftHandUp())
                                __instance.UseInput(button1, GUIHand.InputState.Up);
                            if (__instance.GetInput(button5, GUIHand.InputState.Down))
                            {
                                if (playerTool.OnAltDown())
                                {
                                    __instance.UseInput(button5, GUIHand.InputState.Down | GUIHand.InputState.Held | GUIHand.InputState.Up);
                                    __instance.usedAltAttackThisFrame = true;
                                    playerTool.OnToolActionStart();
                                }
                            }
                            else if (__instance.GetInput(button5, GUIHand.InputState.Held))
                            {
                                if (playerTool.OnAltHeld())
                                    __instance.UseInput(button5, GUIHand.InputState.Down | GUIHand.InputState.Held);
                            }
                            else if (__instance.GetInput(button5, GUIHand.InputState.Up) && playerTool.OnAltUp())
                                __instance.UseInput(button5, GUIHand.InputState.Up);
                            if (__instance.GetInput(button3, GUIHand.InputState.Down) && playerTool.OnReloadDown())
                                __instance.UseInput(button3, GUIHand.InputState.Down);
                            if (__instance.GetInput(button4, GUIHand.InputState.Down) && playerTool.OnExitDown())
                                __instance.UseInput(button4, GUIHand.InputState.Down);
                        }
                        if (action != ItemAction.None && __instance.GetInput(button2, GUIHand.InputState.Down))
                        {
                            if (action == ItemAction.Drop)
                            {
                                __instance.UseInput(button2, GUIHand.InputState.Down | GUIHand.InputState.Held);
                                Inventory.main.DropHeldItem(true);
                            }
                            else
                            {
                                __instance.UseInput(button2, GUIHand.InputState.Down | GUIHand.InputState.Held);
                                Inventory.main.ExecuteItemAction(action, heldItem);
                            }
                        }
                        if (__instance.activeTarget != null)
                        {
                            if (__instance.GetInput(button1, GUIHand.InputState.Down))
                            {
                                __instance.UseInput(button1, GUIHand.InputState.Down | GUIHand.InputState.Held);
                                GUIHand.Send(__instance.activeTarget, HandTargetEventType.Click, __instance);
                            }
                        }
                        else if (KnownTech.Contains(TechType.SnowBall) && !__instance.player.isUnderwater.value && !Player.main.IsInside())
                        {
                            VFXSurfaceTypes vfxSurfaceTypes = VFXSurfaceTypes.none;
                            int layerMask = 1 << LayerID.TerrainCollider | 1 << LayerID.Default;
                            RaycastHit hitInfo;
                            if (Physics.Raycast(MainCamera.camera.transform.position, MainCamera.camera.transform.forward, out hitInfo, 3f, layerMask) && hitInfo.collider.gameObject.layer == LayerID.TerrainCollider)
                                vfxSurfaceTypes = Utils.GetTerrainSurfaceType(hitInfo.point, hitInfo.normal);
                            if (vfxSurfaceTypes == VFXSurfaceTypes.snow)
                            {
                                HandReticle.main.SetIcon(HandReticle.IconType.Hand);
                                HandReticle.main.SetText(HandReticle.TextType.Hand, "PickUpSnow", true, GameInput.Button.LeftHand);
                                if (__instance.GetInput(button1, GUIHand.InputState.Down))
                                {
                                    __instance.UseInput(button1, GUIHand.InputState.Down | GUIHand.InputState.Held);
                                    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(__instance.snowBallPrefab);
                                    if (!Inventory.main.Pickup(gameObject.GetComponent<Pickupable>()))
                                        UnityEngine.Object.Destroy(gameObject);
                                    else
                                        Utils.PlayFMODAsset(__instance.snowballPickupSound, MainCamera.camera.transform);
                                }
                            }
                        }
                    }
                }
                if (AvatarInputHandler.main.IsEnabled() && __instance.GetInput(button6, GUIHand.InputState.Down))
                {
                    __instance.UseInput(button6, GUIHand.InputState.Down | GUIHand.InputState.Held | GUIHand.InputState.Up);
                    GameInput.SetAutoMove(!GameInput.GetAutoMove());
                }
                if (!AvatarInputHandler.main.IsEnabled() || uGUI.isIntro || (uGUI.isLoading || !__instance.GetInput(button7, GUIHand.InputState.Down)))
                    return false;
                __instance.UseInput(button7, GUIHand.InputState.Down | GUIHand.InputState.Held | GUIHand.InputState.Up);
                __instance.player.GetPDA().Open();
                return false;
            }
        }

        [HarmonyPatch(typeof(GUIHand), "OnUpdate")]
        class GUIHand_OnUpdate_Patch
        { // UI tells you if looking at dead fish 
            public static void Postfix(GUIHand __instance)
            {
                if (!__instance.activeTarget)
                    return;
                //AddDebug("activeTarget layer " + __instance.activeTarget.layer);
                //if (__instance.activeTarget.layer == LayerID.NotUseable)
                //    AddDebug("activeTarget Not Useable layer ");
                TechType techType = CraftData.GetTechType(__instance.activeTarget);
                if (techType != TechType.None)
                {
                    //AddDebug("OnUpdate " + __instance.activeTarget.name);
                    LiveMixin liveMixin = __instance.activeTarget.GetComponentInParent<LiveMixin>();
                    if (liveMixin && !liveMixin.IsAlive())
                    {
                        //AddDebug("health " + liveMixin.health);
                        Pickupable pickupable = liveMixin.GetComponent<Pickupable>();
                        //CreatureEgg ce = liveMixin.GetComponent<CreatureEgg>();
                        //if (ce)
                        //    name = Language.main.Get(ce.overrideEggType);
                        string name = Language.main.Get(techType);
                        //AddDebug("name " + name);
                        if (pickupable)
                        {
                            if (pickupable.overrideTechType != TechType.None)
                                name = Language.main.Get(pickupable.overrideTechType);

                            name = Language.main.GetFormat<string>("DeadFormat", name);
                            //name = LanguageCache.GetPickupText(techType);
                            name = Language.main.GetFormat<string>("PickUpFormat", name);
                            HandReticle.main.SetIcon(pickupable.usePackUpIcon ? HandReticle.IconType.PackUp : HandReticle.IconType.Hand);
                            HandReticle.main.SetText(HandReticle.TextType.Hand, name, false, GameInput.Button.LeftHand);
                        }
                        else
                        {
                            //AddDebug("name " + name);
                            name = Language.main.GetFormat<string>("DeadFormat", name);
                            //HandReticle.main.SetInteractTextRaw(name, string.Empty);
                            HandReticle.main.SetText(HandReticle.TextType.Hand, name, true);
                        }

                    }
                }
            }
        }

        [HarmonyPatch(typeof(uGUI_MainMenu), "Update")]
        class uGUI_MainMenu_Update_Patch
        {
            public static void Postfix(uGUI_MainMenu __instance)
            {
                //AddDebug("lastGroup " +__instance.lastGroup);
                if (__instance.lastGroup == "SavedGames")
                {
                    if (Input.GetAxis("Mouse ScrollWheel") > 0f)
                        __instance.subMenu.SelectItemInDirection(0, -1);
                    else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
                        __instance.subMenu.SelectItemInDirection(0, 1);
                }
            }
        }

        [HarmonyPatch(typeof(uGUI_PDA), "Update")]
        class uGUI_PDA_Update_Patch
        {
            public static void Postfix(uGUI_PDA __instance)
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    if (Input.GetAxis("Mouse ScrollWheel") > 0f)
                        __instance.OpenTab(__instance.GetNextTab());
                    else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
                        __instance.OpenTab(__instance.GetPreviousTab());
                }
            }
        }

        [HarmonyPatch(typeof(uGUI_FeedbackCollector), "HintShow")]
        class uGUI_FeedbackCollector_HintShow_Patch
        {
            static bool Prefix(uGUI_FeedbackCollector __instance)
            {
                //AddDebug("uGUI_FeedbackCollector HintShow");
                if (Main.config.disableHints)
                    return false;

                return true;
            }
        }



        [HarmonyPatch(typeof(PlayerWorldArrows), "CreateWorldArrows")]
        internal class PlayerWorldArrows_CreateWorldArrows_Patch
        { // not used?
            internal static bool Prefix(PlayerWorldArrows __instance)
            {
                //AddDebug("CreateWorldArrows");
                return !Main.config.disableHints;
            }
        }

        //[HarmonyPatch(typeof(TooltipFactory), "ItemCommons")]
        class TooltipFactory_ItemCommons_Prefix_Patch
        {
            static void Prefix(StringBuilder sb, TechType techType, GameObject obj)
            {
                CreatureEgg creatureEgg = obj.GetComponent<CreatureEgg>();
                if (creatureEgg)
                {
                    LiveMixin liveMixin = obj.GetComponent<LiveMixin>();
                    if (!liveMixin.IsAlive())
                    {
                        TooltipFactory.WriteTitle(sb, "Dead ");
                    }
                }
            }
        }

        [HarmonyPatch(typeof(TooltipFactory), "ItemCommons")]
        class TooltipFactory_ItemCommons_Patch
        {
            static void Postfix(ref StringBuilder sb, TechType techType, GameObject obj)
            {

                if (Main.english && Crush_Damage.crushDepthEquipment.ContainsKey(techType) && Crush_Damage.crushDepthEquipment[techType] > 0)
                {
                    TooltipFactory.WriteDescription(sb, "Increases your safe diving depth by " + Crush_Damage.crushDepthEquipment[techType] + " meters.");
                }
                if (techType == TechType.FirstAidKit)
                {
                    sb.Clear();
                    string name = Language.main.Get(techType);
                    TooltipFactory.WriteTitle(sb, name);
                    TooltipFactory.WriteDescription(sb, Language.main.Get(TooltipFactory.techTypeTooltipStrings.Get(techType)));
                    TooltipFactory.WriteDescription(sb, Language.main.GetFormat<float>("HealthFormat", Main.config.medKitHP));
                    //TooltipFactory.WriteDescription(sb, "Restores " + Main.config.medKitHP + " health.");
                    //AddDebug("ItemCommons " + sb.ToString());
                }
                else if (techType == TechType.SeaTruckUpgradeHorsePower && Main.english && Main.config.seatruckMoveTweaks)
                {
                    sb.Clear();
                    string name = Language.main.Get(techType);
                    TooltipFactory.WriteTitle(sb, name);
                    TooltipFactory.WriteDescription(sb, "Increases the Seatruck engine's horsepower and energy consumption by 10%. More than 1 can be used simultaneously.");
                    //AddDebug("GetCurrentLanguage " + Language.main.GetCurrentLanguage());
                    //Main.Log("GetCurrentLanguage " + Language.main.GetCurrentLanguage());
                }
                if (Main.config.invMultLand > 0f || Main.config.invMultWater > 0f)
                {
                    Rigidbody rb = obj.GetComponent<Rigidbody>();
                    if (rb)
                        TooltipFactory.WriteDescription(sb, "mass " + rb.mass);
                }
            }
        }

        ////[HarmonyPatch(typeof(uGUI_MainMenu), "OnRightSideOpened")]
        class uGUI_MainMenu_OnRightSideOpened_Patch
        {
            public static void Postfix(uGUI_MainMenu __instance, GameObject root)
            {
                AddDebug("OnRightSideOpened " + __instance.GetCurrentSubMenu());
                //__instance.subMenu = root.GetComponentInChildren<uGUI_INavigableIconGrid>();
                //__instance.subMenu.
                //if (Input.GetKey(KeyCode.LeftShift))
                //{
                //if (Input.GetAxis("Mouse ScrollWheel") > 0f)
                //    __instance.OpenTab(__instance.GetNextTab());
                //else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
                //    __instance.OpenTab(__instance.GetPreviousTab());
                //}
            }
        }

        ////[HarmonyPatch(typeof(uGUI_MainMenu), "OnButtonOptions")]
        class uGUI_MainMenu_OnButtonOptions_Patch
        {
            public static void Postfix(GUIHand __instance)
            {
                AddDebug("uGUI_MainMenu OnButtonOptions");
            }
        }

    }
}
