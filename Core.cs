using Il2Cpp;
using MelonLoader;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

[assembly: MelonInfo(typeof(Kebab_Mod_Menu_Count.MenuLimitModClass), "Kebab Mod Menu Count", "1.0.0", "Mark", null)]
[assembly: MelonGame("Biotech Gameworks", "Kebab Chefs! - Restaurant Simulator")]

namespace Kebab_Mod_Menu_Count
{
    public class MenuLimitModClass : MelonMod
    {
        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("MenuLimitMod loaded!");

            try
            {
                HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
                var patched = HarmonyLib.Harmony.GetAllPatchedMethods().ToList();
                ModSettings.Init();
                ModGUI.Build();
                DiagnosticDump.InstallTmpTextTracer(HarmonyInstance);
                LoggerInstance.Msg($"Harmony PatchAll OK. Patched methods: {patched.Count}");
            }
            catch (Exception e)
            {
                LoggerInstance.Error("Harmony PatchAll failed with error: " + e);
            }
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F6))
            {
                DiagnosticDump.Verbose = !DiagnosticDump.Verbose;
                MelonLoader.MelonLogger.Msg($"[Diag] Verbose = {DiagnosticDump.Verbose}");
            }
            else if (Input.GetKeyDown(KeyCode.F7))
                ModGUI.ToggleGUI();
            /*else if (Input.GetKeyDown(KeyCode.F8))
                DiagnosticDump.DumpCustomerSettings();
            else if (Input.GetKeyDown(KeyCode.F9))
            {
                var manager = UnityEngine.Object.FindObjectOfType<RestaurantManager>();
                if (manager == null) { MelonLoader.MelonLogger.Msg("[Diag] RestaurantManager не найден на сцене (ты не в игре/матче?)"); return; }
                var restaurant = manager.CurrentRestaurant;
                if (restaurant == null) { MelonLoader.MelonLogger.Msg("[Diag] CurrentRestaurant == null (ресторан ещё не выбран/не заспавнен)"); return; }
                DiagnosticDump.DumpRestaurantQueuePoints(restaurant);
            }
            else if (Input.GetKeyDown(KeyCode.F10))
            {
                var manager = UnityEngine.Object.FindObjectOfType<RestaurantManager>();
                if (manager == null) { MelonLoader.MelonLogger.Msg("[Diag] RestaurantManager не найден на сцене (ты не в игре/матче?)"); return; }
                var restaurant = manager.CurrentRestaurant;
                if (restaurant == null) { MelonLoader.MelonLogger.Msg("[Diag] CurrentRestaurant == null (ресторан ещё не выбран/не заспавнен)"); return; }
                DiagnosticDump.FindQueuePointsRealName(restaurant);
            }
            else if (Input.GetKeyDown(KeyCode.F11))
            {
                var npcManager = UnityEngine.Object.FindObjectOfType<NPCManager>();
                if (npcManager == null) { MelonLoader.MelonLogger.Msg("[Diag] NPCManager не найден (не в матче?)"); return; }
                DiagnosticDump.ScanAllNumericMembers(npcManager);
            }
            else if (Input.GetKeyDown(KeyCode.F12))
                DiagnosticDump.DumpRecipeToMaxPointCandidates();
            else if (Input.GetKeyDown(KeyCode.F4))
                DiagnosticDump.DumpFullManagerState();
        }*/
        }
    }
}