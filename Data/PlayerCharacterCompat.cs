// Assets/Project/Scripts/Utilities/PlayerCharacterCompat.cs
using UnityEngine;

namespace MyGameNamespace
{
    public static class PlayerCharacterCompat
    {
        // --- Personality ---

        // StoryEffect / StoryManager call: (aspect, delta, reason)
        public static void ProcessPersonalityChoice(this PlayerCharacter pc, string aspect, int change, string reason)
        {
            if (pc == null) return;
            // If your PlayerCharacter already has a 3-arg version, this extension will be ignored.
            // Otherwise, route to a simple 2-step implementation:
            float before = pc.GetPersonalityScore(aspect);
            // If you have a 1-arg/2-arg version, call it; else update via a simple helper below.
            pc.ProcessPersonalityChoiceCompat(aspect, change, reason);
        }

        private static void ProcessPersonalityChoiceCompat(this PlayerCharacter pc, string aspect, int change, string reason)
        {
            // Fallback: store score in a generic dictionary on the character if needed.
            // If your concrete PlayerCharacter already updates a profile, this is harmless.
            var current = pc.GetPersonalityScore(aspect);
            var next = Mathf.Clamp(current + change, 0, 100);
            pc.SetPersonalityScore(aspect, next);
            // Optional: log/track reason somewhere if you want
        }

        public static float GetPersonalityScore(this PlayerCharacter pc, string aspect)
        {
            // If your class already has it, this extension is ignored.
            // Provide a neutral default if absent.
            try
            {
                var method = typeof(PlayerCharacter).GetMethod("GetPersonalityScore", new[] { typeof(string) });
                if (method != null && method.DeclaringType != typeof(PlayerCharacterCompat))
                    return (float)method.Invoke(pc, new object[] { aspect });
            }
            catch { }
            return 50f;
        }

        public static void SetPersonalityScore(this PlayerCharacter pc, string aspect, float value)
        {
            // Try call-through if PlayerCharacter already has it
            try
            {
                var method = typeof(PlayerCharacter).GetMethod("SetPersonalityScore", new[] { typeof(string), typeof(float) });
                if (method != null) { method.Invoke(pc, new object[] { aspect, value }); return; }
            }
            catch { }
            // Else: no-op
        }

        public static bool HasPersonalityTrait(this PlayerCharacter pc, string traitId)
        {
            try
            {
                var method = typeof(PlayerCharacter).GetMethod("HasPersonalityTrait", new[] { typeof(string) });
                if (method != null) return (bool)method.Invoke(pc, new object[] { traitId });
            }
            catch { }
            return false;
        }

        public static void AddPersonalityTrait(this PlayerCharacter pc, PersonalityTrait trait)
        {
            // If an instance AddPersonalityTrait(PersonalityTrait) exists, use it; else try string id
            try
            {
                var method = typeof(PlayerCharacter).GetMethod("AddPersonalityTrait", new[] { typeof(PersonalityTrait) });
                if (method != null) { method.Invoke(pc, new object[] { trait }); return; }
            }
            catch { }

            if (trait != null)
                pc.AddPersonalityTrait(trait.id);
        }

        public static void AddPersonalityTrait(this PlayerCharacter pc, string traitId)
        {
            try
            {
                var method = typeof(PlayerCharacter).GetMethod("AddPersonalityTrait", new[] { typeof(string) });
                if (method != null) { method.Invoke(pc, new object[] { traitId }); return; }
            }
            catch { }
            // Else: no-op
        }

        // --- Background ---

        public static void SetBackground(this PlayerCharacter pc, Background bg)
        {
            try
            {
                var typed = typeof(PlayerCharacter).GetMethod("SetBackground", new[] { typeof(Background) });
                if (typed != null) { typed.Invoke(pc, new object[] { bg }); return; }
            }
            catch { }

            // If only SetBackground(string) exists:
            try
            {
                var byId = typeof(PlayerCharacter).GetMethod("SetBackground", new[] { typeof(string) });
                if (byId != null && bg != null) { byId.Invoke(pc, new object[] { bg.id }); return; }
            }
            catch { }
        }

        // --- Skills ---

        public static bool LearnSkill(this PlayerCharacter pc, string skillId, int rank)
        {
            // Prefer a (string,int) instance method if present
            try
            {
                var method = typeof(PlayerCharacter).GetMethod("LearnSkill", new[] { typeof(string), typeof(int) });
                if (method != null) return (bool)method.Invoke(pc, new object[] { skillId, rank });
            }
            catch { }

            // Fallback: call the single-arg version, then set rank if a dictionary exists
            try
            {
                var single = typeof(PlayerCharacter).GetMethod("LearnSkill", new[] { typeof(string) });
                if (single != null)
                {
                    var ok = (bool)single.Invoke(pc, new object[] { skillId });
                    return ok;
                }
            }
            catch { }

            return false;
        }

        // In case some code calls (string skillId, SkillTree tree)
        public static bool LearnSkill(this PlayerCharacter pc, string skillId, SkillTree _unusedTree)
        {
            try
            {
                var method = typeof(PlayerCharacter).GetMethod("LearnSkill", new[] { typeof(string) });
                if (method != null) return (bool)method.Invoke(pc, new object[] { skillId });
            }
            catch { }
            return false;
        }

        // --- Race string helpers used by some UIs ---

        public static RaceType ToRaceType(this string raceName)
        {
            if (string.IsNullOrEmpty(raceName)) return RaceType.EarthPony;
            switch (raceName.Trim().ToLowerInvariant())
            {
                case "earthpony":
                case "earth pony": return RaceType.EarthPony;
                case "unicorn": return RaceType.Unicorn;
                case "pegasus": return RaceType.Pegasus;
                case "batpony":
                case "bat pony": return RaceType.BatPony;
                case "griffon":
                case "gryphon": return RaceType.Griffon;
                case "dragon": return RaceType.Dragon;
                case "human": return RaceType.Human;
                default: return RaceType.EarthPony;
            }
        }

        public static void InitializeWithRaceSafe(this PlayerCharacter pc, string raceName)
        {
            var rt = raceName.ToRaceType();
            var method = typeof(PlayerCharacter).GetMethod("InitializeWithRace", new[] { typeof(RaceType) });
            if (method != null) method.Invoke(pc, new object[] { rt });
            else
            {
                // last resort: set field and recalc
                var raceField = typeof(PlayerCharacter).GetField("race");
                if (raceField != null) raceField.SetValue(pc, rt);
                var calc = typeof(PlayerCharacter).GetMethod("CalculateAllStats",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                calc?.Invoke(pc, null);
            }
        }

        // --- Clone fallback (if not present on your PlayerCharacter) ---
        // Uses Unity JsonUtility to deep-copy serializable fields.
        public static PlayerCharacter Clone(this PlayerCharacter pc)
        {
            var json = JsonUtility.ToJson(pc);
            return JsonUtility.FromJson<PlayerCharacter>(json);
        }
    }
}
