using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyGameNamespace
{
    /// <summary>
    /// Heuristically wires your Character Creation UI to the CharacterCreationPortraitBinder:
    /// - Finds race DropdownField or pill Buttons and calls SetRace()
    /// - Finds gender Buttons/Toggles labeled "Female"/"Male" and calls SetGender()
    /// Works with many ID/class/label variants so you don't have to rename your UXML.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class CharacterCreationAutoBinder : MonoBehaviour
    {
        [SerializeField] private CharacterCreationPortraitBinder portraitBinder;
        private UIDocument doc;
        private VisualElement root;

        private static readonly string[] RaceKeywords = new[] { "race", "species" };
        private static readonly string[] GenderFemaleKeys = new[] { "female", "girl", "mare" };
        private static readonly string[] GenderMaleKeys = new[] { "male", "boy", "stallion" };

        private void Awake()
        {
            doc = GetComponent<UIDocument>();
            if (portraitBinder == null) portraitBinder = GetComponent<CharacterCreationPortraitBinder>();
            if (portraitBinder == null) portraitBinder = FindFirstObjectByType<CharacterCreationPortraitBinder>();
        }

        private void OnEnable()
        {
            root = doc != null ? doc.rootVisualElement : null;
            if (root == null || portraitBinder == null) { return; }

            WireRace();
            WireGender();
            ApplyInitialSelections();
        }

        private void WireRace()
        {
            // 1) Try DropdownField
            var dropdowns = root.Query<DropdownField>().ToList();
            foreach (var dd in dropdowns)
            {
                if (MatchesAny(dd.name, RaceKeywords) || MatchesAny(dd.label, RaceKeywords) || dd.ClassListContains("race"))
                {
                    dd.UnregisterValueChangedCallback(OnRaceDropdownChanged);
                    dd.RegisterValueChangedCallback(OnRaceDropdownChanged);
                    return;
                }
            }

            // 2) Try pill container of Buttons (named/classed "RacePills", etc.)
            var buttons = root.Query<Button>().ToList();
            foreach (var b in buttons)
            {
                var txt = (b.text ?? "").Trim().ToLowerInvariant();
                // If a button text looks like a race option, bind it
                if (IsLikelyRace(txt))
                {
                    b.clicked -= () => OnRaceButton(txt);
                    b.clicked += () => OnRaceButton(txt);
                }
            }
        }

        private void WireGender()
        {
            // Find any button/toggle that contains male/female and bind
            var buttons = root.Query<Button>().ToList();
            foreach (var b in buttons)
            {
                var txt = (b.text ?? "").Trim().ToLowerInvariant();
                if (MatchesAny(txt, GenderFemaleKeys))
                {
                    b.clicked -= OnFemaleClicked;
                    b.clicked += OnFemaleClicked;
                }
                else if (MatchesAny(txt, GenderMaleKeys))
                {
                    b.clicked -= OnMaleClicked;
                    b.clicked += OnMaleClicked;
                }
            }

            var toggles = root.Query<Toggle>().ToList();
            foreach (var t in toggles)
            {
                var nm = (t.name ?? "").ToLowerInvariant();
                var lbl = (t.label ?? "").ToLowerInvariant();
                if (MatchesAny(nm, GenderFemaleKeys) || MatchesAny(lbl, GenderFemaleKeys))
                {
                    t.UnregisterValueChangedCallback(OnFemaleToggle);
                    t.RegisterValueChangedCallback(OnFemaleToggle);
                }
                else if (MatchesAny(nm, GenderMaleKeys) || MatchesAny(lbl, GenderMaleKeys))
                {
                    t.UnregisterValueChangedCallback(OnMaleToggle);
                    t.RegisterValueChangedCallback(OnMaleToggle);
                }
            }
        }

        private void ApplyInitialSelections()
        {
            // Try to read current dropdown values if present
            var dropdowns = root.Query<DropdownField>().ToList();
            foreach (var dd in dropdowns)
            {
                if (MatchesAny(dd.name, RaceKeywords) || MatchesAny(dd.label, RaceKeywords) || dd.ClassListContains("race"))
                {
                    portraitBinder.SetRace(dd.value);
                    break;
                }
            }

            // If there is a pre-pressed gender button/toggle, we won't know; default binder already has a default.
            // You can force an initial gender here if you keep state elsewhere.
        }

        private void OnRaceDropdownChanged(ChangeEvent<string> evt)
        {
            portraitBinder.SetRace(evt.newValue);
        }

        private void OnRaceButton(string label)
        {
            portraitBinder.SetRace(label);
        }

        private void OnFemaleClicked()  => portraitBinder.SetGender("Female");
        private void OnMaleClicked()     => portraitBinder.SetGender("Male");

        private void OnFemaleToggle(ChangeEvent<bool> evt) { if (evt.newValue) portraitBinder.SetGender("Female"); }
        private void OnMaleToggle(ChangeEvent<bool> evt)   { if (evt.newValue) portraitBinder.SetGender("Male"); }

        private static bool MatchesAny(string value, IEnumerable<string> keys)
        {
            if (string.IsNullOrEmpty(value)) return false;
            var v = value.ToLowerInvariant();
            foreach (var k in keys) if (v.Contains(k)) return true;
            return false;
        }

        private static bool IsLikelyRace(string txt)
        {
            if (string.IsNullOrEmpty(txt)) return false;
            txt = txt.ToLowerInvariant();
            return txt.Contains("earth") || txt.Contains("unicorn") || txt.Contains("pegas") ||
                   txt.Contains("griff") || txt.Contains("dragon") || txt.Contains("human");
        }
    }
}