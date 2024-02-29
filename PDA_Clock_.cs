
using HarmonyLib;
using UnityEngine;
using TMPro;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal static class PDA_Clock_
    {
        public static GameObject PDA_ClockGO { get; set; }

        public class PDA_Clock : MonoBehaviour
        {
            private TextMeshProUGUI textComponent;
            private const float oneHour = 0.0416666679084301f;
            //public static GameObject TimeLabelObject { private get; set; }
            bool blink = false;

            private void Awake()
            {
                textComponent = GetComponent<TextMeshProUGUI>();
                textComponent.fontSize = 50;
                textComponent.color = Color.white;
            }

            private void Start()
            {
                //transform.GetChild(0).gameObject.SetActive(false);
                Destroy(transform.GetChild(0).gameObject);
                InvokeRepeating("ApplyTimeToText", 0f, 1f);
            }

            //private void Update() => ApplyTimeToText();

            private void ApplyTimeToText()
            {
                if (!gameObject.activeInHierarchy)
                    return;
                //AddDebug("ApplyTimeToText " + gameObject.activeSelf + " " + gameObject.activeInHierarchy);
                float dayScalar = DayNightCycle.main.GetDayScalar();
                int minutes = Mathf.FloorToInt((dayScalar % oneHour / oneHour * 60f));
                int hours = Mathf.FloorToInt(dayScalar * 24f);
                //string str1 = "";
                //string str2 = dayScalar < 0.75 ? (dayScalar < 0.5 ? (dayScalar < 0.25 ? "Midnight" : "Morning") : "Noon") : "Evening";
                //this.textComponent.text = num2.ToString("00") + ":" + num1.ToString("00") + " " + str1 + " (" + str2 + ")";
                var sb = new System.Text.StringBuilder();
                if (GameModeManager.GetOption<bool>(GameOption.BodyTemperatureDecreases))
                {
                    int temp = Mathf.RoundToInt(Util.GetPlayerTemperature());
                    sb.Append(temp.ToString());
                    sb.AppendLine("°C");
                }
                sb.Append(hours.ToString("00"));
                if (blink)
                    sb.Append(":");
                else
                    sb.Append(" ");

                sb.AppendLine(minutes.ToString("00"));
                textComponent.text = sb.ToString();
                //textComponent.text = blink ? hours.ToString("00") + ":" + minutes.ToString("00") : hours.ToString("00") + " " + minutes.ToString("00");
                blink = !blink;
            }
        }

        [HarmonyPatch(typeof(uGUI_InventoryTab), "OnOpenPDA")]
        internal static class uGUI_InventoryTab_OnOpenPDA_Patch
        {
            private static void Prefix()
            {
                if (!Main.config.pdaClock)
                    return;
                PDA_ClockGO.SetActive(false);
            }
        }

        [HarmonyPatch(typeof(uGUI_Equipment), "Init")]
        internal static class uGUI_Equipment_Init_Patch
        {
            private static void Postfix(uGUI_Equipment __instance, Equipment equipment)
            {
                if (!Main.config.pdaClock)
                    return;

                if (equipment.GetCompatibleSlot(EquipmentType.Body, out string str))
                    PDA_ClockGO.SetActive(true);
                else
                    PDA_ClockGO.SetActive(false);
            }
        }

        [HarmonyPatch(typeof(uGUI_InventoryTab), "Awake")]
        internal static class uGUI_InventoryTab_Awake_Patch
        {
            private static void Postfix(uGUI_InventoryTab __instance)
            {
                if (!Main.config.pdaClock)
                    return;
                //GameObject label = CreateGameObject(__instance, "TimeLabel", -280f);
                //PDA_Clock.TimeLabelObject = label;
                //label.GetComponent<Text>().text = "TIME";
                PDA_ClockGO = Object.Instantiate(__instance.storageLabel.gameObject, __instance.gameObject.transform);
                PDA_ClockGO.name = "TimeDisplayText";
                Vector3 localPosition = PDA_ClockGO.transform.localPosition;
                PDA_ClockGO.transform.localPosition = new Vector3(localPosition.x, -350f, localPosition.z);
                PDA_ClockGO.AddComponent<PDA_Clock>();
            }


        }
    }
}