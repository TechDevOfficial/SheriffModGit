using System;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace ClassicUs.SheriffMod
{
    [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
    internal static class VersionShower_Start_Patch
    {
        private static void Postfix(VersionShower __instance)
        {
            try
            {
                if (__instance == null || __instance.text == null) return;

                if (__instance.text.transform.Find("SheriffModVersion") != null) return;

                var go = new GameObject("SheriffModVersion");
                go.transform.SetParent(__instance.text.transform, false);
                go.transform.localPosition = new Vector3(0f, -0.18f, 0f);
                go.transform.localScale = Vector3.one;

                var tmp = go.AddComponent<TextMeshPro>();
                tmp.text = $"Loaded SheriffMod v{SheriffPlugin.Version}";
                tmp.fontSize = __instance.text.fontSize;
                tmp.color = new Color(1f, 0.65f, 0f, 1f);
                tmp.alignment = TextAlignmentOptions.Left;
                tmp.enableWordWrapping = false;
            }
            catch (Exception e)
            {
                SheriffPlugin.Log.LogError("VersionShower patch: " + e);
            }
        }
    }


    [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
    internal static class PingTracker_Update_Patch
    {
        private static void Postfix(PingTracker __instance)
        {
            try
            {
                if (__instance == null || __instance.text == null) return;
                var t = __instance.text;
                if (!t.Text.EndsWith("\nmod by Manu"))
                    t.Text += "\nmod by Manu";
            }
            catch (Exception e)
            {
                SheriffPlugin.Log.LogError("PingTracker patch: " + e);
            }
        }
    }
}
