using System.Collections.Generic;
using UnityEngine;

public class EncounterBuilder : MonoBehaviour
{
    [Tooltip("Assign your Enemy Database asset")]
    public EnemyDatabase database;

    [Tooltip("Used to scale enemies. Replace with your real player level source.")]
    public int fallbackPlayerLevel = 1;

    public Enemy CreateEnemy(string idOrName, int playerLevel)
    {
        if (database == null)
        {
            Debug.LogError("EncounterBuilder: EnemyDatabase not assigned.");
            return null;
        }
        return database.CreateEnemy(idOrName, playerLevel);
    }

    public List<Enemy> BuildEncounter(IEnumerable<string> enemyIdsOrNames, int playerLevel)
    {
        var list = new List<Enemy>();
        if (database == null)
        {
            Debug.LogError("EncounterBuilder: EnemyDatabase not assigned.");
            return list;
        }

        foreach (var key in enemyIdsOrNames)
        {
            var e = database.CreateEnemy(key, playerLevel);
            if (e != null) list.Add(e);
        }
        return list;
    }

    // Example helper for quick tests in inspector
    [ContextMenu("Test: Build 2 Timberwolves")]
    void TestBuild()
    {
        var encounter = BuildEncounter(new[] { "timberwolf_basic", "timberwolf_basic" }, fallbackPlayerLevel);
        Debug.Log($"Built encounter with {encounter.Count} enemies.");
    }
}