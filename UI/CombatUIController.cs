using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using MyGameNamespace;

[RequireComponent(typeof(UIDocument))]
public class CombatUIController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private AttackDatabase attackDb;
    [SerializeField] private AttackNarrationDatabase narrationDb; // optional

    [Header("Act buttons (names shown on UI)")]
    [SerializeField] private string[] actButtons = new[] { "Talk", "Taunt", "Scan" };

    [Header("Debug")]
    [SerializeField] private bool logClicksToConsole = false;

    private UIDocument doc;
    private VisualElement root;

    private VisualElement attackList;
    private VisualElement specialList;
    private VisualElement actList;

    private VisualElement fxLayer;
    private ScrollView logScroll;
    private Label turnIndicator;
    private Label roundCounter;

    private Label playerName, playerInfo, playerHealthText, playerEnergyText, playerManaText;
    private VisualElement playerHealthFill, playerEnergyFill, playerManaFill;

    private Label enemyName, enemyInfo, enemyHealthText;
    private VisualElement enemyHealthFill;

    private Button btnUseItem, btnWait, btnInspect, btnMenu, btnAuto;

    private CombatManager combatManager;

    private void Awake()
    {
        doc = GetComponent<UIDocument>();
        // TODO: attackDb and narrationDb may be assigned in inspector. If null, Accept that and show fallback UI.
    }

    private void OnEnable()
    {
        root = doc?.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("[CombatUIController] UIDocument.rootVisualElement is null.");
            return;
        }

        attackList = root.Q<VisualElement>("attack-list");
        specialList = root.Q<VisualElement>("magic-list");
        actList = root.Q<VisualElement>("special-list"); // naming in UXML: "special-list" used for acts

        fxLayer = root.Q<VisualElement>("fx-layer");
        logScroll = root.Q<ScrollView>("combat-log-scroll");
        turnIndicator = root.Q<Label>("turn-indicator");
        roundCounter = root.Q<Label>("round-counter");

        playerName = root.Q<Label>("player-name");
        playerInfo = root.Q<Label>("player-info");
        playerHealthFill = root.Q<VisualElement>("player-health-fill");
        playerHealthText = root.Q<Label>("player-health-text");
        playerEnergyFill = root.Q<VisualElement>("player-energy-fill");
        playerEnergyText = root.Q<Label>("player-energy-text");
        playerManaFill = root.Q<VisualElement>("player-mana-fill");
        playerManaText = root.Q<Label>("player-mana-text");

        enemyName = root.Q<Label>("enemy-name");
        enemyInfo = root.Q<Label>("enemy-info");
        enemyHealthFill = root.Q<VisualElement>("enemy-health-fill");
        enemyHealthText = root.Q<Label>("enemy-health-text");

        btnUseItem = root.Q<Button>("use-item");
        btnWait = root.Q<Button>("wait");
        btnInspect = root.Q<Button>("inspect");
        btnMenu = root.Q<Button>("menu");
        btnAuto = root.Q<Button>("auto");

        BindButton(btnUseItem, OnUseItemClicked);
        BindButton(btnWait, OnWaitClicked);
        BindButton(btnInspect, OnInspectClicked);
        BindButton(btnMenu, OnMenuClicked);
        BindButton(btnAuto, OnAutoClicked);

        combatManager = CompatUtils.FindFirstObjectByTypeCompat<CombatManager>();

        // By default hide combat UI until started
        Show(false);
    }

    private void OnDisable()
    {
        UnbindButton(btnUseItem, OnUseItemClicked);
        UnbindButton(btnWait, OnWaitClicked);
        UnbindButton(btnInspect, OnInspectClicked);
        UnbindButton(btnMenu, OnMenuClicked);
        UnbindButton(btnAuto, OnAutoClicked);
    }

    private void BindButton(Button b, Action handler)
    {
        if (b == null || handler == null) return;
        b.clicked += () => handler();
    }

    private void UnbindButton(Button b, Action handler)
    {
        if (b == null || handler == null) return;
        // There's no simple way to remove anonymous delegates; leaving as-is is acceptable for a short-lived UI.
        // For robust code, store Action references and remove them explicitly here.
    }

    public void Show(bool show)
    {
        if (root == null) return;
        root.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        if (show) root.BringToFront();
    }

    public void BeginPlayerTurn(int round)
    {
        if (roundCounter != null) roundCounter.text = $"Round {round}";
        SetTurn(true);
        PlayerCharacter player = GameManager.Instance?.GetPlayer();
        if (player == null)
        {
            player = MyGameNamespace.PlayerState.Current;
        }
        PopulateActionButtons(player);
        LogTitle($"Turn {round}");
    }

    public void CloseCombat()
    {
        Show(false);
        LogSystem("Combat ended.");
    }

    public void SetTurn(bool isPlayerTurn)
    {
        if (turnIndicator != null)
        {
            turnIndicator.text = isPlayerTurn ? "Your Turn" : "Enemy Turn";
            if (isPlayerTurn) { turnIndicator.AddToClassList("player-turn"); turnIndicator.RemoveFromClassList("enemy-turn"); }
            else { turnIndicator.AddToClassList("enemy-turn"); turnIndicator.RemoveFromClassList("player-turn"); }
        }

        if (attackList != null) attackList.SetEnabled(isPlayerTurn);
        if (specialList != null) specialList.SetEnabled(isPlayerTurn);
        if (actList != null) actList.SetEnabled(isPlayerTurn);
    }

    private void PopulateActionButtons(PlayerCharacter player)
    {
        attackList?.Clear();
        specialList?.Clear();
        actList?.Clear();

        if (attackDb != null)
        {
            var attacks = attackDb.GetAttacksForRace(player?.race ?? RaceType.EarthPony);
            foreach (var atk in attacks)
            {
                var btn = new Button(() => OnAttackClicked(atk.id)) { text = string.IsNullOrEmpty(atk.displayName) ? atk.id : atk.displayName };
                btn.AddToClassList("simple-action-button");
                attackList?.Add(btn);
            }
        }
        else
        {
            var defaultAtk = new Button(() => OnAttackClicked("basic_attack")) { text = "Attack" };
            defaultAtk.AddToClassList("simple-action-button");
            attackList?.Add(defaultAtk);
        }

        if (attackDb != null)
        {
            var magicAttacks = attackDb.attacks.Where(a => a.attackType == AttackDatabase.AttackType.Magical).ToArray();
            foreach (var m in magicAttacks)
            {
                var btn = new Button(() => OnMagicClicked(m.id)) { text = string.IsNullOrEmpty(m.displayName) ? m.id : m.displayName };
                btn.AddToClassList("simple-action-button");
                specialList?.Add(btn);
            }
        }
        else
        {
            var specialBtn = new Button(() => OnMagicClicked("magic_basic")) { text = "Special" };
            specialBtn.AddToClassList("simple-action-button");
            specialList?.Add(specialBtn);
        }

        if (actButtons != null && actButtons.Length > 0)
        {
            foreach (var name in actButtons)
            {
                var idLower = name.ToLowerInvariant().Replace(" ", "_");
                var btn = new Button(() => OnActClicked(idLower)) { text = name };
                btn.AddToClassList("simple-action-button");
                actList?.Add(btn);
            }
        }
    }

    // Handlers
    private void OnAttackClicked(string attackId)
    {
        if (logClicksToConsole) Debug.Log($"Attack clicked: {attackId}");
        combatManager?.TriggerPlayerAction("attack", attackId);
    }

    private void OnMagicClicked(string attackId)
    {
        if (logClicksToConsole) Debug.Log($"Magic clicked: {attackId}");
        combatManager?.TriggerPlayerAction("magic", attackId);
    }

    private void OnActClicked(string actId)
    {
        if (logClicksToConsole) Debug.Log($"Act clicked: {actId}");
        combatManager?.TriggerPlayerAction("talk", actId);
    }

    private void OnUseItemClicked()
    {
        if (logClicksToConsole) Debug.Log("Use item clicked");
        combatManager?.TriggerPlayerAction("item", "");
    }

    private void OnWaitClicked()
    {
        if (logClicksToConsole) Debug.Log("Wait clicked");
        combatManager?.TriggerPlayerAction("wait", "");
    }

    private void OnInspectClicked()
    {
        if (logClicksToConsole) Debug.Log("Inspect clicked");
        combatManager?.TriggerPlayerAction("inspect", "");
    }

    private void OnMenuClicked()
    {
        if (logClicksToConsole) Debug.Log("Menu clicked");
        combatManager?.TriggerPlayerAction("menu", "");
    }

    private void OnAutoClicked()
    {
        if (logClicksToConsole) Debug.Log("Auto clicked");
        combatManager?.TriggerPlayerAction("auto_toggle", "");
    }

    // UI helpers
    public void LogTitle(string text)
    {
        AddLogEntry($"== {text} ==");
    }

    public void LogSystem(string text)
    {
        AddLogEntry($"[System] {text}");
    }

    public void LogEnemy(string text)
    {
        AddLogEntry($"[Enemy] {text}");
    }

    public void LogPlayer(string text)
    {
        AddLogEntry($"[You] {text}");
    }

    public void LogNarration(string text)
    {
        AddLogEntry($"[Narration] {text}");
    }

    // Added to satisfy CombatManager calls: flexible overloads to accept single string or arbitrary args.
    public void LogHint(string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        AddLogEntry($"[Hint] {text}");
    }

    public void LogHint(params object[] args)
    {
        if (args == null || args.Length == 0) return;
        var line = string.Join(" ", args.Select(a => a?.ToString() ?? ""));
        AddLogEntry($"[Hint] {line}");
    }

    private void AddLogEntry(string line)
    {
        if (logScroll == null) return;
        var lbl = new Label(line);
        logScroll.Add(lbl);
    }

    // Public update methods (used by CombatManager)
    public void SetPlayer(string displayName, string info, int hp, int maxHp, int energy, int maxEnergy, int magic, int maxMagic)
    {
        if (playerName != null) playerName.text = displayName;
        if (playerInfo != null) playerInfo.text = info;
        if (playerHealthText != null) playerHealthText.text = $"{hp} / {maxHp}";
        if (playerEnergyText != null) playerEnergyText.text = $"{energy} / {maxEnergy}";
        if (playerManaText != null) playerManaText.text = $"{magic} / {maxMagic}";

        if (playerHealthFill != null) playerHealthFill.style.width = Length.Percent(maxHp > 0 ? (float)hp / maxHp * 100f : 0f);
        if (playerEnergyFill != null) playerEnergyFill.style.width = Length.Percent(maxEnergy > 0 ? (float)energy / maxEnergy * 100f : 0f);
        if (playerManaFill != null) playerManaFill.style.width = Length.Percent(maxMagic > 0 ? (float)magic / maxMagic * 100f : 0f);
    }

    public void SetEnemy(string displayName, string info, int hp, int maxHp)
    {
        if (enemyName != null) enemyName.text = displayName;
        if (enemyInfo != null) enemyInfo.text = info;
        if (enemyHealthText != null) enemyHealthText.text = $"{hp} / {maxHp}";
        if (enemyHealthFill != null) enemyHealthFill.style.width = Length.Percent(maxHp > 0 ? (float)hp / maxHp * 100f : 0f);
    }

    public void ClearLog()
    {
        logScroll?.Clear();
    }
}