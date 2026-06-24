using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Hazel;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace ClassicUs.SheriffMod
{
    [BepInPlugin(Guid, "Classic Us Sheriff", "1.0.2")]
    [BepInDependency(ClassicUs.Manactor.ManactorPlugin.Guid)]
    public class SheriffPlugin : BasePlugin
    {
        public const string Guid = "classicus.sheriff";
        public const string Version = "1.0.2";
        public const string RoleModName = "ClassicUsSheriff";

        public static string SheriffRoleName = "Sheriff";
        public const byte RpcSyncSettings = 210;

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

            ClassicUs.Manactor.ManactorAPI.Register(RoleModName, Version);
            ClassicUs.Manactor.ManactorAPI.RegisterRpcHandler(RpcSyncSettings, OnSyncSettingsRpc);

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

            ClassicUs.Manactor.ManactorAPI.SendRpc(RpcSyncSettings, w =>
            {
                w.Write(ActiveEnabled);
                w.Write((byte)ActiveCount);
                w.Write(ActiveCooldown);
            });

            Log.LogInfo($"Sheriff settings sent: enabled={ActiveEnabled} count={ActiveCount} cd={ActiveCooldown}");
        }

        private static void OnSyncSettingsRpc(byte senderId, MessageReader reader)
        {
            try
            {
                ActiveEnabled = reader.ReadBoolean();
                ActiveCount = reader.ReadByte();
                ActiveCooldown = reader.ReadSingle();
                Log.LogInfo($"Sheriff settings received: enabled={ActiveEnabled} count={ActiveCount} cd={ActiveCooldown}");
                SheriffMenuInjector.UpdateMenuValues();
            }
            catch (Exception e)
            {
                Log.LogError("Reading Sheriff settings failed: " + e);
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
            return role != null && role.TryCast<SheriffRole>() != null;
        }
    }
}
