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
        static bool fishTooltip = false;

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
                while (!Input.anyKeyDown)
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

        //[HarmonyPatch(typeof(Inventory), "GetAllItemActions")]
        class Inventory_GetAllItemActions_Patch
        {
            static bool Prefix(Inventory __instance, InventoryItem item, ref ItemAction __result)
            {
                //AddDebug("GetAllItemActions " + item.item.GetTechName() + " " + __result);
                //if (Main.IsEatableFish(item.item.gameObject))
                //{
                    //__result = ItemAction.Drop;
                    //__result = __result | ItemAction.Eat;
                    //return false;
                    //if ((__result & ItemAction.Drop) != ItemAction.None)
                //}
                return true;
            }
            static void Postfix(Inventory __instance, InventoryItem item, ItemAction __result)
            {
                AddDebug("GetAllItemActions " + item.item.GetTechName() + " " + __result);

            }
        }

        //[HarmonyPatch(typeof(GUIHand), "GetActionString")]
        class GUIHand_GetActionString_Patch
        { 
            public static void Postfix(GUIHand __instance, ItemAction action, Pickupable pickupable, ref string __result)
            {
                GameModeUtils.GetGameMode(out GameModeOption mode, out GameModeOption _);
                bool survival = !GameModeUtils.IsOptionActive(mode, GameModeOption.NoSurvival);
                //AddDebug("survival " + survival);
                if (Main.IsEatableFish(pickupable.gameObject))
                {
                    bool cantEat = Main.config.cantEatUnderwater && Player.main.IsUnderwater();
                    //AddDebug("GetActionString " + action + " " + __result);
                    //__result = "ItemActionEat";
                    string dropText = __result;

                    //string dropText = HandReticle.main.GetText(GUIHand.GetActionString(action, pickupable), true, GameInput.Button.LeftHand);
                    //HandReticle.main.SetText(HandReticle.TextType.Use, GUIHand.GetActionString(action, pickupable), true, GameInput.Button.RightHand);
                    //HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, text);
                }
            }
        }

        //[HarmonyPatch(typeof(GUIHand), "OnUpdate")]
        class GUIHand_OnUpdate_Prefix_Patch
        { 
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
            static string altToolButton = string.Empty;
            static string rightHandButton = string.Empty;
            public static void Postfix(GUIHand __instance)
            {
                PlayerTool tool = __instance.GetTool();
                if (tool)
                {
                    Flare flare = tool as Flare;
                    if (flare)
                    {
                        bool lit = flare.flareActivateTime > 0;
                        string text = string.Empty;
                        string throwFlare = lit ? Main.config.throwFlare : Main.config.lightAndThrowFlare;
                        if (Inventory.CanDropItemHere(tool.GetComponent<Pickupable>(), false))
                            text = throwFlare + " (" + rightHandButton + ")";
                        if (string.IsNullOrEmpty(altToolButton))
                            altToolButton = uGUI.FormatButton(GameInput.Button.AltTool);
                        if (string.IsNullOrEmpty(rightHandButton))
                            rightHandButton = uGUI.FormatButton(GameInput.Button.RightHand);

                        if (!lit)
                        {
                            string text1 = Main.config.lightFlare + " (" + altToolButton + ")";
                            if (string.IsNullOrEmpty(text))
                                text = text1;
                            else
                                text = text + ",  " + text1;
                        }
                        HandReticle.main.SetTextRaw(HandReticle.TextType.Use, text);
                    }
                }
                else
                {
                    SubRoot subRoot = Player.main.currentSub;
                    if (subRoot && subRoot.isBase)
                    {
                        string text = LanguageCache.GetButtonFormat("SeaglideLightsTooltip", GameInput.Button.Deconstruct);
                        //HandReticle.main.SetUseTextRaw(null, text);
                        HandReticle.main.SetTextRaw(HandReticle.TextType.UseSubscript, text);
                        if (GameInput.GetButtonDown(GameInput.Button.Deconstruct))
                        {
                            Base_Patch.ToggleBaseLight(subRoot);
                        }
                    }
                }

                GameModeUtils.GetGameMode(out GameModeOption mode, out GameModeOption _);
                bool survival = !GameModeUtils.IsOptionActive(mode, GameModeOption.NoSurvival);
                //AddDebug("survival " + survival);
                InventoryItem heldItem = Inventory.main.quickSlots.heldItem;
                if (survival && heldItem != null && Main.IsEatableFish(heldItem.item.gameObject))
                {
                    string text = string.Empty;
                    ItemAction allItemActions = Inventory.main.GetAllItemActions(heldItem);
                    if ((allItemActions & ItemAction.Drop) != ItemAction.None)
                    {
                        text = GUIHand.GetActionString(ItemAction.Drop, heldItem.item);
                        text = HandReticle.main.GetText(text, true, GameInput.Button.RightHand);
                    }
                    bool cantEat = Main.config.cantEatUnderwater && Player.main.IsUnderwater();
                    if (!cantEat)
                    {
                        string eatText = GUIHand.GetActionString(ItemAction.Eat, heldItem.item);
                        eatText = HandReticle.main.GetText(eatText, true, GameInput.Button.AltTool);
                        if (string.IsNullOrEmpty(text))
                            text = eatText;
                        else
                            text = text + ", " + eatText;
                        if (GameInput.GetButtonDown(GameInput.Button.AltTool))
                        {
                            Inventory.main.ExecuteItemAction(ItemAction.Eat, heldItem);
                            return;
                        }
                    }
                    HandReticle.main.SetTextRaw(HandReticle.TextType.Use, text);
                }
                if (!__instance.activeTarget)
                    return;
                //AddDebug("activeTarget layer " + __instance.activeTarget.layer);
                //if (__instance.activeTarget.layer == LayerID.NotUseable)
                //    AddDebug("activeTarget Not Useable layer ");
                TechType targetTT = CraftData.GetTechType(__instance.activeTarget);
                if (targetTT == TechType.None)
                    return;

                if (targetTT == TechType.Flare && Main.english)
                {
                    //AddDebug("activeTarget Flare");
                    string name = Language.main.Get(targetTT);
                    name = "Burnt out " + name;
                    HandReticle.main.SetText(HandReticle.TextType.Hand, name, false);
                }
                //AddDebug("OnUpdate " + __instance.activeTarget.name);
                LiveMixin liveMixin = __instance.activeTarget.GetComponentInParent<LiveMixin>();
                if (liveMixin && !liveMixin.IsAlive())
                {
                    //AddDebug("health " + liveMixin.health);
                    Pickupable pickupable = liveMixin.GetComponent<Pickupable>();
                    //CreatureEgg ce = liveMixin.GetComponent<CreatureEgg>();
                    //if (ce)
                    //    name = Language.main.Get(ce.overrideEggType);
                    string name = Language.main.Get(targetTT);
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
                    return !Main.config.disableHints;
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
            static void Prefix(StringBuilder sb, TechType techType, GameObject obj)
            {
                if (!Main.english)
                    return;

                Flare flare = obj.GetComponent<Flare>();
                if (flare)
                {
                    //AddDebug("flare.energyLeft " + flare.energyLeft);
                    if (flare.energyLeft <= 0f)
                        TooltipFactory.WriteTitle(sb, "Burnt out ");
                    else if (flare.flareActivateTime > 0f)
                        TooltipFactory.WriteTitle(sb, "Lit ");
                }
                fishTooltip = Main.IsEatableFish(obj);
            }
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

        [HarmonyPatch(typeof(Language), "FormatString", new Type[] { typeof(string), typeof(object[]) })]
        class Language_FormatString_Patch
        {
            static void Postfix(string format, ref string __result, object[] args)
            {
                //AddDebug("FormatString " + format + " " + args.Length);
                //AddDebug("FormatString " + __result);
                if (!fishTooltip || Main.config.eatRawFish == Config.EatingRawFish.Vanilla || args.Length == 0 || args[0].GetType() != typeof(int))
                    return;
                //AddDebug("FormatString GetType " + args[0].GetType());
                int value = (int)args[0];
                if (value > 0f && format.Contains("FOOD:") || format.Contains("H₂O:"))
                {
                    string[] tokens = __result.Split(':');
                    if (Main.config.eatRawFish == Config.EatingRawFish.Harmless)
                        __result = tokens[0] + ": min 0, max " + value;
                    else if (Main.config.eatRawFish == Config.EatingRawFish.Risky)
                        __result = tokens[0] + ": min -" + value + ", max " + value;
                    else if (Main.config.eatRawFish == Config.EatingRawFish.Harmful)
                        __result = tokens[0] + ": min -" + value + ", max 0";
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

        [HarmonyPatch(typeof(uGUI_HealthBar), "LateUpdate")]
        class uGUI_HealthBar_LateUpdate_Patch
        {
            public static bool Prefix(uGUI_HealthBar __instance)
            {
                if (!Main.config.alwaysShowHealthNunbers)
                    return true;
                int showNumbers = __instance.showNumbers ? 1 : 0;
                __instance.showNumbers = false;
                Player main = Player.main;
                if (main != null)
                {
                    LiveMixin component = main.GetComponent<LiveMixin>();
                    if (component != null)
                    {
                        if (!__instance.subscribed)
                        {
                            __instance.subscribed = true;
                            component.onHealDamage.AddHandler(__instance.gameObject, new UWE.Event<float>.HandleFunction(__instance.OnHealDamage));
                        }
                        float has = component.health - component.tempDamage;
                        float maxHealth = component.maxHealth;
                        __instance.SetValue(has, maxHealth);
                        float time = 1f - Mathf.Clamp01(has / __instance.pulseReferenceCapacity);
                        __instance.pulseDelay = __instance.pulseDelayCurve.Evaluate(time);
                        if (__instance.pulseDelay < 0F)
                            __instance.pulseDelay = 0f;
                        __instance.pulseTime = __instance.pulseTimeCurve.Evaluate(time);
                        if (__instance.pulseTime < 0F)
                            __instance.pulseTime = 0f;
                        float num2 = __instance.pulseDelay + __instance.pulseTime;
                        if (__instance.pulseTween.duration > 0f && num2 <= 0F)
                            __instance.statePulse.normalizedTime = 0f;
                        __instance.pulseTween.duration = num2;
                    }
                    PDA pda = main.GetPDA();
                    if (Main.config.alwaysShowHealthNunbers || pda != null && pda.isInUse)
                        __instance.showNumbers = true;
                }
                if (__instance.statePulse.enabled)
                {
                    RectTransform icon = __instance.icon;
                    icon.localScale = icon.localScale + __instance.punchScale;
                }
                else
                    __instance.icon.localScale = __instance.punchScale;
                int num3 = __instance.showNumbers ? 1 : 0;
                if (showNumbers != num3)
                    __instance.rotationVelocity += UnityEngine.Random.Range(-__instance.rotationRandomVelocity, __instance.rotationRandomVelocity);
                if (!MathExtensions.CoinRotation(ref __instance.rotationCurrent, __instance.showNumbers ? 180f : 0f, ref __instance.lastFixedUpdateTime, PDA.time, ref __instance.rotationVelocity, __instance.rotationSpringCoef, __instance.rotationVelocityDamp, __instance.rotationVelocityMax))
                    return false;
                __instance.icon.localRotation = Quaternion.Euler(0f, __instance.rotationCurrent, 0f);
                return false;
            }
        }

        [HarmonyPatch(typeof(uGUI_FoodBar), "LateUpdate")]
        class uGUI_FoodBar_LateUpdate_Patch
        {
            public static bool Prefix(uGUI_FoodBar __instance)
            {
                if (!Main.config.alwaysShowHealthNunbers)
                    return true;

                int showNumbers = __instance.showNumbers ? 1 : 0;
                __instance.showNumbers = false;
                Player main = Player.main;
                if (main != null)
                {
                    Survival component = main.GetComponent<Survival>();
                    if (component != null)
                    {
                        if (!__instance.subscribed)
                        {
                            __instance.subscribed = true;
                            component.onEat.AddHandler(__instance.gameObject, new UWE.Event<float>.HandleFunction(__instance.OnEat));
                        }
                        float food = component.food;
                        float capacity = 100f;
                        __instance.SetValue(food, capacity);
                        float time = 1f - Mathf.Clamp01(food / __instance.pulseReferenceCapacity);
                        __instance.pulseDelay = __instance.pulseDelayCurve.Evaluate(time);
                        if (__instance.pulseDelay < 0.0)
                            __instance.pulseDelay = 0.0f;
                        __instance.pulseTime = __instance.pulseTimeCurve.Evaluate(time);
                        if (__instance.pulseTime < 0.0)
                            __instance.pulseTime = 0.0f;
                        float num2 = __instance.pulseDelay + __instance.pulseTime;
                        if (__instance.pulseTween.duration > 0.0 && num2 <= 0.0)
                            __instance.pulseAnimationState.normalizedTime = 0.0f;
                        __instance.pulseTween.duration = num2;
                    }
                    PDA pda = main.GetPDA();
                    if (Main.config.alwaysShowHealthNunbers || pda != null && pda.isInUse)
                        __instance.showNumbers = true;
                }
                if ((TrackedReference)__instance.pulseAnimationState != (TrackedReference)null && __instance.pulseAnimation.enabled)
                {
                    RectTransform icon = __instance.icon;
                    icon.localScale = icon.localScale + __instance.punchScale;
                }
                else
                    __instance.icon.localScale = __instance.punchScale;
                int num3 = __instance.showNumbers ? 1 : 0;
                if (showNumbers != num3)
                    __instance.rotationVelocity += UnityEngine.Random.Range(-__instance.rotationRandomVelocity, __instance.rotationRandomVelocity);
                if (!MathExtensions.CoinRotation(ref __instance.rotationCurrent, __instance.showNumbers ? 180f : 0.0f, ref __instance.lastFixedUpdateTime, PDA.time, ref __instance.rotationVelocity, __instance.rotationSpringCoef, __instance.rotationVelocityDamp, __instance.rotationVelocityMax))
                    return false;
                __instance.icon.localRotation = Quaternion.Euler(0.0f, __instance.rotationCurrent, 0.0f);
                return false;
            }
        }

        [HarmonyPatch(typeof(uGUI_WaterBar), "LateUpdate")]
        class uGUI_WaterBar_LateUpdate_Patch
        {
            public static bool Prefix(uGUI_WaterBar __instance)
            {
                if (!Main.config.alwaysShowHealthNunbers)
                    return true;

                int showNumbers = __instance.showNumbers ? 1 : 0;
                __instance.showNumbers = false;
                Player main = Player.main;
                if (main != null)
                {
                    Survival component = main.GetComponent<Survival>();
                    if (component != null)
                    {
                        if (!__instance.subscribed)
                        {
                            __instance.subscribed = true;
                            component.onDrink.AddHandler(__instance.gameObject, new UWE.Event<float>.HandleFunction(__instance.OnDrink));
                        }
                        float water = component.water;
                        float capacity = 100f;
                        __instance.SetValue(water, capacity);
                        float time = 1f - Mathf.Clamp01(water / __instance.pulseReferenceCapacity);
                        __instance.pulseDelay = __instance.pulseDelayCurve.Evaluate(time);
                        if (__instance.pulseDelay < 0.0)
                            __instance.pulseDelay = 0.0f;
                        __instance.pulseTime = __instance.pulseTimeCurve.Evaluate(time);
                        if (__instance.pulseTime < 0.0)
                            __instance.pulseTime = 0.0f;
                        float num2 = __instance.pulseDelay + __instance.pulseTime;
                        if (__instance.pulseTween.duration > 0.0 && num2 <= 0.0)
                            __instance.pulseAnimationState.normalizedTime = 0.0f;
                        __instance.pulseTween.duration = num2;
                    }
                    PDA pda = main.GetPDA();
                    if (Main.config.alwaysShowHealthNunbers || pda != null && pda.isInUse)
                        __instance.showNumbers = true;
                }
                if ((TrackedReference)__instance.pulseAnimationState != (TrackedReference)null && __instance.pulseAnimationState.enabled)
                {
                    RectTransform icon = __instance.icon;
                    icon.localScale = icon.localScale + __instance.punchScale;
                }
                else
                    __instance.icon.localScale = __instance.punchScale;
                int num3 = __instance.showNumbers ? 1 : 0;
                if (showNumbers != num3)
                    __instance.rotationVelocity += UnityEngine.Random.Range(-__instance.rotationRandomVelocity, __instance.rotationRandomVelocity);
                if (!MathExtensions.CoinRotation(ref __instance.rotationCurrent, __instance.showNumbers ? 180f : 0.0f, ref __instance.lastFixedUpdateTime, PDA.time, ref __instance.rotationVelocity, __instance.rotationSpringCoef, __instance.rotationVelocityDamp, __instance.rotationVelocityMax))
                    return false;
                __instance.icon.localRotation = Quaternion.Euler(0.0f, __instance.rotationCurrent, 0.0f);
                return false;
            }
        }

    }
}
