using System.Collections.Generic;
using UnityEngine;
using Nautilus.Json;
using Nautilus.Options.Attributes;
using Nautilus.Commands;
using Nautilus.Handlers;
using Nautilus.Options;


namespace Tweaks_Fixes
{ 
    //[Menu("Tweaks and Fixes")]
    public class Config : ConfigFile
    {
        //[Slider("damage multiplier", 0f, 33f, DefaultValue = 1f, Step = 1f, Format = "{0:0.#}", Tooltip = "")]
        //public float damageMult = 1f;

        [Slider("Time flow speed multiplier", 0.1f, 10f, DefaultValue = 1f, Step = .1f, Format = "{0:0.#}", Tooltip = "The higher the value the shorter days are. This also affects crafting time, building time, battery charging time."), OnChange(nameof(UpdateTimeFlowSpeed))]
        public float timeFlowMult = 1f;

        [Slider("Player speed multiplier", .1f, 5f, DefaultValue = 1f, Step = .1f, Format = "{0:0.#}", Tooltip = "Your swimming, walking and running speed will be multiplied by this.")]
        public float playerSpeedMult = 1f;

        //[Slider("Vehicle damage multiplier", 0f, 3f, DefaultValue = 1f, Step = .1f, Format = "{0:0.#}", Tooltip = "Amount of damage your vehicles take will be multiplied by this.")]
        //public float vehicleDamageMult = 1f;
        [Slider("Prawn suit speed multiplier", .5f, 3f, DefaultValue = 1f, Step = .1f, Format = "{0:0.#}", Tooltip = "Your prawn suit speed will be multiplied by this.")]
        public float exosuitSpeedMult = 1f;

        [Slider("Seatruck speed multiplier", .5f, 3f, DefaultValue = 1f, Step = .1f, Format = "{0:0.#}", Tooltip = "Your seatruck speed will be multiplied by this.")]
        public float seatruckSpeedMult = 1f;

        [Slider("Snowfox speed multiplier", .5f, 3f, DefaultValue = 1f, Step = .1f, Format = "{0:0.#}", Tooltip = "Your snowfox speed will be multiplied by this.")]
        public float snowfoxSpeedMult = 1f;

        //[Slider("Predator aggression multiplier", 0f, 3f, DefaultValue = 1f, Step = .1f, Format = "{0:0.#}", Tooltip = "The higher it is the more aggressive predators are towards you. When it's 0 you and your vehicles will never be attacked. When it's 3 predators attack you on sight and never flee.")]
        //public float aggrMult = 1f;
        [Slider("Oxygen per breath", 0f, 6f, DefaultValue = 3f, Step = 0.1f, Format = "{0:0.#}", Tooltip = "Amount of oxygen you consume every breath.")]
        public float oxygenPerBreath = 3f;

        [Slider("Getting cold rate multiplier", 0f, 3f, DefaultValue = 1f, Step = 0.1f, Format = "{0:0.#}", Tooltip = "The rate at which you get cold when outside and get warm when inside will be multiplied by this")]
        public float coldMult = 1f;

        [Slider("Knife attack range multiplier", 1f, 5f, DefaultValue = 1f, Step = .1f, Format = "{0:0.#}", Tooltip = "Applies to knife and heatblade. You have to reequip your knife after changing this.")]
        public float knifeRangeMult = 1f;
        [Slider("Knife damage multiplier", 1f, 5f, DefaultValue = 1f, Step = .1f, Format = "{0:0.#}", Tooltip = "Applies to knife and heatblade. You have to reequip your knife after changing this.")]
        public float knifeDamageMult = 1f;

        [Slider("Crafting time multiplier", 0.1f, 4f, DefaultValue = 1f, Step = 0.1f, Format = "{0:0.#}", Tooltip = "Crafting time will be multiplied by this when crafting things with fabricator or modification station. Does not work with EasyCraft mod.")]
        public float craftTimeMult = 1f;

        [Slider("Building time multiplier", 0.1f, 4f, DefaultValue = 1f, Step = 0.1f, Format = "{0:0.#}", Tooltip = "Building time will be multiplied by this when using builder tool.")]
        public float buildTimeMult = 1f;

        [Toggle("Only ambient tempterature makes player warm", Tooltip = "In vanilla game when you are underwater you get warm if moving and get cold when not. When out of water, some areas (caves, your unpowered base) make you warm regardless of ambient tempterature. With this on you get warm only if ambient temperature is above 'Warm temperature' setting in the config file.")]
        public bool useRealTempForPlayerTemp = false;

        [Toggle("Player movement tweaks", Tooltip = "Player vertical, backward, sideways movement speed is halved. Any diving suit reduces your speed by 10% on land and in water. Fins reduce your speed by 10% on land. Lightweight high capacity tank reduces your speed by 5%. Every other tank reduces your speed by 10% on both land and water. You can sprint only if moving forward. Seaglide works only if moving forward. When swimming while your PDA is open your movement speed is halved. When swimming while holding a tool in your hand your movement speed is reduced to 70%.")]
        public bool playerMoveTweaks = false;

        [Toggle("Snowfox movement tweaks", Tooltip = "Snowfox can move on water as good as on land. Its backward and sideways speed is halved. You can boost for as long as you hold the sprint button. Power consumption is doubled when boosting.")]
        public bool hoverbikeMoveTweaks = false;

        [Toggle("Prawn suit movement tweaks", Tooltip = "Prawn suit can not move sideways. No time limit when using thrusters, but they consume twice more power. Vertical speed is reduced when using thrusters. Can't use thrusters to hover above ground when out of water.")]
        public bool exosuitMoveTweaks = false;

        [Toggle("Seatruck movement tweaks", Tooltip = "Seatruck's vertical, sideways and backward speed is halved. Afterburner is active for as long as you hold the 'sprint' key but consumes twice more power. Horsepower upgrade increases seatruck's speed by 10%. You can install more than 1 Horsepower upgrade.")]
        public bool seatruckMoveTweaks = false;

        [Choice("Unmanned vehicles can be attacked", Tooltip = "When 'Only_if_lights_on' is selected, you have to unpower your seatruck to prevent attacks on it.")]
        public EmptyVehicleCanBeAttacked emptyVehicleCanBeAttacked;

        [Slider("First aid kit HP", 10, 100, DefaultValue = 50, Step = 1, Format = "{0:F0}", Tooltip = "HP restored by using first aid kit.")]
        public int medKitHP = 50;

        [Toggle("Can't use first aid kit underwater", Tooltip = "")]
        public bool cantUseMedkitUnderwater = false;

        [Toggle("Always use best LOD models", Tooltip = "A lot of models in the game use different levels of detail depending on how close you are to them. Some of them look different and you can see those objects change as you approach them. With this on best LOD models will always be used. It will affect the game's performance, but with a good GPU it should not be noticable. The game has to be reloaded after changing this.")]
        public bool useBestLOD = false;

        [Slider("Drop pod max power", 0, 100, DefaultValue = 0, Step = 5, Format = "{0:F0}", Tooltip = "If this is not 0 your drop pod's max power will be set to this. Drop pod's power will regenerate during the day. The game has to be reloaded after changing this.")]
        public int dropPodMaxPower = 0;

        [Slider("Fruit growth time", 0, 50, DefaultValue = 0, Step = 1, Format = "{0:F0}", Tooltip = "Time in days it takes a lantern tree fruit, a frost anemone heart, a creepvine seed cluster, a Preston's plant fruit to grow. If this is 0 then vanilla code will run. You have to reload your game after changing this.")]
        public int fruitGrowTime = 0;

        [Toggle("Do not spawn fragments for unlocked blueprints", Tooltip = "You have to reload your game after changing this.")]
        public bool dontSpawnKnownFragments = false;

        [Slider("Crush depth", 50, 500, DefaultValue = 200, Step = 10, Format = "{0:F0}", Tooltip = "Depth in meters below which player starts taking crush damage. Does not work if crush damage multiplier is 0.")]
        public int crushDepth = 200;

        [Slider("Crush damage multiplier", 0f, 1f, DefaultValue = 0f, Step = .01f, Format = "{0:0.#}", Tooltip = "When it's not 0 every 3 seconds player takes 1 damage multiplied by this for every meter below crush depth.")]
        public float crushDamageMult = 0f;

        [Slider("Vehicle crush damage multiplier", 0f, 1f, DefaultValue = 0f, Step = .01f, Format = "{0:0.#}", Tooltip = "When it's not 0, every 3 seconds vehicles take 1 damage multiplied by this for every meter below crush depth.")]
        public float vehicleCrushDamageMult = 0f;

        //[Toggle("Instantly open PDA", Tooltip = "Your PDA will open and close instantly. Direction you are looking at will not change when you open it. Game has to be reloaded after changing this.")]
        public bool instantPDA = false;

        [Slider("Hunger update interval", 1, 100, DefaultValue = 10, Step = 1, Format = "{0:F0}", Tooltip = "Time in seconds it takes your hunger and thirst to update.")]
        public int hungerUpdateInterval = 10;

        [Toggle("New hunger system", Tooltip = "You don't regenerate health when you are full. When you sprint you get hungry and thirsty twice as fast. You don't lose health when your food or water value is 0. Your food and water values can go as low as -100. When your food or water value is below 0 your movement speed will be reduced proportionally to that value. When either your food or water value is -100 your movement speed will be reduced by 50% and you will start taking hunger damage. Your max food and max water value is 200. The higher your food value above 100 is the less food you get when eating: when your food value is 110 you lose 10% of food, when it's 190 you lose 90%.")]
        public bool newHungerSystem = false;

        [Choice("Eating raw fish", Tooltip = "When it's not vanilla, amount of food you get by eating raw fish changes. Harmless: it's a random number between 0 and fish's food value. Risky: it's a random number between fish's food negative value and fish's food value. Harmful: it's a random number between fish's food negative value and 0.")]
        public EatingRawFish eatRawFish;

        [Slider("Fish water/food value ratio", 0f, 1f, DefaultValue = 0f, Step = .01f, Format = "{0:0.#}", Tooltip = "If this is more than 0 then fish's water value will be proportional to its food value. If this is 0.1 then water value will be 10% of food value. If this is 0.9 then water value will be 90% of food value. Game has to be reloaded after changing this.")]
        public float fishFoodWaterRatio = 0f;

        [Toggle("Can't eat underwater", Tooltip = "You won't be able to eat or drink underwater.")]
        public bool cantEatUnderwater = false;

        [Slider("Food decay rate multiplier", 0f, 2f, DefaultValue = 1f, Step = .01f, Format = "{0:0.#}", Tooltip = "Food decay rate will be multiplied by this. The game has to be reloaded after changing this.")]
        public float foodDecayRateMult = 1f;

        [Slider("Water freeze rate multiplier", 0f, 5f, DefaultValue = 1f, Step = .1f, Format = "{0:0.#}", Tooltip = "Bottled water will freeze at this rate if ambient temperature is below 0 C°. The game has to be reloaded after changing this.")]
        public float waterFreezeRate = 1f;

        [Slider("Snowball water value", 0, 30, DefaultValue = 0, Step = 1, Format = "{0:F0}", Tooltip = "When you eat a snowball, you will get this amount of water and lose this amount of warmth. The game has to be reloaded after changing this.")]
        public int snowballWater = 0;

        [Slider("Catchable fish speed multiplier", .1f, 5f, DefaultValue = 1f, Step = .1f, Format = "{0:0.#}", Tooltip = "Swimming speed of fish that you can catch will be multiplied by this.")]
        public float fishSpeedMult = 1f;

        [Slider("Other creatures speed multiplier", .1f, 5f, DefaultValue = 1f, Step = .1f, Format = "{0:0.#}", Tooltip = "Swimming speed of creatures that you can't catch will be multiplied by this.")]
        public float creatureSpeedMult = 1f;

        [Slider("Creature flee chance percent", 0, 100, DefaultValue = 100, Step = 1, Format = "{0:F0}", Tooltip = "Creature flee chance percent when it's under attack and its flee damage threshold is reached.")]
        public int CreatureFleeChance = 100;

        [Toggle("Damage threshold for fleeing creatures", Tooltip = "Most creatures have damage threshold that has to be reached before they start fleeing. If this is false every creature will flee if it takes any damage.")]
        public bool CreatureFleeUseDamageThreshold = true;

        [Toggle("Creature's flee chance depends on its health", Tooltip = "Only creatures's health will be used to decide if it should flee when under attack. Creature with 90% health has 10% chance to flee. Creature with 10% health has 90% chance to flee. This setting overrides both 'Creature flee chance percent' and 'Damage threshold for fleeing creatures'.")]
        public bool CreatureFleeChanceBasedOnHealth = false;

        //[Slider("Egg hatching period multiplier", .1f, 10f, DefaultValue = 1f, Step = .1f, Format = "{0:0.#}", Tooltip = "Time it takes a creature egg to hatch in AC will be multiplied by this.")]
        //public float eggHatchTimeMult = 1f;
        //[Slider("Plant growth time multiplier", .1f, 10f, DefaultValue = 1f, Step = .1f, Format = "{0:0.#}", Tooltip = "Time it takes a plant to grow will be multiplied by this.")]
        //public float plantGrowthTimeMult = 1f;
        [Toggle("Creatures im alien containment can breed", Tooltip = "")]
        public bool waterparkCreaturesBreed = true;

        [Slider("Inventory weight multiplier in water", 0f, 1f, DefaultValue = 0f, Step = .001f, Format = "{0:0.#}", Tooltip = "When it's not 0 and you are swimming you lose 1% of your max speed for every kilo of mass in your inventory multiplied by this.")]
        public float invMultWater = 0f;

        [Slider("Inventory weight multiplier on land", 0f, 1f, DefaultValue = 0f, Step = .001f, Format = "{0:0.#}", Tooltip = "When it's not 0 and you are on land you lose 1% of your max speed for every kilo of mass in your inventory multiplied by this.")]
        public float invMultLand = 0f;

        [Toggle("Can't catch fish with bare hands", Tooltip = "To catch fish you will have to use propulsion cannon or grav trap. Does not apply if you are inside alien containment.")]
        public bool noFishCatching = false;

        [Toggle("Can't break outcrop with bare hands", Tooltip = "You will have to use a knife to break outcrops or collect resources attached to rock or seabed. A piece of scrap metal will spawn next to your crashed ship, so you can craft knife.")]
        public bool noBreakingWithHand = false;

        [Toggle("Disable tutorial messages", Tooltip = "Disable messages that tell you how controls work.")]
        public bool disableHints = false;

        [Toggle("Realistic oxygen consumption", Tooltip = "Vanilla oxygen consumption without rebreather has 3 levels: depth below 200 meters, depth between 200 and 100 meters, depth between 100 and 0 meters. With this on your oxygen consumption will increase in linear progression using 'Crush depth' setting. When you are at crush depth it will be vanilla max oxygen consumption and will increase as you dive deeper.")]
        public bool realOxygenCons = false;

        [Toggle("Player impact damage screen effects", Tooltip = "This toggles cracks on your swimming mask when you take damage.")]
        public bool damageImpactEffect = true;

        [Toggle("Player damage screen effects", Tooltip = "This toggles red screen effects when you take damage.")]
        public bool damageScreenFX = true;

        [Toggle("Drop held tool when taking damage", Tooltip = "Chance to drop your tool is equal to amount of damage taken. If you take 30 damage, there is 30% chance you will drop your tool.")]
        public bool dropHeldTool = false;

        [Slider("Tool power consumption multiplier", 0f, 3f, DefaultValue = 1f, Step = .1f, Format = "{0:0.#}", Tooltip = "Amount of power consumed by your tools will be multiplied by this.")]
        public float toolEnergyConsMult = 1f;

        [Slider("Vehicle power consumption multiplier", 0f, 4f, DefaultValue = 1f, Step = .1f, Format = "{0:0.#}", Tooltip = "Amount of power consumed by your vehicles will be multiplied by this.")]
        public float vehicleEnergyConsMult = 1f;

        [Slider("Base power consumption multiplier", 0f, 4f, DefaultValue = 1f, Step = .1f, Format = "{0:0.#}", Tooltip = "Amount of power consumed by things in your base will be multiplied by this.")]
        public float baseEnergyConsMult = 1f;

        [Slider("Battery charge multiplier", 0.5f, 2f, DefaultValue = 1f, Step = .1f, Format = "{0:0.#}", Tooltip = "Max charge of batteries and power cells will be multiplied by this. Game has to be reloaded after changing this.")]
        public float batteryChargeMult = 1f;

        [Slider("Crafted battery charge percent", 0, 100, DefaultValue = 100, Step = 1, Format = "{0:F0}", Tooltip = "Charge percent of batteries and power cells you craft will be set to this.")]
        public int craftedBatteryCharge = 100;

        [Slider("Free torpedos", 0, 6, DefaultValue = 2, Step = 1, Format = "{0:F0}", Tooltip = "Number of torpedos you get when installing new Prawn Suit Torpedo Arm. After changing this you have to craft a new Torpedo Arm.")]
        public int freeTorpedos = 2;

        [Choice("Losing items when you die", Tooltip = "When set to 'All' you will drop every item in your inventory when you die.")]
        public LoseItemsOnDeath loseItemsOnDeath;

        //[Toggle("Can't repair vehicles in water", Tooltip = "")]
        //public bool cantRepairVehicleInWater = false;

        [Toggle("Camera bobbing", Tooltip = "Camera bobbing when swimming."), OnChange(nameof(ToggleCameraBobbing))]
        public bool cameraBobbing = true;

        [Keybind("Quickslot cycle key", Tooltip = "Press 'Cycle next' or 'Cycle previous' key while holding down this key to cycle tools in your current quickslot.")]
        public KeyCode quickslotKey = KeyCode.LeftAlt;
        [Keybind("Move all items key", Tooltip = "When you have a container open, hold down this key and click an item to move all items.")]
        public KeyCode transferAllItemsKey = KeyCode.LeftControl;
        [Keybind("Move the same items key", Tooltip = "When you have a container open, hold down this key and click an item to move all items of the same type.")]
        public KeyCode transferSameItemsKey = KeyCode.LeftShift;

        public int activeSlot = -1;
        public float medKitHPtoHeal = 0f;

        public Dictionary<string, Dictionary<string, bool>> baseLights = new Dictionary<string, Dictionary<string, bool>>();
        public Dictionary<string, Dictionary<string, Storage_Patch.SavedLabel>> lockerNames = new Dictionary<string, Dictionary<string, Storage_Patch.SavedLabel>>();

        //public HashSet<string> nonRechargeable = new HashSet<string>{
        //    { "someBattery" },
        //};
        public Dictionary<string, float> podPower = new Dictionary<string, float>();

        public HashSet<TechType> predatorExclusion = new HashSet<TechType> { TechType.Crash};
        public Dictionary<string, bool> iceFruitPickedState = new Dictionary<string, bool> ();

        static void UpdateTimeFlowSpeed()
        {
            if (DayNightCycle.main)
                DayNightCycle.main._dayNightSpeed = Main.config.timeFlowMult;
        }

        static void UpdateBaseLight()
        {
            //if (Main.loadingDone)
            //{
            //    Base_Light.UpdateBaseLights();
            //}
        }

        static void ToggleCameraBobbing()
        {
            MiscSettings.cameraBobbing = !MiscSettings.cameraBobbing;
        }

        public enum EatingRawFish { Vanilla, Harmless, Risky, Harmful }
        public enum LoseItemsOnDeath { Vanilla, All, None }
        public enum EmptyVehicleCanBeAttacked { Yes, No, Only_if_lights_on }
        public bool seaglideMap = true;
        public bool seaglideLight = true;
        public bool exosuitLights = false;
        public Screen_Resolution_Fix.ScreenRes screenRes;
        //private void EatRawFishChangedEvent(ChoiceChangedEventArgs e)
        //{
        //    AddDebug("EatRawFishChangedEvent " + eatRawFish); 
        //}

    }

}