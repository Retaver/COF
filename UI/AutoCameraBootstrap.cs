using System;
using System.Reflection;
using UnityEngine;

namespace MyGameNamespace
{
    /// <summary>
    /// Ensures Display 1 always has an enabled camera when Play Mode starts or a scene loads.
    /// Works with built-in or URP (Base/Overlay) using reflection if URP isn't referenced.
    /// </summary>
    public static class AutoCameraBootstrap
    {
        // Run before ANY scene loads so the overlay never shows.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureCamera()
        {
            const int Display1 = 0;

            // Find any active camera already targeting Display 1.
#if UNITY_2023_1_OR_NEWER
            var cams = UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
#else
            var cams = UnityEngine.Object.FindObjectsOfType<Camera>();
#endif
            foreach (var cam in cams)
            {
                if (cam != null && cam.isActiveAndEnabled && cam.targetDisplay == Display1)
                    return; // We’re good.
            }

            // If an active camera exists but targets another display, retarget the first one.
            foreach (var cam in cams)
            {
                if (cam != null && cam.isActiveAndEnabled)
                {
                    Debug.LogWarning($"[AutoCameraBootstrap] Retargeting '{cam.name}' to Display 1.");
                    cam.targetDisplay = Display1;
                    return;
                }
            }

            // No enabled cameras at all -> create a lightweight one that renders nothing.
            var go = new GameObject("Main Camera (Auto)");
            var newCam = go.AddComponent<Camera>();
            newCam.clearFlags = CameraClearFlags.SolidColor;
            newCam.backgroundColor = new Color(0.06f, 0.05f, 0.08f, 1f); // match your theme
            newCam.cullingMask = 0;         // render nothing; just suppress overlay
            newCam.orthographic = true;
            newCam.targetDisplay = Display1;

            try { go.tag = "MainCamera"; } catch { /* tag exists by default */ }

            // If URP is in use and this camera accidentally becomes Overlay, force Base via reflection.
            TryForceUrpBaseCamera(newCam);

            UnityEngine.Object.DontDestroyOnLoad(go);
            Debug.LogWarning("[AutoCameraBootstrap] Created fallback camera for Display 1.");
        }

        private static void TryForceUrpBaseCamera(Camera cam)
        {
            // Avoid hard dependency on URP: use reflection if present.
            var uacd = cam.GetComponent("UnityEngine.Rendering.Universal.UniversalAdditionalCameraData");
            if (uacd == null) uacd = cam.GetComponent("UniversalAdditionalCameraData");
            if (uacd == null) return;

            var rtProp = uacd.GetType().GetProperty("renderType", BindingFlags.Public | BindingFlags.Instance);
            if (rtProp == null) return;

            // enum UniversalAdditionalCameraData.CameraRenderType { Base = 0, Overlay = 1 }
            try { rtProp.SetValue(uacd, 0, null); } catch { /* ignore */ }
        }
    }
}
