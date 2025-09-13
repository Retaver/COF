using System.Collections.Generic;

// Core character interface used by combat/status systems.
// NOTE: keep this minimal and explicit to avoid C# default-interface implementation issues.
public interface ICharacter
{
    // Core info
    string name { get; set; }
    int level { get; set; }

    // Health/vitals
    int health { get; set; }
    int maxHealth { get; set; }

    // Status effects list
    List<StatusEffect> StatusEffects { get; }

    // Actions
    int TakeDamage(int amount);
    void Heal(int amount);
    void AddStatusEffect(StatusEffect effect);

    // Convenience
    bool IsAlive { get; }
}