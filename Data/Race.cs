using System.Collections.Generic;

[System.Serializable]
public class Race
{
    public RaceType raceType;
    public string name;
    public string description;
    public Dictionary<StatType, int> statBonuses;
    public List<string> racialAbilities;
    public List<PerkType> startingPerks;
    public string flavorText;

    public Race(RaceType type, string raceName, string desc, string flavor)
    {
        raceType = type;
        name = raceName;
        description = desc;
        flavorText = flavor;
        statBonuses = new Dictionary<StatType, int>();
        racialAbilities = new List<string>();
        startingPerks = new List<PerkType>();
    }
}