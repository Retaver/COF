using System;
using System.Collections.Generic;
using UnityEngine;
using MyGameNamespace;

[Serializable]
public class GameSave
{
    public string saveName;
    public string saveDate;
    public PlayerCharacter playerData;

    // Time/World
    public int gameDay;
    public string timeOfDay;
    public string currentLocationId;

    // Story
    public string currentNodeId;
    public SerializableDictionary storyFlags;

    // Progress
    public List<string> gameFlags;
    public List<string> completedQuests;
    public List<string> discoveredLocations;

    public GameSave()
    {
        saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        gameFlags = new List<string>();
        storyFlags = new SerializableDictionary();
        completedQuests = new List<string>();
        discoveredLocations = new List<string>();
        currentNodeId = string.Empty;
        gameDay = 1;
        timeOfDay = "Morning";
        currentLocationId = "";
    }

    // Helper method to create a deep copy of the save
    public GameSave Clone()
    {
        GameSave clone = new GameSave();
        clone.saveName = this.saveName;
        clone.saveDate = this.saveDate;
        clone.playerData = this.playerData != null ? this.playerData.Clone() : null;
        clone.gameDay = this.gameDay;
        clone.timeOfDay = this.timeOfDay;
        clone.currentLocationId = this.currentLocationId;
        clone.currentNodeId = this.currentNodeId;

        clone.gameFlags = new List<string>(this.gameFlags);
        clone.storyFlags = this.storyFlags?.Clone() ?? new SerializableDictionary();
        clone.completedQuests = new List<string>(this.completedQuests);
        clone.discoveredLocations = new List<string>(this.discoveredLocations);

        return clone;
    }

    // Helper method to check if a flag exists
    public bool HasFlag(string flagName)
    {
        return gameFlags.Contains(flagName);
    }

    // Helper method to set a flag
    public void SetFlag(string flagName)
    {
        if (!gameFlags.Contains(flagName))
        {
            gameFlags.Add(flagName);
        }
    }

    // Helper method to remove a flag
    public void RemoveFlag(string flagName)
    {
        gameFlags.Remove(flagName);
    }

    // Helper method to check if a story flag exists with a specific value
    public bool HasStoryFlag(string flagName, string value = null)
    {
        if (value == null)
        {
            return storyFlags.ContainsKey(flagName);
        }
        else
        {
            return storyFlags.ContainsKey(flagName) && storyFlags[flagName] == value;
        }
    }

    // Helper method to set a story flag
    public void SetStoryFlag(string flagName, string value)
    {
        storyFlags[flagName] = value;
    }

    // Helper method to check if a quest is completed
    public bool IsQuestCompleted(string questId)
    {
        return completedQuests.Contains(questId);
    }

    // Helper method to mark a quest as completed
    public void CompleteQuest(string questId)
    {
        if (!completedQuests.Contains(questId))
        {
            completedQuests.Add(questId);
        }
    }

    // Helper method to check if a location is discovered
    public bool IsLocationDiscovered(string locationId)
    {
        return discoveredLocations.Contains(locationId);
    }

    // Helper method to mark a location as discovered
    public void DiscoverLocation(string locationId)
    {
        if (!discoveredLocations.Contains(locationId))
        {
            discoveredLocations.Add(locationId);
        }
    }

    // Helper method to get a formatted display name for the save
    public string GetDisplayName()
    {
        return $"{saveName} - {saveDate}";
    }

    // Helper method to get save summary for UI display
    public string GetSaveSummary()
    {
        if (playerData == null)
        {
            return "Invalid save data";
        }

        string raceName = GetDisplayRaceName(playerData.race);
        return $"{playerData.name} - Level {playerData.level} {raceName} - Day {gameDay} {timeOfDay}";
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
}