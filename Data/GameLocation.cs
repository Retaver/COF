using System.Collections.Generic;

[System.Serializable]
public class GameLocation
{
    public string id;
    public string name;
    public string area;
    public string description;
    public List<LocationCharacter> characters;
    public List<string> availableActions;
    public List<string> connectedLocations;

    public GameLocation(string locationId, string locationName, string areaName, string desc)
    {
        id = locationId;
        name = locationName;
        area = areaName;
        description = desc;
        characters = new List<LocationCharacter>();
        availableActions = new List<string>();
        connectedLocations = new List<string>();
    }

    // Add a character to this location
    public void AddCharacter(LocationCharacter character)
    {
        if (!characters.Contains(character))
        {
            characters.Add(character);
        }
    }

    // Remove a character from this location
    public void RemoveCharacter(string characterId)
    {
        characters.RemoveAll(c => c.characterId == characterId);
    }

    // Check if this location is connected to another location
    public bool IsConnectedTo(string locationId)
    {
        return connectedLocations.Contains(locationId);
    }

    // Add a connection to another location
    public void AddConnection(string locationId)
    {
        if (!connectedLocations.Contains(locationId))
        {
            connectedLocations.Add(locationId);
        }
    }

    // Remove a connection to another location
    public void RemoveConnection(string locationId)
    {
        connectedLocations.Remove(locationId);
    }
}

[System.Serializable]
public class LocationCharacter
{
    public string characterId;
    public string displayName;
    public string portraitPath;
    public bool isInteractable;
    public List<string> availableDialogue;

    public LocationCharacter(string id, string name, string portrait = "", bool interactable = true)
    {
        characterId = id;
        displayName = name;
        portraitPath = portrait;
        isInteractable = interactable;
        availableDialogue = new List<string>();
    }

    // Add a dialogue option for this character
    public void AddDialogue(string dialogue)
    {
        if (!availableDialogue.Contains(dialogue))
        {
            availableDialogue.Add(dialogue);
        }
    }

    // Remove a dialogue option from this character
    public void RemoveDialogue(string dialogue)
    {
        availableDialogue.Remove(dialogue);
    }
}