using System;
using System.Collections.Generic;

[Serializable]
public class GameStoryChoice
{
    public string text;
    public string resultText;
    public bool isEnabled = true;
    public bool isPrimary = false;
    public Action action;

    public GameStoryChoice(string choiceText, string result = "")
    {
        text = choiceText;
        resultText = result;
        isEnabled = true;
        isPrimary = false;
    }
}

[Serializable]
public class StoryHistoryEntry
{
    public string title;
    public string text;
    public List<GameStoryChoice> choices;
    public string timestamp;

    public StoryHistoryEntry(string storyTitle, string storyText, List<GameStoryChoice> storyChoices)
    {
        title = storyTitle;
        text = storyText;
        choices = new List<GameStoryChoice>(storyChoices ?? new List<GameStoryChoice>());
        timestamp = DateTime.Now.ToString("HH:mm:ss");
    }
}