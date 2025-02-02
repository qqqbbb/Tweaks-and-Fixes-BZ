using Nautilus.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    public class OptionsMenu : ModOptions
    {
        public OptionsMenu() : base("Tweaks and Fixes")
        {
            ModSliderOption timeFlowSpeedSlider = ConfigMenu.timeFlowSpeed.ToModSliderOption(.1f, 10f, .1f, "{0:0.#}");
            timeFlowSpeedSlider.OnChanged += TimeSpeedUpdated;
            ModSliderOption seaglideSpeedSlider = ConfigMenu.seaglideSpeedMult.ToModSliderOption(.5f, 2f, .1f, "{0:0.#}");
            ModSliderOption playerWaterSpeedSlider = ConfigMenu.playerWaterSpeedMult.ToModSliderOption(.5f, 5f, .1f, "{0:0.#}");
            ModSliderOption playerGroundSpeedSlider = ConfigMenu.playerGroundSpeedMult.ToModSliderOption(.5f, 3f, .1f, "{0:0.#}");
            ModSliderOption exosuitSpeedSlider = ConfigMenu.exosuitSpeedMult.ToModSliderOption(.5f, 3f, .1f, "{0:0.#}");
            ModSliderOption seatruckSpeedSlider = ConfigMenu.seatruckSpeedMult.ToModSliderOption(.5f, 3f, .1f, "{0:0.#}");
            ModSliderOption hoverbikeSpeedSlider = ConfigMenu.hoverbikeSpeedMult.ToModSliderOption(.5f, 3f, .1f, "{0:0.#}");
            ModSliderOption oxygenSlider = ConfigMenu.oxygenPerBreath.ToModSliderOption(0f, 6f, .1f, "{0:0.#}");
            ModSliderOption coldSlider = ConfigMenu.coldMult.ToModSliderOption(0f, 3f, .1f, "{0:0.#}");
            ModSliderOption toolEnergySlider = ConfigMenu.toolEnergyConsMult.ToModSliderOption(0f, 3f, .1f, "{0:0.#}");
            ModSliderOption vehicleEnergySlider = ConfigMenu.vehicleEnergyConsMult.ToModSliderOption(0f, 3f, .1f, "{0:0.#}");
            ModSliderOption baseEnergySlider = ConfigMenu.baseEnergyConsMult.ToModSliderOption(0f, 3f, .1f, "{0:0.#}");
            ModSliderOption knifeRangeSlider = ConfigMenu.knifeRangeMult.ToModSliderOption(1f, 5f, .1f, "{0:0.#}");
            ModSliderOption knifeDamageSlider = ConfigMenu.knifeDamageMult.ToModSliderOption(1f, 5f, .1f, "{0:0.#}");
            ModSliderOption medKitHPslider = ConfigMenu.medKitHP.ToModSliderOption(10, 100, 1);
            ModSliderOption craftTimeSlider = ConfigMenu.craftTimeMult.ToModSliderOption(0.01f, 3f, .01f, "{0:0.0#}");
            ModSliderOption buildTimeSlider = ConfigMenu.buildTimeMult.ToModSliderOption(0.01f, 3f, .01f, "{0:0.0#}");
            ModSliderOption crushDepthSlider = ConfigMenu.crushDepth.ToModSliderOption(50, 500, 10);
            ModSliderOption crushDamageSlider = ConfigMenu.crushDamage.ToModSliderOption(0f, 10f, .1f, "{0:0.0#}");
            ModSliderOption vehicleCrushDamageSlider = ConfigMenu.vehicleCrushDamageMult.ToModSliderOption(0f, 10f, .1f, "{0:0.0#}");
            ModSliderOption crushDamageProgressionSlider = ConfigMenu.crushDamageProgression.ToModSliderOption(0f, 1f, .01f, "{0:0.0#}");

            //ModSliderOption hungerUpdateIntervalSlider = ConfigMenu.hungerUpdateInterval.ToModSliderOption(1, 100, 1);
            ModSliderOption fishFoodWaterRatioSlider = ConfigMenu.fishFoodWaterRatio.ToModSliderOption(0f, 1f, .01f, "{0:0.0#}");
            ModSliderOption foodDecayRateSlider = ConfigMenu.foodDecayRateMult.ToModSliderOption(0f, 3f, .1f, "{0:0.#}");
            ModSliderOption fruitGrowTimeSlider = ConfigMenu.fruitGrowTime.ToModSliderOption(0, 30, 1);
            ModSliderOption fishSpeedSlider = ConfigMenu.fishSpeedMult.ToModSliderOption(0.1f, 5f, .1f, "{0:0.#}");
            ModSliderOption creatureSpeedSlider = ConfigMenu.creatureSpeedMult.ToModSliderOption(0.1f, 5f, .1f, "{0:0.#}");
            ModSliderOption CreatureFleeChanceSlider = ConfigMenu.CreatureFleeChance.ToModSliderOption(0, 100, 1);
            ModSliderOption dropPodMaxPower = ConfigMenu.dropPodMaxPower.ToModSliderOption(0, 100, 5);
            ModSliderOption batteryChargeSlider = ConfigMenu.batteryChargeMult.ToModSliderOption(0.5f, 3f, .1f, "{0:0.#}");
            ModSliderOption craftedBatteryChargeSlider = ConfigMenu.craftedBatteryCharge.ToModSliderOption(0, 100, 1);
            ModSliderOption invMultWaterSlider = ConfigMenu.invMultWater.ToModSliderOption(0f, 5f, .01f, "{0:0.0#}");
            ModSliderOption invMultLandSlider = ConfigMenu.invMultLand.ToModSliderOption(0f, 5f, .01f, "{0:0.0#}");
            ModSliderOption waterFreezeSlider = ConfigMenu.waterFreezeRate.ToModSliderOption(0f, 5f, .1f, "{0:0.#}");
            ModSliderOption snowballWaterSlider = ConfigMenu.snowballWater.ToModSliderOption(0, 30, 1);
            ModSliderOption baseHullStrengthSlider = ConfigMenu.baseHullStrengthMult.ToModSliderOption(1f, 10f, .1f, "{0:0.#}");
            ModSliderOption drillDamageMultSlider = ConfigMenu.drillDamageMult.ToModSliderOption(1f, 10f, .1f, "{0:0.#}");
            ModSliderOption foodLossSlider = ConfigMenu.foodLossMult.ToModSliderOption(0, 3f, .1f, "{0:0.#}");
            ModSliderOption waterLossSlider = ConfigMenu.waterLossMult.ToModSliderOption(0, 3f, .1f, "{0:0.#}");
            ModSliderOption foodWaterHealThresholdSlider = ConfigMenu.foodWaterHealThreshold.ToModSliderOption(1, 300, 5);


            AddItem(timeFlowSpeedSlider);
            AddItem(playerWaterSpeedSlider);
            AddItem(playerGroundSpeedSlider);
            AddItem(seaglideSpeedSlider);
            AddItem(exosuitSpeedSlider);
            AddItem(seatruckSpeedSlider);
            AddItem(hoverbikeSpeedSlider);
            AddItem(oxygenSlider);
            AddItem(foodLossSlider);
            AddItem(waterLossSlider);
            AddItem(foodWaterHealThresholdSlider);
            AddItem(coldSlider);
            AddItem(knifeRangeSlider);
            AddItem(knifeDamageSlider);
            AddItem(craftTimeSlider);
            AddItem(buildTimeSlider);
            AddItem(coldSlider);
            //AddItem(ConfigMenu.playerMoveTweaks.ToModToggleOption());
            //AddItem(ConfigMenu.seatruckMoveTweaks.ToModToggleOption());
            //AddItem(ConfigMenu.exosuitMoveTweaks.ToModToggleOption());
            //AddItem(ConfigMenu.hoverbikeMoveTweaks.ToModToggleOption());
            AddItem(drillDamageMultSlider);
            AddItem(ConfigMenu.emptyVehiclesCanBeAttacked.ToModChoiceOption());
            AddItem(medKitHPslider);
            AddItem(ConfigMenu.cantUseMedkitUnderwater.ToModToggleOption());
            AddItem(ConfigMenu.useBestLOD.ToModToggleOption());
            AddItem(dropPodMaxPower);
            AddItem(fruitGrowTimeSlider);
            AddItem(crushDepthSlider);
            AddItem(crushDamageSlider);
            AddItem(vehicleCrushDamageSlider);
            AddItem(crushDamageProgressionSlider);
            AddItem(baseHullStrengthSlider);

            AddItem(ConfigMenu.newHungerSystem.ToModToggleOption());
            AddItem(ConfigMenu.eatRawFish.ToModChoiceOption());
            AddItem(ConfigMenu.cantEatUnderwater.ToModToggleOption());
            AddItem(fishFoodWaterRatioSlider);
            AddItem(foodDecayRateSlider);
            AddItem(waterFreezeSlider);
            AddItem(snowballWaterSlider);
            AddItem(fishSpeedSlider);
            AddItem(creatureSpeedSlider);
            AddItem(CreatureFleeChanceSlider);
            AddItem(ConfigMenu.creatureFleeUseDamageThreshold.ToModToggleOption());
            AddItem(ConfigMenu.creatureFleeChanceBasedOnHealth.ToModToggleOption());
            AddItem(ConfigMenu.waterparkCreaturesBreed.ToModToggleOption());
            AddItem(ConfigMenu.noFishCatching.ToModToggleOption());
            AddItem(ConfigMenu.noBreakingWithHand.ToModToggleOption());
            AddItem(ConfigMenu.damageImpactEffect.ToModToggleOption());
            AddItem(ConfigMenu.damageScreenFX.ToModToggleOption());
            AddItem(toolEnergySlider);
            AddItem(vehicleEnergySlider);
            AddItem(baseEnergySlider);
            AddItem(batteryChargeSlider);
            AddItem(craftedBatteryChargeSlider);
            AddItem(ConfigMenu.realOxygenCons.ToModToggleOption());
            AddItem(ConfigMenu.dropItemsOnDeath.ToModChoiceOption());
            AddItem(invMultWaterSlider);
            AddItem(invMultLandSlider);
            invMultLandSlider.OnChanged += InvMultSliderUpdated;
            invMultWaterSlider.OnChanged += InvMultSliderUpdated;
            AddItem(ConfigMenu.transferAllItemsButton.ToModKeybindOption());
            AddItem(ConfigMenu.transferSameItemsButton.ToModKeybindOption());
            AddItem(ConfigMenu.quickslotButton.ToModKeybindOption());
            AddItem(ConfigMenu.previousPDATabKey.ToModKeybindOption());
            AddItem(ConfigMenu.nextPDATabKey.ToModKeybindOption());

        }

        void TimeSpeedUpdated(object sender, SliderChangedEventArgs e)
        {
            //AddDebug("UpdateTimeSpeed");
            if (DayNightCycle.main)
                DayNightCycle.main._dayNightSpeed = ConfigMenu.timeFlowSpeed.Value;
        }

        void InvMultSliderUpdated(object sender, SliderChangedEventArgs e)
        {
            //AddDebug("InvMultSliderUpdate");
            Player_Movement.GetInvMod();
        }

    }
}
