using System;
using System.Collections.Generic;
using HarmonyLib;
using Hazel;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ClassicUs.SheriffMod
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    internal static class PlayerControl_FixedUpdate_RoleRegistration_Patch
    {
        private static void Prefix(PlayerControl __instance)
        {
            if (__instance != PlayerControl.LocalPlayer) return;
            RoleRegistration.EnsureSheriffRegistered(RoleManager.Instance);
        }
    }

    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.Start))]
    internal static class RoleManager_Start_Patch
    {
        private static void Prefix(RoleManager __instance)
        {
            RoleRegistration.EnsureSheriffRegistered(__instance);
        }

        private static void Postfix(RoleManager __instance)
        {
            RoleRegistration.EnsureSheriffRegistered(__instance);
        }
    }

    internal static class RoleRegistration
    {
        public static void EnsureSheriffRegistered(RoleManager rm)
        {
            if (rm == null) return;
            try
            {
                if (rm.allRoles != null)
                {
                    foreach (var r in rm.allRoles)
                        if (r != null && r.TryCast<SheriffRole>() != null) return;
                }

                rm.AddRole(Il2CppType.Of<SheriffRole>(), SheriffPlugin.RoleModName);

                if (rm.allRoles == null) return;

                foreach (var role in rm.allRoles)
                {
                    if (role != null && role.TryCast<SheriffRole>() != null)
                    {
                        SheriffPlugin.SheriffRoleName = role.roleCodeName;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                SheriffPlugin.Log.LogError("Failed to register Sheriff role: " + e);
            }
        }
    }

    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.OnAssign))]
    internal static class RoleBehaviour_OnAssign_Patch
    {
        private static void Postfix(RoleBehaviour __instance, PlayerControl player)
        {
            if (__instance == null || __instance.TryCast<SheriffRole>() == null) return;
            try
            {
                __instance.RoleTeamType = RoleTeamTypes.Crewmate;
                __instance.CanUseKillButton = true;
                __instance.CanVent = false;
                __instance.CanSabotage = false;

                var enemies = new Il2CppStructArray<RoleTeamTypes>(3);
                enemies[0] = RoleTeamTypes.Crewmate;
                enemies[1] = RoleTeamTypes.Impostor;
                enemies[2] = RoleTeamTypes.Neutral;
                __instance.enemyTeams = enemies;
            }
            catch (Exception e)
            {
                SheriffPlugin.Log.LogError("Config Sheriff OnAssign: " + e);
            }
        }
    }

    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.roleDisplayName), MethodType.Getter)]
    internal static class RoleBehaviour_DisplayName_Patch
    {
        private static void Postfix(RoleBehaviour __instance, ref string __result)
        {
            if (__instance != null && __instance.TryCast<SheriffRole>() != null)
                __result = "Sheriff";
        }
    }

    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.roleDescription), MethodType.Getter)]
    internal static class RoleBehaviour_Description_Patch
    {
        private static void Postfix(RoleBehaviour __instance, ref string __result)
        {
            if (__instance != null && __instance.TryCast<SheriffRole>() != null)
                __result = "You are a Sheriff. Kill the Impostor with your kill button.\nIf you kill an innocent crewmate, you will die.";
        }
    }

    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.roleDescriptionShort), MethodType.Getter)]
    internal static class RoleBehaviour_DescriptionShort_Patch
    {
        private static void Postfix(RoleBehaviour __instance, ref string __result)
        {
            if (__instance != null && __instance.TryCast<SheriffRole>() != null)
                __result = "Find and kill the Impostor";
        }
    }

    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.KillCooldown), MethodType.Getter)]
    internal static class RoleBehaviour_KillCooldown_Patch
    {
        private static void Postfix(RoleBehaviour __instance, ref float __result)
        {
            if (__instance != null && __instance.TryCast<SheriffRole>() != null)
                __result = SheriffPlugin.ActiveCooldown;
        }
    }

    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.TeamColor), MethodType.Getter)]
    internal static class RoleBehaviour_TeamColor_Patch
    {
        private static void Postfix(RoleBehaviour __instance, ref Color __result)
        {
            if (__instance != null && __instance.TryCast<SheriffRole>() != null)
                __result = new Color(1f, 0.65f, 0f, 1f);
        }
    }

    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.GetTeamColor))]
    internal static class IntroCutscene_GetTeamColor_Patch
    {
        private static void Postfix(RoleBehaviour role, ref Color __result)
        {
            if (role != null && role.TryCast<SheriffRole>() != null)
                __result = new Color(1f, 0.65f, 0f, 1f);
        }
    }

    [HarmonyPatch(typeof(IntroCutscene._BeginTeam_d__18), nameof(IntroCutscene._BeginTeam_d__18.MoveNext))]
    internal static class IntroCutscene_BeginTeam_MoveNext_Patch
    {
        private static void Postfix(IntroCutscene._BeginTeam_d__18 __instance, ref bool __result)
        {
            if (!__result || __instance == null || __instance.__4__this == null) return;

            var local = PlayerControl.LocalPlayer;
            if (local != null && SheriffPlugin.IsSheriff(local))
            {
                __instance.__4__this.Title.text = "Sheriff";
                __instance.__4__this.Title.color = new Color(1f, 0.65f, 0f, 1f);
                __instance.__4__this.DescriptionText.text = "You are a Sheriff. Kill the Impostor with your kill button.\nIf you kill an innocent crewmate, you will die.";
            }
        }
    }

    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.AssignRolesForTeam))]
    internal static class RoleManager_AssignRolesForTeam_Patch
    {
        private static void Prefix(RoleManager __instance, RoleTeamTypes type, int max)
        {
            RoleRegistration.EnsureSheriffRegistered(__instance);

            var client = AmongUsClient.Instance;
            if (client == null || !client.AmHost) return;

            SheriffPlugin.HostBroadcastSettings();
        }

        private static void Postfix(RoleManager __instance, RoleTeamTypes type, int max)
        {
            var client = AmongUsClient.Instance;
            if (client == null || !client.AmHost) return;

            if (type == RoleTeamTypes.Crewmate)
            {
                if (!SheriffPlugin.ActiveEnabled || SheriffPlugin.ActiveCount <= 0) return;

                try
                {
                    AssignSheriffs();
                }
                catch (Exception e)
                {
                    SheriffPlugin.Log.LogError("Failed to assign Sheriffs: " + e);
                }
            }
        }

        private static void AssignSheriffs()
        {
            var rm = RoleManager.Instance;
            if (rm == null)
            {
                SheriffPlugin.Log.LogError("[AssignSheriffs] RoleManager.Instance is null");
                return;
            }

            var candidates = new List<PlayerControl>();
            foreach (var p in PlayerControl.AllPlayerControls)
            {
                if (p == null || p.Data == null || p.Data.Disconnected || p.Data.IsDead) continue;
                var role = p.Data.myRole;
                if (role == null) continue;
                if (role.RoleTeamType != RoleTeamTypes.Crewmate) continue;
                if (SheriffPlugin.IsImpostor(p)) continue;
                candidates.Add(p);
            }

            var rng = new System.Random();
            for (int i = candidates.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
            }

            int toAssign = Math.Min(SheriffPlugin.ActiveCount, candidates.Count);
            SheriffPlugin.Log.LogInfo($"[AssignSheriffs] Assigning {toAssign} Sheriff(s) from {candidates.Count} candidates");
            for (int i = 0; i < toAssign; i++)
            {
                var p = candidates[i];
                rm.AssignRole(p, SheriffPlugin.SheriffRoleName);
                SheriffPlugin.Log.LogInfo($"[AssignSheriffs] Sheriff assigned to playerId={p.Data.PlayerId}");
            }
        }
    }

    [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
    internal static class ExileController_Begin_Patch
    {
        private static void Postfix(ExileController __instance, GameData.PlayerInfo exiled, bool tie)
        {
            if (__instance == null || exiled == null) return;
            try
            {
                var role = exiled.myRole;
                if (role == null || role.TryCast<SheriffRole>() == null) return;

                string text = $"{exiled.PlayerName} was the Sheriff.";
                if (__instance.Text != null) __instance.Text.Text = text;
                __instance.completeString = text;
            }
            catch (Exception e)
            {
                SheriffPlugin.Log.LogError("ExileController.Begin Sheriff text patch: " + e);
            }
        }
    }

    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin))]
    internal static class IntroCutscene_CoBegin_Patch
    {
        private static void Prefix()
        {
            try
            {
                SheriffPlugin.Log.LogInfo($"[Diag] IntroCutscene.CoBegin. AmHost={AmongUsClient.Instance?.AmHost} ActiveEnabled={SheriffPlugin.ActiveEnabled} ActiveCount={SheriffPlugin.ActiveCount}");
                foreach (var p in PlayerControl.AllPlayerControls)
                {
                    if (p == null || p.Data == null) continue;
                    var roleName = p.Data.myRole != null ? p.Data.myRole.roleCodeName : "null";
                    var roleTeam = p.Data.myRole != null ? p.Data.myRole.RoleTeamType.ToString() : "null";
                    SheriffPlugin.Log.LogInfo($"[Diag] Player ID={p.Data.PlayerId} Name={p.Data.PlayerName} Role={roleName} Team={roleTeam}");
                }
            }
            catch (Exception e)
            {
                SheriffPlugin.Log.LogError("[Diag] IntroCutscene.CoBegin log error: " + e);
            }
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
    internal static class AmongUsClient_OnPlayerJoined_Patch
    {
        private static void Postfix(AmongUsClient __instance)
        {
            if (__instance == null || !__instance.AmHost) return;

            SheriffPlugin.HostBroadcastSettings();
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckMurder))]
    internal static class PlayerControl_CheckMurder_Patch
    {
        private static bool Prefix(PlayerControl __instance, PlayerControl t)
        {
            if (!SheriffPlugin.IsSheriff(__instance)) return true;

            var client = AmongUsClient.Instance;
            if (client == null || !client.AmHost) return true;

            try
            {
                if (t == null || t.Data == null || t.Data.IsDead) return false;

                if (SheriffPlugin.IsImpostor(t))
                    __instance.RpcMurderPlayer(t, MurderResultFlags.Succeeded);
                else
                    __instance.RpcMurderPlayer(__instance, MurderResultFlags.Succeeded);

                var role = __instance.Data.myRole;
                if (role != null) role.SetKillTimer(SheriffPlugin.ActiveCooldown);
            }
            catch (Exception e)
            {
                SheriffPlugin.Log.LogError("Sheriff CheckMurder: " + e);
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(SettingMenu), nameof(SettingMenu.OnEnable))]
    internal static class SettingMenu_OnEnable_Patch
    {
        private static void Postfix(SettingMenu __instance)
        {
            var gameMenu = __instance.TryCast<GameSettingMenu>();
            if (gameMenu != null)
            {
                SheriffMenuInjector.ActiveMenu = gameMenu;
                try { SheriffMenuInjector.Inject(gameMenu); }
                catch (Exception e) { SheriffPlugin.Log.LogError("Inject toggle Sheriff: " + e); }
            }
        }
    }

    internal static class SheriffMenuInjector
    {
        public static GameSettingMenu ActiveMenu;
        private static int _injectedCount;
        private static readonly Dictionary<int, float> _scrollerBaseMax = new();
        private static readonly Dictionary<string, TextMeshPro> _valueTexts = new();

        public static void Inject(GameSettingMenu menu)
        {
            if (menu == null || menu.AllItems == null || menu.AllItems.Count == 0) return;
            var parent = menu.AllItems[0].parent;
            if (parent == null) return;
            var template = menu.keyvaluePrefab;
            if (template == null) return;

            _injectedCount = 0;

            InjectToggle(menu, parent, template, "SheriffToggle", "Enable Sheriff",
                () => {
                    if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
                        return SheriffPlugin.CfgEnabled.Value;
                    else
                        return SheriffPlugin.ActiveEnabled;
                },
                (val) => {
                    SheriffPlugin.CfgEnabled.Value = val;
                    SheriffPlugin.CfgEnabled.ConfigFile.Save();
                    if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
                    {
                        SheriffPlugin.HostBroadcastSettings();
                    }
                });

            InjectNumeric(menu, parent, template, "SheriffCount", "Sheriff Count", 1f, 1f, 3f, "0",
                () => {
                    if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
                        return SheriffPlugin.CfgCount.Value;
                    else
                        return SheriffPlugin.ActiveCount;
                },
                (val) => {
                    SheriffPlugin.CfgCount.Value = (int)val;
                    SheriffPlugin.CfgEnabled.ConfigFile.Save();
                    if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
                    {
                        SheriffPlugin.HostBroadcastSettings();
                    }
                });

            InjectNumeric(menu, parent, template, "SheriffCooldown", "Sheriff Kill Cooldown", 5f, 5f, 60f, "0s",
                () => {
                    if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
                        return SheriffPlugin.CfgCooldown.Value;
                    else
                        return SheriffPlugin.ActiveCooldown;
                },
                (val) => {
                    SheriffPlugin.CfgCooldown.Value = val;
                    SheriffPlugin.CfgEnabled.ConfigFile.Save();
                    if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
                    {
                        SheriffPlugin.HostBroadcastSettings();
                    }
                });

            var scroller = parent.GetComponentInParent<Scroller>();
            if (scroller != null && scroller.YBounds != null)
            {
                int id = scroller.GetInstanceID();
                if (!_scrollerBaseMax.TryGetValue(id, out float baseMax))
                {
                    baseMax = scroller.YBounds.max;
                    _scrollerBaseMax[id] = baseMax;
                }

                var yb = scroller.YBounds;
                scroller.YBounds = new FloatRange(yb.min, baseMax + 1.5f);
            }
        }

        public static void UpdateMenuValues()
        {
            if (ActiveMenu == null || !ActiveMenu.gameObject.activeInHierarchy) return;

            try
            {
                if (_valueTexts.TryGetValue("SheriffToggle", out var toggleText) && toggleText != null)
                    toggleText.text = SheriffPlugin.ActiveEnabled ? "On" : "Off";

                if (_valueTexts.TryGetValue("SheriffCount", out var countText) && countText != null)
                    countText.text = SheriffPlugin.ActiveCount.ToString("0");

                if (_valueTexts.TryGetValue("SheriffCooldown", out var cooldownText) && cooldownText != null)
                    cooldownText.text = SheriffPlugin.ActiveCooldown.ToString("0s");
            }
            catch (Exception e)
            {
                SheriffPlugin.Log.LogError("Error updating client menu: " + e);
            }
        }

        private static void InjectToggle(GameSettingMenu menu, Transform parent, NumberOption template, string name, string label, Func<bool> getter, Action<bool> setter)
        {
            var isHost = AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost;
            var existing = parent.Find(name);
            Transform target;
            TextMeshPro valueText;

            if (existing != null)
            {
                target = existing;
                float yPos = menu.YStart - (menu.AllItems.Count + _injectedCount) * menu.YOffset;
                target.localPosition = new Vector3(target.localPosition.x, yPos, target.localPosition.z);
                _valueTexts.TryGetValue(name, out valueText);
            }
            else
            {
                var go = UnityEngine.Object.Instantiate(template.gameObject, parent);
                go.name = name;
                float y = menu.YStart - (menu.AllItems.Count + _injectedCount) * menu.YOffset;
                go.transform.localPosition = new Vector3(template.transform.localPosition.x, y, template.transform.localPosition.z);
                go.transform.localScale = Vector3.one;
                go.transform.localRotation = Quaternion.identity;
                go.SetActive(true);
                target = go.transform;

                var no = go.GetComponent<NumberOption>();
                var titleText = no != null ? no.TitleText : null;
                valueText = no != null ? no.ValueText : null;
                if (titleText != null) titleText.text = label;
                if (no != null) UnityEngine.Object.Destroy(no);

                _valueTexts[name] = valueText;
            }

            _injectedCount++;

            if (valueText != null) valueText.text = getter() ? "On" : "Off";

            foreach (var pb in target.GetComponentsInChildren<PassiveButton>())
            {
                if (pb == null) continue;
                pb.gameObject.SetActive(isHost);
                if (!isHost || pb.OnClick == null) continue;
                pb.OnClick.RemoveAllListeners();
                var capturedText = valueText;
                pb.OnClick.AddListener((UnityAction)(() =>
                {
                    setter(!getter());
                    if (capturedText != null)
                        capturedText.text = getter() ? "On" : "Off";
                }));
            }
        }

        private static void InjectNumeric(GameSettingMenu menu, Transform parent, NumberOption template, string name, string label, float step, float min, float max, string format, Func<float> getter, Action<float> setter)
        {
            var isHost = AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost;
            var existing = parent.Find(name);
            Transform target;
            TextMeshPro valueText;

            if (existing != null)
            {
                target = existing;
                float yPos = menu.YStart - (menu.AllItems.Count + _injectedCount) * menu.YOffset;
                target.localPosition = new Vector3(target.localPosition.x, yPos, target.localPosition.z);
                _valueTexts.TryGetValue(name, out valueText);
            }
            else
            {
                var go = UnityEngine.Object.Instantiate(template.gameObject, parent);
                go.name = name;
                float y = menu.YStart - (menu.AllItems.Count + _injectedCount) * menu.YOffset;
                go.transform.localPosition = new Vector3(template.transform.localPosition.x, y, template.transform.localPosition.z);
                go.transform.localScale = Vector3.one;
                go.transform.localRotation = Quaternion.identity;
                go.SetActive(true);
                target = go.transform;

                var no = go.GetComponent<NumberOption>();
                var titleText = no != null ? no.TitleText : null;
                valueText = no != null ? no.ValueText : null;
                if (titleText != null) titleText.text = label;
                if (no != null) UnityEngine.Object.Destroy(no);

                _valueTexts[name] = valueText;
            }

            _injectedCount++;

            if (valueText != null) valueText.text = getter().ToString(format);

            var buttons = target.GetComponentsInChildren<PassiveButton>();
            var sorted = new List<PassiveButton>();
            foreach (var b in buttons) if (b != null) sorted.Add(b);
            sorted.Sort((a, b) => a.transform.localPosition.x.CompareTo(b.transform.localPosition.x));

            foreach (var pb in sorted) pb.gameObject.SetActive(isHost);

            if (isHost && sorted.Count >= 2)
            {
                var dec = sorted[0];
                var inc = sorted[sorted.Count - 1];
                var capturedText = valueText;

                dec.OnClick.RemoveAllListeners();
                dec.OnClick.AddListener((UnityAction)(() =>
                {
                    float val = Math.Max(min, getter() - step);
                    setter(val);
                    if (capturedText != null)
                        capturedText.text = getter().ToString(format);
                }));

                inc.OnClick.RemoveAllListeners();
                inc.OnClick.AddListener((UnityAction)(() =>
                {
                    float val = Math.Min(max, getter() + step);
                    setter(val);
                    if (capturedText != null)
                        capturedText.text = getter().ToString(format);
                }));
            }
        }
    }
}
