
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement; // Added for scene loading
using MyGameNamespace; // for GameManager, CompatUtils, etc.

namespace MyGameNamespace
{
    public class CharacterCreationController : MonoBehaviour
    {
        #region Enums / Data

        public enum Stat { Strength, Dexterity, Constitution, Intelligence, Wisdom, Charisma }

        public enum Race
        {
            EarthPony, Pegasus, Unicorn, BatPony, Changeling, Griffon, Dragon, Human
        }

        public enum Background
        {
            Drifter, Farmhand, Scholar, Guard, Artisan, Outcast
        }

        public enum Gender { Female, Male }

        public enum BodyBuild { Slender, Average, Sturdy, Muscular, Plush }

        [Serializable]
        public class CharacterCreationData
        {
            public string Name = "Anon";
            public Gender Gender = Gender.Female;
            public Race Race = Race.Human;
            public Background Background = Background.Drifter;
            public BodyBuild Build = BodyBuild.Average;

            public int PointsRemaining = 10;
            public Dictionary<Stat, int> Stats = new Dictionary<Stat, int>
            {
                { Stat.Strength, 5 },
                { Stat.Dexterity, 5 },
                { Stat.Constitution, 5 },
                { Stat.Intelligence, 5 },
                { Stat.Wisdom, 5 },
                { Stat.Charisma, 5 },
            };
        }

        #endregion

        #region UI refs

        private UIDocument _doc;
        private VisualElement _root;

        private TextField nameField;
        private Label stanceValue;
        private TextField stanceField;

        // gender pills
        private Button femaleBtn, maleBtn;

        // race pills
        private readonly Dictionary<Race, Button> raceButtons = new Dictionary<Race, Button>();

        // background pills
        private readonly Dictionary<Background, Button> bgButtons = new Dictionary<Background, Button>();

        // build pills (support legacy BuildLithe → Slender rename)
        private Button buildSlender, buildAverage, buildSturdy, buildMuscular, buildPlush, buildLitheLegacy;
        private Label buildDesc;

        // points + stat rows
        private Label pointsLabel;

        private class StatRow
        {
            public Button Minus;
            public Label Value;
            public Button Plus;
        }
        private readonly Dictionary<Stat, StatRow> statUI = new Dictionary<Stat, StatRow>();

        // nav
        private Button backBtn, finishBtn;

        #endregion

        private readonly CharacterCreationData data = new CharacterCreationData();

        // Cache portrait element for runtime sprite assignment
        private VisualElement portraitElement;
        // Stores the current portrait resource path selected for race/gender
        private string currentPortraitResourcePath;

        // Helpers to update portrait based on current race/gender
        private void UpdatePortrait()
        {
            if (portraitElement == null) return;
            // Determine resource path based on race and gender
            string resourcePath = null;
            switch (data.Race)
            {
                case Race.EarthPony:
                    resourcePath = data.Gender == Gender.Female
                        ? "Portraits/earthpony_female/Earth Pony Female"
                        : "Portraits/Earth Pony Male";
                    break;
                case Race.Pegasus:
                    resourcePath = data.Gender == Gender.Female
                        ? "Portraits/Earth Pony Male" // fallback if missing specific asset
                        : "Portraits/Earth Pony Male";
                    break;
                case Race.Unicorn:
                    resourcePath = data.Gender == Gender.Female
                        ? "Portraits/Earth Pony Male"
                        : "Portraits/Earth Pony Male";
                    break;
                case Race.BatPony:
                    resourcePath = data.Gender == Gender.Female
                        ? "Portraits/Earth Pony Male"
                        : "Portraits/Earth Pony Male";
                    break;
                case Race.Changeling:
                    resourcePath = data.Gender == Gender.Female
                        ? "Portraits/Earth Pony Male"
                        : "Portraits/Earth Pony Male";
                    break;
                case Race.Griffon:
                    resourcePath = data.Gender == Gender.Female
                        ? "Portraits/Earth Pony Male"
                        : "Portraits/Earth Pony Male";
                    break;
                case Race.Dragon:
                    resourcePath = data.Gender == Gender.Female
                        ? "Portraits/Earth Pony Male"
                        : "Portraits/Earth Pony Male";
                    break;
                case Race.Human:
                    resourcePath = data.Gender == Gender.Female
                        ? "Portraits/Earth Pony Male"
                        : "Portraits/Earth Pony Male";
                    break;
                default:
                    resourcePath = null;
                    break;
            }
            if (!string.IsNullOrEmpty(resourcePath))
            {
                var tex = Resources.Load<Texture2D>(resourcePath);
                if (tex != null)
                {
                    portraitElement.style.backgroundImage = new StyleBackground(tex);
                    // Remember the path for saving into the PlayerCharacter later
                    currentPortraitResourcePath = resourcePath;
                }
            }
        }

        #region Unity

        private void Awake()
        {
            _doc = GetComponent<UIDocument>();
            _root = _doc != null ? _doc.rootVisualElement : null;

            if (_root == null)
            {
                Debug.LogError("[CharacterCreation] No UIDocument / rootVisualElement found.");
                return;
            }

            QueryWidgets();
            WireEvents();
            RefreshAll();
        }

        #endregion

        #region Query / Helpers

        private T Q<T>(string name) where T : VisualElement
            => string.IsNullOrEmpty(name) ? null : _root.Q<T>(name);


        // === Compatibility helpers for multiple UXML naming schemes ===
        private Button QB(params string[] names)
        {
            foreach (var n in names)
            {
                var b = Q<Button>(n);
                if (b != null) return b;
            }
            return null;
        }
        private Label QL(params string[] names)
        {
            foreach (var n in names)
            {
                var l = Q<Label>(n);
                if (l != null) return l;
            }
            return null;
        }
        private TextField QT(params string[] names)
        {
            foreach (var n in names)
            {
                var t = Q<TextField>(n);
                if (t != null) return t;
            }
            return null;
        }

        private StatRow FindStatCompat(Stat s)
        {
            // Try container rows first (StrengthRow -> Minus/Value/Plus)
            string rowName = s switch
            {
                Stat.Strength => "StrengthRow",
                Stat.Dexterity => "DexterityRow",
                Stat.Constitution => "ConstitutionRow",
                Stat.Intelligence => "IntelligenceRow",
                Stat.Wisdom => "WisdomRow",
                Stat.Charisma => "CharismaRow",
                _ => null
            };
            var row = string.IsNullOrEmpty(rowName) ? null : Q<VisualElement>(rowName);
            if (row != null)
            {
                return new StatRow
                {
                    Minus = row.Q<Button>("Minus"),
                    Value = row.Q<Label>("Value"),
                    Plus = row.Q<Button>("Plus"),
                };
            }

            // Fallback: individual element names like "StrMinus", "StrValue", "StrPlus"
            string prefix = s switch
            {
                Stat.Strength => "Str",
                Stat.Dexterity => "Dex",
                Stat.Constitution => "Con",
                Stat.Intelligence => "Int",
                Stat.Wisdom => "Wis",
                Stat.Charisma => "Cha",
                _ => null
            };
            if (string.IsNullOrEmpty(prefix)) return null;
            return new StatRow
            {
                Minus = QB(prefix + "Minus"),
                Value = QL(prefix + "Value"),
                Plus = QB(prefix + "Plus")
            };
        }
        private StatRow FindStat(string rowName)
        {
            var row = Q<VisualElement>(rowName);
            if (row == null) return null;

            return new StatRow
            {
                Minus = row.Q<Button>("Minus"),
                Value = row.Q<Label>("Value"),
                Plus = row.Q<Button>("Plus")
            };
        }

        private void ToggleSelected(VisualElement ve, bool on)
        {
            if (ve == null) return;
            if (on) ve.AddToClassList("selected");
            else ve.RemoveFromClassList("selected");
        }

        private static bool IsBipedal(Race r) => r == Race.Human || r == Race.Dragon;
        private string StanceText(Race r) => IsBipedal(r) ? "Bipedal" : "Quadruped";

        #endregion

        #region Query all widgets

        private void QueryWidgets()
        {
            nameField = Q<TextField>("NameField");
            stanceValue = Q<Label>("StanceValue");
            stanceField = QT("StanceField");

            // Gender
            femaleBtn = Q<Button>("FemaleButton");
            maleBtn = Q<Button>("MaleButton");

            // Race pills
            raceButtons[Race.EarthPony] = QB("RaceEarthPony", "Race_EarthPony");
            raceButtons[Race.Pegasus] = QB("RacePegasus", "Race_Pegasus");
            raceButtons[Race.Unicorn] = QB("RaceUnicorn", "Race_Unicorn");
            raceButtons[Race.BatPony] = QB("RaceBatPony", "Race_BatPony");
            raceButtons[Race.Changeling] = QB("RaceChangeling", "Race_Changeling");
            raceButtons[Race.Griffon] = QB("RaceGriffon", "Race_Griffon");
            raceButtons[Race.Dragon] = QB("RaceDragon", "Race_Dragon");
            raceButtons[Race.Human] = QB("RaceHuman", "Race_Human");

            // Background pills
            bgButtons[Background.Drifter] = QB("BgDrifter", "Bg_Drifter");
            bgButtons[Background.Farmhand] = QB("BgFarmhand", "Bg_Farmhand");
            bgButtons[Background.Scholar] = QB("BgScholar", "Bg_Scholar");
            bgButtons[Background.Guard] = QB("BgGuard", "Bg_Guard");
            bgButtons[Background.Artisan] = QB("BgArtisan", "Bg_Artisan");
            bgButtons[Background.Outcast] = QB("BgOutcast", "Bg_Outcast");

            // Build pills (new ids + legacy)
            buildSlender = Q<Button>("BuildSlender");
            buildLitheLegacy = Q<Button>("BuildLithe"); // legacy
            buildAverage = Q<Button>("BuildAverage");
            buildSturdy = Q<Button>("BuildSturdy");
            buildMuscular = Q<Button>("BuildMuscular");
            buildPlush = Q<Button>("BuildPlush");

            if (buildLitheLegacy != null) buildLitheLegacy.text = "Slender";

            buildDesc = Q<Label>("BuildDescLabel")
                        ?? Q<Label>("BuildDescription")
                        ?? Q<Label>("BuildDesc");

            pointsLabel = Q<Label>("PointsLabel") ?? Q<Label>("PointsTitle");

            // Stat rows
            statUI[Stat.Strength] = FindStatCompat(Stat.Strength);
            statUI[Stat.Dexterity] = FindStatCompat(Stat.Dexterity);
            statUI[Stat.Constitution] = FindStatCompat(Stat.Constitution);
            statUI[Stat.Intelligence] = FindStatCompat(Stat.Intelligence);
            statUI[Stat.Wisdom] = FindStatCompat(Stat.Wisdom);
            statUI[Stat.Charisma] = FindStatCompat(Stat.Charisma);

            // Nav
            backBtn = Q<Button>("BackButton");
            finishBtn = Q<Button>("FinishButton");

            // Portrait (optional) for gender/race preview
            portraitElement = Q<VisualElement>("Portrait");
        }

        #endregion

        #region Wire events

        private void WireEvents()
        {
            // Name
            if (nameField != null)
                nameField.RegisterValueChangedCallback(evt => data.Name = evt.newValue ?? "Anon");

            // Gender
            if (femaleBtn != null) femaleBtn.clicked += () => SetGender(Gender.Female);
            if (maleBtn != null) maleBtn.clicked += () => SetGender(Gender.Male);

            // Race pills
            foreach (var kv in raceButtons)
            {
                if (kv.Value == null) continue;
                var r = kv.Key; kv.Value.clicked += () => SetRace(r);
            }

            // Background pills
            foreach (var kv in bgButtons)
            {
                if (kv.Value == null) continue;
                var b = kv.Key; kv.Value.clicked += () => SetBackground(b);
            }

            // Build pills (including legacy)
            if (buildSlender != null) buildSlender.clicked += () => SetBuild(BodyBuild.Slender);
            if (buildLitheLegacy != null) buildLitheLegacy.clicked += () => SetBuild(BodyBuild.Slender);
            if (buildAverage != null) buildAverage.clicked += () => SetBuild(BodyBuild.Average);
            if (buildSturdy != null) buildSturdy.clicked += () => SetBuild(BodyBuild.Sturdy);
            if (buildMuscular != null) buildMuscular.clicked += () => SetBuild(BodyBuild.Muscular);
            if (buildPlush != null) buildPlush.clicked += () => SetBuild(BodyBuild.Plush);

            // Stat +/- rows
            foreach (var row in statUI)
            {
                var stat = row.Key; var ui = row.Value;
                if (ui == null) continue;
                if (ui.Minus != null) ui.Minus.clicked += () => DecrementStat(stat);
                if (ui.Plus != null) ui.Plus.clicked += () => IncrementStat(stat);
            }

            // Nav
            if (backBtn != null) backBtn.clicked += OnBack;
            if (finishBtn != null) finishBtn.clicked += OnFinish;
        }

        #endregion

        #region Setters

        private void SetGender(Gender g)
        {
            data.Gender = g;
            ToggleSelected(femaleBtn, g == Gender.Female);
            ToggleSelected(maleBtn, g == Gender.Male);
            // Update portrait preview when gender changes
            UpdatePortrait();
        }

        private void SetRace(Race r)
        {
            data.Race = r;
            foreach (var kv in raceButtons)
                ToggleSelected(kv.Value, kv.Key == r);

            if (stanceValue != null) stanceValue.text = StanceText(r);
            if (stanceField != null) stanceField.value = StanceText(r);
            // Update portrait preview when race changes
            UpdatePortrait();
        }

        private void SetBackground(Background b)
        {
            data.Background = b;
            foreach (var kv in bgButtons)
                ToggleSelected(kv.Value, kv.Key == b);
        }

        private void SetBuild(BodyBuild b)
        {
            data.Build = b;

            ToggleSelected(buildSlender, b == BodyBuild.Slender);
            ToggleSelected(buildLitheLegacy, b == BodyBuild.Slender);
            ToggleSelected(buildAverage, b == BodyBuild.Average);
            ToggleSelected(buildSturdy, b == BodyBuild.Sturdy);
            ToggleSelected(buildMuscular, b == BodyBuild.Muscular);
            ToggleSelected(buildPlush, b == BodyBuild.Plush);

            if (buildDesc != null)
            {
                string txt =
                    b == BodyBuild.Slender ? "Slender: +Dexterity, −Constitution. Agile and quick." :
                    b == BodyBuild.Average ? "Average: Balanced build, no tradeoffs." :
                    b == BodyBuild.Sturdy ? "Sturdy: +Constitution, −Dexterity. Steady and tough." :
                    b == BodyBuild.Muscular ? "Muscular: +Strength, −Charisma. Powerful physique." :
                    b == BodyBuild.Plush ? "Plush: +Charisma, −Strength. Resilient and personable." :
                                               string.Empty;
                buildDesc.text = txt;
            }
        }

        #endregion

        #region Stats

        private const int StatMin = 1;
        private const int StatMax = 15;

        private void IncrementStat(Stat s)
        {
            if (data.PointsRemaining <= 0) return;
            var cur = data.Stats[s];
            if (cur >= StatMax) return;

            data.Stats[s] = cur + 1;
            data.PointsRemaining--;
            RefreshPointsAndStat(s);
        }

        private void DecrementStat(Stat s)
        {
            var cur = data.Stats[s];
            if (cur <= StatMin) return;

            data.Stats[s] = cur - 1;
            data.PointsRemaining++;
            RefreshPointsAndStat(s);
        }

        private void RefreshPointsAndStat(Stat s)
        {
            if (pointsLabel != null)
                pointsLabel.text = $"Attribute points remaining: {data.PointsRemaining}";

            if (statUI.TryGetValue(s, out var ui) && ui != null && ui.Value != null)
                ui.Value.text = data.Stats[s].ToString();
        }

        private void RefreshAll()
        {
            if (nameField != null) nameField.value = data.Name;
            SetGender(data.Gender);
            SetRace(data.Race);
            SetBackground(data.Background);
            SetBuild(data.Build);

            foreach (var pair in statUI)
            {
                var s = pair.Key;
                var ui = pair.Value;
                if (ui != null && ui.Value != null) ui.Value.text = data.Stats[s].ToString();
            }
            if (pointsLabel != null)
                pointsLabel.text = $"Attribute points remaining: {data.PointsRemaining}";
        }

        #endregion

        #region Nav / Game start

        private void OnBack()
        {
            Debug.Log("[CharacterCreation] Back pressed.");
        }

        private void OnFinish()
        {
            // Ensure a name exists
            if (string.IsNullOrWhiteSpace(data.Name))
            {
                data.Name = "Anon";
            }

            // Clone stats and apply body-build nudges (optional)
            var s = new Dictionary<Stat, int>(data.Stats);
            ApplyBuildNudges(data.Build, s);

            try
            {
                // Build a new PlayerCharacter manually
                var pc = new PlayerCharacter();
                // PlayerCharacter uses the 'name' field/property for character name
                pc.name = data.Name.Trim();
                // Initialize systems and base stats
                pc.InitializeModernSystems();
                if (pc.stats == null) pc.stats = new CharacterStats();
                // Map stats from creation data to CharacterStats baseStats
                pc.stats.baseStats[StatType.Strength] = s[Stat.Strength];
                pc.stats.baseStats[StatType.Dexterity] = s[Stat.Dexterity];
                pc.stats.baseStats[StatType.Constitution] = s[Stat.Constitution];
                pc.stats.baseStats[StatType.Intelligence] = s[Stat.Intelligence];
                pc.stats.baseStats[StatType.Wisdom] = s[Stat.Wisdom];
                pc.stats.baseStats[StatType.Charisma] = s[Stat.Charisma];
                // Apply race using the provided Race enum value. Convert to RaceType via string name.
                try
                {
                    var raceName = data.Race.ToString();
                    if (!string.IsNullOrEmpty(raceName) && System.Enum.TryParse<RaceType>(raceName, out var raceEnum))
                    {
                        pc.InitializeWithRace(raceEnum);
                    }
                }
                catch
                {
                    // ignore if InitializeWithRace fails
                }
                // Apply background if available
                // BackgroundDatabase.GetBackground is a static method, so call it directly
                {
                    var bg = global::BackgroundDatabase.GetBackground(data.Background.ToString());
                    pc.background = bg;
                }
                // Finalize stats
                pc.CalculateAllStats();
                // Persist the chosen portrait resource path on the player so
                // that the same image is shown in the HUD and combat UI. If
                // currentPortraitResourcePath is empty, the UIController will
                // fall back to race/gender-based portraits.
                if (!string.IsNullOrEmpty(currentPortraitResourcePath))
                {
                    pc.portraitResourcePath = currentPortraitResourcePath;
                }
                // Store player globally via PlayerState
                MyGameNamespace.PlayerState.Current = pc;
                // Notify systems via GameEventSystem
                try
                {
                    GameEventSystem.Instance?.RaisePlayerCharacterChanged(pc);
                    GameEventSystem.Instance?.RaisePlayerStatsChanged(pc);
                }
                catch { }
                // Reset story to beginning
                try
                {
                    global::StoryManager.Instance?.ResetStory();
                }
                catch { }
                // Load the Game scene
                SceneManager.LoadScene("Game", LoadSceneMode.Single);
                // After starting the game, ensure HUD shows up and hotkeys re-enable
                try
                {
                    var gameUI = MLPGameUI.Instance;
                    if (gameUI != null)
                        gameUI.ShowAfterCharacterCreation();
                }
                catch { /* ignore errors if HUD not yet created */ }
            }
            catch (Exception e)
            {
                Debug.LogError($"[CharacterCreation] OnFinish failed: {e}");
            }
        }

        private GameManager FindOrCreateGameManager()
        {
            // 1) Use singleton if present
            var gm = GameManager.Instance;
            if (gm != null) return gm;

            // 2) Try to find in scene
            gm = CompatUtils.FindFirstObjectByTypeCompat<GameManager>();
            if (gm != null) return gm;

            // 3) Create one if none exists; Awake() will wire essentials & DontDestroyOnLoad
            var go = new GameObject("GameManager");
            gm = go.AddComponent<GameManager>();
            return gm;
        }

        private void ApplyBuildNudges(BodyBuild b, Dictionary<Stat, int> s)
        {
            if (s == null) return;

            switch (b)
            {
                case BodyBuild.Slender:
                    Bump(s, Stat.Dexterity, +1);
                    Bump(s, Stat.Constitution, -1);
                    break;
                case BodyBuild.Sturdy:
                    Bump(s, Stat.Constitution, +1);
                    Bump(s, Stat.Dexterity, -1);
                    break;
                case BodyBuild.Muscular:
                    Bump(s, Stat.Strength, +1);
                    Bump(s, Stat.Charisma, -1);
                    break;
                case BodyBuild.Plush:
                    Bump(s, Stat.Charisma, +1);
                    Bump(s, Stat.Strength, -1);
                    break;
                    // Average: no change
            }
        }

        private void Bump(Dictionary<Stat, int> s, Stat key, int delta)
        {
            var v = Mathf.Clamp(s[key] + delta, StatMin, StatMax);
            s[key] = v;
        }

        #endregion
    }
}
