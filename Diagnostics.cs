using System;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;

namespace ClassicUs.SheriffMod
{
    [HarmonyPatch(typeof(ClassInjector), "RewriteType")]
    internal static class RewriteType_Fix
    {
        private static bool Prefix(Type type, ref Type __result)
        {
            if (type != null && type.IsEnum)
            {
                __result = type;
                return false;
            }
            return true;
        }

        private static void Finalizer(Exception __exception, Type type)
        {
            if (__exception != null)
                SheriffPlugin.Log.LogWarning(
                    $"[diag] RewriteType throw su type='{type?.FullName}' asm='{type?.Assembly?.GetName()?.Name}'");
        }
    }
}
