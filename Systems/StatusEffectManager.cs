using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StatusEffectManager
{
    public static void ProcessStatusEffects(ICharacter character, CombatUI combatUI)
    {
        var effectsToRemove = new List<StatusEffect>();

        foreach (var effect in character.StatusEffects)
        {
            bool hadImmediateEffect = effect.ProcessTurn(character);

            if (hadImmediateEffect)
            {
                string message = "";
                switch (effect.type)
                {
                    case AttackDatabase.StatusEffectType.Poison:
                        message = $"{character.name} takes {effect.value} poison damage!";
                        break;
                    case AttackDatabase.StatusEffectType.Burn:
                        message = $"{character.name} takes {effect.value} fire damage!";
                        break;
                    case AttackDatabase.StatusEffectType.Regeneration:
                        message = $"{character.name} heals {effect.value} HP!";
                        break;
                }

                if (!string.IsNullOrEmpty(message))
                    combatUI.AddToCombatLog(message);
            }

            if (effect.IsExpired)
            {
                effectsToRemove.Add(effect);
                combatUI.AddToCombatLog($"{character.name} recovers from {effect.name}.");
            }
        }

        foreach (var effect in effectsToRemove)
        {
            character.StatusEffects.Remove(effect);
        }
    }

    public static void ApplyStatusEffect(ICharacter target, AttackDatabase.StatusEffectChance effectChance, CombatUI combatUI)
    {
        if (Random.Range(0f, 1f) <= effectChance.chance)
        {
            var effect = new StatusEffect(effectChance.effectType, effectChance.duration, effectChance.value);

            var existingEffect = target.StatusEffects.FirstOrDefault(e => e.type == effect.type);
            if (existingEffect != null)
            {
                existingEffect.duration = Mathf.Max(existingEffect.duration, effect.duration);
                if (effect.type == AttackDatabase.StatusEffectType.Shield)
                {
                    existingEffect.value += effect.value;
                }
            }
            else
            {
                target.AddStatusEffect(effect);
            }

            combatUI.AddToCombatLog($"{target.name} is {effect.name.ToLower()}!");
        }
    }
}
