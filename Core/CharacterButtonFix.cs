// Forces the HUD "Character" button to open/close the Character Sheet,
// and prevents any old listeners from opening Character Creation.

using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class CharacterButtonFix : MonoBehaviour
{
    [Header("Which HUD button?")]
    [Tooltip("Exact name (id) of the Character button in your HUD UXML.")]
    // Default ID for the Character button in the HUD.  In the current
    // MLPGameUI UXML the button is named "CharacterButton", so use that
    // instead of the legacy "character-button".  You can override this in
    // the Inspector if your UI uses a different name.
    [SerializeField] private string buttonId = "CharacterButton";

    [Tooltip("Fallback: match by visible text if id not found (case-insensitive).")]
    [SerializeField] private string textContains = "character";

    [Header("Optional Hotkey")]
    [SerializeField] private bool enableHotkey = true;
    [SerializeField] private KeyCode hotkey = KeyCode.C;

    private Button hudButton;

    private void OnEnable()
    {
        // The UIController now manages the Character button click and toggles
        // the character sheet.  To avoid duplicate toggles, this component
        // no longer registers its own click callback.  We still cache the
        // button in case other systems reference it, but we deliberately
        // skip registering any handlers here.
        var doc = GetComponent<UIDocument>();
        var root = doc ? doc.rootVisualElement : null;
        if (root == null) return;

        // Prefer exact id
        hudButton = string.IsNullOrEmpty(buttonId) ? null : root.Q<Button>(buttonId);

        // Fallback by visible text
        if (hudButton == null)
        {
            hudButton = root.Query<Button>().ToList()
                .FirstOrDefault(b => !string.IsNullOrEmpty(b.text) &&
                                     b.text.ToLowerInvariant().Contains(textContains.ToLowerInvariant()));
        }

        // Do not register any click handlers in this component.  All click
        // handling is now performed by UIController.  If hudButton is null,
        // log a warning so the developer can adjust the button ID.
        if (hudButton == null)
        {
            Debug.LogWarning("[CharacterButtonFix] Could not find HUD Character button. Set 'buttonId' to your HUD button name.");
        }
    }

    private void Update()
    {
        if (enableHotkey && Input.GetKeyDown(hotkey))
            ToggleCharacterSheet();
    }

    private void ToggleCharacterSheet()
    {
        // Do nothing while in CharacterCreation scene
        if (SceneManager.GetActiveScene().name == "CharacterCreation")
            return;

        var sheet = UnityEngine.Object.FindAnyObjectByType<CharacterSheetController>(FindObjectsInactive.Include);
        if (sheet == null) return;

        var doc = sheet.GetComponent<UIDocument>();
        var root = doc ? doc.rootVisualElement : null;
        if (root == null) return;

        root.style.display = (root.resolvedStyle.display == DisplayStyle.None)
            ? DisplayStyle.Flex
            : DisplayStyle.None;

        var m = typeof(CharacterSheetController).GetMethod("RefreshUI",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        m?.Invoke(sheet, null);
    }
}
