using UnityEngine;
using UnityEngine.UIElements;

namespace MyGameNamespace   // <-- keep your project namespace; change if needed
{
    /// <summary>
    /// Adds Hide()/Show() to CharacterCreationController without touching the original file.
    /// </summary>
    public static class CharacterCreationControllerExtensions
    {
        public static void Hide(this CharacterCreationController self)
        {
            if (self == null) return;

            var doc = self.GetComponent<UIDocument>();
            if (doc != null && doc.rootVisualElement != null)
            {
                // Hide via UI Toolkit so the object can stay active if other systems need it
                doc.rootVisualElement.style.display = DisplayStyle.None;
            }
        }

        public static void Show(this CharacterCreationController self)
        {
            if (self == null) return;

            var doc = self.GetComponent<UIDocument>();
            if (doc != null && doc.rootVisualElement != null)
            {
                // Show again
                doc.rootVisualElement.style.display = DisplayStyle.Flex;
            }
        }
    }
}
