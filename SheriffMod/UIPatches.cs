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

                var parent = versionText.transform.parent != null ? versionText.transform.parent : versionText.transform;
                if (parent.Find("SheriffModVersion") != null) return;

                versionText.ForceMeshUpdate(false, false);
                var rend = versionText.GetComponent<MeshRenderer>();
                Bounds worldBounds = rend != null ? rend.bounds : new Bounds(versionText.transform.position, Vector3.zero);
                float gap = worldBounds.size.y > 0f ? worldBounds.size.y * 0.25f : 0.05f;

                var go = new GameObject("SheriffModVersion");
                go.transform.SetParent(parent, true);
                go.transform.localScale = versionText.transform.localScale;
                go.transform.position = new Vector3(versionText.transform.position.x, worldBounds.min.y - gap, versionText.transform.position.z);

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
