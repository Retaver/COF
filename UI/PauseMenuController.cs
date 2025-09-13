using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

namespace MyGameNamespace
{
    public class PauseMenuController : MonoBehaviour
    {
        [SerializeField] private UIDocument pauseDocument;
        private VisualElement root, panel;
        private Button closeBtn;

        private static bool _openOnNextSceneScheduled;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _openOnNextSceneScheduled = false;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (_openOnNextSceneScheduled)
            {
                _openOnNextSceneScheduled = false;
                var inst = FindFirstInstance();
                if (inst != null) inst.ShowPauseMenu();
            }
        }

        /// <summary>Schedules the pause menu to open automatically when the next scene loads.</summary>
        public static void ScheduleOpenOnNextScene()
        {
            _openOnNextSceneScheduled = true;
        }

        /// <summary>Static helper so legacy callers can do PauseMenuController.OpenPauseMenu().</summary>
        public static void OpenPauseMenu()
        {
            var inst = FindFirstInstance();
            if (inst != null) inst.ShowPauseMenu();
            else Debug.LogWarning("[PauseMenuController] No instance found to open pause menu.");
        }

        private static PauseMenuController FindFirstInstance()
        {
            var inst = Object.FindFirstObjectByType<PauseMenuController>();
            if (inst == null) inst = Object.FindAnyObjectByType<PauseMenuController>();
            return inst;
        }

        private void Awake()
        {
            if (pauseDocument == null) pauseDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            root = pauseDocument != null ? pauseDocument.rootVisualElement : null;
            if (root == null) { Debug.LogWarning("[PauseMenuController] Missing UIDocument/root."); return; }
            panel = root.Q<VisualElement>("PauseMenu") ?? root.Q<VisualElement>("PauseMenuRoot") ?? root;

            closeBtn = root.Q<Button>("PauseClose") ?? root.Q<Button>("pause-close") ?? root.Q<Button>("CloseButton");
            if (closeBtn != null)
            {
                closeBtn.clicked -= HidePauseMenu;
                closeBtn.clicked += HidePauseMenu;
            }
            HidePauseMenu();
        }

        public void ShowPauseMenu()
        {
            if (panel == null) return;
            panel.style.display = DisplayStyle.Flex;
        }

        public void HidePauseMenu()
        {
            if (panel == null) return;
            panel.style.display = DisplayStyle.None;
        }

        public void ShowOptions()
        {
            ShowPauseMenu();
            var optionsTab = root.Q<Button>("OptionsTab") ?? root.Q<Button>("options-tab");
            optionsTab?.Focus();
        }

        // Instance alias kept distinct to avoid signature clash.
        public void OpenPauseMenuInstance() => ShowPauseMenu();
    }
}