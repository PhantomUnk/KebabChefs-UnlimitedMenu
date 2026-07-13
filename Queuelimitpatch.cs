using System;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;
using UnityEngine;
using Kebab_Mod_Menu_Count;


// Same approach as with CharacterItemInteract.itemHolderQueue
// physically increase the Transform-points array via AccessTools.Property,
// generate missing points at runtime with extrapolation of the offset between
// the last two existing points.
public static class Queuelimitpatch
{
    [HarmonyPatch(typeof(Restaurant), "Start")]
    public static class Restaurant_Start_ExtendQueues
    {
        static void Postfix(Restaurant __instance)
        {
            ExtendTransformArrayProperty(__instance, "restaurantQueuePoints", ModSettings.QueuePointsCount, "restaurantQueuePoints");
            ExtendTransformArrayProperty(__instance, "GroupSpawnPoints", ModSettings.GroupSpawnPointsCount, "GroupSpawnPoints");
        }
    }

    // In case Start has already executed by the time the patch is applied (e.g., the object was
    // spawned before the mod was fully initialized) - duplicate on InitializationFinished.
    [HarmonyPatch(typeof(Restaurant), "InitializationFinished")]
    public static class Restaurant_Init_ExtendQueues
    {
        static void Postfix(Restaurant __instance)
        {
            ExtendTransformArrayProperty(__instance, "restaurantQueuePoints", ModSettings.QueuePointsCount, "restaurantQueuePoints");
            ExtendTransformArrayProperty(__instance, "GroupSpawnPoints", ModSettings.GroupSpawnPointsCount, "GroupSpawnPoints");
        }
    }

    private static void ExtendTransformArrayProperty(Restaurant restaurant, string propertyName, int targetCount, string logLabel)
    {
        try
        {
            var prop = AccessTools.Property(typeof(Restaurant), propertyName);
            if (prop == null)
            {
                MelonLogger.Msg($"[QueueLimit] Property '{propertyName}' not found!");
                return;
            }

            var current = prop.GetValue(restaurant) as Il2CppReferenceArray<Transform>;
            if (current == null)
            {
                MelonLogger.Msg($"[QueueLimit] '{propertyName}' == null, skipping (object not ready yet?)");
                return;
            }

            int oldLength = current.Length;
            if (oldLength >= targetCount)
            {
                // Already extended (e.g., repeated Start/InitializationFinished call,
                // or targetCount was lowered in settings - array is not shrunk) - do nothing.
                return;
            }

            if (oldLength < 2)
            {
                MelonLogger.Msg($"[QueueLimit] '{propertyName}': insufficient points ({oldLength}) for offset extrapolation, skipping.");
                return;
            }

            // Extrapolate the offset between the last two existing points
            Transform last = current[oldLength - 1];
            Transform prev = current[oldLength - 2];
            Vector3 offset = last.position - prev.position;
            Quaternion rotation = last.rotation;
            Transform parent = last.parent;

            var extended = new Il2CppReferenceArray<Transform>(targetCount);
            for (int i = 0; i < oldLength; i++)
                extended[i] = current[i];

            for (int i = oldLength; i < targetCount; i++)
            {
                var go = new GameObject($"{propertyName}_Generated_{i}");
                var t = go.transform;
                t.SetParent(parent, worldPositionStays: false);
                t.position = last.position + offset * (i - oldLength + 1);
                t.rotation = rotation;
                extended[i] = t;
            }

            prop.SetValue(restaurant, extended);
            MelonLogger.Msg($"[QueueLimit] {logLabel} extended: {oldLength} -> {targetCount}");
        }
        catch (Exception e)
        {
            MelonLogger.Msg($"[QueueLimit] Error extending '{propertyName}': {e}");
        }
    }
}

// Removes the daily customer cap: RestaurantManager.ExpectedCustomers getter is patched
// to always return a fixed high value instead of whatever the game's AnimationCurve computes.
public static class ExpectedCustomersPatch
{
    [HarmonyPatch(typeof(RestaurantManager), nameof(RestaurantManager.ExpectedCustomers), MethodType.Getter)]
    public static class RestaurantManager_ExpectedCustomers_Uncapped
    {
        static void Postfix(ref int __result)
        {
            __result = ModSettings.ExpectedCustomers;
        }
    }
}