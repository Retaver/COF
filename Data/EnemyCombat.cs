using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class Enemy
{
    public List<StatusEffect> statusEffects = new List<StatusEffect>();

    public List<StatusEffect> StatusEffects => statusEffects;

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }

    public void AddStatusEffect(StatusEffect effect)
    {
        statusEffects.Add(effect);
    }

    public void RemoveStatusEffect(AttackDatabase.StatusEffectType type)
    {
        statusEffects.RemoveAll(e => e.type == type);
    }

    public float GetAccuracyModifier()
    {
        float modifier = 1f;
        foreach (var effect in statusEffects)
        {
            modifier *= effect.GetAccuracyModifier();
        }
        return modifier;
    }

    public float GetDamageModifier()
    {
        float modifier = 1f;
        foreach (var effect in statusEffects)
        {
            modifier *= effect.GetDamageModifier();
        }
        return modifier;
    }

    public bool CanAct()
    {
        return !statusEffects.Any(e => e.type == AttackDatabase.StatusEffectType.Stun);
    }
}