using Il2Cpp;
using MelonLoader;
using HarmonyLib;


namespace Kebab_Mod_Menu_Count
{
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
    //[HarmonyPatch(typeof(Menu), "CanAddToMenu")]
    //internal static class CanAddToMenuPatch
    //{
    //    static void Postfix(Menu __instance, bool __result)
    //    {
    //        if (__instance == null || Shared.maxItemProp == null) return;
    //        int currentMax = (int)Shared.maxItemProp.GetValue(__instance);
    //        int currentCount = __instance.GetMenuRecipeCount();
    //        MelonLogger.Msg($"CanAddToMenu() -> result={__result}, maxItemOnMenu={currentMax}, currentCount={currentCount}");
    //    }
    //}
}
