using System;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class CombatUI : MonoBehaviour, ICombatUI
{
    [Header("References")]
    [SerializeField] private UIDocument uiDocument;

    [Header("Element names (adjust to match your UXML)")]
    [SerializeField] private string combatLogLabelName = "CombatLog";
    [SerializeField] private string combatLogContainerName = "CombatLogContainer";
    [SerializeField] private string playerActionContainerName = "PlayerActionButtons";

    private VisualElement _root;
    private Label _combatLogLabel;
    private VisualElement _combatLogContainer;
    private VisualElement _playerActionContainer;
    private bool _cached = false;

    // Delegate property to allow assignment and invocation
    public Action<object[]> OnPlayerAction { get; set; }

    private void Awake()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        CacheRootAndElements();

        // optional default handler to avoid nulls
        if (OnPlayerAction == null)
            OnPlayerAction = (args) => { Debug.Log("[CombatUI] Default OnPlayerAction: " + JoinArgs(args)); };
    }

    private void OnEnable()
    {
        if (!_cached) CacheRootAndElements();
    }

    private void CacheRootAndElements()
    {
        _cached = true;
        if (uiDocument == null)
        {
            var doc = GetComponent<UIDocument>();
            if (doc != null) uiDocument = doc;
        }

        if (uiDocument == null)
        {
            _root = null;
            _combatLogLabel = null;
            _combatLogContainer = null;
            _playerActionContainer = null;
            return;
        }

        _root = uiDocument.rootVisualElement;
        if (_root == null) return;

        _combatLogLabel = _root.Q<Label>(combatLogLabelName);
        _combatLogContainer = _root.Q<VisualElement>(combatLogContainerName);
        _playerActionContainer = _root.Q<VisualElement>(playerActionContainerName);
    }

    public void ShowUI()
    {
        EnsureRoot();
        if (_root == null) return;
        _root.style.display = DisplayStyle.Flex;
        _root.BringToFront();
    }

    public void HideUI()
    {
        EnsureRoot();
        if (_root == null) return;
        _root.style.display = DisplayStyle.None;
    }

    public void StartCombat()
    {
        Debug.Log("[CombatUI] StartCombat()");
        EnsureRoot();
        if (_root != null)
        {
            _root.style.display = DisplayStyle.Flex;
            _root.BringToFront();
        }
        ClearCombatLog();
    }

    public void StartCombat(params object[] args)
    {
        Debug.Log("[CombatUI] StartCombat(params): " + JoinArgs(args));
        StartCombat();
    }

    public void EndCombat()
    {
        Debug.Log("[CombatUI] EndCombat()");
        EnsureRoot();
        if (_root != null)
            _root.style.display = DisplayStyle.None;
    }

    public void EndCombat(params object[] args)
    {
        Debug.Log("[CombatUI] EndCombat(params): " + JoinArgs(args));
        EndCombat();
    }

    public void InvokeOnPlayerAction(params object[] args)
    {
        try
        {
            Debug.Log("[CombatUI] InvokeOnPlayerAction: " + JoinArgs(args));
            OnPlayerAction?.Invoke(args);
        }
        catch (Exception ex)
        {
            Debug.LogError("[CombatUI] Exception in OnPlayerAction handler: " + ex);
        }
    }

    public void AddToCombatLog(params object[] args)
    {
        string message = JoinArgs(args);
        Debug.Log("[CombatUI] AddToCombatLog: " + message);
        EnsureRoot();

        if (_combatLogLabel != null)
        {
            string prev = string.IsNullOrEmpty(_combatLogLabel.text) ? "" : _combatLogLabel.text + "\n";
            _combatLogLabel.text = prev + message;
            return;
        }

        if (_combatLogContainer != null)
        {
            var lbl = new Label(message);
            _combatLogContainer.Add(lbl);
            return;
        }
    }

    public void SpawnFloatingTextOverPlayer(params object[] args) { Debug.Log("[CombatUI] SpawnFloatingTextOverPlayer: " + JoinArgs(args)); }
    public void SpawnFloatingTextOverEnemy(params object[] args) { Debug.Log("[CombatUI] SpawnFloatingTextOverEnemy: " + JoinArgs(args)); }
    public void PlayHitFlashOnPlayer(params object[] args) { Debug.Log("[CombatUI] PlayHitFlashOnPlayer: " + JoinArgs(args)); }
    public void PlayHitFlashOnEnemy(params object[] args) { Debug.Log("[CombatUI] PlayHitFlashOnEnemy: " + JoinArgs(args)); }

    public void RefreshPlayerDisplaySafe() { EnsureRoot(); }
    public void RefreshEnemyDisplaySafe() { EnsureRoot(); }

    public void SetPlayerTurn(bool isPlayerTurn)
    {
        EnsureRoot();
        if (_playerActionContainer != null) _playerActionContainer.SetEnabled(isPlayerTurn);
    }

    public void SetPlayerTurn(params object[] args)
    {
        if (args != null && args.Length > 0 && args[0] is bool b) SetPlayerTurn(b);
        else Debug.Log("[CombatUI] SetPlayerTurn(params) invalid args: " + JoinArgs(args));
    }

    private void EnsureRoot()
    {
        if (_root == null) CacheRootAndElements();
    }

    private string JoinArgs(object[] args)
    {
        if (args == null || args.Length == 0) return "";
        try
        {
            if (args.Length == 1 && args[0] != null) return args[0].ToString();
            return string.Join(" ", Array.ConvertAll(args, a => a?.ToString() ?? "null"));
        }
        catch { return "[Could not stringify args]"; }
    }

    private void ClearCombatLog()
    {
        if (_combatLogLabel != null) _combatLogLabel.text = "";
        if (_combatLogContainer != null) _combatLogContainer.Clear();
    }
}