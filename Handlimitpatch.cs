using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;
using System.Reflection;
using UnityEngine;

namespace Kebab_Mod_Menu_Count
{ 
    internal static class HandLimitShared
    {
        //internal int TargetQueueCapacity = ModSettings.MaxHandItems; // maximum slots

        internal static readonly PropertyInfo itemHolderQueueProp =
            AccessTools.Property(typeof(CharacterItemInteract), "itemHolderQueue");

        internal static readonly PropertyInfo itemHolderQueueParentProp =
            AccessTools.Property(typeof(CharacterItemInteract), "itemHolderQueueParent");

        internal static void ExtendQueue(CharacterItemInteract instance, string source)
        {
            if (instance == null) return;

            if (itemHolderQueueProp == null)
            {
                MelonLogger.Error($"[{source}] itemHolderQueueProp == NULL — свойство 'itemHolderQueue' не найдено!");
                return;
            }

            try
            {
                var currentArr = itemHolderQueueProp.GetValue(instance) as Il2CppReferenceArray<Transform>;
                if (currentArr == null)
                {
                    MelonLogger.Warning($"[{source}] itemHolderQueue == null на этом инстансе, пропускаем.");
                    return;
                }

                int originalLength = currentArr.Length;

                if (originalLength >= ModSettings.MaxHandItems)
                {
                    return;
                }

                if (originalLength == 0)
                {
                    MelonLogger.Warning($"[{source}] itemHolderQueue.Length == 0, нечего использовать как базу для расширения.");
                    return;
                }

                Transform lastTf = currentArr[originalLength - 1];
                if (lastTf == null)
                {
                    MelonLogger.Warning($"[{source}] Последний элемент itemHolderQueue == null, пропускаем.");
                    return;
                }

                Vector3 delta = new Vector3(0f, 0.15f, 0f); // fallback if points < 2

                if (originalLength >= 2 && currentArr[originalLength - 2] != null)
                {
                    delta = lastTf.localPosition - currentArr[originalLength - 2].localPosition;
                }

                Transform parentTf = itemHolderQueueParentProp?.GetValue(instance) as Transform;
                if (parentTf == null) parentTf = lastTf.parent;

                var newArr = new Il2CppReferenceArray<Transform>(ModSettings.MaxHandItems);
                for (int i = 0; i < originalLength; i++)
                    newArr[i] = currentArr[i];

                for (int i = originalLength; i < ModSettings.MaxHandItems; i++)
                {
                    var go = new GameObject($"HandLimitMod_QueueSlot_{i}");
                    go.transform.SetParent(parentTf, false);
                    go.transform.localPosition = lastTf.localPosition + delta * (i - originalLength + 1);
                    go.transform.localRotation = lastTf.localRotation;
                    newArr[i] = go.transform;
                }

                itemHolderQueueProp.SetValue(instance, newArr);
                MelonLogger.Msg($"[{source}] itemHolderQueue extended: {originalLength} -> {ModSettings.MaxHandItems}");
            }
            catch (Exception e)
            {
                MelonLogger.Error($"[{source}] ExtendQueue failed: {e}");
            }
        }
    }


    [HarmonyPatch(typeof(CharacterItemInteract), "Start")]
    internal static class HandLimit_StartPatch
    {
        static void Postfix(CharacterItemInteract __instance) =>
            HandLimitShared.ExtendQueue(__instance, "Start");
    }

    [HarmonyPatch(typeof(CharacterItemInteract), "Init")]
    internal static class HandLimit_InitPatch
    {
        static void Postfix(CharacterItemInteract __instance) =>
            HandLimitShared.ExtendQueue(__instance, "Init");
    }
}