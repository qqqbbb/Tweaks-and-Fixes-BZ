using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Pickupable_Patch
    {
        static float healTime = 0f;
        public static Dictionary<TechType, float> itemMass = new Dictionary<TechType, float>();

        [HarmonyPatch(typeof(Pickupable), "Awake")]
        class Pickupable_Awake_Patch
        {
            static void Postfix(Pickupable __instance)
            {
                TechType tt = __instance.GetTechType();
                if (itemMass.ContainsKey(tt))
                {
                    Rigidbody rb = __instance.GetComponent<Rigidbody>();
                    if (rb)
                        rb.mass = itemMass[tt];
                }
            }
        }

        [HarmonyPatch(typeof(Player), "Update")]
        class Player_Update_Patch
        {
            static void Postfix(Player __instance)
            { // not checking savegame slot
                if (Main.config.medKitHPtoHeal > 0 && Time.time > healTime)
                {
                    healTime = Time.time + 1f;
                    __instance.liveMixin.AddHealth(Main.config.medKitHPperSecond);
                    Main.config.medKitHPtoHeal -= Main.config.medKitHPperSecond;
                    if (Main.config.medKitHPtoHeal < 0)
                        Main.config.medKitHPtoHeal = 0;

                    //AddDebug("Player Update heal " + Main.config.medKitHPperSecond);
                    //AddDebug("Player Update medKitHPtoHeal " + Main.config.medKitHPtoHeal);
                    //Main.config.Save();
                }
            }
        }

        [HarmonyPatch(typeof(Survival), "Use")]
        class Survival_Awake_Patch
        {
            static bool Prefix(Survival __instance, GameObject useObj, ref bool __result, Inventory inventory)
            {
                __result = false;
                if (useObj == null)
                    return false;

                TechType techType = CraftData.GetTechType(useObj);
                //AddDebug("Use" + techType);
                if (techType == TechType.None)
                {
                    Pickupable p = useObj.GetComponent<Pickupable>();
                    if (p)
                        techType = p.GetTechType();
                }
                if (techType == TechType.FirstAidKit)
                {
                    if (Player.main.GetComponent<LiveMixin>().health == 100f)
                        AddMessage(Language.main.Get("HealthFull"));
                    else
                    {
                        __result = true;
                        if (Main.config.medKitHPperSecond >= Main.config.medKitHP)
                        {
                            Player.main.GetComponent<LiveMixin>().AddHealth(Main.config.medKitHP);
                        }
                        else
                        {
                            Main.config.medKitHPtoHeal += Main.config.medKitHP;
                            healTime = Time.time;
                        }

                    }
                }
                else if (techType == TechType.WaterPurificationTablet && inventory.DestroyItem(TechType.SnowBall))
                {
                    __instance.StartCoroutine(CraftData.AddToInventoryAsync(TechType.BigFilteredWater, (IOut<GameObject>)DiscardTaskResult<GameObject>.Instance));
                    __result = true;
                }
                if (__result)
                {
                    FMODAsset useSound = Player.main.GetUseSound(TechData.GetSoundType(techType));
                    if (useSound)
                        Utils.PlayFMODAsset(useSound, Player.main.transform.position);
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(Inventory), "GetItemAction")]
        internal class Inventory_GetItemAction_Patch
        {
            internal static void Postfix(Inventory __instance, ref ItemAction __result, InventoryItem item, int button)
            {
                //AddDebug("GetItemAction button " + button + " " + item.item.name + " " + __result);
                Pickupable pickupable = item.item;
                TechType tt = pickupable.GetTechType();
                if (__result == ItemAction.Eat && Main.config.cantEatUnderwater && pickupable.gameObject.GetComponent<Eatable>() && Player.main.IsUnderwater())
                {
                    __result = ItemAction.None;
                }
                else if (__result == ItemAction.Use && Main.config.cantUseMedkitUnderwater && tt == TechType.FirstAidKit && Player.main.IsUnderwater())
                {
                    __result = ItemAction.None;
                }
            }
        }

        [HarmonyPatch(typeof(TooltipFactory), "ItemActions")]
        internal class TooltipFactory_ItemActions_Patch
        { // for some items UI did not tell they can be dropped 
            internal static bool Prefix(StringBuilder sb, global::InventoryItem item)
            {
                //AddDebug("ItemActions " + item.item.name);
                bool canBindItem = Inventory.main.GetCanBindItem(item) && GameInput.IsKeyboardAvailable();
                ItemAction itemAction1 = Inventory.main.GetItemAction(item, 0);
                ItemAction itemAction2 = Inventory.main.GetItemAction(item, 1);
                ItemAction itemAction3 = Inventory.main.GetItemAction(item, 2);
                ItemAction itemAction4 = Inventory.main.GetItemAction(item, 3);
                bool usingController = GameInput.GetPrimaryDevice() == GameInput.Device.Controller;
                //if (!canBindItem && (itemAction1 | itemAction3 | itemAction4) == ItemAction.None)
                //if (itemAction1 == ItemAction.None && itemAction2 == ItemAction.None && itemAction3 == ItemAction.None && itemAction4 == ItemAction.None)
                //{
                //    AddDebug("return");
                //    return false;
                //}

                 if (canBindItem && !usingController)
                    TooltipFactory.WriteAction(sb, TooltipFactory.stringKeyRange15, TooltipFactory.stringBindQuickSlot);
                if (itemAction4 != ItemAction.None)
                    TooltipFactory.WriteAction(sb, TooltipFactory.stringButton3, TooltipFactory.GetUseActionString(itemAction4));
                if (itemAction1 != ItemAction.None)
                    TooltipFactory.WriteAction(sb, TooltipFactory.stringButton0, TooltipFactory.GetUseActionString(itemAction1));
                if (itemAction3 != ItemAction.None)
                    TooltipFactory.WriteAction(sb, TooltipFactory.stringButton2, TooltipFactory.GetUseActionString(itemAction3));
                if (itemAction2 == ItemAction.None)
                    return false;
                //AddDebug("WriteAction");
                TooltipFactory.WriteAction(sb, TooltipFactory.stringButton1, TooltipFactory.GetUseActionString(itemAction2));
                return false;
            }
        }

    }
}
