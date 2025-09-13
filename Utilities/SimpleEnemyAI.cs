// SimpleEnemyAI.cs — cooldown-aware weighted picker
using System;
using System.Collections.Generic;
using UnityEngine;

public static class SimpleEnemyAI
{
    private static System.Random rng = new System.Random();

    public static Enemy.EnemyMove ChooseNextMove(Enemy enemy)
    {
        if (enemy?.moves == null || enemy.moves.Count == 0) return null;

        // Ready pool
        var pool = new List<Enemy.EnemyMove>();
        foreach (var m in enemy.moves)
            if (m != null && m.currentCd <= 0 && !string.IsNullOrWhiteSpace(m.id))
                pool.Add(m);

        if (pool.Count == 0) pool.AddRange(enemy.moves); // all cooling → pick anyway

        int total = 0; foreach (var m in pool) total += Mathf.Max(1, m.weight);
        int roll = rng.Next(1, Math.Max(1, total) + 1);

        foreach (var m in pool)
        {
            roll -= Mathf.Max(1, m.weight);
            if (roll <= 0) return m;
        }
        return pool[pool.Count - 1];
    }

    public static void ApplyBaseCooldown(Enemy.EnemyMove move)
    {
        if (move == null) return;
        move.currentCd = Mathf.Max(move.currentCd, Mathf.Max(0, move.cooldown));
    }

    public static void TickCooldowns(Enemy enemy)
    {
        if (enemy?.moves == null) return;
        foreach (var m in enemy.moves)
            if (m.currentCd > 0) m.currentCd = Mathf.Max(0, m.currentCd - 1);
    }
}
