using System;

public interface ICombatUI
{
    // Basic show/hide
    void ShowUI();
    void HideUI();

    // Combat lifecycle
    void StartCombat();
    void StartCombat(params object[] args);
    void EndCombat();
    void EndCombat(params object[] args);

    // Player actions
    void InvokeOnPlayerAction(params object[] args);

    // Logging
    void AddToCombatLog(params object[] args);

    // UI updates
    void RefreshPlayerDisplaySafe();
    void RefreshEnemyDisplaySafe();

    // Visuals
    void SpawnFloatingTextOverPlayer(params object[] args);
    void SpawnFloatingTextOverEnemy(params object[] args);
    void PlayHitFlashOnPlayer(params object[] args);
    void PlayHitFlashOnEnemy(params object[] args);

    // Turn control
    void SetPlayerTurn(bool isPlayerTurn);
    void SetPlayerTurn(params object[] args);
}