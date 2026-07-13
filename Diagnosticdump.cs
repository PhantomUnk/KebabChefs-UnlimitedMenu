// Diagnostic class.
// F6  - enable/disable detailed (spammy) logs (Verbose)
// F7  - GUI (ModGUI, not from here)
// F8  - DumpCustomerSettings (fameCurve / groupCountByFameCurve)
// F9  - DumpRestaurantQueuePoints (length of the queue points array at the entrance)
// F10 - FindQueuePointsRealName (bruteforce the real name of the queue field/property)
// F11 - ScanAllNumericMembers(NPCManager) (numeric fields of NPCManager)
// F12 - DumpRecipeToMaxPointCandidates (main candidate for the customer ceiling)

using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DiagnosticDump
{
    // Global toggle for spammy logs (ExpectedCustomers every frame,
    // line-by-line output of each TMP-text animation frame). Disabled by default.
    public static bool Verbose = false;

    // ============================================================
    //  COMMON HELPERS (reused by all Dump* methods below)
    // ============================================================

    // Searches for a Type by one of the full candidate names, and if not found -
    // iterates through all loaded assemblies by short name (last segment after the dot).
    // Does not require compile-time using on a specific interop assembly.
    public static Type ResolveType(params string[] candidateFullNames)
    {
        foreach (var name in candidateFullNames)
        {
            var t = AccessTools.TypeByName(name);
            if (t != null) return t;
        }

        string shortName = candidateFullNames.LastOrDefault() ?? "";
        int dot = shortName.LastIndexOf('.');
        if (dot >= 0) shortName = shortName.Substring(dot + 1);

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                var t = asm.GetTypes().FirstOrDefault(x => x.Name == shortName);
                if (t != null) return t;
            }
            catch { /* assemblies that cannot be loaded via reflection are ignored */ }
        }

        return null;
    }

    // Universal recursive dumper: prints primitives/strings as-is,
    // expands AnimationCurve by keys, iterates through collections (arrays/List/Il2Cpp-collections)
    // element by element, dumps complex objects' Fields+Properties up to maxDepth levels deep.
    // Depth is limited to avoid getting stuck in circular references and spamming the log.
    public static void DumpMembers(object instance, string label = null, int depth = 0, int maxDepth = 1)
    {
        string indent = new string(' ', depth * 2);

        if (instance == null)
        {
            MelonLogger.Msg($"[Diag] {indent}{label ?? "value"} = null");
            return;
        }

        var type = instance.GetType();

        if (instance is AnimationCurve curve)
        {
            MelonLogger.Msg($"[Diag] {indent}{label ?? type.Name}: AnimationCurve, keys={curve.keys.Length}, postWrap={curve.postWrapMode}");
            foreach (var k in curve.keys)
                MelonLogger.Msg($"[Diag] {indent}  key: time={k.time}, value={k.value}");
            return;
        }

        if (type.IsPrimitive || instance is string || type.IsEnum || instance is decimal)
        {
            MelonLogger.Msg($"[Diag] {indent}{label ?? type.Name} = {instance}");
            return;
        }

        if (instance is System.Collections.IEnumerable enumerable)
        {
            MelonLogger.Msg($"[Diag] {indent}{label ?? type.Name}: collection ({type.Name})");
            int i = 0;
            foreach (var item in enumerable)
            {
                if (depth >= maxDepth)
                    MelonLogger.Msg($"[Diag] {indent}  [{i}] = {item}");
                else
                    DumpMembers(item, $"[{i}]", depth + 1, maxDepth);

                i++;
                if (i > 40)
                {
                    MelonLogger.Msg($"[Diag] {indent}  ... truncated at 40 elements");
                    break;
                }
            }
            return;
        }

        MelonLogger.Msg($"[Diag] {indent}--- {label ?? type.Name} ({type.FullName}) ---");
        if (depth >= maxDepth)
        {
            MelonLogger.Msg($"[Diag] {indent}  (depth exhausted, ToString: {instance})");
            return;
        }

        foreach (var f in type.GetFields(AccessTools.all))
        {
            try { DumpMembers(f.GetValue(instance), $"Field '{f.Name}'", depth + 1, maxDepth); }
            catch (Exception e) { MelonLogger.Msg($"[Diag] {indent}  Field '{f.Name}' -> error: {e.Message}"); }
        }

        foreach (var p in type.GetProperties(AccessTools.all))
        {
            if (p.GetIndexParameters().Length > 0) continue;
            try { DumpMembers(p.GetValue(instance), $"Property '{p.Name}'", depth + 1, maxDepth); }
            catch (Exception e) { MelonLogger.Msg($"[Diag] {indent}  Property '{p.Name}' -> error: {e.Message}"); }
        }
    }

    // Scans a specific instance for fields/properties whose name OR type contains the given substring -
    // useful when we don't know the exact field name (version mismatch) but know what to look for.
    public static void ScanInstanceForTypeNameSubstring(object instance, string substring)
    {
        if (instance == null) return;
        var type = instance.GetType();
        MelonLogger.Msg($"[Diag] === Scanning {type.Name} for fields/properties related to '{substring}' ===");

        bool Matches(string memberName, string memberTypeName) =>
            memberTypeName.IndexOf(substring, StringComparison.OrdinalIgnoreCase) >= 0
            || memberName.IndexOf(substring, StringComparison.OrdinalIgnoreCase) >= 0
            || (memberName.IndexOf("recipe", StringComparison.OrdinalIgnoreCase) >= 0
                && memberName.IndexOf("point", StringComparison.OrdinalIgnoreCase) >= 0);

        foreach (var f in type.GetFields(AccessTools.all))
        {
            if (!Matches(f.Name, f.FieldType.Name)) continue;
            try { DumpMembers(f.GetValue(instance), $"Field '{f.Name}'"); }
            catch (Exception e) { MelonLogger.Msg($"[Diag] Field '{f.Name}' -> error: {e.Message}"); }
        }

        foreach (var p in type.GetProperties(AccessTools.all))
        {
            if (p.GetIndexParameters().Length > 0) continue;
            if (!Matches(p.Name, p.PropertyType.Name)) continue;
            try { DumpMembers(p.GetValue(instance), $"Property '{p.Name}'"); }
            catch (Exception e) { MelonLogger.Msg($"[Diag] Property '{p.Name}' -> error: {e.Message}"); }
        }
    }

    // ============================================================
    //  NEW: structural helpers for RecipeToMaxPoint (do not require an instance
    //  / search by exact type match, not by short name substring)
    // ============================================================

    // Dumps the type structure itself (without an instance) - finds its fields/properties,
    // even if no live object of this type could be found.
    public static void DumpTypeDefinition(Type t)
    {
        if (t == null)
        {
            MelonLogger.Msg("[Diag] DumpTypeDefinition: type == null");
            return;
        }

        MelonLogger.Msg($"[Diag] === Structure of type {t.FullName} ===");
        foreach (var f in t.GetFields(AccessTools.all))
            MelonLogger.Msg($"[Diag]   Field '{f.Name}' : {f.FieldType.FullName}");

        foreach (var p in t.GetProperties(AccessTools.all))
        {
            if (p.GetIndexParameters().Length > 0) continue;
            MelonLogger.Msg($"[Diag]   Property '{p.Name}' : {p.PropertyType.FullName}");
        }
    }

    // Exact search across ALL loaded types in ALL assemblies: a field/property either exactly
    // targetType, or a generic wrapper (array/List/Il2CppReferenceArray, etc.) where
    // targetType is one of the generic arguments. Unlike ScanInstanceForTypeNameSubstring
    // it doesn't depend on an instance and doesn't skip arrays/lists (compares by actual Type, not .Name).
    public static void FindReferencesToType(Type target)
    {
        if (target == null)
        {
            MelonLogger.Msg("[Diag] FindReferencesToType: target == null");
            return;
        }

        MelonLogger.Msg($"[Diag] === Searching for references to {target.FullName} across all loaded assemblies ===");
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;
            try { types = asm.GetTypes(); }
            catch { continue; }

            foreach (var type in types)
            {
                try
                {
                    foreach (var f in type.GetFields(AccessTools.all))
                    {
                        if (f.FieldType == target
                            || (f.FieldType.IsGenericType && f.FieldType.GetGenericArguments().Contains(target)))
                        {
                            MelonLogger.Msg($"[Diag] FOUND: {type.FullName}.{f.Name} ({f.FieldType.FullName})");
                        }
                    }

                    foreach (var p in type.GetProperties(AccessTools.all))
                    {
                        if (p.GetIndexParameters().Length > 0) continue;
                        if (p.PropertyType == target
                            || (p.PropertyType.IsGenericType && p.PropertyType.GetGenericArguments().Contains(target)))
                        {
                            MelonLogger.Msg($"[Diag] FOUND: {type.FullName}.{p.Name} ({p.PropertyType.FullName})");
                        }
                    }
                }
                catch { /* some types cannot be fully reflected, skip */ }
            }
        }
        MelonLogger.Msg("[Diag] === Reference search completed ===");
    }

    // ============================================================
    //  EXISTING METHODS (F8-F11) - no changes in logic
    // ============================================================

    public static void DumpCustomerSettings()
    {
        var allSettings = Resources.FindObjectsOfTypeAll<CustomerSettings>();
        MelonLogger.Msg($"[Diag] Found CustomerSettings assets: {allSettings.Length}");

        foreach (var settings in allSettings)
        {
            MelonLogger.Msg($"--- CustomerSettings: {settings.name} ---");
            DumpCurveByPropertyName(settings, "FameCurve");
            DumpCurveByPropertyName(settings, "GroupCountByFameCurve");

            foreach (var prop in typeof(CustomerSettings).GetProperties(AccessTools.all))
            {
                if (prop.Name.IndexOf("curve", StringComparison.OrdinalIgnoreCase) >= 0
                    || prop.Name.IndexOf("fame", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    try { DumpMembers(prop.GetValue(settings), $"Property '{prop.Name}'"); }
                    catch (Exception e) { MelonLogger.Msg($"[Diag] Property {prop.Name} -> read error: {e.Message}"); }
                }
            }
        }
    }

    private static void DumpCurveByPropertyName(CustomerSettings settings, string propName)
    {
        var prop = AccessTools.Property(typeof(CustomerSettings), propName);
        if (prop == null)
        {
            MelonLogger.Msg($"[Diag] Property {propName} not found!");
            return;
        }
        DumpMembers(prop.GetValue(settings), propName);
    }

    public static void DumpRestaurantQueuePoints(Restaurant restaurant)
    {
        try
        {
            var field = AccessTools.Field(typeof(Restaurant), "restaurantQueuePoints");
            var prop = AccessTools.Property(typeof(Restaurant), "restaurantQueuePoints");
            object val = field != null ? field.GetValue(restaurant) : prop?.GetValue(restaurant);

            // IMPORTANT: the actual runtime type is Il2CppReferenceArray<Transform>, not Transform[]/System.Array.
            // "val as Transform[]" will always return null here - this is not a field search bug, but a cast bug.
            var arr = val as Il2CppReferenceArray<Transform>;
            MelonLogger.Msg($"[Diag] restaurantQueuePoints: accessed via {(field != null ? "Field" : "Property")}, length = {arr?.Length}");

            var count = restaurant.GetQueuePointCount();
            MelonLogger.Msg($"[Diag] GetQueuePointCount() = {count}");
        }
        catch (Exception e)
        {
            MelonLogger.Msg($"[Diag] Error dumping restaurantQueuePoints: {e}");
        }
    }

    public static void FindQueuePointsRealName(Restaurant restaurant)
    {
        var type = typeof(Restaurant);
        MelonLogger.Msg("[Diag] --- Scanning all Properties of type Restaurant for Transform[]/Il2CppReferenceArray<Transform> ---");
        foreach (var prop in type.GetProperties(AccessTools.all))
        {
            try
            {
                if (prop.GetIndexParameters().Length > 0) continue;
                var val = prop.GetValue(restaurant);
                if (val is Il2CppReferenceArray<Transform> arr)
                    MelonLogger.Msg($"[Diag] FOUND: Property '{prop.Name}' -> Il2CppReferenceArray<Transform>[{arr.Length}]");
                else if (prop.PropertyType.Name.Contains("Transform") || prop.Name.IndexOf("queue", StringComparison.OrdinalIgnoreCase) >= 0)
                    MelonLogger.Msg($"[Diag] Similar property: '{prop.Name}' (type={prop.PropertyType.Name}) = {val}");
            }
            catch (Exception e)
            {
                MelonLogger.Msg($"[Diag] Property '{prop.Name}' -> read error: {e.Message}");
            }
        }

        MelonLogger.Msg("[Diag] --- Scanning all Fields of type Restaurant for Transform[]/Il2CppReferenceArray<Transform> ---");
        foreach (var field in type.GetFields(AccessTools.all))
        {
            try
            {
                if (field.GetValue(restaurant) is Il2CppReferenceArray<Transform> arr)
                    MelonLogger.Msg($"[Diag] FOUND: Field '{field.Name}' -> Il2CppReferenceArray<Transform>[{arr.Length}]");
            }
            catch (Exception e)
            {
                MelonLogger.Msg($"[Diag] Field '{field.Name}' -> read error: {e.Message}");
            }
        }
    }

    public static void ScanAllNumericMembers(object instance)
    {
        var type = instance.GetType();
        MelonLogger.Msg($"[Diag] === Numeric Fields of type {type.Name} ===");
        foreach (var f in type.GetFields(AccessTools.all))
        {
            if (f.FieldType != typeof(int) && f.FieldType != typeof(float) && f.FieldType != typeof(double)) continue;
            try { MelonLogger.Msg($"[Diag] Field '{f.Name}' ({f.FieldType.Name}) = {f.GetValue(instance)}"); }
            catch (Exception e) { MelonLogger.Msg($"[Diag] Field '{f.Name}' -> error: {e.Message}"); }
        }

        MelonLogger.Msg($"[Diag] === Numeric Properties of type {type.Name} ===");
        foreach (var p in type.GetProperties(AccessTools.all))
        {
            if (p.GetIndexParameters().Length > 0) continue;
            if (p.PropertyType != typeof(int) && p.PropertyType != typeof(float) && p.PropertyType != typeof(double)) continue;
            try { MelonLogger.Msg($"[Diag] Property '{p.Name}' ({p.PropertyType.Name}) = {p.GetValue(instance)}"); }
            catch (Exception e) { MelonLogger.Msg($"[Diag] Property '{p.Name}' -> error: {e.Message}"); }
        }
    }

    // ============================================================
    //  F12: RecipeToMaxPoint - main candidate for the customer ceiling.
    //  The name directly hints at a "number of dishes -> max points" table,
    //  which would perfectly explain the stagnation around 19-20 dishes.
    // ============================================================

    public static void DumpRecipeToMaxPointCandidates()
    {
        // 1) Try to find it as a standalone UnityEngine.Object (ScriptableObject/asset)
        var targetType = ResolveType("Il2Cpp.RecipeToMaxPoint", "RecipeToMaxPoint");
        if (targetType != null)
        {
            MelonLogger.Msg($"[Diag] RecipeToMaxPoint type found: {targetType.FullName}");

            // Structure of the type itself - works even without a single live instance.
            DumpTypeDefinition(targetType);

            try
            {
                // IMPORTANT: FindObjectsOfTypeAll in Il2CppInterop expects Il2CppSystem.Type, not System.Type -
                // hence the explicit Il2CppType.From(...) is needed, otherwise CS1503 at compile time.
                var allObjs = Resources.FindObjectsOfTypeAll(Il2CppType.From(targetType));
                MelonLogger.Msg($"[Diag] Resources.FindObjectsOfTypeAll({targetType.Name}) -> {allObjs.Length} pcs.");
                foreach (var obj in allObjs)
                    DumpMembers(obj, $"Instance '{obj}'");
            }
            catch (Exception e)
            {
                // Expected if RecipeToMaxPoint is just a serializable class/struct,
                // not a UnityEngine.Object (then it won't be found via Resources, only as a field).
                MelonLogger.Msg($"[Diag] RecipeToMaxPoint not found via Resources (possibly not a UnityEngine.Object): {e.Message}");
            }

            // Exact search across ALL types/assemblies (including arrays/List), not just
            // RestaurantManager/CustomerSettings and not just by short name substring.
            FindReferencesToType(targetType);
        }
        else
        {
            MelonLogger.Msg("[Diag] RecipeToMaxPoint type doesn't resolve directly by name - will search as a field/array inside known managers.");
        }

        // 2) Search as a field/property/array inside RestaurantManager and all CustomerSettings -
        //    works even if targetType == null (search by name substring), and serves as an additional check.
        var restaurantManager = UnityEngine.Object.FindObjectOfType<RestaurantManager>();
        if (restaurantManager != null)
            ScanInstanceForTypeNameSubstring(restaurantManager, "RecipeToMaxPoint");
        else
            MelonLogger.Msg("[Diag] RestaurantManager not found in the scene (are you not in a match?) - skipping this scan.");

        foreach (var cs in Resources.FindObjectsOfTypeAll<CustomerSettings>())
            ScanInstanceForTypeNameSubstring(cs, "RecipeToMaxPoint");
    }

    // ============================================================
    //  TMP_Text tracer (for reference/future use - already served its purpose,
    //  confirmed that DayStartHUD only displays the ready-made value)
    // ============================================================

    private static readonly System.Text.RegularExpressions.Regex RangePattern =
        new System.Text.RegularExpressions.Regex(@"\d+\s*-\s*\d+");

    public static void InstallTmpTextTracer(HarmonyLib.Harmony harmony)
    {
        var tmpType = ResolveType("TMPro.TMP_Text", "Il2CppTMPro.TMP_Text", "Il2Cpp.TMPro.TMP_Text");
        if (tmpType == null)
        {
            MelonLogger.Msg("[Diag] DID NOT FIND TMP_Text type in any loaded assembly!");
            return;
        }

        var setter = AccessTools.PropertySetter(tmpType, "text");
        if (setter == null)
        {
            MelonLogger.Msg("[Diag] Found TMP_Text type, but did not find the 'text' property setter!");
            return;
        }

        var prefix = typeof(DiagnosticDump).GetMethod(nameof(TmpTextTracerPrefix), AccessTools.all);
        harmony.Patch(setter, prefix: new HarmonyMethod(prefix));
        MelonLogger.Msg("[Diag] TMP_Text.text tracer installed successfully.");
    }

    private static readonly HashSet<int> _alreadyDumped = new HashSet<int>();

    private static void TmpTextTracerPrefix(object __instance, string value)
    {
        if (string.IsNullOrEmpty(value)) return;
        if (!RangePattern.IsMatch(value)) return;

        string goName = "???";
        int instanceId = __instance.GetHashCode();
        GameObject go = null;
        try
        {
            var goProp = __instance.GetType().GetProperty("gameObject");
            go = goProp?.GetValue(__instance) as GameObject;
            goName = go != null ? go.name : "???";
        }
        catch { }

        // Previously this log was always on and spammed on every counter animation frame (0-1, 19-24, ...).
        // Now - only if Verbose (F6) is explicitly enabled.
        if (Verbose)
            MelonLogger.Msg($"[Diag] TMP_Text.text = '{value}' on GameObject '{goName}'");

        // Hierarchy dump - once per unique GameObject, doesn't spam by default
        if (go != null && _alreadyDumped.Add(instanceId))
            DumpHierarchyAndOwner(go);
    }

    private static void DumpHierarchyAndOwner(GameObject go)
    {
        MelonLogger.Msg("[Diag] --- Hierarchy upwards from text ---");
        var t = go.transform;
        while (t != null)
        {
            var components = t.GetComponents<Component>();
            var compNames = string.Join(", ", components.Select(c => c?.GetType().Name ?? "null"));
            MelonLogger.Msg($"[Diag] GameObject '{t.gameObject.name}' -> components: [{compNames}]");
            t = t.parent;
        }

        var hud = go.GetComponentInParent<DayStartHUD>();
        if (hud != null)
            DumpMembers(hud, "DayStartHUD");
    }

    // add to DiagnosticDump.cs
    public static void DumpFullManagerState()
    {
        var manager = UnityEngine.Object.FindObjectOfType<RestaurantManager>();
        if (manager != null)
            DumpMembers(manager, "RestaurantManager", 0, 3); // maxDepth=3 instead of 1 - dig deeper

        var statsType = ResolveType("Il2Cpp.StatsManager", "StatsManager");
        var stats = statsType != null ? UnityEngine.Object.FindObjectOfType(Il2CppType.From(statsType)) : null;
        if (stats != null)
            DumpMembers(stats, "StatsManager", 0, 3);
        else
            MelonLogger.Msg("[Diag] StatsManager not found in the scene.");
    }

    // ============================================================
    //  NPCManager (F11 helper) - remains, but 150 has already been confirmed
    //  as likely UNRELATED "limit of simultaneously active NPCs".
    // ============================================================

    public static void DumpNPCManagerState(NPCManager manager)
    {
        try
        {
            DumpMembers(manager, "NPCManager");
        }
        catch (Exception e)
        {
            MelonLogger.Msg($"[Diag] Error dumping NPCManager: {e}");
        }
    }

    [HarmonyPatch(typeof(RestaurantManager), "get_ExpectedCustomers")]
    public static class ExpectedCustomers_DiagPatch
    {
        static void Postfix(RestaurantManager __instance, int __result)
        {
            // Previously spammed on every getter call (effectively every frame). Now - under Verbose.
            if (!DiagnosticDump.Verbose) return;

            MelonLogger.Msg(
                $"[Diag] ExpectedCustomers={__result} | RestaurantPoints={__instance.RestaurantPoints} " +
                $"| Level={__instance.Level} | ExperiencePoint={__instance.ExperiencePoint} " +
                $"| CustomerRating={__instance.CustomerRating}");
        }
    }
}