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
    [BepInPlugin(Guid, "Classic Us Sheriff", "1.0.9")]
    [BepInDependency(ManactorPlugin.Guid)]
    public class SheriffPlugin : BasePlugin
    {
        public const string Guid = "classicus.sheriff";
        public const string Version = "1.0.9";
        public const string RoleModName = "ClassicUsSheriff";

        public static string SheriffRoleName = "Sheriff";
        public const string RpcSyncSettingsKey = "classicus.sheriff.SyncSettings";

        public static ManualLogSource Log;

        public static ConfigEntry<bool> CfgEnabled;
        public static ConfigEntry<int> CfgCount;
        public static ConfigEntry<float> CfgCooldown;

        public static bool ActiveEnabled;
        public static int ActiveCount = 1;
        public static float ActiveCooldown = 25f;

        public override void Load()
        {
            Log = base.Log;

            CfgEnabled = Config.Bind("Game", "EnableSheriff", true,
                "Abilita il ruolo Sheriff: un crewmate con killbutton che deve uccidere l'impostore.");
            CfgCount = Config.Bind("Game", "SheriffCount", 1,
                new ConfigDescription("Quanti Sheriff assegnare per partita.",
                    new AcceptableValueRange<int>(0, 3)));
            CfgCooldown = Config.Bind("Game", "SheriffKillCooldown", 25f,
                new ConfigDescription("Cooldown del killbutton dello Sheriff (secondi).",
                    new AcceptableValueRange<float>(5f, 60f)));

            ManactorAPI.Register(RoleModName, Version);
            ManactorAPI.RegisterRpcMethods(this);

            new Harmony(Guid).PatchAll();

            try
            {
                ClassInjector.RegisterTypeInIl2Cpp<SheriffRole>();
                Log.LogInfo("SheriffRole type registered in IL2CPP.");
            }
            catch (Exception e)
            {
                Log.LogError("SheriffRole registration failed: " + e);
            }

            Log.LogInfo("Classic Us Sheriff loaded.");
        }

        public static void HostBroadcastSettings()
        {
            ActiveEnabled = CfgEnabled.Value;
            ActiveCount = CfgCount.Value;
            ActiveCooldown = CfgCooldown.Value;

            ManactorAPI.SendRpcMethod(RpcSyncSettingsKey, ActiveEnabled, (byte)ActiveCount, ActiveCooldown);

            Log.LogInfo($"Sheriff settings sent: enabled={ActiveEnabled} count={ActiveCount} cd={ActiveCooldown}");
        }

        [ManactorRpc(RpcSyncSettingsKey)]
        private static void OnSyncSettingsRpc(byte senderId, bool enabled, byte count, float cooldown)
        {
            ActiveEnabled = enabled;
            ActiveCount = count;
            ActiveCooldown = cooldown;
            Log.LogInfo($"Sheriff settings received: enabled={ActiveEnabled} count={ActiveCount} cd={ActiveCooldown}");
            SheriffMenuInjector.UpdateMenuValues();
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
            return role != null && role.TryCast<SheriffRole>() != null;
        }
    }
}
