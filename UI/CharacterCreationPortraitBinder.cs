using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Globalization;
using System.Collections.Generic;

namespace MyGameNamespace
{
    /// <summary>
    /// Portrait binder that loads: Resources/Portraits/{Race}/{Gender}/portrait
    /// If not found, it falls back to scan ALL sprites under "Portraits" and chooses
    /// the one whose name best matches the selected race+gender.
    /// Accepts many name variants e.g. "earthpony_female", "Earth Pony Female", "earthPonyFemale", etc.
    /// </summary>
    public class CharacterCreationPortraitBinder : MonoBehaviour
    {
        [Header("UI Binding")]
        [SerializeField] private UIDocument document;
        [SerializeField] private string portraitImageName = "PortraitImage";
        [SerializeField] private string portraitElementName = "Portrait";

        private VisualElement root;
        private Image portraitImage;
        private VisualElement portraitElement;

        [Header("Selection State")]
        [SerializeField] private string currentRace = "EarthPony";
        [SerializeField] private string currentGender = "Female";

        private void Awake()
        {
            if (document == null) document = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            root = document != null ? document.rootVisualElement : null;
            if (root == null) { Debug.LogWarning("[PortraitBinder] Missing UIDocument/root."); return; }

            portraitImage = root.Q<Image>(portraitImageName);
            portraitElement = root.Q<VisualElement>(portraitElementName);

            ApplyPortrait();
        }

        public void SetRace(string raceLabel)
        {
            currentRace = NormalizeRace(raceLabel);
            ApplyPortrait();
        }

        public void SetGender(string genderLabel)
        {
            currentGender = NormalizeGender(genderLabel);
            ApplyPortrait();
        }

        private void ApplyPortrait()
        {
            // 1) Folder-first
            var path = $"Portraits/{currentRace}/{currentGender}/portrait";
            var sprite = Resources.Load<Sprite>(path);
            if (sprite == null)
            {
                // 2) Fallback: scan any sprite under "Portraits" that contains race+gender tokens
                sprite = FindBestSpriteByName(currentRace, currentGender);
            }

            if (sprite == null)
            {
                Debug.LogWarning($"[PortraitBinder] No portrait found for {currentRace} {currentGender}. Place one at Resources/Portraits/{currentRace}/{currentGender}/portrait.png or name a sprite like '{currentRace}_{currentGender}'.");
                ClearPortrait();
                return;
            }

            if (portraitImage != null)
            {
                portraitImage.sprite = sprite;
            }
            if (portraitElement != null)
            {
                portraitElement.style.backgroundImage = new StyleBackground(sprite);
                // Use USS on the element to control sizing:
                // background-position: center; background-repeat: no-repeat; background-size: contain;
            }
        }

        private void ClearPortrait()
        {
            if (portraitImage != null) portraitImage.sprite = null;
            if (portraitElement != null) portraitElement.style.backgroundImage = null;
        }

        private static string NormalizeRace(string label)
        {
            if (string.IsNullOrEmpty(label)) return "EarthPony";
            label = label.Trim().ToLowerInvariant();
            if (label.Contains("earth")) return "EarthPony";
            if (label.Contains("unicorn")) return "Unicorn";
            if (label.Contains("pegas")) return "Pegasus";
            if (label.Contains("griff")) return "Griffon";
            if (label.Contains("dragon")) return "Dragon";
            if (label.Contains("human")) return "Human";
            // TitleCase & remove spaces as a fallback
            return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(label).Replace(" ", "");
        }

        private static string NormalizeGender(string label)
        {
            if (string.IsNullOrEmpty(label)) return "Female";
            label = label.Trim().ToLowerInvariant();
            if (label.StartsWith("f")) return "Female";
            if (label.StartsWith("m")) return "Male";
            return char.ToUpperInvariant(label[0]) + label.Substring(1);
        }

        private static Sprite FindBestSpriteByName(string race, string gender)
        {
            // Race and gender tokens we expect to match against the sprite name
            string r = race.ToLowerInvariant();
            string g = gender.ToLowerInvariant();

            // Load all sprites under "Portraits". Requires assets to be anywhere under Resources/Portraits.
            var sprites = Resources.LoadAll<Sprite>("Portraits");
            Sprite best = null;
            int bestScore = -1;

            foreach (var s in sprites)
            {
                if (s == null) continue;
                var name = s.name.ToLowerInvariant();

                // Basic scoring: +2 for race token, +2 for gender token, +1 for combined race+gender in any order
                int score = 0;
                if (name.Contains(r)) score += 2;
                if (name.Contains(g)) score += 2;
                if (name.Contains(r + "_" + g) || name.Contains(r + g) || name.Contains(r + "-" + g)) score += 1;
                if (name.Contains(g + "_" + r) || name.Contains(g + r) || name.Contains(g + "-" + r)) score += 1;

                if (score > bestScore)
                {
                    bestScore = score;
                    best = s;
                }
            }

            return best;
        }
    }
}