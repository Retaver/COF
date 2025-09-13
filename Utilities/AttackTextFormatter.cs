using UnityEngine;

public static class AttackTextFormatter
{
    public struct Context
    {
        public string actor;      // Player’s name
        public string target;     // Enemy’s name
        public string attack;     // Attack display name or id
        public int damage;        // Damage dealt (0 if miss)
        public int hp;            // Target current HP after result
        public int maxHp;         // Target max HP
        public int enCost;        // EN cost
        public int mpCost;        // MP cost
        public bool crit;         // Critical hit
        public string status;     // Optional status text (e.g., "Bleeding", "Stunned")
    }

    // Supported tokens in lines: {actor} {target} {attack} {damage} {hp} {maxHp} {hpPct} {en} {mp} {status}
    public static string Format(string line, in Context ctx)
    {
        if (string.IsNullOrEmpty(line)) return string.Empty;

        float pct = ctx.maxHp > 0 ? (ctx.hp / (float)ctx.maxHp) * 100f : 0f;

        string s = line;
        s = s.Replace("{actor}", ctx.actor ?? "You");
        s = s.Replace("{target}", ctx.target ?? "enemy");
        s = s.Replace("{attack}", ctx.attack ?? "attack");
        s = s.Replace("{damage}", ctx.damage.ToString());
        s = s.Replace("{hp}", Mathf.Max(0, ctx.hp).ToString());
        s = s.Replace("{maxHp}", Mathf.Max(1, ctx.maxHp).ToString());
        s = s.Replace("{hpPct}", Mathf.RoundToInt(pct).ToString());
        s = s.Replace("{en}", Mathf.Max(0, ctx.enCost).ToString());
        s = s.Replace("{mp}", Mathf.Max(0, ctx.mpCost).ToString());
        s = s.Replace("{status}", string.IsNullOrEmpty(ctx.status) ? "" : ctx.status);

        return s;
    }

    public static string PickOne(string[] lines)
    {
        if (lines == null || lines.Length == 0) return null;
        int idx = Random.Range(0, lines.Length);
        return lines[idx];
    }
}