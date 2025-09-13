using UnityEngine;
using UnityEngine.UIElements;

namespace MyGameNamespace
{
    /// <summary>
    /// Centralized controller to apply aspect-preserving scale mode to portrait
    /// images across various UI panels (character creation, game HUD, combat UI).
    /// Also listens for player and combat events to refresh portraits.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class PortraitController : MonoBehaviour
    {
        private UIDocument _doc;
        private VisualElement _root;

        // We accept both Image and VisualElement types for portraits.  Images will
        // receive textures via the Image.image property; generic VisualElements
        // will receive textures via backgroundImage style.  Using VisualElement
        // instead of Image ensures compatibility with the combat UI which
        // declares portraits as plain VisualElements with the "character-portrait"
        // class.
        private VisualElement ccPortrait;
        private VisualElement hudPortrait;
        private VisualElement combatPlayerPortrait;
        private VisualElement combatEnemyPortrait;

        private void Awake()
        {
            _doc = GetComponent<UIDocument>();
            _root = _doc != null ? _doc.rootVisualElement : null;

            if (_root == null) return;

            // Attempt to find portrait elements by name or style class.  Many UXML files use
            // either Image or VisualElement for portraits.  Accept both types and fall back
            // to class or name conventions used in character creation, HUD and combat UIs.
            ccPortrait =
                _root.Q<VisualElement>("CCPortrait") ?? _root.Q<VisualElement>("PortraitImage") ??
                _root.Q<VisualElement>("Portrait") ?? _root.Q<VisualElement>("player-portrait");
            hudPortrait =
                _root.Q<VisualElement>("CharacterPortrait") ?? _root.Q<VisualElement>("HUDPortrait");
            // Combat UI may use names like "player-portrait" or "enemy-portrait" instead of Images.
            combatPlayerPortrait =
                _root.Q<VisualElement>("CombatPlayerPortrait") ?? _root.Q<VisualElement>("PlayerPortrait") ??
                _root.Q<VisualElement>("player-portrait");
            combatEnemyPortrait =
                _root.Q<VisualElement>("CombatEnemyPortrait") ?? _root.Q<VisualElement>("EnemyPortrait") ??
                _root.Q<VisualElement>("enemy-portrait");

            // Apply scale mode and styling to any found portraits
            ApplyPortraitStyle(ccPortrait);
            ApplyPortraitStyle(hudPortrait);
            ApplyPortraitStyle(combatPlayerPortrait);
            ApplyPortraitStyle(combatEnemyPortrait);

            // Subscribe to relevant game events.  We always listen for combat
            // starts so that the enemy portrait and player portrait refresh
            // regardless of any compile-time flags.  Previously this used
            // a COMBAT_EVENTS directive which meant the handler was never
            // attached in release builds.  Removing the conditional ensures
            // portraits update correctly when entering combat.
            var ges = GameEventSystem.Instance;
            if (ges != null)
            {
                ges.OnPlayerCharacterChanged += OnPlayerCharacterChanged;
                ges.OnCombatStarted += OnCombatStarted;
            }
        }

        private void OnDestroy()
        {
            var ges = GameEventSystem.Instance;
            if (ges != null)
            {
                ges.OnPlayerCharacterChanged -= OnPlayerCharacterChanged;
                // Always unsubscribe from combat started events as well
                ges.OnCombatStarted -= OnCombatStarted;
            }
        }

        private static void ApplyPortraitStyle(VisualElement elem)
        {
            if (elem == null) return;
            // If this element is an Image, set its scale mode.  Otherwise
            // rely on USS rules defined in Portraits.uss for VisualElements.
            if (elem is Image img)
            {
                img.scaleMode = ScaleMode.ScaleToFit;
            }
            // Always ensure the "character-portrait" class is present so
            // USS rules like background scale-to-fit apply.  Avoid adding
            // duplicates: AddToClassList silently ignores duplicates.
            elem.AddToClassList("character-portrait");
        }

        private void OnPlayerCharacterChanged(object payload)
        {
            RefreshPlayerPortrait();
        }

        private void RefreshPlayerPortrait()
        {
            var player = MyGameNamespace.PlayerState.Current;
            if (player == null) return;

            var tex = ResolvePortrait(player);
            if (tex != null)
            {
                SetElementImage(ccPortrait, tex);
                SetElementImage(hudPortrait, tex);
                SetElementImage(combatPlayerPortrait, tex);
            }
        }

        private void OnCombatStarted(global::CombatStartData data)
        {
            RefreshPlayerPortrait();
            if (combatEnemyPortrait != null && data != null)
            {
                var enemyTex = ResolveEnemyPortrait(data.enemyId);
                if (enemyTex != null)
                {
                    SetElementImage(combatEnemyPortrait, enemyTex);
                }
            }
        }

        private static Texture2D ResolvePortrait(PlayerCharacter player)
        {
            if (player == null) return null;

            // Highest priority: saved portrait path from character creation
            if (!string.IsNullOrEmpty(player.portraitResourcePath))
            {
                var tex = Resources.Load<Texture2D>(player.portraitResourcePath);
                if (tex != null) return tex;
            }

            // Fallback by race and gender
            string raceLower = player.race.ToString().ToLowerInvariant();
            string genderLower = string.IsNullOrEmpty(player.gender) ? "" : player.gender.ToLowerInvariant();

            Texture2D resourceTexture = null;
            // First try the canonical "race_gender" format (e.g. earthpony_female)
            if (!string.IsNullOrEmpty(genderLower))
                resourceTexture = Resources.Load<Texture2D>($"Portraits/{raceLower}_{genderLower}");
            // Second try the race-only (e.g. earthpony)
            if (resourceTexture == null)
                resourceTexture = Resources.Load<Texture2D>($"Portraits/{raceLower}");
            // As a final fallback, attempt to load resources that use spaces and proper casing
            // Many portrait assets are stored as "Earth Pony Female.png" or similar, so attempt
            // to construct that path.  Replace "Pony" with " Pony" to insert a space.
            if (resourceTexture == null)
            {
                string properRace = player.race.ToString();
                // Insert a space before "Pony" if missing (e.g. EarthPony → Earth Pony)
                if (properRace.EndsWith("Pony") && !properRace.Contains(" "))
                {
                    properRace = properRace.Replace("Pony", " Pony");
                }
                string properGender = string.IsNullOrEmpty(player.gender) ? string.Empty : char.ToUpperInvariant(player.gender[0]) + player.gender.Substring(1).ToLowerInvariant();
                if (!string.IsNullOrEmpty(properGender))
                {
                    // Try subfolder path like Portraits/earthpony_female/Earth Pony Female
                    string subFolder = $"Portraits/{raceLower}_{genderLower}/{properRace} {properGender}";
                    resourceTexture = Resources.Load<Texture2D>(subFolder);
                    if (resourceTexture == null)
                    {
                        // Try flat path like Portraits/Earth Pony Female
                        string flat = $"Portraits/{properRace} {properGender}";
                        resourceTexture = Resources.Load<Texture2D>(flat);
                    }
                }
            }
            // If no specific portrait was found, attempt a race‑specific silhouette.
            if (resourceTexture == null)
            {
                // Try lowercase race name (e.g. "earthpony_silhouette") under Portraits/Silhouettes
                var silhouette = Resources.Load<Texture2D>($"Portraits/Silhouettes/{raceLower}_silhouette");
                if (silhouette == null)
                {
                    // Try proper‑cased race with space inserted before "Pony" (e.g. "Earth Pony Silhouette")
                    string properRace = player.race.ToString();
                    if (properRace.EndsWith("Pony") && !properRace.Contains(" "))
                    {
                        properRace = properRace.Replace("Pony", " Pony");
                    }
                    silhouette = Resources.Load<Texture2D>($"Portraits/Silhouettes/{properRace} Silhouette");
                }
                if (silhouette != null) return silhouette;
            }
            // As a final fallback, return the generic default silhouette.  This ensures that
            // the portrait never appears blank even if race‑specific silhouettes are missing.
            return resourceTexture ?? Resources.Load<Texture2D>("Portraits/Default_Silhouette");
        }

        private static Texture2D ResolveEnemyPortrait(string enemyId)
        {
            if (string.IsNullOrEmpty(enemyId)) return null;
            return Resources.Load<Texture2D>($"images/enemies/{enemyId}") ?? Resources.Load<Texture2D>($"Portraits/Enemies/{enemyId}");
        }

        /// <summary>
        /// Assigns a texture to either an Image or VisualElement. For Image elements
        /// this sets the `image` property and scaleMode; for other VisualElements it
        /// sets the backgroundImage style. This centralizes how portraits are applied.
        /// </summary>
        private void SetElementImage(VisualElement elem, Texture2D tex)
        {
            if (elem == null || tex == null) return;
            if (elem is Image img)
            {
                img.scaleMode = ScaleMode.ScaleToFit;
                img.image = tex;
            }
            else
            {
                // Use StyleBackground to set the image on a generic VisualElement
                elem.style.backgroundImage = new StyleBackground(tex);
            }
        }
    }
}