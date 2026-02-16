using BepInEx.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class ConfigMenu
    {
        public static ConfigEntry<KeyCode> transferAllItemsKey;
        public static ConfigEntry<KeyCode> transferSameItemsKey;
        public static ConfigEntry<KeyCode> quickslotKey;
        public static ConfigEntry<KeyCode> nextPDATabKey;
        public static ConfigEntry<KeyCode> previousPDATabKey;
        public static ConfigEntry<float> timeFlowSpeed;
        public static ConfigEntry<float> seaglideSpeedMult;
        public static ConfigEntry<float> playerWaterSpeedMult;
        public static ConfigEntry<float> playerGroundSpeedMult;

        public static ConfigEntry<float> exosuitSpeedMult;
        public static ConfigEntry<float> seatruckSpeedMult;
        public static ConfigEntry<float> hoverbikeSpeedMult;

        public static ConfigEntry<bool> useRealTempForPlayer;
        public static ConfigEntry<float> coldMult;
        public static ConfigEntry<float> oxygenPerBreath;
        public static ConfigEntry<float> toolEnergyConsMult;
        public static ConfigEntry<float> vehicleEnergyConsMult;
        public static ConfigEntry<float> baseEnergyConsMult;
        public static ConfigEntry<float> knifeRangeMult;
        public static ConfigEntry<float> knifeDamageMult;
        public static ConfigEntry<int> medKitHP;
        public static ConfigEntry<float> craftTimeMult;
        public static ConfigEntry<float> buildTimeMult;
        public static ConfigEntry<bool> useBestLOD;

        public static ConfigEntry<int> crushDepth;
        public static ConfigEntry<float> crushDamage;
        public static ConfigEntry<float> vehicleCrushDamageMult;
        public static ConfigEntry<float> crushDamageProgression;
        public static ConfigEntry<EmptyVehiclesCanBeAttacked> emptyVehiclesCanBeAttacked;
        public static ConfigEntry<int> fishFoodWaterRatio;
        public static ConfigEntry<EatingRawFish> eatRawFish;
        public static ConfigEntry<bool> cantEatUnderwater;
        public static ConfigEntry<bool> cantUseMedkitUnderwater;
        public static ConfigEntry<float> foodDecayRateMult;
        //public static ConfigEntry<int> fruitGrowTime;
        public static ConfigEntry<float> fishSpeedMult;
        public static ConfigEntry<float> creatureSpeedMult;
        public static ConfigEntry<int> CreatureFleeChance;
        public static ConfigEntry<bool> creatureFleeUseDamageThreshold;
        public static ConfigEntry<bool> creatureFleeChanceBasedOnHealth;
        public static ConfigEntry<bool> waterparkCreaturesBreed;
        public static ConfigEntry<bool> noFishCatching;
        public static ConfigEntry<bool> noBreakingWithHand;
        public static ConfigEntry<bool> damageImpactEffect;
        public static ConfigEntry<bool> damageScreenFX;
        public static ConfigEntry<bool> realOxygenCons;
        public static ConfigEntry<int> dropPodMaxPower;
        public static ConfigEntry<float> batteryChargeMult;
        public static ConfigEntry<int> craftedBatteryCharge;
        public static ConfigEntry<DropItemsOnDeath> dropItemsOnDeath;
        public static ConfigEntry<float> invMultWater;
        public static ConfigEntry<float> waterFreezeRate;
        public static ConfigEntry<int> snowballWater;
        public static ConfigEntry<float> baseHullStrengthMult;
        public static ConfigEntry<float> drillDamageMult;
        public static ConfigEntry<float> foodLossMult;
        public static ConfigEntry<float> waterLossMult;
        public static ConfigEntry<int> foodHealThreshold;
        public static ConfigEntry<float> invMultLand;


        public static void Bind()
        {  // “ ” ‛
            seatruckSpeedMult = Main.configMenu.Bind("", "TF_seatruck_speed_mult", 1f, "");
            hoverbikeSpeedMult = Main.configMenu.Bind("", "TF_hoverbike_speed_mult", 1f, "");
            coldMult = Main.configMenu.Bind("", "TF_cold_mult", 1f, "TF_cold_mult_desc");
            useRealTempForPlayer = Main.configMenu.Bind("", "TF_use_real_temp_for_player", false, "TF_use_real_temp_for_player_desc");
            useBestLOD = Main.configMenu.Bind("", "TF_use_best_LOD", false, "TF_use_best_LOD_desc");
            waterFreezeRate = Main.configMenu.Bind("", "TF_water_freeze_rate", 0f, "TF_water_freeze_rate_desc");
            snowballWater = Main.configMenu.Bind("", "TF_snowball_water", 0, "TF_snowball_water_desc");
            timeFlowSpeed = Main.configMenu.Bind("", "TF_time_flow", 1f, "TF_time_flow_desc");
            foodLossMult = Main.configMenu.Bind("", "TF_food_loss_mult", 1f, "TF_food_loss_mult_desc");
            waterLossMult = Main.configMenu.Bind("", "TF_water_loss_mult", 1f, "TF_water_loss_mult_desc");
            playerWaterSpeedMult = Main.configMenu.Bind("Player_movement", "TF_player_speed_mult_water", 1f);
            playerGroundSpeedMult = Main.configMenu.Bind("Player_movement", "TF_player_speed_mult_ground", 1f);
            exosuitSpeedMult = Main.configMenu.Bind("", "TF_prawn_suit_speed_mult", 1f, "");
            oxygenPerBreath = Main.configMenu.Bind("", "TF_oxygen_per_breath", 3f);
            toolEnergyConsMult = Main.configMenu.Bind("", "TF_tool_power_mult", 1f, "TF_tool_power_mult_desc");
            vehicleEnergyConsMult = Main.configMenu.Bind("", "TF_vehicle_power_mult", 1f, "TF_vehicle_power_mult_desc");
            baseEnergyConsMult = Main.configMenu.Bind("", "TF_base_power_mult", 1f, "TF_base_power_mult_desc");
            baseHullStrengthMult = Main.configMenu.Bind("", "TF_base_hull_strength_mult", 1f, "");
            knifeRangeMult = Main.configMenu.Bind("", "TF_knife_range_mult", 1f, "TF_knife_damage_mult_desc");
            knifeDamageMult = Main.configMenu.Bind("", "TF_knife_damage_mult", 1f, "TF_knife_damage_mult_desc");
            medKitHP = Main.configMenu.Bind("", "TF_med_kit_health", 50, "TF_med_kit_health_desc");
            craftTimeMult = Main.configMenu.Bind("", "TF_crafting_time_mult", 1f, "TF_crafting_time_mult_desc");
            buildTimeMult = Main.configMenu.Bind("", "TF_building_time_mult", 1f, "TF_building_time_mult_desc");
            seaglideSpeedMult = Main.configMenu.Bind("Player movement", "TF_seaglide_speed_mult", 1f, "");

            crushDepth = Main.configMenu.Bind("", "TF_crush_depth", 200, "TF_crush_depth_desc");
            crushDamage = Main.configMenu.Bind("", "TF_crush_damage", 0f, "TF_crush_damage_desc");
            crushDamageProgression = Main.configMenu.Bind("", "TF_crush_damage_progression", 0f, "TF_crush_damage_progression_desc");
            vehicleCrushDamageMult = Main.configMenu.Bind("", "TF_vehicle_crush_damage_mult", 1f);
            emptyVehiclesCanBeAttacked = Main.configMenu.Bind("", "TF_unmanned_vehicles_attack", EmptyVehiclesCanBeAttacked.TF_default_setting, "TF_unmanned_vehicles_attack_desc");

            fishFoodWaterRatio = Main.configMenu.Bind("", "TF_fish_water_food_value_ratio", 0, "TF_fish_water_food_value_ratio_desc");
            eatRawFish = Main.configMenu.Bind("", "TF_eating_raw_fish", EatingRawFish.TF_default_setting, "TF_eating_raw_fish_desc");
            cantEatUnderwater = Main.configMenu.Bind("", "TF_can_not_eat_underwater", false, "TF_can_not_eat_underwater_desc");
            cantUseMedkitUnderwater = Main.configMenu.Bind("", "TF_can_not_use_med_kit_underwater", false, "TF_can_not_use_med_kit_underwater_desc");
            foodDecayRateMult = Main.configMenu.Bind("", "TF_food_decay_rate_mult", 1f, "TF_reload_game");
            fishSpeedMult = Main.configMenu.Bind("", "TF_fish_speed_mult", 1f, "TF_fish_speed_mult_desc");
            creatureSpeedMult = Main.configMenu.Bind("", "TF_creature_speed_mult", 1f, "TF_creature_speed_mult_desc");
            CreatureFleeChance = Main.configMenu.Bind("", "TF_creature_flee_chance_percent", 100, "TF_creature_flee_chance_percent_desc");
            creatureFleeUseDamageThreshold = Main.configMenu.Bind("", "TF_creature_flee_damage_threshold", true, "TF_creature_flee_damage_threshold_desc");
            creatureFleeChanceBasedOnHealth = Main.configMenu.Bind("", "TF_creature_flee_chance_depends_on_its_health", false, "TF_creature_flee_chance_depends_on_its_health_desc");
            waterparkCreaturesBreed = Main.configMenu.Bind("", "TF_creatures_in_alien_containment_can_breed", true);
            noFishCatching = Main.configMenu.Bind("", "TF_can_not_catch_fish_with_bare_hand", false, "TF_can_not_catch_fish_with_bare_hand_desc");
            noBreakingWithHand = Main.configMenu.Bind("", "TF_can_not_break_outcrop_with_bare_hand", false, "TF_can_not_break_outcrop_with_bare_hand_desc");
            damageImpactEffect = Main.configMenu.Bind("", "TF_player_impact_damage_screen_effects", true, "TF_player_impact_damage_screen_effects_desc");
            damageScreenFX = Main.configMenu.Bind("", "TF_player_damage_screen_effects", true, "TF_player_damage_screen_effects_desc");
            realOxygenCons = Main.configMenu.Bind("", "TF_realistic_oxygen_consumption", false, "TF_realistic_oxygen_consumption_desc");
            dropPodMaxPower = Main.configMenu.Bind("", "TF_life_pod_power_cell_max_charge", 0, "TF_life_pod_power_cell_max_charge_desc");
            batteryChargeMult = Main.configMenu.Bind("", "TF_battery_charge_mult", 1f);
            craftedBatteryCharge = Main.configMenu.Bind("", "TF_crafted_battery_charge_percent", 100, "TF_crafted_battery_charge_percent_desc");
            dropItemsOnDeath = Main.configMenu.Bind("", "TF_drop_items_when_you_die", DropItemsOnDeath.TF_default_setting);
            invMultLand = Main.configMenu.Bind("", "TF_inventory_weight_mult_ground", 0f, "TF_inventory_weight_mult_ground_desc");
            invMultWater = Main.configMenu.Bind("", "TF_inventory_weight_mult_water", 0f, "TF_inventory_weight_mult_water_desc");
            drillDamageMult = Main.configMenu.Bind("", "TF_prawn_suit_drill_arm_damage_mult", 1f, "TF_reload_game");
            foodHealThreshold = Main.configMenu.Bind("", "TF_food_heal_threshold", 150, "TF_food_heal_threshold_desc");

            transferAllItemsKey = Main.configMenu.Bind("", "TF_move_all_items", KeyCode.None, "TF_move_all_items_desc");
            transferSameItemsKey = Main.configMenu.Bind("", "TF_move_same_items", KeyCode.None, "TF_move_same_items_desc");
            quickslotKey = Main.configMenu.Bind("", "TF_quick_slot_cycle", KeyCode.None, "TF_quick_slot_cycle_desc");
            previousPDATabKey = Main.configMenu.Bind("", "TF_previous_PDA_tab", KeyCode.None, "TF_previous_PDA_tab_desc");
            nextPDATabKey = Main.configMenu.Bind("", "TF_next_PDA_tab", KeyCode.None, "TF_next_PDA_tab_desc");
            //Main.logger.LogMessage("configMenu.Bind !!! ");
        }

        public enum DropItemsOnDeath { TF_default_setting, TF_drop_items_death_setting_everything, TF_drop_items_death_setting_nothing }
        public enum EmptyVehiclesCanBeAttacked { TF_default_setting, Yes, No, TF_empty_vehicle_can_be_attacked_setting_light }
        public enum EatingRawFish { TF_default_setting, TF_eat_raw_fish_setting_harmless, TF_eat_raw_fish_setting_risky, TF_eat_raw_fish_setting_harmful }
    }
}
