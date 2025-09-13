using UnityEngine;

namespace MyGameNamespace
{
    /// <summary>
    /// Listens for enemy defeated events and awards experience to the player.
    /// Notifies the GameEventSystem when the player levels up. Attach this
    /// component to a GameObject in your scene (e.g., the GameManager).
    /// </summary>
    public class ExperienceSystem : MonoBehaviour
    {
        private void OnEnable()
        {
            var ges = GameEventSystem.Instance;
            if (ges != null)
            {
                ges.OnEnemyDefeated += OnEnemyDefeated;
            }
        }

        private void OnDisable()
        {
            var ges = GameEventSystem.Instance;
            if (ges != null)
            {
                ges.OnEnemyDefeated -= OnEnemyDefeated;
            }
        }

        private void OnEnemyDefeated(string enemyId, int xp)
        {
            // PlayerState.Current holds the current PlayerCharacter.  Do not attempt
            // to access a non‑existent 'player' property on it.
            var player = PlayerState.Current;
            if (player == null || xp <= 0) return;

            int previousLevel = player.level;
            player.AddExperience(xp);

            // Notify that stats have changed (health/mana may scale with level)
            GameEventSystem.Instance?.RaisePlayerStatsChanged();

            // If the player leveled up, raise an event
            if (player.level > previousLevel)
            {
                GameEventSystem.Instance?.RaisePlayerLeveledUp(player.level);
            }
        }
    }
}