using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace MyGameNamespace
{
    [DisallowMultipleComponent]
    public class MainMenuController : MonoBehaviour
    {
        [Header("Optional: assign if the UIDocument is not on this GameObject")]
        [SerializeField] private UIDocument menuDocument;

        [Header("Button element names (UXML)")]
        [SerializeField] private string startButtonId = "start-button";
        [SerializeField] private string continueButtonId = "continue-button";
        [SerializeField] private string exitButtonId = "exit-button";
    // Additional buttons (Load and Options)
    [SerializeField] private string loadButtonId = "load-button";
    [SerializeField] private string optionsButtonId = "options-button";

        private VisualElement _root;
        private Button _startBtn, _continueBtn, _exitBtn;
    private Button _loadBtn, _optionsBtn;

        private void Awake()
        {
            // If CharacterCreationController is visible in Main Menu, hide it
            var creators = FindObjectsByType<CharacterCreationController>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var cc in creators)
            {
                if (cc != null) cc.Hide();
            }
        }

        private void OnEnable()
        {
            Debug.Log("[MainMenuController] OnEnable called");

            var doc = menuDocument;
            if (doc == null) TryGetComponent(out doc);
            if (doc == null) doc = GetComponentInChildren<UIDocument>(true);
            if (doc == null)
            {
                var any = FindObjectsByType<UIDocument>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                if (any.Length > 0) doc = any[0];
            }

            if (doc == null || doc.rootVisualElement == null)
            {
                Debug.LogError("[MainMenuController] UIDocument missing. Assign 'menuDocument' or add a UIDocument here.");
                enabled = false;
                return;
            }

            _root = doc.rootVisualElement;
            _root.style.display = DisplayStyle.Flex;

            // Find buttons by id, with sensible fallbacks
            _startBtn = _root.Q<Button>(startButtonId)
                     ?? _root.Q<Button>("StartButton")
                     ?? FindButtonByText("start")
                     ?? FindButtonByText("new game");

            _continueBtn = _root.Q<Button>(continueButtonId)
                        ?? _root.Q<Button>("ContinueButton")
                        ?? FindButtonByText("continue");

            _exitBtn = _root.Q<Button>(exitButtonId)
                     ?? _root.Q<Button>("ExitButton")
                     ?? FindButtonByText("exit")
                     ?? FindButtonByText("quit");

        // Additional buttons: Load and Options
        _loadBtn = _root.Q<Button>(loadButtonId)
                   ?? _root.Q<Button>("LoadButton")
                   ?? FindButtonByText("load");

        _optionsBtn = _root.Q<Button>(optionsButtonId)
                      ?? _root.Q<Button>("OptionsButton")
                      ?? FindButtonByText("options");

            // Wire events (only if found)
            if (_startBtn != null) { _startBtn.clicked -= OnNewGame; _startBtn.clicked += OnNewGame; }
            if (_continueBtn != null) { _continueBtn.clicked -= OnContinue; _continueBtn.clicked += OnContinue; }
            if (_exitBtn != null) { _exitBtn.clicked -= OnExit; _exitBtn.clicked += OnExit; }

        if (_loadBtn != null) { _loadBtn.clicked -= OnLoad; _loadBtn.clicked += OnLoad; }
        if (_optionsBtn != null) { _optionsBtn.clicked -= OnOptions; _optionsBtn.clicked += OnOptions; }

            Debug.Log($"[MainMenuController] Buttons found - Start: {_startBtn != null}, Continue: {_continueBtn != null}, Exit: {_exitBtn != null}");
        }

        private void OnDisable()
        {
            if (_startBtn != null) _startBtn.clicked -= OnNewGame;
            if (_continueBtn != null) _continueBtn.clicked -= OnContinue;
            if (_exitBtn != null) _exitBtn.clicked -= OnExit;

        if (_loadBtn != null) _loadBtn.clicked -= OnLoad;
        if (_optionsBtn != null) _optionsBtn.clicked -= OnOptions;
        }

        private Button FindButtonByText(string containsLower)
        {
            if (_root == null) return null;
            var needle = (containsLower ?? "").ToLowerInvariant();
            return _root.Query<Button>().ToList().FirstOrDefault(b =>
                b != null && !string.IsNullOrEmpty(b.text) &&
                b.text.ToLowerInvariant().Contains(needle));
        }

        private void OnNewGame()
        {
            Debug.Log("[MainMenuController] New Game clicked -> Character Creation");
            var gsm = GameObject.FindAnyObjectByType<GameSceneManager>(FindObjectsInactive.Include);
            if (gsm != null)
            {
                gsm.ShowCharacterCreate();
                return;
            }
            // Fallback: load by scene name (ensure in Build Settings)
            SceneManager.LoadScene("Charactercreation", LoadSceneMode.Single);
        }

        private void OnContinue()
        {
            Debug.Log("[MainMenuController] Continue clicked -> Game");
            var gsm = GameObject.FindAnyObjectByType<GameSceneManager>(FindObjectsInactive.Include);
            if (gsm != null)
            {
                gsm.ShowGame();
                return;
            }
            SceneManager.LoadScene("Game", LoadSceneMode.Single);
        }

        private void OnExit()
        {
            Debug.Log("[MainMenuController] Exit clicked");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>
        /// Called when the "Load" button is clicked. Schedules the pause menu to
        /// open once the Game scene is loaded, then transitions to the Game
        /// scene. This allows players to load or save from the same pause menu
        /// after leaving the Main Menu.
        /// </summary>
        private void OnLoad()
        {
            Debug.Log("[MainMenuController] Load clicked -> Game with pause menu");
            // Tell the pause menu to open on the next scene load
            global::PauseMenuController.ScheduleOpenOnNextScene();
            // Transition to the Game scene via GameSceneManager or directly
            var gsm = GameObject.FindAnyObjectByType<GameSceneManager>(FindObjectsInactive.Include);
            if (gsm != null)
            {
                gsm.ShowGame();
            }
            else
            {
                SceneManager.LoadScene("Game", LoadSceneMode.Single);
            }
        }

        /// <summary>
        /// Called when the "Options" button is clicked. By default this
        /// opens the pause menu directly within the Main Menu scene so
        /// players can access settings without loading into the Game scene.
        /// Requires a PauseMenuController + UIDocument in the Main Menu scene.
        /// </summary>
        private void OnOptions()
        {
            Debug.Log("[MainMenuController] Options clicked -> Pause menu");
            // Open the pause menu directly on the main menu. This will
            // auto-find any PauseMenuController in the current scene and
            // display it without changing scenes. Ensure a pause menu
            // exists in your Main Menu scene for this to work.
            global::PauseMenuController.OpenPauseMenu();
        }
    }
}
