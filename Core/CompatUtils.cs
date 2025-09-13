using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MyGameNamespace
{
    /// <summary>
    /// Runtime compatibility helpers to avoid compile-time use of deprecated Object.Find* APIs.
    /// Uses reflection to call newer APIs when available and falls back to Resources.FindObjectsOfTypeAll when needed.
    /// This lets the same code compile and run on Unity 6.2 and newer Unity versions.
    /// </summary>
    public static class CompatUtils
    {
        public static T FindFirstObjectByTypeCompat<T>() where T : UnityEngine.Object
        {
            var objectType = typeof(UnityEngine.Object);
            var methods = objectType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            // Prefer FindFirstObjectByType<T>()
            var findFirst = methods.FirstOrDefault(m => m.Name == "FindFirstObjectByType" && m.IsGenericMethodDefinition);
            if (findFirst != null)
            {
                try
                {
                    var gm = findFirst.MakeGenericMethod(typeof(T));
                    var res = gm.Invoke(null, null);
                    return res as T;
                }
                catch { /* ignore and fallback */ }
            }

            // Next prefer FindAnyObjectByType<T>()
            var findAny = methods.FirstOrDefault(m => m.Name == "FindAnyObjectByType" && m.IsGenericMethodDefinition);
            if (findAny != null)
            {
                try
                {
                    var gm = findAny.MakeGenericMethod(typeof(T));
                    var res = gm.Invoke(null, null);
                    return res as T;
                }
                catch { /* ignore and fallback */ }
            }

            // Fallback: Resources.FindObjectsOfTypeAll<T>()
            try
            {
                var all = Resources.FindObjectsOfTypeAll<T>();
                return all.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        public static T[] FindObjectsOfTypeCompat<T>(bool includeInactive = true) where T : UnityEngine.Object
        {
            var objectType = typeof(UnityEngine.Object);
            var methods = objectType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            // Try to find a generic FindObjectsOfType/FindObjectsByType method via reflection
            var candidate = methods.FirstOrDefault(m => m.Name == "FindObjectsOfType" && m.IsGenericMethodDefinition);
            if (candidate != null)
            {
                try
                {
                    var gm = candidate.MakeGenericMethod(typeof(T));
                    var parameters = candidate.GetParameters();
                    if (parameters.Length == 1 && parameters[0].ParameterType == typeof(bool))
                    {
                        var res = gm.Invoke(null, new object[] { includeInactive });
                        return res as T[] ?? new T[0];
                    }

                    // If parameterless default exists
                    if (parameters.Length == 0)
                    {
                        var res = gm.Invoke(null, null);
                        return res as T[] ?? new T[0];
                    }
                }
                catch
                {
                    // fall through to fallback
                }
            }

            // Final fallback: Resources.FindObjectsOfTypeAll
            try
            {
                var all = Resources.FindObjectsOfTypeAll<T>() ?? new T[0];
                if (!includeInactive)
                {
                    // Filter out assets, keep scene objects only
                    return all.Where(o =>
                    {
                        if (o == null) return false;
                        if (o.hideFlags != HideFlags.None) return false;
                        if (o is GameObject go) return go.scene.isLoaded;
                        if (o is Component c) return c.gameObject.scene.isLoaded;
                        return true;
                    }).ToArray();
                }
                return all;
            }
            catch
            {
                return new T[0];
            }
        }
    }
}