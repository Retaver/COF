using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

namespace MyGameNamespace
{
    public class MLPGameUI : MonoBehaviour
    {
        public static MLPGameUI Instance { get; private set; }

        [SerializeField] private UIDocument uiDocument;
        private VisualElement root;
        private Button characterBtn, menuBtn, optionsBtn;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void OnEnable()
        {
            root = uiDocument != null ? uiDocument.rootVisualElement : null;
            if (root == null) { Debug.LogWarning("[MLPGameUI] Missing UIDocument/root."); return; }

            characterBtn = root.Q<Button>("CharacterButton") ?? root.Q<Button>(className: "character-button");
            menuBtn      = root.Q<Button>("MenuButton") ?? root.Q<Button>(className: "menu-button");
            optionsBtn   = root.Q<Button>("OptionsButton") ?? root.Q<Button>(className: "options-button");

            // Heuristic fallback: search any Button whose text includes keywords
            if (characterBtn == null) characterBtn = root.Query<Button>().ToList().FirstOrDefault(b => (b.text ?? "").ToLowerInvariant().Contains("character"));
            if (menuBtn == null)      menuBtn      = root.Query<Button>().ToList().FirstOrDefault(b => (b.text ?? "").ToLowerInvariant().Contains("menu"));
            if (optionsBtn == null)   optionsBtn   = root.Query<Button>().ToList().FirstOrDefault(b => (b.text ?? "").ToLowerInvariant().Contains("option"));

            if (characterBtn != null)
            {
                characterBtn.clicked -= OnCharacter;
                characterBtn.clicked += OnCharacter;
            }
            else Debug.LogWarning("[MLPGameUI] Character button not found. Add id 'CharacterButton' or class 'character-button'.");

            if (menuBtn != null)
            {
                menuBtn.clicked -= OnMenu;
                menuBtn.clicked += OnMenu;
            }
            else Debug.LogWarning("[MLPGameUI] Menu button not found. Add id 'MenuButton' or class 'menu-button'.");

            if (optionsBtn != null)
            {
                optionsBtn.clicked -= OnOptions;
                optionsBtn.clicked += OnOptions;
            }
            else Debug.Log("[MLPGameUI] Options button not found (optional).");
        }

        private void OnCharacter()
        {
            var cs = FindFirstObjectByType<CharacterSheetController>();
            if (cs == null) { Debug.LogWarning("[MLPGameUI] CharacterSheetController not found in scene."); return; }
            cs.ToggleCharacterSheet();
        }

        private void OnMenu()
        {
            var pause = FindFirstObjectByType<PauseMenuController>();
            if (pause == null) { Debug.LogWarning("[MLPGameUI] PauseMenuController not found in scene."); return; }
            pause.ShowPauseMenu();
        }

        private void OnOptions()
        {
            var pause = FindFirstObjectByType<PauseMenuController>();
            if (pause == null) { Debug.LogWarning("[MLPGameUI] PauseMenuController not found in scene."); return; }
            pause.ShowOptions();
        }

        public void ShowAfterCharacterCreation()
        {
            if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
            if (root == null && uiDocument != null) root = uiDocument.rootVisualElement;
            if (root != null) root.style.display = DisplayStyle.Flex;
            gameObject.SetActive(true);
        }

        public static void ShowAfterCharacterCreationStatic() => Instance?.ShowAfterCharacterCreation();
    }
}