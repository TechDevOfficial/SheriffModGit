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
                if (__instance == null || versionText == null)
                {
                    SheriffPlugin.Log.LogInfo("[VersionShower] instance or text is null, aborting");
                    return;
                }

                var parent = versionText.transform.parent != null ? versionText.transform.parent : versionText.transform;
                if (parent.Find("SheriffModVersion") != null)
                {
                    SheriffPlugin.Log.LogInfo("[VersionShower] label already exists, skipping");
                    return;
                }

                versionText.ForceMeshUpdate(false, false);
                var rend = versionText.GetComponent<MeshRenderer>();
                Bounds worldBounds = rend != null ? rend.bounds : new Bounds(versionText.transform.position, Vector3.zero);
                float lineHeight = worldBounds.size.y > 0f ? worldBounds.size.y : 0.2f;
                float gap = lineHeight * 0.25f;

                SheriffPlugin.Log.LogInfo($"[VersionShower] versionText.text='{versionText.text}' rend={(rend != null)} bounds={worldBounds} font={(versionText.font != null)} mat={(versionText.fontSharedMaterial != null)} parent={parent.name} scale={versionText.transform.localScale} layer={versionText.gameObject.layer}");

                var go = UnityEngine.Object.Instantiate(versionText.gameObject, parent);
                go.name = "SheriffModVersion";
                go.transform.position = new Vector3(worldBounds.min.x, worldBounds.min.y - gap, versionText.transform.position.z);

                foreach (var comp in go.GetComponents<MonoBehaviour>())
                {
                    if (comp == null || comp is TextMeshPro) continue;
                    UnityEngine.Object.Destroy(comp);
                }

                var tmp = go.GetComponent<TextMeshPro>();
                tmp.text = $"Loaded SheriffMod v{SheriffPlugin.Version}";
                tmp.color = new Color(1f, 0.65f, 0f, 1f);
                tmp.alignment = TextAlignmentOptions.TopLeft;
                tmp.ForceMeshUpdate(true, true);

                SheriffPlugin.Log.LogInfo($"[VersionShower] label created at {go.transform.position}, active={go.activeInHierarchy}, rendererEnabled={(tmp.renderer != null ? tmp.renderer.enabled.ToString() : "no renderer")}");
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
