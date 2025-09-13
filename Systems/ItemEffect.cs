using UnityEngine;
using MyGameNamespace;

// Create assets via Unity Editor (CreateAssetMenu).
// Do NOT instantiate ItemEffect with 'new'—use Unity's ScriptableObject asset system.
// Store assets in: Assets/Data/Resources/ItemEffects/

[CreateAssetMenu(fileName = "New Item Effect", menuName = "Game/Item Effect")]
public abstract class ItemEffect : ScriptableObject
{
    [SerializeField] protected string effectName;
    [SerializeField] protected string description;

    public string EffectName => effectName;
    public string Description => description;

    public abstract void Apply(PlayerCharacter player);
    public abstract bool CanUse(PlayerCharacter player);
}

// Health potion effect
[CreateAssetMenu(fileName = "Health Effect", menuName = "Game/Effects/Health Effect")]
public class HealthEffect : ItemEffect
{
    [SerializeField] private int healthAmount = 25;

    public override void Apply(PlayerCharacter player)
    {
        if (player?.gameStats != null)
        {
            int oldHealth = player.gameStats.health;
            player.gameStats.health = Mathf.Min(player.gameStats.health + healthAmount, player.gameStats.maxHealth);
            Debug.Log($"Used Health Effect: {oldHealth} -> {player.gameStats.health} HP");

            GameEventSystem.Instance?.RaisePlayerStatsChanged(player);
        }
    }

    public override bool CanUse(PlayerCharacter player)
    {
        return player?.gameStats != null && player.gameStats.health < player.gameStats.maxHealth;
    }
}

// ... other ItemEffect subclasses unchanged ...