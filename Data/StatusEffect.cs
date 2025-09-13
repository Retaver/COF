using UnityEngine;
using UnityEngine.TextCore.Text;

[System.Serializable]
public class StatusEffect
{
    public AttackDatabase.StatusEffectType type;
    public int duration;
    public int value;
    public string name;
    public string description;

    public StatusEffect(AttackDatabase.StatusEffectType type, int duration, int value = 0)
    {
        this.type = type;
        this.duration = duration;
        this.value = value;

        switch (type)
        {
            case AttackDatabase.StatusEffectType.Blind:
                name = "Blinded";
                description = "Accuracy reduced";
                break;
            case AttackDatabase.StatusEffectType.Poison:
                name = "Poisoned";
                description = $"Takes {value} damage per turn";
                break;
            case AttackDatabase.StatusEffectType.Burn:
                name = "Burning";
                description = $"Takes {value} fire damage per turn";
                break;
            case AttackDatabase.StatusEffectType.Stun:
                name = "Stunned";
                description = "Cannot act";
                break;
            case AttackDatabase.StatusEffectType.Weakness:
                name = "Weakened";
                description = "Damage reduced";
                break;
            case AttackDatabase.StatusEffectType.Shield:
                name = "Shielded";
                description = $"Absorbs {value} damage";
                break;
            case AttackDatabase.StatusEffectType.Regeneration:
                name = "Regenerating";
                description = $"Heals {value} HP per turn";
                break;
            default:
                name = type.ToString();
                description = "";
                break;
        }
    }

    /// <summary>
    /// Compatibility hook — some older code called OnApply when adding effects.
    /// Keep it safe/no-op; shields, etc. don’t need an immediate mutate here.
    /// </summary>
    public void OnApply(ICharacter character)
    {
        // Intentionally empty — runtime impact is handled in ProcessTurn / modifiers.
        // You can put one-shot consequences here if you ever add them.
    }

    /// <summary>
    /// Compatibility hook — some older code called OnRemove when removing effects.
    /// </summary>
    public void OnRemove(ICharacter character)
    {
        // Intentionally empty — nothing to undo for the current effects.
    }

    /// <summary>
    /// Called once per turn; returns true if it produced an immediate visible effect (damage/heal).
    /// </summary>
    public bool ProcessTurn(ICharacter character)
    {
        duration--;

        switch (type)
        {
            case AttackDatabase.StatusEffectType.Poison:
            case AttackDatabase.StatusEffectType.Burn:
                character.TakeDamage(value);
                return true;

            case AttackDatabase.StatusEffectType.Regeneration:
                character.Heal(value);
                return true;

            case AttackDatabase.StatusEffectType.Shield:
                // Shield persists; only consumed via AbsorbDamage
                return false;

            default:
                return false;
        }
    }

    public bool IsExpired => duration <= 0;

    public float GetAccuracyModifier()
    {
        return type == AttackDatabase.StatusEffectType.Blind ? 0.5f : 1f;
    }

    public float GetDamageModifier()
    {
        return type == AttackDatabase.StatusEffectType.Weakness ? 0.7f : 1f;
    }

    /// <summary>
    /// For Shield: subtracts from value until depleted; returns remaining incoming damage.
    /// </summary>
    public int AbsorbDamage(int incomingDamage)
    {
        if (type != AttackDatabase.StatusEffectType.Shield)
            return incomingDamage;

        int absorbed = Mathf.Min(incomingDamage, value);
        value -= absorbed;

        if (value <= 0)
            duration = 0; // Shield depleted

        return incomingDamage - absorbed;
    }

    /// <summary>
    /// Convenience: does this effect block actions?
    /// </summary>
    public bool PreventsAction => type == AttackDatabase.StatusEffectType.Stun;
}
