using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;

/// <summary>
/// Controls the Dialogue UI built with Unity UI Toolkit.
/// Handles updating portraits, names, dialogue text, choices, and button events.
/// </summary>
public class DialogueUIController : MonoBehaviour
{
    [Header("UI Toolkit Document")]
    [SerializeField] private UIDocument dialogueDocument;

    // Portraits & Names
    private Image playerPortrait;
    private Label playerName;
    private Image npcPortrait;
    private Label npcName;

    // Dialogue
    private Label dialogueText;
    private ScrollView dialogueScroll;

    // Choices
    private VisualElement responseOptions;
    private ScrollView responseScroll;

    // Controls
    private Button continueButton;

    // Callback for continue button (set by your dialogue system)
    public Action OnContinue;

    // Is dialogue currently showing?
    public bool IsVisible => dialogueDocument != null && dialogueDocument.rootVisualElement.style.display == DisplayStyle.Flex;

    private void Awake()
    {
        if (dialogueDocument == null)
            dialogueDocument = GetComponent<UIDocument>();

        var root = dialogueDocument.rootVisualElement;

        // Get UI elements
        playerPortrait = root.Q<Image>("player-portrait");
        playerName = root.Q<Label>("player-name");
        npcPortrait = root.Q<Image>("npc-portrait");
        npcName = root.Q<Label>("npc-name");
        dialogueText = root.Q<Label>("dialogue-text");
        dialogueScroll = root.Q<ScrollView>("dialogue-scroll");
        responseOptions = root.Q<VisualElement>("response-options");
        responseScroll = root.Q<ScrollView>("response-scroll");
        continueButton = root.Q<Button>("continue-button");

        // Wire up continue button
        continueButton.clicked += HandleContinueButton;

        HideDialogue(); // Hide on start
    }

    /// <summary>
    /// Shows the dialogue panel and populates all fields.
    /// </summary>
    /// <param name="npcNameText">NPC's display name</param>
    /// <param name="npcPortraitTexture">NPC's portrait image</param>
    /// <param name="playerNameText">Player's display name</param>
    /// <param name="playerPortraitTexture">Player's portrait image</param>
    /// <param name="dialogue">Dialogue text</param>
    /// <param name="choices">List of (option text, callback) pairs. If null/empty, only Continue shows.</param>
    public void ShowDialogue(
        string npcNameText,
        Texture2D npcPortraitTexture,
        string playerNameText,
        Texture2D playerPortraitTexture,
        string dialogue,
        List<(string optionText, Action callback)> choices = null
    )
    {
        dialogueDocument.rootVisualElement.style.display = DisplayStyle.Flex;

        // Set portraits and names
        npcName.text = npcNameText;
        npcPortrait.image = npcPortraitTexture;
        playerName.text = playerNameText;
        playerPortrait.image = playerPortraitTexture;

        // Set dialogue text
        dialogueText.text = dialogue;
        dialogueScroll.ScrollTo(dialogueText);

        // Set responses
        responseOptions.Clear();
        if (choices != null && choices.Count > 0)
        {
            continueButton.style.display = DisplayStyle.None;
            foreach (var choice in choices)
            {
                var optionBtn = new Button(() =>
                {
                    HideDialogue();
                    choice.callback?.Invoke();
                })
                {
                    text = choice.optionText
                };
                optionBtn.AddToClassList("dialogue-button");
                responseOptions.Add(optionBtn);
            }
            responseScroll.ScrollTo(responseOptions);
        }
        else
        {
            continueButton.style.display = DisplayStyle.Flex;
        }
    }

    /// <summary>
    /// Hides the dialogue panel.
    /// </summary>
    public void HideDialogue()
    {
        if (dialogueDocument != null && dialogueDocument.rootVisualElement != null)
            dialogueDocument.rootVisualElement.style.display = DisplayStyle.None;
    }

    /// <summary>
    /// Handler for the Continue button.
    /// </summary>
    private void HandleContinueButton()
    {
        HideDialogue();
        OnContinue?.Invoke();
    }

    /// <summary>
    /// Utility to quickly show choices-only (for branching options).
    /// </summary>
    public void ShowChoices(List<(string optionText, Action callback)> choices)
    {
        ShowDialogue("", null, "", null, "", choices);
    }

    /// <summary>
    /// Utility to quickly show narration/dialogue-only (no choices).
    /// </summary>
    public void ShowText(string dialogue, Action onContinue = null)
    {
        OnContinue = onContinue;
        ShowDialogue("", null, "", null, dialogue, null);
    }
}