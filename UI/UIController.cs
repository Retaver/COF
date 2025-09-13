// UIController.cs
// Updated to reliably wire bottom-bar buttons ("Character", "Items", "Menu") at runtime
// without adding new files. Brings HUD root to front and sets pickingMode to allow clicks.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using MyGameNamespace;

public class UIController : MonoBehaviour
{
    [Header("UI Document")]
    [SerializeField] private UIDocument gameUIDocument;

    [Header("Portrait Resources")]
    [SerializeField] private Texture2D earthPonyPortrait;
    [SerializeField] private Texture2D unicornPortrait;
    [SerializeField] private Texture2D pegasusPortrait;
    [SerializeField] private Texture2D batPonyPortrait;
    [SerializeField] private Texture2D griffonPortrait;
    [SerializeField] private Texture2D dragonPortrait;
    [SerializeField] private Texture2D humanPortrait;

    [Header("Visual Settings")]
    [SerializeField] private int headerFontSize = 22;
    [SerializeField] private int storyFontSize = 18;
    [SerializeField] private int choiceFontSize = 16;
    [SerializeField] private float barAnimationDuration = 0.6f;
    [SerializeField] private bool enableBarPulse = true;
    [SerializeField] private bool enableColorTransitions = true;

    [Header("Bar Colors")]
    [SerializeField] private Color healthColor = new Color(0.86f, 0.2f, 0.2f, 1f);
    [SerializeField] private Color healthLowColor = new Color(0.7f, 0.12f, 0.12f, 1f);
    [SerializeField] private Color energyColor = new Color(0.2f, 0.59f, 0.86f, 1f);
    [SerializeField] private Color magicColor = new Color(0.59f, 0.2f, 0.86f, 1f);
    [SerializeField] private Color friendshipColor = new Color(0.86f, 0.59f, 0.2f, 1f);
    [SerializeField] private Color discordColor = new Color(0.47f, 0.2f, 0.59f, 1f);
    [SerializeField] private Color discordHighColor = new Color(0.31f, 0.12f, 0.39f, 1f);

    [Header("Debug")]
    [SerializeField] private bool verboseLogging = true;

    // Core state
    private VisualElement root;
    private bool isInitialized = false;
    private PlayerCharacter currentPlayer;

    // UI Element Cache
    private Image characterPortrait;
    private Label characterName;
    private Label raceLabel;
    private Label levelLabel;
    private Label bitsValue;

    // Stats
    private Label healthValue;
    private Label staminaValue;
    private Label manaValue;
    private Label harmonyValue;
    private Label discordValue;
    private ProgressBar healthBar;
    private ProgressBar staminaBar;
    private ProgressBar manaBar;
    private ProgressBar harmonyBar;
    private ProgressBar discordBar;

    // Attributes
    private Label strengthValue;
    private Label dexterityValue;
    private Label constitutionValue;
    private Label intelligenceValue;
    private Label wisdomValue;
    private Label charismaValue;

    // Story Display
    private Label storyTitleLabel;
    private Label storyText;
    private Image storyImage;
    // Container for rich story content (inline images and formatted text)
    private VisualElement storyBody;
    private VisualElement choicesPanel;

    // Choice System
    private readonly List<Button> choiceButtons = new();
    private readonly string[] hotkeyLabels = { "[1]", "[2]", "[3]", "[4]", "[5]", "[6]", "[7]", "[8]" };

    // Animation System
    private readonly Dictionary<string, BarAnimationData> barAnimations = new();
    private Coroutine pulseCoroutine;
    private AnimationCurve barAnimationCurve;

    // Bottom bar buttons (wired at runtime)
    private Button bottomCharacterButton;
    private Button bottomItemsButton;
    private Button bottomMenuButton;

    // New map button (wired at runtime)
    private Button bottomMapButton;

    private class BarAnimationData
    {
        public ProgressBar bar;
        public VisualElement fill;
        public float currentValue;
        public float targetValue;
        public float maxValue;
        public Color normalColor;
        public Color lowColor;
        public Color highColor;
        public bool isAnimating;
        public Coroutine animationCoroutine;
        public float lowThreshold = 0.25f;
        public float highThreshold = 0.75f;
        public string cssClass;
    }

    #region Unity Lifecycle

    private void Awake()
    {
        if (barAnimationCurve == null)
            barAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        // Use a coroutine-based initialization to avoid threading issues with Unity APIs.
        StartCoroutine(InitializeCoroutine());
    }

    private void OnEnable()
    {
        if (!isInitialized)
            StartCoroutine(InitializeCoroutine());
    }

    #endregion

    #region Initialization

    public void Initialize()
    {
        if (!isInitialized)
            StartCoroutine(InitializeCoroutine());
    }

    private IEnumerator InitializeCoroutine()
    {
        if (isInitialized) yield break;

        if (!SetupUIDocument())
        {
            Debug.LogWarning("UIController: UIDocument not ready. Will retry next frame.");
            yield return null;
            if (!SetupUIDocument())
            {
                Debug.LogError("UIController: No UIDocument found. Initialization aborted.");
                yield break;
            }
        }

        // Ensure this root accepts pointer events and is front-most to avoid invisible overlays blocking bottom buttons
        try
        {
            root.pickingMode = PickingMode.Position;
            root.BringToFront();
        }
        catch { /* bring to front/pickingMode might not be critical on some Unity versions */ }

        CacheUIElements();
        SetupAnimatedBars();
        SetupChoiceButtons();
        SetupHotkeys();

        // Bring the bottom navigation container to the front and ensure it can
        // receive pointer events. Without this, other UI layers (e.g. story panel)
        // may sit above the bottom bar and intercept clicks, causing the Menu
        // button to appear non‑functional in the Game scene.
        try
        {
            var bottomNav = root.Q<VisualElement>("BottomNav");
            if (bottomNav != null)
            {
                bottomNav.BringToFront();
                bottomNav.pickingMode = PickingMode.Position;
            }
        }
        catch
        {
            // Some runtime profiles may not support BringToFront; ignore errors
        }

        // Wait for GameEventSystem to exist and then subscribe (avoid blocking)
        yield return StartCoroutine(WaitForGameEventSystemAndSubscribe());

        // Wire bottom-bar buttons (Character / Items / Menu)
        WireBottomBarButtons();

        ApplyUIStyles();

        isInitialized = true;
        if (verboseLogging) Debug.Log("UIController: Initialized successfully");

        // Update with current player if available
        var gm = GameManager.Instance;
        PlayerCharacter player = null;
        if (gm != null)
        {
            player = gm.GetPlayer();
        }
        // Fallback: use PlayerState.Current if GameManager has no player
        if (player == null)
        {
            player = MyGameNamespace.PlayerState.Current;
        }
        if (player != null)
        {
            // Ensure the player's stats are fully restored at the start of the game.
            // This prevents the UI from showing low or leftover health values from a
            // previous play session and gives players a fair starting point.
            if (player.gameStats != null)
            {
                player.gameStats.RestoreAll();
            }
            UpdatePlayerUI(player);
        }
    }

    private bool SetupUIDocument()
    {
        if (gameUIDocument == null)
        {
            gameUIDocument = GetComponent<UIDocument>();
            if (gameUIDocument == null)
            {
                Debug.LogError("UIController: No UIDocument found!");
                return false;
            }
        }

        root = gameUIDocument.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("UIController: No root visual element!");
            return false;
        }

        return true;
    }

    private void CacheUIElements()
    {
        // Character info
        characterPortrait = root.Q<Image>("CharacterPortrait");
        characterName = root.Q<Label>("CharacterName");
        raceLabel = root.Q<Label>("RaceLabel");
        levelLabel = root.Q<Label>("LevelLabel");
        bitsValue = root.Q<Label>("BitsValue");

        // Stats values
        healthValue = root.Q<Label>("HealthValue");
        staminaValue = root.Q<Label>("StaminaValue");
        manaValue = root.Q<Label>("ManaValue");
        harmonyValue = root.Q<Label>("HarmonyValue");
        discordValue = root.Q<Label>("DiscordValue");

        // Stats bars
        healthBar = root.Q<ProgressBar>("HealthBar");
        staminaBar = root.Q<ProgressBar>("StaminaBar");
        manaBar = root.Q<ProgressBar>("ManaBar");
        harmonyBar = root.Q<ProgressBar>("HarmonyBar");
        discordBar = root.Q<ProgressBar>("DiscordBar");

        // Attributes
        strengthValue = root.Q<Label>("StrengthValue");
        dexterityValue = root.Q<Label>("DexterityValue");
        constitutionValue = root.Q<Label>("ConstitutionValue");
        intelligenceValue = root.Q<Label>("IntelligenceValue");
        wisdomValue = root.Q<Label>("WisdomValue");
        charismaValue = root.Q<Label>("CharismaValue");

        // Story elements
        storyTitleLabel = root.Q<Label>("StoryTitle");
        // Optional legacy elements
        storyText = root.Q<Label>("StoryText");
        storyImage = root.Q<Image>("StoryImage");
        // Rich story body container (newer dynamic content)
        storyBody = root.Q<VisualElement>("StoryBody");
        choicesPanel = root.Q<VisualElement>("ChoicesPanel");

        if (verboseLogging)
        {
            Debug.Log($"UIController: Cached elements - Portrait: {characterPortrait != null}, Name: {characterName != null}, Choices: {choicesPanel != null}");
        }
    }

    #endregion

    #region Bottom-bar wiring (fix for nonresponsive bottom buttons)

    private void WireBottomBarButtons()
    {
        // Attempt 1: look for well-known IDs inside this UIDocument root
        bottomCharacterButton = FindButtonInRootByNames(new[] { "character-button", "Character", "character", "CharacterButton" });
        bottomItemsButton    = FindButtonInRootByNames(new[] { "items-button", "inventory-button", "items", "inventory", "Item", "Items", "InventoryButton" });
        bottomMenuButton    = FindButtonInRootByNames(new[] { "menu-button", "menu", "pause-button", "game-menu", "Menu", "MenuButton" });
        bottomMapButton     = FindButtonInRootByNames(new[] { "map-button", "map", "Map", "MapButton" });

        // Attempt 2: if any not found in this doc, search across all UIDocuments in the scene
        if (bottomCharacterButton == null || bottomItemsButton == null || bottomMenuButton == null)
        {
            var docs = CompatUtils.FindObjectsOfTypeCompat<UIDocument>();
            foreach (var d in docs)
            {
                if (d == null || d.rootVisualElement == null) continue;
                // skip our own doc to avoid re-checking
                if (d == gameUIDocument) continue;

                if (bottomCharacterButton == null)
                    bottomCharacterButton = FindButtonInDocumentByText(d, "character");

                if (bottomItemsButton == null)
                    bottomItemsButton = FindButtonInDocumentByText(d, "item") ?? FindButtonInDocumentByText(d, "inventory");

                if (bottomMenuButton == null)
                    bottomMenuButton = FindButtonInDocumentByText(d, "menu") ?? FindButtonInDocumentByText(d, "pause");

                if (bottomCharacterButton != null && bottomItemsButton != null && bottomMenuButton != null)
                    break;
            }
        }

        // Wire handlers
        if (bottomCharacterButton != null)
        {
            bottomCharacterButton.clicked -= OnBottomCharacterClicked;
            bottomCharacterButton.clicked += OnBottomCharacterClicked;
            if (verboseLogging) Debug.Log("[UIController] Wired Character bottom button.");
        }
        else if (verboseLogging) Debug.LogWarning("[UIController] Character bottom button not found.");

        if (bottomItemsButton != null)
        {
            bottomItemsButton.clicked -= OnBottomItemsClicked;
            bottomItemsButton.clicked += OnBottomItemsClicked;
            if (verboseLogging) Debug.Log("[UIController] Wired Items bottom button.");
        }
        else if (verboseLogging) Debug.LogWarning("[UIController] Items bottom button not found.");

        if (bottomMenuButton != null)
        {
            bottomMenuButton.clicked -= OnBottomMenuClicked;
            bottomMenuButton.clicked += OnBottomMenuClicked;
            if (verboseLogging) Debug.Log("[UIController] Wired Menu bottom button.");
        }
        else if (verboseLogging) Debug.LogWarning("[UIController] Menu bottom button not found.");

        if (bottomMapButton != null)
        {
            bottomMapButton.clicked -= OnBottomMapClicked;
            bottomMapButton.clicked += OnBottomMapClicked;
            if (verboseLogging) Debug.Log("[UIController] Wired Map bottom button.");
        }
        else if (verboseLogging) Debug.LogWarning("[UIController] Map bottom button not found.");
    }

    private void OnBottomMapClicked()
    {
        if (verboseLogging) Debug.Log("[UIController] Bottom Map clicked");
        // In a complete implementation this would show or hide the map UI.
        // You might load a new scene, toggle a panel or instruct the
        // MapController to become visible.  For now we simply log the click.
    }

    private Button FindButtonInRootByNames(string[] nameOrTextCandidates)
    {
        if (root == null) return null;

        // First try by element name (ID)
        foreach (var candidate in nameOrTextCandidates)
        {
            var b = root.Q<Button>(candidate);
            if (b != null) return b;
        }

        // Next try by text/content
        var allButtons = root.Query<Button>().ToList();
        foreach (var candidate in nameOrTextCandidates)
        {
            var lower = candidate.ToLowerInvariant();
            var byText = allButtons.FirstOrDefault(btn => (!string.IsNullOrEmpty(btn.text) && btn.text.ToLowerInvariant().Contains(lower)));
            if (byText != null) return byText;
        }

        return null;
    }

    private Button FindButtonInDocumentByText(UIDocument doc, string partialText)
    {
        if (doc == null || doc.rootVisualElement == null) return null;
        var buttons = doc.rootVisualElement.Query<Button>().ToList();
        var lower = partialText.ToLowerInvariant();
        return buttons.FirstOrDefault(b => !string.IsNullOrEmpty(b.text) && b.text.ToLowerInvariant().Contains(lower));
    }

    private void OnBottomCharacterClicked()
    {
        if (verboseLogging) Debug.Log("[UIController] Bottom Character clicked");
        OpenCharacterSheet();
    }

    private void OnBottomItemsClicked()
    {
        if (verboseLogging) Debug.Log("[UIController] Bottom Items clicked");
        OpenInventoryScreen();
    }

    private void OnBottomMenuClicked()
    {
        if (verboseLogging) Debug.Log("[UIController] Bottom Menu clicked");
        OpenPauseMenu();
    }

    private void OpenCharacterSheet()
    {
        // Try to find a CharacterSheetController first
        // Prefer to use the CharacterSheetController if it exists
        var charController = CompatUtils.FindFirstObjectByTypeCompat<CharacterSheetController>();
        if (charController != null)
        {
            // Toggle the character sheet rather than always showing it.  This allows
            // the same button to open and close the sheet, matching expectations for
            // Trials in Tainted Space–style interfaces.  If the sheet is currently
            // hidden it will be shown, and if it is visible it will be hidden again.
            charController.ToggleCharacterSheet();
            if (verboseLogging) Debug.Log("[UIController] Toggled Character Sheet via CharacterSheetController.");
            return;
        }

        // Fallback: find any UIDocument that contains a sheet container and toggle it on
        var docs = CompatUtils.FindObjectsOfTypeCompat<UIDocument>();
        foreach (var d in docs)
        {
            if (d == null || d.rootVisualElement == null) continue;
            if (d.rootVisualElement.Q<VisualElement>("character-sheet-container") != null ||
                d.rootVisualElement.Q<VisualElement>("character-sheet-modal") != null)
            {
                var rootVE = d.rootVisualElement.Q<VisualElement>("character-sheet-modal") ?? d.rootVisualElement.Q<VisualElement>("character-sheet-container");
                if (rootVE != null)
                {
                    bool isHidden = rootVE.resolvedStyle.display == DisplayStyle.None;
                    rootVE.style.display = isHidden ? DisplayStyle.Flex : DisplayStyle.None;
                    d.rootVisualElement.BringToFront();
                    if (verboseLogging) Debug.Log("[UIController] Toggled Character Sheet via UIDocument.");
                    return;
                }
            }
        }

        if (verboseLogging) Debug.LogWarning("[UIController] Character sheet UIDocument not found.");
    }

    private void OpenInventoryScreen()
    {
        // Prefer calling InventoryScreenController.Show() if present
        var invCtrl = CompatUtils.FindFirstObjectByTypeCompat<InventoryScreenController>();
        if (invCtrl != null)
        {
            invCtrl.Show();
            if (verboseLogging) Debug.Log("[UIController] Inventory opened via InventoryScreenController.Show()");
            return;
        }

        // Fallback: find UIDocument with inventory-container and show it
        var docs = CompatUtils.FindObjectsOfTypeCompat<UIDocument>();
        foreach (var d in docs)
        {
            if (d == null || d.rootVisualElement == null) continue;
            var invContainer = d.rootVisualElement.Q<VisualElement>("inventory-container") ?? d.rootVisualElement.Q<Label>("inventory-title");
            if (invContainer != null)
            {
                d.rootVisualElement.style.display = DisplayStyle.Flex;
                invContainer.style.display = DisplayStyle.Flex;
                d.rootVisualElement.BringToFront();
                if (verboseLogging) Debug.Log("[UIController] Inventory opened via UIDocument.");
                return;
            }
        }

        if (verboseLogging) Debug.LogWarning("[UIController] Inventory UI not found.");
    }

    private void OpenPauseMenu()
    {
        // Use the static PauseMenuController helper to open the pause menu. This
        // method automatically finds the pause menu in the current scene and
        // opens it. If no pause menu exists, it will log a warning.
        PauseMenuController.OpenPauseMenu();
        // If a pause menu is present, the above call will handle showing it.
        // We intentionally do not fall back to manipulating the pause-menu
        // VisualElement manually because PauseMenuController encapsulates all
        // related logic (time scale, panel switching, etc.).
    }

    #endregion

    #region Player UI Updates

    public void UpdatePlayerUI(PlayerCharacter player)
    {
        if (player == null) return;
        if (!isInitialized)
        {
            // If not initialized yet, cache for later update
            currentPlayer = player;
            return;
        }

        currentPlayer = player;
        UpdateCharacterInfo();
        UpdateStats();
        UpdateAttributes();
        UpdatePortrait();

        if (verboseLogging) Debug.Log($"UIController: Updated UI for {player.name}");
    }

    public void UpdateInventoryUI(List<InventoryItem> items)
    {
        // This UIController doesn't handle inventory display - that's handled by InventoryScreenController
        if (verboseLogging) Debug.Log($"UIController: Inventory update notification ({items?.Count ?? 0} items)");
    }

    private void UpdateCharacterInfo()
    {
        if (currentPlayer == null) return;

        if (characterName != null) characterName.text = currentPlayer.name ?? "Unnamed";
        if (raceLabel != null) raceLabel.text = $"Race: {GetDisplayRaceName(currentPlayer.race)}";
        if (levelLabel != null) levelLabel.text = $"Level {currentPlayer.level}";
        if (bitsValue != null) bitsValue.text = currentPlayer.bits.ToString();
    }

    private void UpdateStats()
    {
        if (currentPlayer?.gameStats == null) return;

        var stats = currentPlayer.gameStats;

        // Update text values
        if (healthValue != null) healthValue.text = $"{stats.health}/{stats.maxHealth}";
        if (staminaValue != null) staminaValue.text = $"{stats.energy}/{stats.maxEnergy}";
        if (manaValue != null) manaValue.text = $"{stats.magic}/{stats.maxMagic}";
        if (harmonyValue != null) harmonyValue.text = $"{stats.friendship}/{stats.maxFriendship}";
        if (discordValue != null) discordValue.text = $"{stats.corruption}/{stats.maxCorruption}";

        // Animate bars
        AnimateBarToValue("health", stats.health, stats.maxHealth);
        AnimateBarToValue("energy", stats.energy, stats.maxEnergy);
        AnimateBarToValue("magic", stats.magic, stats.maxMagic);
        AnimateBarToValue("friendship", stats.friendship, stats.maxFriendship);
        AnimateBarToValue("discord", stats.corruption, stats.maxCorruption);
    }

    private void UpdateAttributes()
    {
        if (currentPlayer?.stats == null) return;

        var s = currentPlayer.stats;

        if (strengthValue != null) strengthValue.text = s.GetTotalStat(StatType.Strength).ToString();
        if (dexterityValue != null) dexterityValue.text = s.GetTotalStat(StatType.Dexterity).ToString();
        if (constitutionValue != null) constitutionValue.text = s.GetTotalStat(StatType.Constitution).ToString();
        if (intelligenceValue != null) intelligenceValue.text = s.GetTotalStat(StatType.Intelligence).ToString();
        if (wisdomValue != null) wisdomValue.text = s.GetTotalStat(StatType.Wisdom).ToString();
        if (charismaValue != null) charismaValue.text = s.GetTotalStat(StatType.Charisma).ToString();
    }

    private void UpdatePortrait()
    {
        if (currentPlayer == null || characterPortrait == null) return;

        var portraitTexture = GetPortraitForRace(currentPlayer.race);
        if (portraitTexture != null)
        {
            // Ensure the portrait maintains aspect ratio by using ScaleToFit
            characterPortrait.scaleMode = ScaleMode.ScaleToFit;
            characterPortrait.image = portraitTexture;
            return;
        }

        // Try to load from Resources
        string raceLower = currentPlayer.race.ToString().ToLowerInvariant();
        string genderLower = string.IsNullOrEmpty(currentPlayer.gender) ? "unknown" : currentPlayer.gender.ToLowerInvariant();

        var resourceTexture = Resources.Load<Texture2D>($"Portraits/{raceLower}_{genderLower}") ??
                             Resources.Load<Texture2D>($"Portraits/{raceLower}");

        if (resourceTexture != null)
        {
            characterPortrait.scaleMode = ScaleMode.ScaleToFit;
            characterPortrait.image = resourceTexture;
        }
    }

    private string GetDisplayRaceName(RaceType race)
    {
        return race switch
        {
            RaceType.EarthPony => "Earth Pony",
            RaceType.BatPony => "Bat Pony",
            _ => race.ToString()
        };
    }

    private Texture2D GetPortraitForRace(RaceType race)
    {
        return race switch
        {
            RaceType.EarthPony => earthPonyPortrait,
            RaceType.Unicorn => unicornPortrait,
            RaceType.Pegasus => pegasusPortrait,
            RaceType.BatPony => batPonyPortrait,
            RaceType.Griffon => griffonPortrait,
            RaceType.Dragon => dragonPortrait,
            RaceType.Human => humanPortrait,
            _ => null
        };
    }

    /// <summary>
    /// Applies simple template tokens in the story text based on the current player's attributes.
    /// Supported tokens: {name}, {race}, {gender}, {unicorn_horn}, {pegasus_wings}.
    /// Unicorn horn text is inserted only if the player's race is Unicorn; otherwise it is removed.
    /// Pegasus wings text is inserted only if the player's race is Pegasus; otherwise it is removed.
    /// </summary>
    private string ApplyStoryTokens(string rawContent)
    {
        if (string.IsNullOrEmpty(rawContent)) return rawContent;

        var player = currentPlayer;
        if (player == null) return rawContent;

        string output = rawContent;

        // Name token
        if (!string.IsNullOrEmpty(player.name))
            output = output.Replace("{name}", player.name);
        else
            output = output.Replace("{name}", "Anon");

        // Race token
        output = output.Replace("{race}", player.race.ToString());

        // Gender token (may be empty)
        string gender = player.gender ?? "";
        output = output.Replace("{gender}", gender);

        // Racial features: horns and wings
        // Insert a short descriptive snippet if the player's race has the feature, otherwise remove the token completely.
        if (player.race == RaceType.Unicorn)
        {
            output = output.Replace("{unicorn_horn}", "A spiral horn catches the light. ");
        }
        else
        {
            output = output.Replace("{unicorn_horn}", "");
        }

        if (player.race == RaceType.Pegasus)
        {
            output = output.Replace("{pegasus_wings}", "Feathers flex at your sides. ");
        }
        else
        {
            output = output.Replace("{pegasus_wings}", "");
        }

        return output;
    }

    #endregion

    #region Story Display

    public void UpdateStoryDisplay(StoryNode node)
    {
        if (node == null || !isInitialized) return;

        if (storyTitleLabel != null)
            storyTitleLabel.text = string.IsNullOrEmpty(node.title) ? "" : node.title;

        // Render the story content into the appropriate container.  If a rich story body is present,
        // use the RichStoryRenderer to support inline images and formatted text; otherwise fall back
        // to the legacy storyText label.
        string processed = ApplyStoryTokens(node.content);
        if (storyBody != null)
        {
            // Clear legacy elements to avoid overlap
            if (storyImage != null)
            {
                storyImage.image = null;
                storyImage.style.display = DisplayStyle.None;
            }
            if (storyText != null)
            {
                storyText.text = string.Empty;
                storyText.style.display = DisplayStyle.None;
            }
            // Populate the story body using the renderer
            MyGameNamespace.RichStoryRenderer.Render(storyBody, processed);
        }
        else if (storyText != null)
        {
            // Legacy fallback: no rich body container available
            storyText.text = processed;
        }

        // Update story image only when storyBody is absent; otherwise images are rendered inline
        if (storyBody == null)
        {
            UpdateStoryImage(node);
        }
        UpdateChoiceButtons(node.choices ?? new List<StoryChoice>());
    }

    private void UpdateStoryImage(StoryNode node)
    {
        if (storyImage == null) return;

        if (string.IsNullOrEmpty(node?.imagePath))
        {
            storyImage.image = null;
            storyImage.style.display = DisplayStyle.None;
            return;
        }

        var texture = Resources.Load<Texture2D>(node.imagePath);
        if (texture != null)
        {
            storyImage.image = texture;
            storyImage.style.display = DisplayStyle.Flex;
        }
        else
        {
            storyImage.image = null;
            storyImage.style.display = DisplayStyle.None;
        }
    }

    private void UpdateChoiceButtons(List<StoryChoice> choices)
    {
        for (int i = 0; i < choiceButtons.Count; i++)
        {
            var btn = choiceButtons[i];

            if (i < choices.Count && choices[i].isEnabled)
            {
                string label = choices[i].text ?? "";
                btn.text = $"{hotkeyLabels[i]} {label}";
                btn.SetEnabled(true);
                btn.style.display = DisplayStyle.Flex;
                btn.tooltip = GetChoiceTooltip(choices[i]);
            }
            else
            {
                btn.text = hotkeyLabels[i];
                btn.SetEnabled(false);
                btn.style.display = DisplayStyle.None;
                btn.tooltip = "";
            }
        }
    }

    private string GetChoiceTooltip(object choice)
    {
        if (choice == null) return "";

        var type = choice.GetType();
        var properties = type.GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

        foreach (var prop in properties)
        {
            if ((prop.Name.ToLower().Contains("tooltip") || prop.Name.ToLower().Contains("description"))
                && prop.PropertyType == typeof(string))
            {
                var value = prop.GetValue(choice) as string;
                if (!string.IsNullOrEmpty(value)) return value;
            }
        }

        return "";
    }

    #endregion

    #region Choice System

    private void SetupChoiceButtons()
    {
        choiceButtons.Clear();

        for (int i = 1; i <= 8; i++)
        {
            var button = root.Q<Button>($"Choice{i}");
            if (button == null && choicesPanel != null)
            {
                button = new Button { name = $"Choice{i}", text = hotkeyLabels[i - 1] };
                button.AddToClassList("choice-btn");
                choicesPanel.Add(button);
            }

            if (button != null)
            {
                int idx = i - 1;
                button.clicked += () => OnChoiceClicked(idx);
                choiceButtons.Add(button);
            }
        }

        // Hide buttons initially
        foreach (var btn in choiceButtons)
        {
            btn.style.display = DisplayStyle.None;
            btn.SetEnabled(false);
        }

        if (verboseLogging) Debug.Log($"UIController: Setup {choiceButtons.Count} choice buttons");
    }

    private void SetupHotkeys()
    {
        if (root != null)
            root.RegisterCallback<KeyDownEvent>(OnKeyDown);
    }

    private void OnKeyDown(KeyDownEvent evt)
    {
        int? index = evt.keyCode switch
        {
            KeyCode.Alpha1 => 0,
            KeyCode.Alpha2 => 1,
            KeyCode.Alpha3 => 2,
            KeyCode.Alpha4 => 3,
            KeyCode.Alpha5 => 4,
            KeyCode.Alpha6 => 5,
            KeyCode.Alpha7 => 6,
            KeyCode.Alpha8 => 7,
            _ => null
        };

        if (index.HasValue)
        {
            int i = index.Value;
            if (i >= 0 && i < choiceButtons.Count && choiceButtons[i].enabledSelf)
            {
                OnChoiceClicked(i);
                evt.StopImmediatePropagation();
            }
        }
    }

    private void OnChoiceClicked(int choiceIndex)
    {
        var storyManager = StoryManager.Instance;
        var currentNode = storyManager?.CurrentNode;

        if (storyManager != null && currentNode != null &&
            choiceIndex >= 0 && choiceIndex < currentNode.choices.Count)
        {
            var choice = currentNode.choices[choiceIndex];

            // Let GameManager handle the choice effects
            GameManager.Instance?.OnStoryChoiceMade(choice);

            if (verboseLogging) Debug.Log($"Choice selected: {choice.text}");
        }
    }

    #endregion

    #region Animation System

    private void SetupAnimatedBars()
    {
        barAnimations.Clear();

        SetupBarAnimation("health", healthBar, healthColor, healthLowColor, healthColor, 0.25f, 0.75f);
        SetupBarAnimation("energy", staminaBar, energyColor, MultiplyColor(energyColor, 0.7f), energyColor, 0.25f, 0.75f);
        SetupBarAnimation("magic", manaBar, magicColor, MultiplyColor(magicColor, 0.7f), magicColor, 0.25f, 0.75f);
        SetupBarAnimation("friendship", harmonyBar, friendshipColor, MultiplyColor(friendshipColor, 0.7f), BrightenColor(friendshipColor, 1.2f), 0.25f, 0.8f);
        SetupBarAnimation("discord", discordBar, discordColor, discordColor, discordHighColor, 0.25f, 0.6f);

        if (enableBarPulse && pulseCoroutine == null)
            pulseCoroutine = StartCoroutine(PulseEffectCoroutine());

        if (verboseLogging) Debug.Log($"Setup {barAnimations.Count} animated bars");
    }

    private void SetupBarAnimation(string key, ProgressBar bar, Color normalColor, Color lowColor, Color highColor, float lowThreshold, float highThreshold)
    {
        if (bar == null) return;

        var data = new BarAnimationData
        {
            bar = bar,
            fill = bar.Q(className: "unity-progress-bar__progress"),
            normalColor = normalColor,
            lowColor = lowColor,
            highColor = highColor,
            lowThreshold = lowThreshold,
            highThreshold = highThreshold
        };

        data.currentValue = bar.value;
        data.maxValue = bar.highValue > 0 ? bar.highValue : 100f;
        barAnimations[key] = data;

        SetupBarVisuals(data);
    }

    private void SetupBarVisuals(BarAnimationData data)
    {
        if (data.bar == null) return;

        var barStyle = data.bar.style;
        barStyle.backgroundColor = new Color(0, 0, 0, 0.3f);
        barStyle.borderTopLeftRadius = barStyle.borderTopRightRadius =
        barStyle.borderBottomLeftRadius = barStyle.borderBottomRightRadius = 8;

        var borderColor = new Color(1f, 1f, 1f, 0.2f);
        barStyle.borderLeftColor = barStyle.borderRightColor =
        barStyle.borderTopColor = barStyle.borderBottomColor = borderColor;
        barStyle.borderLeftWidth = barStyle.borderRightWidth =
        barStyle.borderTopWidth = barStyle.borderBottomWidth = 1;

        if (data.fill == null)
        {
            data.bar.schedule.Execute(() =>
            {
                data.fill = data.bar.Q(className: "unity-progress-bar__progress");
                if (data.fill != null)
                    SetupFillVisuals(data);
            });
        }
        else
        {
            SetupFillVisuals(data);
        }
    }

    private void SetupFillVisuals(BarAnimationData data)
    {
        var fillStyle = data.fill.style;
        fillStyle.backgroundColor = data.normalColor;
        fillStyle.borderTopLeftRadius = fillStyle.borderTopRightRadius =
        fillStyle.borderBottomLeftRadius = fillStyle.borderBottomRightRadius = 6;
    }

    private void AnimateBarToValue(string barKey, float newValue, float maxValue)
    {
        if (!barAnimations.TryGetValue(barKey, out var data)) return;

        data.targetValue = newValue;
        data.maxValue = Mathf.Max(1f, maxValue);

        if (data.animationCoroutine != null)
            StopCoroutine(data.animationCoroutine);

        data.animationCoroutine = StartCoroutine(AnimateBarCoroutine(data));
    }

    private IEnumerator AnimateBarCoroutine(BarAnimationData data)
    {
        data.isAnimating = true;
        float startValue = data.currentValue;
        float elapsedTime = 0f;

        if (data.fill == null)
            data.fill = data.bar.Q(className: "unity-progress-bar__progress");

        while (elapsedTime < barAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / barAnimationDuration);
            float curveValue = barAnimationCurve?.Evaluate(progress) ?? progress;

            data.currentValue = Mathf.Lerp(startValue, data.targetValue, curveValue);
            data.bar.highValue = data.maxValue;
            data.bar.value = data.currentValue;

            if (enableColorTransitions)
                UpdateBarColor(data);

            yield return null;
        }

        data.currentValue = data.targetValue;
        data.bar.highValue = data.maxValue;
        data.bar.value = data.targetValue;

        if (enableColorTransitions)
            UpdateBarColor(data);

        data.isAnimating = false;
        data.animationCoroutine = null;
    }

    private void UpdateBarColor(BarAnimationData data)
    {
        if (data.fill == null || data.maxValue <= 0f) return;

        float percentage = data.currentValue / data.maxValue;
        Color targetColor;

        if (percentage <= data.lowThreshold)
        {
            float t = percentage / Mathf.Max(0.0001f, data.lowThreshold);
            targetColor = Color.Lerp(data.lowColor, data.normalColor, t);
        }
        else if (percentage >= data.highThreshold)
        {
            float t = (percentage - data.highThreshold) / Mathf.Max(0.0001f, (1f - data.highThreshold));
            targetColor = Color.Lerp(data.normalColor, data.highColor, t);
        }
        else
        {
            targetColor = data.normalColor;
        }

        data.fill.style.backgroundColor = targetColor;
    }

    private IEnumerator PulseEffectCoroutine()
    {
        while (true)
        {
            float time = Time.time * 2f;
            float pulse = 0.85f + 0.15f * Mathf.Sin(time);

            foreach (var kv in barAnimations)
            {
                var key = kv.Key;
                var data = kv.Value;
                if (data.fill == null || data.maxValue <= 0f) continue;

                float percentage = data.currentValue / data.maxValue;

                bool critical = key switch
                {
                    "health" => percentage <= data.lowThreshold,
                    "discord" => percentage >= data.highThreshold,
                    _ => false
                };

                data.fill.style.opacity = critical ? pulse : 1f;
            }

            yield return null;
        }
    }

    private static Color MultiplyColor(Color color, float multiplier)
    {
        return new Color(color.r * multiplier, color.g * multiplier, color.b * multiplier, color.a);
    }

    private static Color BrightenColor(Color color, float multiplier)
    {
        return new Color(Mathf.Clamp01(color.r * multiplier), Mathf.Clamp01(color.g * multiplier), Mathf.Clamp01(color.b * multiplier), color.a);
    }

    #endregion

    #region Event System

    private IEnumerator WaitForGameEventSystemAndSubscribe()
    {
        int attempts = 0;
        while (GameEventSystem.Instance == null && attempts < 30)
        {
            attempts++;
            yield return new WaitForSeconds(0.05f);
        }

        if (GameEventSystem.Instance != null)
        {
            // Subscribe to GameEventSystem events using the += operator. The events
            // defined in GameEventSystem are plain Action types, so AddListener
            // cannot be used here.
            GameEventSystem.Instance.OnPlayerStatsChanged += OnPlayerStatsChangedEmpty;
            GameEventSystem.Instance.OnStoryNodeChanged += OnStoryNodeChanged;
            GameEventSystem.Instance.OnPlayerCharacterChanged += OnPlayerCharacterChangedObj;
            if (verboseLogging) Debug.Log("UIController: Subscribed to GameEventSystem events");

            // If a story node already exists (e.g., StoryManager set it in Awake), update
            // the display immediately. Otherwise the first node change may have been
            // missed before this subscription. We cannot assume StoryManager.Instance is
            // ready here, so check for nulls.  Use PlayerState.Current to provide
            // player context for token replacement if available.
            try
            {
                // Ensure a StoryManager exists.  If one is not present in the scene,
                // create a new GameObject with a StoryManager component.  This
                // guarantees that story data is loaded and a start node is set.
                var sm = global::StoryManager.Instance;
                if (sm == null)
                {
                    var storyObj = new GameObject("StoryManager");
                    sm = storyObj.AddComponent<global::StoryManager>();
                }

                if (sm != null)
                {
                    // If a current node already exists, update the display immediately.
                    if (sm.CurrentNode != null)
                    {
                        UpdateStoryDisplay(sm.CurrentNode);
                    }
                    // Otherwise, if story data is loaded but no node is active yet, set the
                    // current node to the start node.  This ensures the first story node
                    // appears in the HUD even if StoryManager has not yet called SetCurrentNode.
                    else if (sm.Data != null)
                    {
                        var start = sm.Data.GetStartNode();
                        sm.SetCurrentNode(start);
                    }
                }
            }
            catch
            {
                // ignore exceptions retrieving story data
            }
        }
        else
        {
            if (verboseLogging) Debug.LogWarning("UIController: GameEventSystem.Instance was not found during initialization");
        }
    }

    private void OnPlayerStatsChanged(PlayerCharacter player) => UpdatePlayerUI(player);
    private void OnPlayerCharacterChanged(PlayerCharacter player) => UpdatePlayerUI(player);

    private void OnStoryNodeChanged(StoryNode node)
    {
        if (verboseLogging)
        {
            var count = node?.choices?.Count ?? 0;
            Debug.Log($"UIController: Story node changed to '{node?.id}' with {count} choices");
        }
        UpdateStoryDisplay(node);
    }

    // Bridging handlers for GameEventSystem events.  GameEventSystem defines
    // untyped events (Action and Action<object>), so we need to convert them
    // into calls to our typed handlers.  These methods subscribe via
    // WaitForGameEventSystemAndSubscribe and are unsubscribed in OnDisable/OnDestroy.
    private void OnPlayerStatsChangedEmpty()
    {
        // When player stats change, refresh the UI for the current player.  We
        // fetch the player from the GameManager because the event carries no
        // payload in the canonical GameEventSystem.
        var player = GameManager.Instance?.GetPlayer();
        if (player == null)
        {
            player = MyGameNamespace.PlayerState.Current;
        }
        if (player != null)
        {
            UpdatePlayerUI(player);
        }
    }

    private void OnPlayerCharacterChangedObj(object payload)
    {
        // Cast the payload to PlayerCharacter if possible and refresh the UI.
        if (payload is PlayerCharacter pc)
        {
            UpdatePlayerUI(pc);
        }
    }

    #endregion

    #region Styling and Utilities

    private void ApplyUIStyles()
    {
        if (characterPortrait != null)
        {
            var style = characterPortrait.style;
            style.width = style.height = 150;
            style.borderTopLeftRadius = style.borderTopRightRadius =
            style.borderBottomLeftRadius = style.borderBottomRightRadius = 12;

            var borderColor = new Color(0.7f, 0.5f, 1f, 1f);
            style.borderTopColor = style.borderBottomColor =
            style.borderLeftColor = style.borderRightColor = borderColor;
            style.borderTopWidth = style.borderBottomWidth =
            style.borderLeftWidth = style.borderRightWidth = 3;
        }

        if (storyTitleLabel != null) storyTitleLabel.style.fontSize = headerFontSize;
        if (storyText != null) storyText.style.fontSize = storyFontSize;

        foreach (var btn in choiceButtons)
            btn.style.fontSize = choiceFontSize;
    }

    public void Show() { if (root != null) root.style.display = DisplayStyle.Flex; }
    public void Hide() { if (root != null) root.style.display = DisplayStyle.None; }

    #endregion

    #region Cleanup

    private void OnDisable()
    {
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }

        foreach (var data in barAnimations.Values)
        {
            if (data.animationCoroutine != null)
            {
                StopCoroutine(data.animationCoroutine);
                data.animationCoroutine = null;
            }
        }

        if (root != null)
            root.UnregisterCallback<KeyDownEvent>(OnKeyDown);

        if (GameEventSystem.Instance != null)
        {
            // Unsubscribe from GameEventSystem events using the -= operator
            GameEventSystem.Instance.OnPlayerStatsChanged -= OnPlayerStatsChangedEmpty;
            GameEventSystem.Instance.OnStoryNodeChanged -= OnStoryNodeChanged;
            GameEventSystem.Instance.OnPlayerCharacterChanged -= OnPlayerCharacterChangedObj;
        }
    }

    private void OnDestroy()
    {
        if (GameEventSystem.Instance != null)
        {
            GameEventSystem.Instance.OnPlayerStatsChanged -= OnPlayerStatsChangedEmpty;
            GameEventSystem.Instance.OnStoryNodeChanged -= OnStoryNodeChanged;
            GameEventSystem.Instance.OnPlayerCharacterChanged -= OnPlayerCharacterChangedObj;
        }
    }

    #endregion

    #region Debug Methods

    [ContextMenu("Test UI Elements")]
    public void TestUIElements()
    {
        Debug.Log("=== UI ELEMENTS TEST ===");
        Debug.Log($"Root: {root != null}");
        Debug.Log($"Character Portrait: {characterPortrait != null}");
        Debug.Log($"Character Name: {characterName != null}");
        Debug.Log($"Health Bar: {healthBar != null}");
        Debug.Log($"Story Text: {storyText != null}");
        Debug.Log($"Choices Panel: {choicesPanel != null}");
        Debug.Log($"Choice Buttons: {choiceButtons.Count}");
        Debug.Log($"Current Player: {currentPlayer?.name ?? "None"}");
        Debug.Log($"BottomCharBtn: {(bottomCharacterButton != null ? bottomCharacterButton.name + " / " + bottomCharacterButton.text : "null")}");
        Debug.Log($"BottomItemsBtn: {(bottomItemsButton != null ? bottomItemsButton.name + " / " + bottomItemsButton.text : "null")}");
        Debug.Log($"BottomMenuBtn: {(bottomMenuButton != null ? bottomMenuButton.name + " / " + bottomMenuButton.text : "null")}");
    }

    [ContextMenu("Force Player Update")]
    public void ForcePlayerUpdate()
    {
        var player = GameManager.Instance?.GetPlayer();
        if (player == null)
        {
            player = MyGameNamespace.PlayerState.Current;
        }
        if (player != null)
        {
            UpdatePlayerUI(player);
            Debug.Log("Forced player UI update");
        }
        else
        {
            Debug.LogWarning("No player available for update");
        }
    }

    #endregion
}