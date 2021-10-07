using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    public static class Base_Light
    {
        public static HashSet<BaseCellLighting> bcls = new HashSet<BaseCellLighting>();

        public static void UpdateBaseLights()
        {
            foreach (BaseCellLighting bcl in bcls)
            {
                bcl.ApplyCurrentIntensity();
            }
        }

        [HarmonyPatch(typeof(BaseCellLighting), "Start")]
        public class BaseCellLighting_Start_Patch : MonoBehaviour
        {
            static void Postfix(BaseCellLighting __instance)
            {
                bcls.Add(__instance);
            }
        }

        [HarmonyPatch(typeof(BaseCellLighting), "ApplyCurrentIntensity")]
        public class BaseCellLighting_ApplyCurrentIntensity_Patch : MonoBehaviour
        {
            static bool Prefix(BaseCellLighting __instance)
            {
                if (__instance.block == null)
                    __instance.block = new MaterialPropertyBlock();
                float newIntensity = Mathf.Max(__instance.GetPowerLossValue(), Main.config.baseLightOff ? 1 : 0);
                if (__instance.appliedIntensity == newIntensity && !__instance.geometryChanged)
                    return false;

                //AddDebug("ApplyCurrentIntensity " + newIntensity);
                __instance.appliedIntensity = newIntensity;
                __instance.interiorSky.MasterIntensity = __instance.interiorMasterIntensity.Lerp(1f - newIntensity);
                __instance.interiorSky.DiffIntensity = __instance.interiorDiffuseIntensity.Lerp(1f - newIntensity);
                __instance.interiorSky.SpecIntensity = __instance.interiorSpecIntensity.Lerp(1f - newIntensity);
                __instance.glassSky.MasterIntensity = __instance.glassMasterIntensity.Lerp(1f - newIntensity);
                __instance.glassSky.DiffIntensity = __instance.glassDiffuseIntensity.Lerp(1f - newIntensity);
                __instance.glassSky.SpecIntensity = __instance.glassSpecIntensity.Lerp(1f - newIntensity);
                foreach (Renderer renderer in __instance.interior)
                {
                    if (!(renderer == null))
                    {
                        __instance.block.Clear();
                        renderer.GetPropertyBlock(__instance.block);
                        __instance.block.SetFloat(ShaderPropertyID._UwePowerLoss, newIntensity);
                        __instance.interiorSky.ApplyToBlock(ref __instance.block, 0);
                        renderer.SetPropertyBlock(__instance.block);
                    }
                }
                foreach (Renderer renderer in __instance.glass)
                {
                    __instance.block.Clear();
                    renderer.GetPropertyBlock(__instance.block);
                    __instance.block.SetFloat(ShaderPropertyID._UwePowerLoss, newIntensity);
                    __instance.glassSky.ApplyToBlock(ref __instance.block, 0);
                    renderer.SetPropertyBlock(__instance.block);
                }
                __instance.geometryChanged = false;
                return false;
            }
        }

    }
}
