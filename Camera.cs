using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Tweaks_Fixes
{
    internal class Camera_
    {
        [HarmonyPatch(typeof(MainCameraControl), "ShakeCamera")]
        class DamageSystem_CalculateDamage_Patch
        {
            public static void Postfix(MainCameraControl __instance)
            {
                if (ConfigToEdit.cameraShake.Value)
                    return;

                __instance.camShake = 0;
            }
        }


        [HarmonyPatch(typeof(DamageFX), "AddHudDamage")]
        class DamageFX_AddHudDamage_Patch
        {
            public static bool Prefix(DamageFX __instance, float damageScalar, Vector3 damageSource, DamageInfo damageInfo, bool isUnderwater)
            {
                //AddDebug("AddHudDamage " + damageInfo.type);
                if (!ConfigToEdit.crushDamageScreenEffect.Value && damageInfo.type == DamageType.Pressure)
                    return false;

                if (ConfigMenu.damageImpactEffect.Value)
                    __instance.CreateImpactEffect(damageScalar, damageSource, damageInfo.type, isUnderwater);

                if (ConfigMenu.damageScreenFX.Value)
                    __instance.PlayScreenFX(damageInfo);

                return false;
            }
        }




    }
}
