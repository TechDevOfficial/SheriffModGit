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

                var go = new GameObject("SheriffModVersion");
                go.transform.SetParent(versionText.transform, false);
                go.transform.localPosition = new Vector3(0f, versionText.fontSize * 0.1f, 0f);
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;

                var tmp = go.AddComponent<TextMeshPro>();
                tmp.font = versionText.font;
                tmp.fontSharedMaterial = versionText.fontSharedMaterial;
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
                if (__instance != null && __instance.text != null)
                {
                    var t = __instance.text;
                    if (!t.Text.EndsWith("\nmod by Manu"))
                        t.Text += "\nmod by Manu";
                }
            }
            catch (Exception e)
            {
                SheriffPlugin.Log.LogError("PingTracker patch: " + e);
            }

            try
            {
                if (HudManager.InstanceExists)
                {
                    var tmp = HudManager.Instance.GameSettingsTMP;
                    if (tmp != null && !string.IsNullOrEmpty(tmp.text) && !tmp.text.Contains("Sheriff Mod"))
                        tmp.text += "\n<color=#FFA600>< Sheriff Mod ></color>";
                }
            }
            catch (Exception e)
            {
                SheriffPlugin.Log.LogError("HudManager GameSettings patch: " + e);
            }
        }
    }
}
