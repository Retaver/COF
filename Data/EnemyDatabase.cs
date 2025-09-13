using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "Enemy Database", menuName = "MLP Game/Enemy Database")]
public class EnemyDatabase : ScriptableObject
{
    [System.Serializable]
    public class EnemyTemplate
    {
        [Tooltip("Stable key (no spaces). Used to reference this enemy in code and data.")]
        public string id;

        [Tooltip("Display name shown to the player.")]
        public string name;

        [Header("Base Stats")]
        public int baseLevel = 1;
        public int baseHealth = 10;
        public int baseAttack = 2;
        public int baseDefense = 0;          // physical defense
        public int baseTalkDefense = 0;      // social/talk resist

        [Header("Rewards")]
        public int xpReward = 100;
        public int bitsReward = 10;

        [Header("Personality / Disposition")]
        public EnemyDisposition defaultDisposition = EnemyDisposition.Hostile;
        [Range(0, 100)] public int baseMorale = 60;
        [Range(0, 100)] public int baseWillpower = 50;

        [Header("Presentation")]
        [Tooltip("Resources-relative portrait path without extension, e.g., 'images/enemies/Timberwolf'")]
        public string portraitPath;
        [TextArea] public string description;

        [Header("Moves")]
        [Tooltip("Attack ids (from AttackDatabase) this enemy can use, with optional weights.")]
        public List<EnemyMoveRef> moves = new List<EnemyMoveRef>();
    }

    [Serializable]
    public class EnemyMoveRef
    {
        [Tooltip("Attack id from AttackDatabase (e.g., 'wolf_bite', 'wolf_claw').")]
        public string id;
        [Tooltip("Higher = selected more often.")]
        [Range(1, 100)] public int weight = 1;
    }

    public List<EnemyTemplate> enemies = new List<EnemyTemplate>();

    EnemyTemplate FindTemplate(string enemyIdOrName)
    {
        if (string.IsNullOrWhiteSpace(enemyIdOrName)) return null;

        // Prefer id (case-insensitive)
        var tpl = enemies.Find(e => !string.IsNullOrEmpty(e.id) &&
                                    string.Equals(e.id, enemyIdOrName, StringComparison.OrdinalIgnoreCase));
        if (tpl != null) return tpl;

        // Fallback to display name (case-insensitive)
        tpl = enemies.Find(e => !string.IsNullOrEmpty(e.name) &&
                                string.Equals(e.name, enemyIdOrName, StringComparison.OrdinalIgnoreCase));
        return tpl;
    }

    public Enemy CreateEnemy(string enemyIdOrName, int playerLevel)
    {
        var template = FindTemplate(enemyIdOrName);
        if (template == null)
        {
            Debug.LogError($"Enemy template '{enemyIdOrName}' not found!");
            return null;
        }

        // Level delta
        int delta = playerLevel - template.baseLevel;

        // Simple linear scaling with clamps
        int scaledHealth = Mathf.Max(1, template.baseHealth + delta * 10);
        int scaledAttack = Mathf.Max(0, template.baseAttack + delta * 2);
        int scaledDefense = Mathf.Max(0, template.baseDefense + Mathf.RoundToInt(delta * 1f));
        int scaledTalkDefense = Mathf.Max(0, template.baseTalkDefense + Mathf.RoundToInt(delta * 1f));
        int scaledXP = Mathf.Max(0, template.xpReward + Mathf.Max(0, delta) * 5);
        int scaledBits = Mathf.Max(0, template.bitsReward + Mathf.Max(0, delta) * 3);

        // Construct with physical defense
        var enemy = new Enemy(
            template.name,
            playerLevel,
            scaledHealth,
            scaledAttack,
            scaledDefense,
            scaledXP,
            scaledBits
        );

        // Fill remaining fields
        enemy.id = string.IsNullOrWhiteSpace(template.id) ? template.name : template.id;
        enemy.talkDefense = scaledTalkDefense;
        enemy.disposition = template.defaultDisposition;
        enemy.morale = template.baseMorale;
        enemy.willpower = template.baseWillpower;
        enemy.imagePath = template.portraitPath;
        enemy.currentHealth = enemy.maxHealth;

        // Copy moves to runtime structure
        enemy.moves.Clear();
        if (template.moves != null)
        {
            foreach (var m in template.moves)
            {
                if (m == null || string.IsNullOrWhiteSpace(m.id)) continue;
                enemy.moves.Add(new Enemy.EnemyMove
                {
                    id = m.id.Trim(),
                    weight = Mathf.Max(1, m.weight),
                    cooldown = 0
                });
            }
        }

        return enemy;
    }
}