using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using System;
using System.Linq;
using System.Reflection;

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
                LoggerInstance.Msg($"Harmony PatchAll OK. Patched methods: {patched.Count}");
            }
            catch (Exception e)
            {
                LoggerInstance.Error("Harmony PatchAll failed with error: " + e);
            }
        }
    }

    // Shared helpers for raising the menu item limit via reflection.
    internal static class Shared
    {
        internal static readonly PropertyInfo maxItemProp =
            typeof(Menu).GetProperty("maxItemOnMenu", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

        internal static void Override(Menu instance, string source)
        {
            if (instance == null) return;

            if (maxItemProp == null)
            {
                MelonLogger.Error($"[{source}] maxItemProp == NULL — property 'maxItemOnMenu' not found!");
                return;
            }

            int oldValue = (int)maxItemProp.GetValue(instance);
            maxItemProp.SetValue(instance, 999);
            MelonLogger.Msg($"[{source}] maxItemOnMenu changed: {oldValue} -> 999");
        }
    }

    // Re-apply the limit whenever the menu initializes or refreshes.
    [HarmonyPatch(typeof(Menu), "Awake")]
    internal static class AwakePatch
    {
        static void Postfix(Menu __instance) => Shared.Override(__instance, "Awake");
    }

    [HarmonyPatch(typeof(Menu), "OnNetworkSpawn")]
    internal static class SpawnPatch
    {
        static void Postfix(Menu __instance) => Shared.Override(__instance, "OnNetworkSpawn");
    }

    [HarmonyPatch(typeof(Menu), "RefreshMenuStats")]
    internal static class RefreshMenuStatsPatch
    {
        static void Postfix(Menu __instance) => Shared.Override(__instance, "RefreshMenuStats");
    }

    [HarmonyPatch(typeof(Menu), "LoadRecipes")]
    internal static class LoadRecipesPatch
    {
        static void Postfix(Menu __instance) => Shared.Override(__instance, "LoadRecipes");
    }

    // Logs current menu capacity when the game checks whether a recipe can be added.
    [HarmonyPatch(typeof(Menu), "CanAddToMenu")]
    internal static class CanAddToMenuPatch
    {
        static void Postfix(Menu __instance, bool __result)
        {
            if (__instance == null || Shared.maxItemProp == null) return;
            int currentMax = (int)Shared.maxItemProp.GetValue(__instance);
            int currentCount = __instance.GetMenuRecipeCount();
            MelonLogger.Msg($"CanAddToMenu() -> result={__result}, maxItemOnMenu={currentMax}, currentCount={currentCount}");
        }
    }
}