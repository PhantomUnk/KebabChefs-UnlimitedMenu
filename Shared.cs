using Il2Cpp;
using MelonLoader;
using System.Reflection;

namespace Kebab_Mod_Menu_Count
{
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
            maxItemProp.SetValue(instance, ModSettings.MaxMenuItems);
            MelonLogger.Msg($"[{source}] maxItemOnMenu changed: {oldValue} -> 999");
        }
    }
}
