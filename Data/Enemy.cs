using UnityEngine;

[System.Serializable]
public partial class Enemy : ICharacter
{
    public string id;
    public string name;
    public int level;

    // Core stats
    public int maxHealth;
    public int currentHealth;
    public int attack;
    public int defense;

    // Social/Resolve
    [Range(0, 100)] public int morale = 60;
    [Range(0, 100)] public int willpower = 50;
    public int talkDefense = 0;
    public EnemyDisposition disposition = EnemyDisposition.Hostile;

    // Rewards
    public int xpReward = 100;
    public int bitsReward = 10;

    // Optional: icon/sprite path (Resources path without extension)
    public string imagePath = "";

    // ICharacter explicit interface implementations
    string ICharacter.name { get => name; set => name = value; }
    int ICharacter.level { get => level; set => level = value; }
    int ICharacter.health { get => currentHealth; set => currentHealth = value; }
    int ICharacter.maxHealth { get => maxHealth; set => maxHealth = value; }
    bool ICharacter.IsAlive => currentHealth > 0;

    public Enemy() { }

    public Enemy(string name, int level, int maxHealth, int attack, int defense, int xpReward = 100, int bitsReward = 10)
    {
        this.name = name;
        this.level = level;
        this.maxHealth = maxHealth;
        this.currentHealth = maxHealth;
        this.attack = attack;
        this.defense = defense;
        this.xpReward = xpReward;
        this.bitsReward = bitsReward;
    }

    public int TakeDamage(int rawDamage)
    {
        int damage = Mathf.Max(0, rawDamage - defense);
        currentHealth = Mathf.Max(0, currentHealth - damage);
        return damage;
    }

    // ICharacter interface method
    int ICharacter.TakeDamage(int damage) => TakeDamage(damage);

    public bool IsDefeated => currentHealth <= 0;

    public void AdjustDisposition(int steps)
    {
        int n = Mathf.Clamp((int)disposition + steps, (int)EnemyDisposition.Hostile, (int)EnemyDisposition.Enamored);
        disposition = (EnemyDisposition)n;
    }

    public string GetDispositionText()
    {
        switch (disposition)
        {
            case EnemyDisposition.Hostile: return "Hostile";
            case EnemyDisposition.Aggressive: return "Aggressive";
            case EnemyDisposition.Neutral: return "Neutral";
            case EnemyDisposition.Friendly: return "Friendly";
            case EnemyDisposition.Enamored: return "Enamored";
            default: return "Unknown";
        }
    }
}