// Resets common singletons at domain reload / app start to avoid "invalid GC handle" spam.
using System;
using System.Reflection;
using UnityEngine;

public static class DomainReset
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        TryResetSingleton("CompositionRoot", "Instance");                 // global injector
        TryResetSingleton("GameSceneManager", "Instance");                 // scene router
        TryResetSingleton("MyGameNamespace.GameManager", "Instance");                 // core manager
        // Add others here if you have them: UIManager, AudioManager, etc.
    }

    private static void TryResetSingleton(string typeName, string propertyName)
    {
        var t = Type.GetType(typeName);
        if (t == null)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                t = asm.GetType(typeName);
                if (t != null) break;
            }
        }
        if (t == null) return;

        var p = t.GetProperty(propertyName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        if (p != null && p.CanWrite) { p.SetValue(null, null); return; }

        // Private auto-property backing field fallback
        var f = t.GetField($"<{propertyName}>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic);
        if (f != null) f.SetValue(null, null);
    }
}
