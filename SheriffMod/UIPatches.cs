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
                var versionText = __instance.text;
                if (__instance == null || versionText == null) return;

                if (versionText.transform.Find("SheriffModVersion") != null) return;

                versionText.ForceMeshUpdate(false, false);
                float lineHeight = versionText.textBounds.size.y;
                if (lineHeight <= 0f) lineHeight = versionText.fontSize * 0.01f;

                var go = new GameObject("SheriffModVersion");
                go.transform.SetParent(versionText.transform, false);
                go.transform.localPosition = new Vector3(0f, -lineHeight * 1.1f, 0f);
                go.transform.localScale = Vector3.one;

                var tmp = go.AddComponent<TextMeshPro>();
                tmp.text = $"Loaded SheriffMod v{SheriffPlugin.Version}";
                tmp.fontSize = versionText.fontSize;
                tmp.color = new Color(1f, 0.65f, 0f, 1f);
                tmp.alignment = versionText.alignment;
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
