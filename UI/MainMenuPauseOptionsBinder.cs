using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Linq;

namespace MyGameNamespace
{
    /// <summary>
    /// Attaches to the Main Menu UIDocument; ensures "Options" opens the PauseMenu:
    /// - If a PauseMenuController exists in the current scene, shows it immediately (Options tab if available).
    /// - Else schedules opening on the next scene via PauseMenuController.ScheduleOpenOnNextScene().
    /// Works even if the button ID/class is not exact by searching button text.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class MainMenuPauseOptionsBinder : MonoBehaviour
    {
        [SerializeField] private string optionsButtonId = "OptionsButton";
        private UIDocument doc;
        private VisualElement root;

        private void Awake()
        {
            doc = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            root = doc != null ? doc.rootVisualElement : null;
            if (root == null) return;

            var btn = root.Q<Button>(optionsButtonId)
                   ?? root.Q<Button>(className: "options-button")
                   ?? root.Query<Button>().ToList().FirstOrDefault(b => (b.text ?? "").ToLowerInvariant().Contains("option"));

            if (btn != null)
            {
                btn.clicked -= OnOptionsClicked;
                btn.clicked += OnOptionsClicked;
            }
            else
            {
                Debug.Log("[MainMenuPauseOptionsBinder] Options button not found; add id 'OptionsButton' or class 'options-button'.");
            }
        }

        private void OnOptionsClicked()
        {
            var pause = Object.FindFirstObjectByType<PauseMenuController>() ?? Object.FindAnyObjectByType<PauseMenuController>();
            if (pause != null)
            {
                pause.ShowOptions();
                return;
            }
            // Else: open on next scene
            MyGameNamespace.PauseMenuController.ScheduleOpenOnNextScene();
        }
    }
}