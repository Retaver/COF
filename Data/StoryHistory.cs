using System;
using System.Collections.Generic;

[System.Serializable]
public class GameStoryChoiceV2
{
    public string text;
    public string resultText;
    public bool isEnabled = true;
    public bool isPrimary = false;
    public Action action;

    public GameStoryChoiceV2(string choiceText, string result = "")
    {
        text = choiceText;
        resultText = result;
        isEnabled = true;
        isPrimary = false;
    }
}

[System.Serializable]
public class StoryHistoryEntryV2
{
    public string title;
    public string text;
    public List<GameStoryChoiceV2> choices;
    public string timestamp;

    public StoryHistoryEntryV2(string storyTitle, string storyText, List<GameStoryChoiceV2> storyChoices)
    {
        title = storyTitle;
        text = storyText;
        choices = new List<GameStoryChoiceV2>(storyChoices ?? new List<GameStoryChoiceV2>());
        timestamp = DateTime.Now.ToString("HH:mm:ss");
    }
}
