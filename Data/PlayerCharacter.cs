using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MyGameNamespace
{
    [System.Serializable]
    public class LegacyPlayerCharacter : ICharacter
    {
        [Header("Basic Info (Legacy - hidden to avoid conflicts)")]
        [HideInInspector] public string name;
        [HideInInspector] public string raceString;        // kept for backward compat
        [HideInInspector] public string backgroundString;
        [HideInInspector] public string gender;

        [Header("Stats (Legacy - hidden to avoid conflicts)")]
        [HideInInspector] public int strength;
        [HideInInspector] public int dexterity;
        [HideInInspector] public int constitution;
        [HideInInspector] public int intelligence;
        [HideInInspector] public int wisdom;
        [HideInInspector] public int charisma;

        [Header("Derived Stats (Legacy - hidden to avoid conflicts)")]
        [HideInInspector] public int level;
        [HideInInspector] public int hitPoints;
        [HideInInspector] public int maxHitPoints;
        [HideInInspector] public int experience;

        [Header("Combat")]
        public List<StatusEffect> statusEffects = new List<StatusEffect>();

        // ===== Explicit ICharacter property implementations =====
        string ICharacter.name
        {
            get => name;
            set => name = value;
        }

        int ICharacter.level
        {
            get => level;
            set => level = value;
        }

        int ICharacter.health
        {
            get => hitPoints;
            set
            {
                hitPoints = value;
                if (gameStats != null) gameStats.health = value;
            }
        }

        int ICharacter.maxHealth
        {
            get => maxHitPoints;
            set
            {
                maxHitPoints = value;
                if (gameStats != null) gameStats.maxHealth = value;
            }
        }

        List<StatusEffect> ICharacter.StatusEffects => statusEffects;
        bool ICharacter.IsAlive => hitPoints > 0;

        // ===== Modern Systems =====
        [Header("Modern Systems")]
        public CharacterStats stats;
        public GameStats gameStats;
        public List<PerkType> perks = new List<PerkType>();
        public int bits = 100;
        public int skillPoints = 0;
        protected int _perkPoints = 0;  // Changed from private to protected

        public int perkPoints => _perkPoints;

        // Race property that works with both string and enum
        public RaceType race
        {
            get
            {
                if (System.Enum.TryParse<RaceType>(raceString, out RaceType result))
                    return result;
                return RaceType.EarthPony;
            }
            set
            {
                raceString = value.ToString();
            }
        }

        // Background property
        public Background background { get; set; }

        // Level system
        public LevelSystem levelSystem = new LevelSystem();

        // Current location
        public GameLocation currentLocation;

        // Legacy helpers for UI text
        public string RaceText => race.ToString();

        public class Inventory
        {
            public List<InventoryItem> items = new List<InventoryItem>();
            public int capacity = 30;
        }

        public Inventory inventory = new Inventory();

        // ===== Legacy calc/helpers =====
        public void InitializeModernSystems()
        {
            if (stats == null) stats = new CharacterStats();
            if (gameStats == null) gameStats = new GameStats();
            if (levelSystem == null) levelSystem = new LevelSystem();

            // Initialize defaults if needed
            if (maxHitPoints <= 0) maxHitPoints = 10 + (constitution / 2);
            if (hitPoints <= 0) hitPoints = maxHitPoints;

            // Sync legacy to modern
            gameStats.maxHealth = maxHitPoints;
            gameStats.health = hitPoints;
        }

        public void CalculateDerivedStats()
        {
            if (stats == null) stats = new CharacterStats();
            if (gameStats == null) gameStats = new GameStats();

            maxHitPoints = Mathf.Max(1, 10 + GetModifier(constitution) + stats.GetTotalStat(StatType.Constitution));
            hitPoints = Mathf.Clamp(hitPoints, 0, maxHitPoints);
            gameStats.maxHealth = maxHitPoints;
            gameStats.health = hitPoints;
        }

        public int GetModifier(int statValue) => (statValue - 10) / 2;

        // Non-interface public method retained for gameplay scripts
        public void TakeDamage(int damage)
        {
            hitPoints = Mathf.Max(0, hitPoints - damage);
            if (stats != null) stats.health = hitPoints;
            if (gameStats != null) gameStats.health = hitPoints;
        }

        // Explicit interface method
        int ICharacter.TakeDamage(int damage)
        {
            foreach (var shield in statusEffects.Where(e => e.type == AttackDatabase.StatusEffectType.Shield))
            {
                damage = shield.AbsorbDamage(damage);
                if (damage <= 0) break;
            }

            hitPoints = Mathf.Max(0, hitPoints - Mathf.Max(0, damage));
            if (stats != null) stats.health = hitPoints;
            if (gameStats != null) gameStats.health = hitPoints;
            return hitPoints;
        }

        public void Heal(int amount)
        {
            hitPoints = Mathf.Min(maxHitPoints, hitPoints + Mathf.Max(0, amount));
            if (stats != null) stats.health = hitPoints;
            if (gameStats != null) gameStats.health = hitPoints;
        }

        public void AddStatusEffect(StatusEffect effect)
        {
            if (effect == null) return;
            statusEffects.Add(effect);
            effect.OnApply(this);
        }

        public void RemoveStatusEffect(StatusEffect effect)
        {
            if (effect == null) return;
            if (statusEffects.Remove(effect))
                effect.OnRemove(this);
        }

        public bool CanAct() => !statusEffects.Any(e => e.type == AttackDatabase.StatusEffectType.Stun);

        // Personality score helpers
        private Dictionary<string, int> _personalityScores;
        public Dictionary<string, int> PersonalityScores => _personalityScores ??= new Dictionary<string, int>();

        public int GetPersonalityScore(string aspect) => PersonalityScores.TryGetValue(aspect, out var v) ? v : 0;
        public void SetPersonalityScore(string aspect, int value)
        {
            if (string.IsNullOrEmpty(aspect)) return;
            PersonalityScores[aspect] = value;
        }
        public void ModifyPersonalityScore(string aspect, int delta) => SetPersonalityScore(aspect, GetPersonalityScore(aspect) + delta);
        public Dictionary<string, int> GetAllPersonalityScores() => new Dictionary<string, int>(PersonalityScores);
        public bool HasPersonalityAspect(string aspect) => PersonalityScores.ContainsKey(aspect);
        public void RemovePersonalityAspect(string aspect) => PersonalityScores.Remove(aspect);

        public override string ToString() => $"{name} ({race} {background?.name ?? "None"}) - Level {level}";
    }

    public partial class PlayerCharacter : LegacyPlayerCharacter
    {
        // Personality system properties
        public PersonalityProfile personalityProfile = new PersonalityProfile();
        public List<PersonalityTrait> personalityTraits = new List<PersonalityTrait>();

        // ===== Missing Methods Implementation =====

        public void InitializePersonalitySystem()
        {
            if (personalityProfile == null)
                personalityProfile = new PersonalityProfile();

            if (personalityTraits == null)
                personalityTraits = new List<PersonalityTrait>();
        }

        public void InitializeWithRace(RaceType raceType)
        {
            // Ensure modern systems exist before applying race data so statBonuses & starting perks are applied.
            InitializeModernSystems();

            race = raceType;

            // Fetch race data and apply bonuses/perks
            var raceData = MLPDatabase.Instance?.GetRace(raceType);
            if (raceData != null)
            {
                ApplyRacialBonuses(raceData);
            }

            CalculateAllStats();
        }

        public void CalculateAllStats()
        {
            InitializeModernSystems();
            CalculateDerivedStats();

            if (gameStats != null)
            {
                gameStats.CalculateFromStats(stats);
            }
        }

        public void ApplyRacialBonuses(Race race)
        {
            if (race == null) return;

            // Ensure stats object exists so stat bonuses can be applied
            if (stats == null) stats = new CharacterStats();

            // Apply stat bonuses if present
            if (race.statBonuses != null)
            {
                foreach (var bonus in race.statBonuses)
                {
                    if (!stats.bonusStats.ContainsKey(bonus.Key))
                        stats.bonusStats[bonus.Key] = 0;
                    stats.bonusStats[bonus.Key] += bonus.Value;
                }
            }

            // Apply starting perks regardless of stats being present
            if (race.startingPerks != null)
            {
                foreach (var perk in race.startingPerks)
                {
                    if (!perks.Contains(perk))
                    {
                        perks.Add(perk);
                        Debug.Log($"[PlayerCharacter] Added starting perk '{perk}' for race '{(race?.name ?? race.ToString())}'");
                    }
                }
            }
        }

        public void AddExperience(int amount)
        {
            if (levelSystem == null) levelSystem = new LevelSystem();

            levelSystem.AddExperience(amount);
            level = levelSystem.level;
            skillPoints = levelSystem.skillPoints;
            _perkPoints = levelSystem.perkPoints;
        }

        public bool AddPerk(PerkType perkType)
        {
            if (perks.Contains(perkType)) return false;
            if (_perkPoints <= 0) return false;

            var perk = MLPDatabase.Instance?.GetPerk(perkType);
            if (perk == null) return false;

            if (!perk.IsAvailableForRace(race)) return false;
            if (level < perk.levelRequirement) return false;

            // Check stat requirements
            foreach (var req in perk.statRequirements)
            {
                if (stats.GetTotalStat(req.Key) < req.Value) return false;
            }

            // Check prerequisites
            foreach (var prereq in perk.prerequisites)
            {
                if (!perks.Contains(prereq)) return false;
            }

            perks.Add(perkType);
            _perkPoints--;

            // Apply perk effects
            foreach (var bonus in perk.statBonuses)
            {
                stats.bonusStats[bonus.Key] += bonus.Value;
            }

            CalculateAllStats();
            return true;
        }

        public bool AllocateStatPoint(StatType stat, int points = 1)
        {
            if (levelSystem == null) return false;
            if (levelSystem.statPoints < points) return false;

            stats.baseStats[stat] += points;
            levelSystem.statPoints -= points;
            CalculateAllStats();
            return true;
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

        public void ProcessPersonalityChoice(string aspect, int change, string reason)
        {
            if (personalityProfile == null) InitializePersonalitySystem();

            switch (aspect.ToLower())
            {
                case "kindness":
                    personalityProfile.kindness = Mathf.Clamp(personalityProfile.kindness + change, 0, 100);
                    break;
                case "confidence":
                    personalityProfile.confidence = Mathf.Clamp(personalityProfile.confidence + change, 0, 100);
                    break;
                case "curiosity":
                    personalityProfile.curiosity = Mathf.Clamp(personalityProfile.curiosity + change, 0, 100);
                    break;
                case "optimism":
                    personalityProfile.optimism = Mathf.Clamp(personalityProfile.optimism + change, 0, 100);
                    break;
                case "honesty":
                    personalityProfile.honesty = Mathf.Clamp(personalityProfile.honesty + change, 0, 100);
                    break;
                case "assertiveness":
                    personalityProfile.assertiveness = Mathf.Clamp(personalityProfile.assertiveness + change, 0, 100);
                    break;
            }
        }

        public bool HasPersonalityTrait(string traitId)
        {
            return personalityTraits?.Any(t => t.id == traitId) ?? false;
        }

        public void AddPersonalityTrait(PersonalityTrait trait)
        {
            if (trait == null || personalityTraits == null) return;
            if (!personalityTraits.Any(t => t.id == trait.id))
                personalityTraits.Add(trait);
        }

        public void AddPersonalityTrait(string traitId)
        {
            var trait = PersonalityDatabase.GetTrait(traitId);
            if (trait != null)
                AddPersonalityTrait(trait);
        }

        public void SetBackground(Background bg)
        {
            background = bg;
            if (bg?.statBonuses != null)
            {
                foreach (var bonus in bg.statBonuses)
                {
                    stats.bonusStats[bonus.Key] += bonus.Value;
                }
            }
        }

        public bool LearnSkill(string skillId, SkillTree skillTree)
        {
            var skill = skillTree?.GetSkill(skillId);
            if (skill == null) return false;

            if (!skill.CanUpgrade(this, skillTree)) return false;

            skill.UpgradeSkill(this, skillTree);
            return true;
        }

        public bool LearnSkill(string skillId, int rank)
        {
            var skillTree = CompatUtils.FindFirstObjectByTypeCompat<SkillTree>();
            return LearnSkill(skillId, skillTree);
        }

        public PlayerCharacter Clone()
        {
            var clone = new PlayerCharacter();

            // Copy basic info
            clone.name = this.name;
            clone.race = this.race;
            clone.gender = this.gender;
            clone.level = this.level;
            clone.bits = this.bits;
            clone.skillPoints = this.skillPoints;
            clone._perkPoints = this._perkPoints;

            // Deep copy systems
            clone.stats = this.stats?.Clone();
            clone.gameStats = this.gameStats?.Clone();
            clone.levelSystem = this.levelSystem?.Clone();

            // Copy collections
            clone.perks = new List<PerkType>(this.perks);
            clone.personalityTraits = new List<PersonalityTrait>(this.personalityTraits);

            // Copy personality profile
            if (this.personalityProfile != null)
            {
                clone.personalityProfile = new PersonalityProfile
                {
                    kindness = this.personalityProfile.kindness,
                    confidence = this.personalityProfile.confidence,
                    curiosity = this.personalityProfile.curiosity,
                    optimism = this.personalityProfile.optimism,
                    honesty = this.personalityProfile.honesty,
                    assertiveness = this.personalityProfile.assertiveness,
                    primaryArchetype = this.personalityProfile.primaryArchetype
                };
            }

            clone.background = this.background;
            clone.currentLocation = this.currentLocation;

            return clone;
        }
    }
}