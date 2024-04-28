using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx.Configuration;
using Nautilus.Json;
using UnityEngine;

namespace Tweaks_Fixes
{
    public static class ConfigToEdit
    {
        public static ConfigEntry<bool> heatBladeCooks;
        public static ConfigEntry<bool> dontSpawnKnownFragments;
        public static ConfigEntry<bool> noKillParticles;
        public static ConfigEntry<bool> alwaysShowHealthFoodNunbers;
        public static ConfigEntry<bool> pdaClock;
        public static ConfigEntry<Button> transferAllItemsButton;
        public static ConfigEntry<Button> transferSameItemsButton;
        public static ConfigEntry<Button> quickslotButton;
        public static ConfigEntry<string> gameStartWarningText;
        public static ConfigEntry<string> newGameLoot;
        public static ConfigEntry<string> crushDepthEquipment;
        public static ConfigEntry<string> crushDamageEquipment;
        public static ConfigEntry<string> itemMass;
        public static ConfigEntry<string> unmovableItems;
        public static ConfigEntry<Vector3> bloodColor;
        public static ConfigEntry<string> gravTrappable;
        public static ConfigEntry<float> medKitHPperSecond;
        //public static ConfigEntry<string> silentCreatures;
        public static ConfigEntry<string> eatableFoodValue;
        public static ConfigEntry<string> eatableWaterValue;
        public static ConfigEntry<bool> fixMelon;
        public static ConfigEntry<bool> randomPlantRotation;
        public static ConfigEntry<bool> silentReactor;
        //public static ConfigEntry<bool> newUIstrings;
        public static ConfigEntry<bool> disableUseText;
        public static ConfigEntry<bool> craftWithoutBattery;
        public static ConfigEntry<bool> builderPlacingWhenFinishedBuilding;
        public static ConfigEntry<bool> crushDamageScreenEffect;
        public static ConfigEntry<bool> disableGravityForExosuit;
        public static ConfigEntry<bool> replaceDealDamageOnImpactScript;
        //public static ConfigEntry<float> exosuitDealDamageMinSpeed;
        //public static ConfigEntry<float> exosuitTakeDamageMinSpeed;
        //public static ConfigEntry<float> exosuitTakeDamageMinMass;
        //public static ConfigEntry<float> seamothDealDamageMinSpeed;
        //public static ConfigEntry<float> seamothTakeDamageMinSpeed;
        //public static ConfigEntry<float> seamothTakeDamageMinMass;
        public static ConfigEntry<float> solarPanelMaxDepth;
        public static ConfigEntry<bool> newStorageUI;
        public static ConfigEntry<bool> canReplantMelon;
        public static ConfigEntry<int> eatingOutsideCold;
        public static ConfigEntry<int> brinewingAttackColdDamage;
        public static ConfigEntry<bool> fixCoral;
        public static ConfigEntry<string> notRespawningCreatures;
        public static ConfigEntry<string> notRespawningCreaturesIfKilledByPlayer;
        public static ConfigEntry<bool> warmKelpWater;
        public static ConfigEntry<int> brinicleDaysToGrow;
        public static ConfigEntry<bool> exosuitTakesDamageFromCollisions;
        public static ConfigEntry<bool> vehiclesTakeDamageOnImpact;
        public static ConfigEntry<bool> vehiclesDealDamageOnImpact;
        public static ConfigEntry<bool> exosuitTakesDamageWhenCollidingWithTerrain;
        public static ConfigEntry<string> decayingFood;
        public static ConfigEntry<string> respawnTime;
        public static ConfigEntry<bool> craftVehicleUpgradesOnlyInMoonpool;
        public static ConfigEntry<int> warmTemp;
        public static ConfigEntry<int> insideBaseTemp;
        public static ConfigEntry<bool> propulsionCannonGrabFX;
        public static ConfigEntry<int> rockPuncherChanceToFindRock;
        public static ConfigEntry<bool> lowOxygenWarning;
        public static ConfigEntry<bool> lowOxygenAudioWarning;
        public static ConfigEntry<bool> disableHints;
        public static ConfigEntry<bool> dropHeldTool;
        public static ConfigEntry<int> freeTorpedos;

        

        public static AcceptableValueRange<float> medKitHPperSecondRange = new AcceptableValueRange<float>(0.001f, 100f);
        public static AcceptableValueRange<int> percentRange = new AcceptableValueRange<int>(0, 100);

        public static void Bind()
        {
            //Main.logger.LogMessage("ConfigToEdit bind start ");
            heatBladeCooks = Main.configToEdit.Bind("", "Thermoblade cooks fish on kill", true);
            dontSpawnKnownFragments = Main.configToEdit.Bind("", "Do not spawn fragments for unlocked blueprints", false);
            noKillParticles = Main.configToEdit.Bind("", "No particles when creature dies", false, "No yellow cloud particles will spawn when a creature dies. Game has to be reloaded after changing this. ");
            alwaysShowHealthFoodNunbers = Main.configToEdit.Bind("", "Always show health and food values in UI", false);
            pdaClock = Main.configToEdit.Bind("", "PDA clock", true);
            newGameLoot = Main.configToEdit.Bind("", "Drop pod items", "FilteredWater 0, NutrientBlock 0, Flare 0", "Items you find in your drop pod when you start a new game. The format is item ID, space, number of items. Every entry is separated by comma.");
            crushDepthEquipment = Main.configToEdit.Bind("", "Crush depth equipment", "ReinforcedDiveSuit 11", "Allows you to make your equipment increase your crush depth. The format is: item ID, space, number of meters that will be added to your crush depth. Every entry is separated by comma.");
            crushDamageEquipment = Main.configToEdit.Bind("", "Crush damage equipment", "ReinforcedDiveSuit 0", "Allows you to make your equipment reduce your crush damage. The format is: item ID, space, crush damage percent that will be blocked. Every entry is separated by comma.");
            itemMass = Main.configToEdit.Bind("", "Item mass", "flare 22", "Allows you to change mass of pickupable items. The format is: item ID, space, item mass in kg as decimal point number. Every entry is separated by comma.");
            unmovableItems = Main.configToEdit.Bind("", "Unmovable items", "", "Comma separated list of pickupable item IDs. Items in this list can not be moved by bumping into them. You will always find them where you dropped them.");
            bloodColor = Main.configToEdit.Bind("", "Blood color", new Vector3(0.784f, 1f, 0.157f), "Lets you change the color of creatures' blood. Each value is a decimal point number from 0 to 1. First number is red. Second number is green. Third number is blue.");
            gravTrappable = Main.configToEdit.Bind("", "Gravtrappable items", "seaglide, airbladder, flare, flashlight, builder, lasercutter, ledlight, divereel, propulsioncannon, welder, repulsioncannon, scanner, stasisrifle, knife, heatblade, metaldetector, teleportationtool, precursorkey_blue, precursorkey_orange, precursorkey_purple, precursorkey_red, precursorkey_white, compass, fins, fireextinguisher, suitboostertank, firstaidkit, doubletank, plasteeltank, coldsuit, flashlighthelmet, rebreather, reinforceddivesuit, maproomhudchip, tank, stillsuit, swimchargefins, ultraglidefins, highcapacitytank,", "Comma separated list of items affected by grav trap.");
            medKitHPperSecond = Main.configToEdit.Bind("", "Amount of HP restored by first aid kit every second", 100f, new ConfigDescription("Set this to a low number to slowly restore HP after using first aid kit.", medKitHPperSecondRange));
            //silentCreatures = Main.configB.Bind("", "Silent creatures", "", "List of creature IDs separated by comma. Creatures in this list will be silent.");
            eatableFoodValue = Main.configToEdit.Bind("", "Eatable component food value", "CreepvineSeedCluster 5", "Items from this list will be made eatable. The format is: item ID, space, food value. Every entry is separated by comma.");
            eatableWaterValue = Main.configToEdit.Bind("", "Eatable component water value", "HangingFruit 5", "Items from this list will be made eatable. The format is: item ID, space, water value. Every entry is separated by comma.");

            eatingOutsideCold = Main.configToEdit.Bind("", "Warmth lost when eating ouside", 0, "You will lose this amount of warmth when eating ouside.");
            
            fixMelon = Main.configToEdit.Bind("", "Fix melon and Preston plant", false, "If true, you will be able to plant only 1 Preston plant or melon in a pot and only 4 in a planter.");
            randomPlantRotation = Main.configToEdit.Bind("", "Random plant rotation", true, "If true plants in planters to have random rotation.");
            silentReactor = Main.configToEdit.Bind("", "Silent nuclear reactor", false, "If true nuclear reactor will be silent.");
            //newUIstrings = Main.configB.Bind("", "New UI text", true, "If false new UI elements added by the mod wil be disabled.");
            newStorageUI = Main.configToEdit.Bind("", "New storage UI", true);
            disableUseText = Main.configToEdit.Bind("", "Disable quickslots text", false, "If true text above your quickslots will be disabled.");
            craftWithoutBattery = Main.configToEdit.Bind("", "Craft without battery", false, "If true your newly crafted tools and vehicles will not have batteries in them.");
            builderPlacingWhenFinishedBuilding = Main.configToEdit.Bind("", "Builder tool placing mode when finished building", true, "If false your builder tool will exit placing mode when you finish building.");
            crushDamageScreenEffect = Main.configToEdit.Bind("", "Crush damage screen effect", true, "If false there will be no screen effects when player takes crush damage.");
            disableGravityForExosuit = Main.configToEdit.Bind("", "Disable gravity for prawn suit", false, "If true prawn suit will ignore gravity when you are not piloting it. Use this if your prawn suit falls through the ground.");

            //exosuitDealDamageMinSpeed = Main.configB.Bind("", "Prawn suit min speed to deal damage", 7f, "Min speed in meters per second at which prawn suit deals damage when colliding with objects. Works only if 'replaceDealDamageOnImpactScript' setting is true.");
            //exosuitTakeDamageMinSpeed = Main.configB.Bind("", "Prawn suit min speed to take damage", 7f, "Min speed in meters per second at which prawn suit takes damage when colliding with objects. Works only if 'replaceDealDamageOnImpactScript' setting is true.");
            //exosuitTakeDamageMinMass = Main.configB.Bind("", "Min mass that can damage prawn suit", 5f, "Min mass in kg for objects that can damage prawn suit when colliding with it. Works only if 'replaceDealDamageOnImpactScript' setting is true.");
            //seamothDealDamageMinSpeed = Main.configB.Bind("", "Seamoth min speed to deal damage", 7f, "Min speed in meters per second at which seamoth deals damage when colliding with objects. Works only if 'replaceDealDamageOnImpactScript' setting is true.");
            //seamothTakeDamageMinSpeed = Main.configB.Bind("", "Seamoth min speed to take damage", 7f, "Min speed in meters per second at which seamoth takes damage when colliding with objects. Works only if 'replaceDealDamageOnImpactScript' setting is true.");
            //seamothTakeDamageMinMass = Main.configB.Bind("", "Min mass that can damage seamoth", 5f, "Min mass in kg for objects that can damage seamoth when colliding with it. Works only if 'replaceDealDamageOnImpactScript' setting is true.");
            solarPanelMaxDepth = Main.configToEdit.Bind("", "Solar panel max depth", 250f, "Depth in meters below which solar panel does not produce power.");
            
            canReplantMelon = Main.configToEdit.Bind("", "Can replant melon", true, "If false gel sack and melon can't be replanted.");
            brinewingAttackColdDamage = Main.configToEdit.Bind("", "Brinewing freeze damage", 0, "When this is not 0 brinewing attack will drain your cold meter by this amount instead of freezing you.");

            fixCoral = Main.configToEdit.Bind("", "Fix table coral", true, "If true then table coral will always be attached horizontally to rocks and its animation will be disabled.");
            notRespawningCreatures = Main.configToEdit.Bind("", "Not respawning creatures", "TrivalveBlue, TrivalveYellow", "Comma separated list of creature IDs that will not respawn.");
            notRespawningCreaturesIfKilledByPlayer = Main.configToEdit.Bind("", "Not respawning creatures if killed by player", "TitanHolefish, BruteShark, Cryptosuchus, SnowStalker, SnowStalkerBaby, RockPuncher, SquidShark", "Comma separated list of creature IDs that will respawn only if killed by another creature.");
            respawnTime = Main.configToEdit.Bind("", "Creature respawn time", "", "Number of days it takes a creature to respawn. The format is: creature ID, space, number of days it takes to respawn. By default fish and big creatures respawn in 12 hours, leviathans respawn after 1 day.");
            warmKelpWater = Main.configToEdit.Bind("", "Warm kelp water", true, "Water is always warm near kelps. Set this to false to disable it.");
            brinicleDaysToGrow = Main.configToEdit.Bind("", "Brinicle growth time", 0, "Number of days it takes a brinicle to grow. Set this to 0 to use vanilla game value.");
            replaceDealDamageOnImpactScript = Main.configToEdit.Bind("", "Replace DealDamageOnImpact script", true, "Replace script that handles vehicle collisions.");
            vehiclesTakeDamageOnImpact = Main.configToEdit.Bind("", "Vehicles take damage from collisions", true, "Works only if 'Replace DealDamageOnImpact script' is true");
            exosuitTakesDamageFromCollisions = Main.configToEdit.Bind("", "Prawn suit takes damage from collisions", true, "Works only if 'Replace DealDamageOnImpact script' is true");
            vehiclesDealDamageOnImpact = Main.configToEdit.Bind("", "Vehicles deal damage when colliding", true, "Works only if 'Replace DealDamageOnImpact script' is true");
            exosuitTakesDamageWhenCollidingWithTerrain = Main.configToEdit.Bind("", "Prawn suit takes damage when colliding with terrain", true, "Works only if 'Replace DealDamageOnImpact script' is true");
            decayingFood = Main.configToEdit.Bind("", "Decaying food", "SpicyFruitSalad", "Comma separated list of food item IDs. Food from this list will decay.");
            craftVehicleUpgradesOnlyInMoonpool = Main.configToEdit.Bind("", "Only Vehicle upgrade console can craft vehicle upgrades", false, "Fabricator will not be able to craft vehicle upgrades if this is true.");
            warmTemp = Main.configToEdit.Bind("", "Warm temperature", 15, "Player is warm when ambient temperature is above this celsius value.");
            insideBaseTemp = Main.configToEdit.Bind("", "Temperature inside base", 22, "Celsius temperature inside powered base or vehicle. Used only when 'Only ambient tempterature makes player warm' setting is on.");
            gameStartWarningText = Main.configToEdit.Bind("", "Game start warning text", "", "Text shown when the game starts. If this field is empty the warning will be skipped.");
            propulsionCannonGrabFX = Main.configToEdit.Bind("", "Propulsion cannon sphere effect", true, "Blue sphere visual effect you see when holding an object with propulsion cannon will be disabled if this is false.");
            rockPuncherChanceToFindRock = Main.configToEdit.Bind("", "Rock puncher chance percent to find rock", 20, new ConfigDescription("", percentRange));
            lowOxygenWarning = Main.configToEdit.Bind("", "Low oxygen onscreen warning", true);
            lowOxygenAudioWarning = Main.configToEdit.Bind("", "Low oxygen audio warning", true);

            disableHints = Main.configToEdit.Bind("", "Disable tutorial messages", true, "This disables messages that tell you to 'eat something', 'break limestone', etc. Game has to be reloaded after changing this.");

            dropHeldTool = Main.configToEdit.Bind("", "Drop held tool when taking damage", false, "Chance percent to drop your tool is equal to amount of damage taken.");

            freeTorpedos = Main.configToEdit.Bind("", "Free torpedos", 2, "Number of torpedos you get when installing new Prawn Suit Torpedo Arm. After changing this you have to craft a new Torpedo Arm.");

            transferAllItemsButton = Main.configToEdit.Bind("", "Move all items button", Button.None, "Press this button to move all items from one container to another. This works only with controller. Use this if you can not bind a controller button in the mod menu.");
            transferSameItemsButton = Main.configToEdit.Bind("", "Move same items button", Button.None, "Press this button to move all items of the same type from one container to another. This works only with controller. Use this if you can not bind a controller button in the mod menu.");
            quickslotButton = Main.configToEdit.Bind("", "Quickslot cycle button", Button.None, "Press 'Cycle next' or 'Cycle previous' button while holding down this button to cycle tools in your current quickslot. This works only with controller. Use this if you can not bind a controller button in the mod menu.");
            //Main.logger.LogMessage("ConfigToEdit bind end ");
        }

        private static Dictionary<TechType, int> ParseIntDicFromString (string input)
        {
            Dictionary<TechType, int> dic = new Dictionary<TechType, int>();
            string[] entries = input.Split(',');
            for (int i = 0; i < entries.Length; i++)
            {
                string s = entries[i].Trim();
                string techType;
                string amount;
                int index = s.IndexOf(' ');
                if (index == -1)
                    continue;

                techType = s.Substring(0, index);
                amount = s.Substring(index);
                if (!TechTypeExtensions.FromString(techType, out TechType tt, true))
                    continue;
                // no simple way to check if techType is pickupable
                int a = 0;
                try
                {
                    a = int.Parse(amount);
                }
                catch (Exception)
                {
                    continue;
                }
                if (a < 1)
                    continue;

                dic.Add(tt, a);
            }
            return dic;
        }

        private static Dictionary<TechType, float> ParseFloatDicFromString(string input)
        {
            Dictionary<TechType, float> dic = new Dictionary<TechType, float>();
            string[] entries = input.Split(',');
            for (int i = 0; i < entries.Length; i++)
            {
                string s = entries[i].Trim();
                string techType;
                string amount;
                int index = s.IndexOf(' ');
                if (index == -1)
                    continue;

                techType = s.Substring(0, index);
                amount = s.Substring(index);
                if (!TechTypeExtensions.FromString(techType, out TechType tt, true))
                    continue;
                // no simple way to check if techType is pickupable
                float fl = 0;
                try
                {
                    fl = float.Parse(amount);
                }
                catch (Exception)
                {
                    continue;
                }
                if (fl < 1)
                    continue;

                dic.Add(tt, fl);
            }
            return dic;
        }

        private static HashSet<TechType> ParseSetFromString(string input)
        {
            HashSet<TechType> set = new HashSet<TechType>();
            string[] entries = input.Split(',');
            for (int i = 0; i < entries.Length; i++)
            {
                string techType = entries[i].Trim();

                if (!TechTypeExtensions.FromString(techType, out TechType tt, true))
                    continue;
                // no simple way to check if techType is pickupable
                set.Add(tt);
                //Main.logger.LogDebug("ParseSetFromString " + tt );
            }
            return set;
        }

        public static void ParseFromConfig()
        {
            Crush_Damage.crushDepthEquipment = ParseIntDicFromString(crushDepthEquipment.Value);
            Crush_Damage.crushDamageEquipment = ParseIntDicFromString(crushDamageEquipment.Value);
            Pickupable_Patch.itemMass = ParseFloatDicFromString(itemMass.Value);
            Pickupable_Patch.unmovableItems = ParseSetFromString(unmovableItems.Value);
            Gravsphere_Patch.gravTrappable = ParseSetFromString(gravTrappable.Value);
            //Creature_Tweaks.silentCreatures = ParseSetFromString<TechType>(silentCreatures.Value);
            Pickupable_Patch.eatableFoodValue = ParseIntDicFromString(eatableFoodValue.Value);
            Pickupable_Patch.eatableWaterValue = ParseIntDicFromString(eatableWaterValue.Value);
            Drop_Pod_Patch.newGameLoot = ParseIntDicFromString(newGameLoot.Value);
            Creature_Patch.notRespawningCreatures = ParseSetFromString(notRespawningCreatures.Value);
            Creature_Patch.notRespawningCreaturesIfKilledByPlayer = ParseSetFromString(notRespawningCreaturesIfKilledByPlayer.Value);
            Food_Patch.decayingFood = ParseSetFromString(decayingFood.Value);
            Creature_Patch.respawnTime = ParseIntDicFromString(respawnTime.Value);

            Enum.TryParse(transferAllItemsButton.Value.ToString(), out Inventory_Patch.transferAllItemsButton);
            Enum.TryParse(transferSameItemsButton.Value.ToString(), out Inventory_Patch.transferSameItemsButton);
            Enum.TryParse(quickslotButton.Value.ToString(), out QuickSlots_Patch.quickslotButton);

            //Main.logger.LogInfo("decayingFood str  " + decayingFood.Value);
            //Main.logger.LogInfo("decayingFood.Count  " + Food_Patch.decayingFood.Count);
        }

        public enum Button
        {
            Jump,
            PDA,
            Deconstruct,
            Exit,
            LeftHand,
            RightHand,
            CycleNext,
            CyclePrev,
            AltTool,
            TakePicture,
            Reload,
            Sprint,
            LookUp,
            LookDown,
            LookLeft,
            LookRight,
            None
        }

    }
}
