using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace MyGameNamespace
{
    public class CharacterCreationPillsV2 : MonoBehaviour
    {
        // Safe setter for build help label
        private void SetBuildHelp(string msg)
        {
            if (_buildHelp != null) _buildHelp.text = msg;
        }

        UIDocument _doc;

        // Gender
        Button _female, _male;

        // Build
        Button _buildLithe, _buildAverage, _buildSturdy, _buildMuscular, _buildPlush;
        Label _buildHelp;

        // Race / Background
        VisualElement _racePills, _bgPills;
        Label _raceHelp, _bgHelp, _raceDesc;

        // Stance & fields
        TextField _stance;

        // Cached lists for groups
        List<Button> _raceButtons = new();
        List<Button> _bgButtons = new();

        // Simple rule tables (edit freely to match your DB)
        readonly Dictionary<string, string> RaceStance = new()
        {
            { "Earth Pony", "Quadruped" },
            { "Pegasus",    "Quadruped" },
            { "Unicorn",    "Quadruped" },
            { "Bat Pony",   "Quadruped" },
            { "Changeling", "Quadruped" },
            { "Griffon",    "Quadruped" },
            { "Dragon",     "Bipedal"   },
            { "Human",      "Bipedal"   },
        };

        readonly Dictionary<string, string> RaceFlavor = new()
        {
            { "Earth Pony", "Sturdy and dependable." },
            { "Pegasus",    "Swift flyers with weathercraft." },
            { "Unicorn",    "Gifted with focused magic." },
            { "Bat Pony",   "Nocturnal senses and stealth." },
            { "Changeling", "Shapeshifters bound by hivecraft." },
            { "Griffon",    "Proud hunters of sky and stone." },
            { "Dragon",     "Hardy scales and fiery breath." },
            { "Human",      "Adaptable wanderers from afar." },
        };

        readonly Dictionary<string, string> BgFlavor = new()
        {
            { "Drifter",  "You�ve seen a bit of everything and trust your gut." },
            { "Farmhand", "Strong back, steady heart, honest work." },
            { "Scholar",  "Books first, answers second; curiosity always." },
            { "Guard",    "Trained watch, disciplined stance, quick response." },
            { "Artisan",  "Hands, tools, and craft�make it better than before." },
            { "Outcast",  "You live between places and read between lines." },
        };

        void Awake()
        {
            _doc = GetComponent<UIDocument>();
            if (_doc == null) { Debug.LogWarning("[CharacterCreationPillsV2] No UIDocument on this GameObject."); return; }
            var root = _doc.rootVisualElement;
            if (root == null) { Debug.LogWarning("[CharacterCreationPillsV2] UIDocument has no rootVisualElement."); return; }

            // --- Gender ---
            _female = root.Q<Button>("FemaleButton");
            _male = root.Q<Button>("MaleButton");
            HookPill(_female, () => TogglePair(_female, _male));
            HookPill(_male, () => TogglePair(_male, _female));
            TogglePair(_female, _male); // default

            // --- Build ---
            _buildLithe = root.Q<Button>("BuildLithe");
            _buildAverage = root.Q<Button>("BuildAverage");
            _buildSturdy = root.Q<Button>("BuildSturdy");
            _buildMuscular = root.Q<Button>("BuildMuscular");
            _buildPlush = root.Q<Button>("BuildPlush");
            _buildHelp = root.Q<Label>("BuildHelp") ?? root.Q<Label>("BuildDescLabel");

            HookPill(_buildLithe, () => SelectBuild("Lithe"));
            HookPill(_buildAverage, () => SelectBuild("Average"));
            HookPill(_buildSturdy, () => SelectBuild("Sturdy"));
            HookPill(_buildMuscular, () => SelectBuild("Muscular"));
            HookPill(_buildPlush, () => SelectBuild("Plush"));
            SelectBuild("Average");

            // --- Race & Background (pills) ---
            _racePills = root.Q<VisualElement>("RacePills");
            _bgPills = root.Q<VisualElement>("BgPills");
            _raceHelp = root.Q<Label>("RaceHelp");
            _bgHelp = root.Q<Label>("BgHelp");
            _raceDesc = root.Q<Label>("RaceDescription") ?? root.Q<Label>("RaceHelp");
            _stance = root.Q<TextField>("StanceField");

            // Collect group buttons by class
            _raceButtons = _racePills.Query<Button>(className: "pill--race").ToList();
            _bgButtons = _bgPills.Query<Button>(className: "pill--bg").ToList();

            // Wire clicks
            foreach (var b in _raceButtons)
                HookPill(b, () => SelectRace(Pretty(b.text)));

            foreach (var b in _bgButtons)
                HookPill(b, () => SelectBackground(Pretty(b.text)));

            // Defaults
            SelectRace("Human");        // choose any default you like
            SelectBackground("Drifter");
        }

        // ---------- Selections ----------

        void SelectRace(string race)
        {
            // visual
            SelectOne(_raceButtons, race);

            // stance + flavor
            if (RaceStance.TryGetValue(race, out var stance))

                if (_stance != null) _stance.value = stance;
                else

                if (_stance != null) _stance.value = "Bipedal";

            if (RaceFlavor.TryGetValue(race, out var rf))


                if (_raceDesc != null) _raceDesc.text = rf;
                else

                if (_raceDesc != null) _raceDesc.text = "Choose your race.";
        }

        void SelectBackground(string bg)
        {
            SelectOne(_bgButtons, bg);

            if (BgFlavor.TryGetValue(bg, out var bf))
                _bgHelp.text = bf;
            else
                _bgHelp.text = "Pick a life path.";
        }

        void SelectBuild(string id)
        {
            _buildLithe?.RemoveFromClassList("selected");
            _buildAverage?.RemoveFromClassList("selected");
            _buildSturdy?.RemoveFromClassList("selected");
            _buildMuscular?.RemoveFromClassList("selected");
            _buildPlush?.RemoveFromClassList("selected");

            switch (id)
            {
                case "Lithe":
                    _buildLithe?.AddToClassList("selected");
                    SetBuildHelp("Lithe: +Initiative, -Carry. Agile and quick.");
                    break;
                case "Average":
                    _buildAverage?.AddToClassList("selected");
                    SetBuildHelp("Average: balanced build, no tradeoffs.");
                    break;
                case "Sturdy":
                    _buildSturdy?.AddToClassList("selected");
                    SetBuildHelp("Sturdy: +Carry/HP, -Initiative.");
                    break;
                case "Muscular":
                    _buildMuscular?.AddToClassList("selected");
                    SetBuildHelp("Muscular: +Melee damage, -Ranged finesse.");
                    break;
                case "Plush":
                    _buildPlush?.AddToClassList("selected");
                    SetBuildHelp("Plush: +Social checks, -Raw STR tests.");
                    break;
            }
        }

        // ---------- Small helpers ----------

        void HookPill(Button btn, System.Action onClick)
        {
            if (btn == null) return;
            btn.clicked += () => onClick?.Invoke();
        }

        void TogglePair(VisualElement on, VisualElement off)
        {
            if (on == null || off == null) return;
            on.AddToClassList("selected");
            off.RemoveFromClassList("selected");
        }

        void SelectOne(List<Button> group, string textToMatch)
        {
            foreach (var b in group)
            {
                if (Pretty(b.text) == textToMatch) b.AddToClassList("selected");
                else b.RemoveFromClassList("selected");
            }
        }

        static string Pretty(string s) => (s ?? "").Trim();
    }
}
