using UnityEngine;
using UnityEngine.UIElements;

namespace Project.UI
{
    /// <summary>
    /// Global UI settings helpers for scale and theme classes.
    /// </summary>
    public class SettingsService : MonoBehaviour
    {
        private UIDocument _doc;

        private void Awake()
        {
            _doc = GetComponentInParent<UIDocument>();
        }

        /// <summary>
        /// Uniformly scales the UI root using a CSS scale transform.
        /// </summary>
        /// <param name="root">UI root; if null, uses the scene's UIDocument root.</param>
        /// <param name="scale">Scale factor clamped to [0.5, 2.0].</param>
        public void ApplyScale(VisualElement root, float scale)
        {
            if (root == null) root = _doc != null ? _doc.rootVisualElement : null;
            if (root == null) return;

            scale = Mathf.Clamp(scale, 0.5f, 2.0f);
            root.style.scale = new Scale(new Vector3(scale, scale, 1f));
        }

        /// <summary>
        /// Sets the root theme class to twilight (dark) or daylight (light).
        /// </summary>
        public void SetTheme(VisualElement root, bool dark)
        {
            if (root == null) root = _doc != null ? _doc.rootVisualElement : null;
            if (root == null) return;

            root.RemoveFromClassList("theme--twilight");
            root.RemoveFromClassList("theme--daylight");
            root.AddToClassList(dark ? "theme--twilight" : "theme--daylight");
        }

        /// <summary>
        /// Toggles high contrast theme variant.
        /// </summary>
        public void ToggleContrast(VisualElement root, bool on)
        {
            if (root == null) root = _doc != null ? _doc.rootVisualElement : null;
            if (root == null) return;

            if (on) root.AddToClassList("theme--contrast");
            else root.RemoveFromClassList("theme--contrast");
        }

        /// <summary>
        /// Toggles colorblind protection classes (e.g., theme--cb-prot/deut/trit).
        /// </summary>
        public void ToggleColorBlindVariant(VisualElement root, string variant, bool on)
        {
            if (root == null) root = _doc != null ? _doc.rootVisualElement : null;
            if (root == null || string.IsNullOrEmpty(variant)) return;

            string cls = $"theme--cb-{variant}";
            if (on) root.AddToClassList(cls);
            else root.RemoveFromClassList(cls);
        }
    }
}