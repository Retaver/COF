// Assets/Project/Scripts/UI/UIController_BarsPatch.cs
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace MyGameNamespace
{
    [DefaultExecutionOrder(100)] // run after most UI setup
    public sealed class UIController_BarsPatch : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;

        private void Awake()
        {
            if (_uiDocument == null)
                _uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable() => StartCoroutine(LateFix());

        private IEnumerator LateFix()
        {
            // Wait one frame so UIController had time to create/capture bars
            yield return null;

            var root = _uiDocument != null ? _uiDocument.rootVisualElement : null;
            if (root == null)
            {
                Debug.LogWarning("[BarsPatch] No UIDocument/root found.");
                yield break;
            }

            // The default UXML layout already defines the five bars and stat lines.
            // However, some Unity versions lay out the bar horizontally next to the stat label.
            // To ensure each bar sits below its label, set each stat-item container to vertical.
            var statItems = root.Query<VisualElement>(className: "stat-item").ToList();
            foreach (var item in statItems)
            {
                // Ensure items stack vertically
                item.style.flexDirection = FlexDirection.Column;
                // Unity 6.2's IStyle API does not expose a 'gap' property, so simulate the spacing
                // by applying a margin-top to all children except the first.
                int ci = 0;
                foreach (var child in item.Children())
                {
                    if (ci > 0)
                    {
                        child.style.marginTop = 2f;
                    }
                    ci++;
                }
            }

            // Also make sure each progress bar fills its parent horizontally.
            var bars = root.Query<ProgressBar>().ToList();
            foreach (var bar in bars)
            {
                bar.style.width = Length.Percent(100);
                bar.style.flexGrow = 0;
                bar.style.flexShrink = 0;
            }

            Debug.Log("[BarsPatch] Adjusted stat-item layout to stack labels and bars vertically.");
        }

        private static ProgressBar EnsureBar(VisualElement root, VisualElement container, string name, string title, string extraClass)
        {
            // Try anywhere in tree
            var bar = root.Q<ProgressBar>(name);
            if (bar == null)
            {
                // Create a new progress bar if one isn't already defined in the UXML
                bar = new ProgressBar { name = name, title = title, lowValue = 0, highValue = 100, value = 0 };
                bar.AddToClassList("stat-bar");
                if (!string.IsNullOrEmpty(extraClass))
                    bar.AddToClassList(extraClass);
                // Place the bar into the container
                container.Add(bar);
                Debug.Log($"[BarsPatch] Created '{name}'.");
            }
            else
            {
                // Do not reparent existing bars. Just ensure they have the right classes and title.
                bar.AddToClassList("stat-bar");
                if (!string.IsNullOrEmpty(extraClass))
                    bar.AddToClassList(extraClass);
                if (string.IsNullOrEmpty(bar.title))
                    bar.title = title;
                Debug.Log($"[BarsPatch] Reused '{name}'.");
            }

            // Ensure the bar fills its parent horizontally and does not stretch vertically
            bar.style.flexGrow = 0;
            bar.style.flexShrink = 0;
            bar.style.width = Length.Percent(100);

            return bar;
        }
    }
}
