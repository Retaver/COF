using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Attack Narration Database", menuName = "MLP Game/Attack Narration Database")]
public class AttackNarrationDatabase : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        [Tooltip("Attack id to match (e.g., 'strike', 'stomp', 'magic_bolt', 'talk_taunt'). Must match the id passed from AttackDatabase/UI.")]
        public string id;

        [Header("UI")]
        [Tooltip("Shown as tooltip on the button.")]
        [TextArea] public string shortDescription;

        [Header("Narration Lines (randomly picked)")]
        [Tooltip("Shown immediately when player uses the attack (before hit/miss is resolved).")]
        [TextArea] public string[] useLines;

        [Tooltip("Shown when attack hits.")]
        [TextArea] public string[] hitLines;

        [Tooltip("Shown when attack misses.")]
        [TextArea] public string[] missLines;

        [Tooltip("Shown when attack crits (optional). If empty, falls back to 'hitLines'.")]
        [TextArea] public string[] critLines;
    }

    [SerializeField] private List<Entry> entries = new List<Entry>();

    public Entry Get(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        for (int i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            if (!string.IsNullOrEmpty(e.id) && string.Equals(e.id, id, StringComparison.OrdinalIgnoreCase))
                return e;
        }
        return null;
    }
}