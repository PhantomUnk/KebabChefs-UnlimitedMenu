using MelonLoader;
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
                LoggerInstance.Msg($"Harmony PatchAll OK. Patched methods: {patched.Count}");
            }
            catch (Exception e)
            {
                LoggerInstance.Error("Harmony PatchAll failed with error: " + e);
            }
        }
    public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F7))
                ModGUI.ToggleGUI();
        }
    }
}