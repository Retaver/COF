using UnityEngine;
using UnityEngine.UIElements;
using MyGameNamespace;

namespace MyGameNamespace
{
    /// <summary>
    /// Forces the MLPGameUI UXML + USS to be applied at runtime and
    /// repairs layout if some containers ended up at the root.
    /// Put this on the same GameObject that already has the UIDocument for the Game HUD.
    /// Assign the VisualTreeAsset (MLPGameUI.uxml) and StyleSheet (MLPGameUI.uss) in the Inspector.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public sealed class MLPGameUIBootstrap : MonoBehaviour
    {
        [Header("Assign assets (UXML + USS)")]
        [SerializeField] private VisualTreeAsset gameHudUxml;
        [SerializeField] private StyleSheet gameHudStyles;

        [Header("Character Sheet Resources")]
        [SerializeField] private VisualTreeAsset characterSheetUxml;
        [SerializeField] private StyleSheet characterSheetStyles;

        private UIDocument doc;

        private void Awake()
        {
            doc = GetComponent<UIDocument>();
            if (doc == null)
            {
                Debug.LogError("[MLPGameUIBootstrap] No UIDocument found on this GameObject.");
                enabled = false;
                return;
            }

            // Make sure the right UXML is bound.
            if (gameHudUxml != null && doc.visualTreeAsset != gameHudUxml)
                doc.visualTreeAsset = gameHudUxml;

            // Make sure our USS is applied once.
            if (gameHudStyles != null && !doc.rootVisualElement.styleSheets.Contains(gameHudStyles))
                doc.rootVisualElement.styleSheets.Add(gameHudStyles);

            // Apply theme classes on root.
            var root = doc.rootVisualElement;
            root.EnableInClassList("gamehud-root", true);
            root.EnableInClassList("theme--twilight", true);
            root.EnableInClassList("font--ui", true);

        // Ensure a MapController exists on this GameObject so the integrated
        // map panel is populated at runtime.  If one is already present,
        // do nothing.  Otherwise add a new MapController and assign our
        // UIDocument.  The [RequireComponent] attribute on MapController
        // does not automatically add the UIDocument, so we set it here.
        var existingMap = GetComponent<MapController>();
        if (existingMap == null)
        {
            try
            {
                var mc = gameObject.AddComponent<MapController>();
                mc.uiDocument = doc;
            }
            catch
            {
                // Catch exceptions in case MapController cannot be added
                Debug.LogWarning("[MLPGameUIBootstrap] Unable to add MapController. Interactive map will not function.");
            }
        }

        // Previously this component added a CharacterButtonFix to intercept the
        // bottom Character button and toggle the sheet.  The UIController now
        // handles toggling directly, so no additional component is required.

        // Attempt to load character sheet assets from Resources if they
        // were not assigned via the Inspector.  Assets must reside in
        // Assets/Resources/UI/CharacterSheet.uxml and .uss to be found.
        if (characterSheetUxml == null)
        {
            characterSheetUxml = Resources.Load<VisualTreeAsset>("UI/CharacterSheet");
        }
        if (characterSheetStyles == null)
        {
            characterSheetStyles = Resources.Load<StyleSheet>("UI/CharacterSheet");
        }

        // At runtime, create a CharacterSheetController if none exists.
        // This allows the character sheet to function in the Game scene even
        // when no CharacterSheetController is present in the hierarchy.  It
        // instantiates a new GameObject with a UIDocument and assigns the
        // provided UXML and StyleSheet.  The existing panelSettings are
        // reused so the character sheet appears on the same UI layer.  If
        // the assets are not assigned in the inspector, this step is skipped.
        try
        {
            var existingSheet = FindFirstObjectByType<CharacterSheetController>();
            if (existingSheet == null && characterSheetUxml != null)
            {
                var sheetGo = new GameObject("CharacterSheetUI");
                var sheetDoc = sheetGo.AddComponent<UIDocument>();
                sheetDoc.visualTreeAsset = characterSheetUxml;
                // Use the same panel settings as the main HUD to ensure
                // consistent layering.  If no panel settings are found, the
                // UIDocument will create its own.
                sheetDoc.panelSettings = doc.panelSettings;
                // Add styles if provided
                if (characterSheetStyles != null)
                {
                    sheetDoc.rootVisualElement.styleSheets.Add(characterSheetStyles);
                }
                // Add the controller; it will find the UIDocument itself in Awake
                sheetGo.AddComponent<CharacterSheetController>();
                // Initially hide the sheet
                sheetDoc.rootVisualElement.style.display = DisplayStyle.None;
            }
        }
        catch
        {
            Debug.LogWarning("[MLPGameUIBootstrap] Failed to bootstrap CharacterSheetController.");
        }
        }

        private void OnEnable()
        {
            // Delay one frame so UI Toolkit finishes cloning the tree.
            doc?.rootVisualElement?.schedule.Execute(RepairLayout).StartingIn(0);
        }

        private void RepairLayout()
        {
            var root = doc.rootVisualElement;
            if (root == null) return;

            // Top row = horizontal
            var topRow = root.Q<VisualElement>(className: "top-row");
            if (topRow != null)
            {
                topRow.style.flexDirection = FlexDirection.Row;
                topRow.style.flexGrow = 1f;
                topRow.style.minHeight = 0f;
            }

            // Left pane = fixed width, no stretch
            var playerPane = root.Q<VisualElement>(className: "player-pane");
            if (playerPane != null)
            {
                playerPane.style.width = 360;        // px
                playerPane.style.flexGrow = 0f;
                playerPane.style.flexShrink = 0f;
                playerPane.style.alignSelf = Align.FlexStart;
            }

            // Center pane = fills remaining space
            var centerPane = root.Q<VisualElement>(className: "center-pane");
            if (centerPane != null)
            {
                centerPane.style.flexGrow = 1f;
                centerPane.style.minWidth = 0f;
            }

            // Bars are defined in the UXML underneath their respective stat-item
            // containers. Unity 6.2 sometimes places them correctly already.  Do
            // not forcibly re-parent them into statsPanel here, as that breaks
            // the vertical layout of each stat item.  Instead, simply ensure
            // they have the appropriate CSS classes.  If a stat bar is found
            // somewhere else (e.g. root), the FixBar method will not change
            // its parent but will still apply classes.
            var statsPanel = root.Q<VisualElement>(className: "stats-panel");
            if (statsPanel != null)
            {
                FixBar(root, statsPanel, "HealthBar", "health-bar");
                FixBar(root, statsPanel, "EnergyBar", "energy-bar");
                FixBar(root, statsPanel, "ManaBar", "mana-bar");
                FixBar(root, statsPanel, "HarmonyBar", "harmony-bar");
                FixBar(root, statsPanel, "DiscordBar", "discord-bar");
            }

            // Bottom nav alignment (just in case)
            var bottomNav = root.Q<VisualElement>(className: "bottom-nav");
            if (bottomNav != null)
            {
                bottomNav.style.flexDirection = FlexDirection.Row;
                bottomNav.style.justifyContent = Justify.Center;
                bottomNav.style.alignItems = Align.Center;
            }

            Debug.Log("[MLPGameUIBootstrap] Layout repaired (twilight theme).");
        }

        private static void FixBar(VisualElement root, VisualElement targetParent, string barName, string colorClass)
        {
            var bar = root.Q<ProgressBar>(barName);
            if (bar == null) return;

            // Do not re-parent bars away from their stat-item container.  Reparenting
            // into the stats panel breaks the desired layout (label over bar).  Only
            // move the bar if it has no parent (e.g. created dynamically) or its
            // parent is null.  Otherwise leave it where it is.
            if (bar.parent == null)
            {
                targetParent.Add(bar);
            }

            // Ensure styling classes exist.  Adding a class twice has no effect.
            bar.AddToClassList("stat-bar");
            bar.AddToClassList(colorClass);
        }
    }
}
