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
                var rend = versionText.GetComponent<MeshRenderer>();
                Bounds worldBounds = rend != null ? rend.bounds : new Bounds(versionText.transform.position, Vector3.zero);
                float gap = (worldBounds.size.y > 0f ? worldBounds.size.y : 0.3f) * 0.25f;
                float rightShift = (worldBounds.size.y > 0f ? worldBounds.size.y : 0.3f) * 0.23f;

                var go = new GameObject("SheriffModVersion");
                go.transform.SetParent(versionText.transform, true);
                go.transform.localScale = Vector3.one;
                go.transform.localRotation = Quaternion.identity;
                go.transform.position = new Vector3(versionText.transform.position.x + rightShift, worldBounds.min.y - gap, versionText.transform.position.z);

                var tmp = go.AddComponent<TextMeshPro>();
                tmp.font = versionText.font;
                tmp.fontSharedMaterial = versionText.fontSharedMaterial;
                tmp.text = $"loaded SheriffMod v{SheriffPlugin.Version}";
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
