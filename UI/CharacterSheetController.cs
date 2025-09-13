using UnityEngine;
using UnityEngine.UIElements;

namespace MyGameNamespace
{
    /// <summary>
    /// Minimal controller to show/hide/toggle the Character Sheet UI.
    /// Back-compat: exposes ShowCharacterSheet()/HideCharacterSheet() for legacy callers.
    /// </summary>
    public class CharacterSheetController : MonoBehaviour
    {
        [SerializeField] private UIDocument sheetDocument;
        private VisualElement sheetRoot;
        private VisualElement panel;
        private Button closeBtn;
        private bool initialized;

        private void Awake()
        {
            if (sheetDocument == null) sheetDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            Initialize();
            Hide(); // start hidden
        }

        private void Initialize()
        {
            if (initialized) return;
            if (sheetDocument == null) { Debug.LogWarning("[CharacterSheetController] Missing UIDocument."); return; }

            sheetRoot = sheetDocument.rootVisualElement;
            if (sheetRoot == null) { Debug.LogWarning("[CharacterSheetController] rootVisualElement is null."); return; }

            panel = sheetRoot.Q<VisualElement>("CharacterSheet")
                 ?? sheetRoot.Q<VisualElement>("CharacterSheetPanel")
                 ?? sheetRoot.Q<VisualElement>("character-sheet")
                 ?? sheetRoot;

            closeBtn = sheetRoot.Q<Button>("CharacterSheetClose")
                    ?? sheetRoot.Q<Button>("character-sheet-close")
                    ?? sheetRoot.Q<Button>("CloseButton");

            if (closeBtn != null)
            {
                closeBtn.clicked -= Hide;
                closeBtn.clicked += Hide;
            }

            initialized = true;
        }

        public void Show()
        {
            Initialize();
            if (panel == null) return;
            panel.style.display = DisplayStyle.Flex;
        }

        public void Hide()
        {
            Initialize();
            if (panel == null) return;
            panel.style.display = DisplayStyle.None;
        }

        public void ToggleCharacterSheet()
        {
            Initialize();
            if (panel == null) return;
            panel.style.display = panel.style.display == DisplayStyle.None ? DisplayStyle.Flex : DisplayStyle.None;
        }

        // ---- Backward-compatible aliases ----
        public void ShowCharacterSheet() => Show();
        public void HideCharacterSheet() => Hide();
    }
}