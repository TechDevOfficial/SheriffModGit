using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using ClassicUs.Manactor;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace ClassicUs.SheriffMod
{
    [BepInPlugin(Guid, "Classic Us Sheriff", "1.0.11")]
    [BepInDependency(ManactorPlugin.Guid)]
    public class SheriffPlugin : BasePlugin
    {
        public const string Guid = "classicus.sheriff";
        public const string Version = "1.0.11";
        public const string RoleModName = "ClassicUsSheriff";

        public static string SheriffRoleName = "Sheriff";
        public const string RpcSyncSettingsKey = "classicus.sheriff.SyncSettings";
        public const string RequestKillKey = "classicus.sheriff.RequestKill";

        public static ManualLogSource Log;

        public static ConfigEntry<bool> CfgEnabled;
        public static ConfigEntry<int> CfgCount;
        public static ConfigEntry<float> CfgCooldown;
        public static ConfigEntry<float> CfgRoleChance;

        public static bool ActiveEnabled;
        public static int ActiveCount = 1;
        public static float ActiveCooldown = 25f;
        public static float ActiveRoleChance = 100f;

        public override void Load()
        {
            Log = base.Log;

            CfgEnabled = Config.Bind("Game", "EnableSheriff", true,
                "Enables the Sheriff role: a crewmate with a kill button who must kill the impostor.");
            CfgCount = Config.Bind("Game", "SheriffCount", 1,
                new ConfigDescription("How many Sheriffs to assign per match.",
                    new AcceptableValueRange<int>(0, 3)));
            CfgCooldown = Config.Bind("Game", "SheriffKillCooldown", 25f,
                new ConfigDescription("Cooldown of the Sheriff's kill button (seconds).",
                    new AcceptableValueRange<float>(5f, 60f)));
            CfgRoleChance = Config.Bind("Game", "SheriffRoleChance", 100f,
                new ConfigDescription("Chance that a selected candidate becomes Sheriff.",
                    new AcceptableValueRange<float>(0f, 100f)));

            ManactorAPI.Register(RoleModName, Version);
            ManactorAPI.RegisterRpcMethods(this);

            new Harmony(Guid).PatchAll();

            Log.LogInfo("Classic Us Sheriff loaded.");
        }

        public static bool IsTypeReady;

        public static void EnsureIl2CppTypeRegistered()
        {
            if (_il2CppTypeRegistered) return;
            _il2CppTypeRegistered = true;

            ManactorAPI.RegisterIl2CppType(() =>
            {
                try
                {
                    ClassInjector.RegisterTypeInIl2Cpp<SheriffRole>();
                    IsTypeReady = true;
                    Log.LogInfo("SheriffRole type registered in IL2CPP.");
                }
                catch (Exception e)
                {
                    Log.LogError("SheriffRole registration failed: " + e);
                }
            });
        }

        private static bool _il2CppTypeRegistered;

        public static void HostBroadcastSettings()
        {
            ActiveEnabled = CfgEnabled.Value;
            ActiveCount = CfgCount.Value;
            ActiveCooldown = CfgCooldown.Value;
            ActiveRoleChance = CfgRoleChance.Value;

            ManactorAPI.SendRpcMethod(RpcSyncSettingsKey, ActiveEnabled, (byte)ActiveCount, ActiveCooldown, ActiveRoleChance);

            Log.LogInfo($"Sheriff settings sent: enabled={ActiveEnabled} count={ActiveCount} cd={ActiveCooldown} chance={ActiveRoleChance}");
        }

        [ManactorRpc(RpcSyncSettingsKey)]
        private static void OnSyncSettingsRpc(byte senderId, bool enabled, byte count, float cooldown, float roleChance)
        {
            ActiveEnabled = enabled;
            ActiveCount = count;
            ActiveCooldown = cooldown;
            ActiveRoleChance = roleChance;
            Log.LogInfo($"Sheriff settings received: enabled={ActiveEnabled} count={ActiveCount} cd={ActiveCooldown} chance={ActiveRoleChance}");
            SheriffMenuInjector.UpdateMenuValues();
        }

        [ManactorRpc(RequestKillKey)]
        private static void OnSheriffKillRequest(byte senderId, byte targetPlayerId)
        {
            var client = AmongUsClient.Instance;
            if (client == null || !client.AmHost) return;

            PlayerControl sheriff = null, target = null;
            foreach (var p in PlayerControl.AllPlayerControls)
            {
                if (p == null || p.Data == null) continue;
                if (p.Data.PlayerId == senderId) sheriff = p;
                if (p.Data.PlayerId == targetPlayerId) target = p;
            }

            if (sheriff == null || target == null || !IsSheriff(sheriff)) return;
            ResolveSheriffKill(sheriff, target);
        }

        public static void ResolveSheriffKill(PlayerControl sheriff, PlayerControl target)
        {
            if (sheriff == null || sheriff.Data == null) return;
            if (target == null || target.Data == null || target.Data.IsDead) return;

            try
            {
                if (IsImpostor(target))
                    sheriff.RpcMurderPlayer(target, MurderResultFlags.Succeeded);
                else
                    sheriff.RpcMurderPlayer(sheriff, MurderResultFlags.Succeeded);

                var role = sheriff.Data.myRole;
                if (role != null) role.SetKillTimer(ActiveCooldown);
            }
            catch (Exception e)
            {
                Log.LogError("ResolveSheriffKill: " + e);
            }
        }

        public static bool IsImpostor(PlayerControl p)
        {
            if (p == null || p.Data == null) return false;
            try { return RoleManager.IsTeam(p.Data, RoleTeamTypes.Impostor); }
            catch { return false; }
        }

        public static bool IsSheriff(PlayerControl p)
        {
            if (p == null || p.Data == null) return false;
            var role = p.Data.myRole;
            return role != null && role.SafeTryCast<SheriffRole>() != null;
        }
    }
}
