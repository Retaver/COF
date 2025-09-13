using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace MyGameNamespace
{
    /// <summary>
    /// Runtime controller for the world map.  This component reads a simple
    /// data structure describing a collection of locations laid out in a
    /// rectangular grid, generates corresponding UI tiles at runtime and
    /// handles click events to allow the player to move between connected
    /// locations.  It is intentionally minimal and meant as a foundation
    /// for more advanced systems – you could extend this to load map data
    /// from a ScriptableObject, display icons for shops or encounters,
    /// or integrate with your story manager to change the narrative when
    /// entering a new location.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class MapController : MonoBehaviour
    {
        [SerializeField]
        public UIDocument uiDocument;

        [Tooltip("Size of the map grid (columns x rows).  Each cell can hold at most one location.")]
        public Vector2Int mapSize = new Vector2Int(3, 3);

        // Data structure describing all known locations keyed by their identifier.
        private readonly Dictionary<string, MapLocation> locations = new();

        // Cache of UI root elements.  These are looked up once at initialization
        // to avoid repeated queries every frame.
        private VisualElement mapGrid;
        private Label mapDescription;

        // The currently selected/active location.  Highlighted on the map.
        private MapLocation currentLocation;

        /// <summary>
        /// Called by Unity when this component is first created.  Perform
        /// initialization here instead of in Start() to ensure UI Toolkit
        /// elements are ready.
        /// </summary>
        private void Awake()
        {
            // Auto assign the UIDocument if not explicitly set.
            if (uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
            }
            var root = uiDocument != null ? uiDocument.rootVisualElement : null;
            if (root == null)
            {
                Debug.LogError("MapController: UIDocument missing or rootVisualElement is null.");
                enabled = false;
                return;
            }

            mapGrid = root.Q<VisualElement>("MapGrid");
            mapDescription = root.Q<Label>("MapDescription");
            if (mapGrid == null || mapDescription == null)
            {
                Debug.LogError("MapController: MapGrid or MapDescription elements not found in UXML.");
                enabled = false;
                return;
            }

            // Build a default map definition.  In a real project you would load
            // this data from JSON or ScriptableObject assets.
            BuildDefaultMap();

            // Assign a starting location.  If no explicit starting ID is set
            // we'll pick the first location in the collection.
            if (currentLocation == null)
            {
                foreach (var loc in locations.Values)
                {
                    currentLocation = loc;
                    break;
                }
            }

            // Render the initial map.  This will populate the visual tree.
            RenderMap();
            UpdateDescription();
        }

        /// <summary>
        /// Populates the <see cref="locations"/> dictionary with a simple sample map.
        /// The sample uses a 3x3 grid and demonstrates how to define
        /// connections between tiles.  You can replace this with a more
        /// sophisticated loader that reads from external data.
        /// </summary>
        private void BuildDefaultMap()
        {
            locations.Clear();
            // Centre tile
            var centre = new MapLocation
            {
                id = "centre",
                name = "Town Square",
                description = "The bustling heart of the village. Stalls, shops and friendly faces abound.",
                row = 1,
                col = 1,
                north = "forest",
                south = "farm",
                east = "lake",
                west = "mountain"
            };
            // North tile
            var forest = new MapLocation
            {
                id = "forest",
                name = "Whispering Woods",
                description = "A thick forest filled with towering trees and distant howls. Rumours say that timberwolves roam here.",
                row = 0,
                col = 1,
                south = "centre"
            };
            // South tile
            var farm = new MapLocation
            {
                id = "farm",
                name = "Sunny Farm",
                description = "Rows of crops sway gently in the breeze. The farmer always greets visitors with a smile.",
                row = 2,
                col = 1,
                north = "centre"
            };
            // East tile
            var lake = new MapLocation
            {
                id = "lake",
                name = "Crystal Lake",
                description = "A serene lake that glistens like crystal. Fish dart beneath the surface.",
                row = 1,
                col = 2,
                west = "centre"
            };
            // West tile
            var mountain = new MapLocation
            {
                id = "mountain",
                name = "Grey Mountains",
                description = "Jagged peaks loom above, often shrouded in mist. The path up looks treacherous.",
                row = 1,
                col = 0,
                east = "centre"
            };
            // Add to dictionary
            locations[centre.id] = centre;
            locations[forest.id] = forest;
            locations[farm.id] = farm;
            locations[lake.id] = lake;
            locations[mountain.id] = mountain;
        }

        /// <summary>
        /// Rebuilds the visual representation of the map.  This method clears
        /// and repopulates the grid container, so it should be called
        /// whenever the player moves or when the map size changes.
        /// </summary>
        private void RenderMap()
        {
            if (mapGrid == null) return;
            mapGrid.Clear();

            for (int row = 0; row < mapSize.y; row++)
            {
                var rowElem = new VisualElement();
                rowElem.AddToClassList("map-row");
                for (int col = 0; col < mapSize.x; col++)
                {
                    var tile = new VisualElement();
                    tile.AddToClassList("map-tile");
                    // Find location matching this coordinate
                    MapLocation loc = null;
                    foreach (var kvp in locations)
                    {
                        var l = kvp.Value;
                        if (l.row == row && l.col == col)
                        {
                            loc = l;
                            break;
                        }
                    }
                    if (loc != null)
                    {
                        tile.userData = loc;
                        // Display the first character of the location name as a label
                        var label = new Label(loc.name.Substring(0, 1).ToUpper());
                        label.AddToClassList("tile-label");
                        tile.Add(label);
                        // Highlight if this is the current location
                        if (currentLocation != null && currentLocation.id == loc.id)
                        {
                            tile.AddToClassList("active");
                        }
                    }
                    else
                    {
                        tile.AddToClassList("invalid");
                    }
                    // Register a click callback on every tile.  When the tile itself
                    // (not a child label) is clicked, attempt to move to that
                    // destination.  See OnTileClicked for details.
                    tile.RegisterCallback<ClickEvent>(OnTileClicked);

                    // Register hover callbacks to preview a location without
                    // committing to a move.  When the pointer hovers over a
                    // tile containing a MapLocation, temporarily show that
                    // location’s name and description in the description area
                    // and add a CSS class for visual feedback.  When the
                    // pointer exits, restore the description of the current
                    // location.  UI Toolkit’s pointer events are part of
                    // UnityEngine.UIElements beginning with 2022.1 and are
                    // available in Unity 6.2【237242815793716†L82-L95】.
                    tile.RegisterCallback<PointerEnterEvent>(OnTileHovered);
                    tile.RegisterCallback<PointerLeaveEvent>(OnTileExited);
                    rowElem.Add(tile);
                }
                mapGrid.Add(rowElem);
            }
        }

        /// <summary>
        /// Responds to click events on map tiles.  If the tile contains a
        /// valid <see cref="MapLocation"/>, attempt to move to that location.
        /// </summary>
        /// <param name="evt">Event data describing the click.</param>
        private void OnTileClicked(ClickEvent evt)
        {
            // evt.currentTarget refers to the VisualElement on which the callback is registered.
            // We only want to handle the click when it originates on the tile itself,
            // not when bubbling up from a child label.  Compare target to currentTarget.
            var tile = evt.currentTarget as VisualElement;
            if (tile == null || evt.target != tile) return;
            if (tile.userData is MapLocation loc)
            {
                MoveTo(loc.id);
            }
        }

        /// <summary>
        /// Handles pointer enter (hover) events on map tiles.  When the user
        /// hovers over a valid location, display its details in the map
        /// description area and add a CSS class for hover styling.  This
        /// preview does not actually move the player; it provides a quick
        /// way to explore the map, similar to hover effects in many
        /// interactive fiction maps.
        /// </summary>
        /// <param name="evt">Pointer enter event.</param>
        private void OnTileHovered(PointerEnterEvent evt)
        {
            var tile = evt.currentTarget as VisualElement;
            if (tile == null) return;
            if (tile.userData is MapLocation loc)
            {
                // Show the location’s details in the description panel
                if (mapDescription != null)
                {
                    mapDescription.text = $"{loc.name}\n{loc.description}";
                }
                // Add hover class for visual highlight
                tile.AddToClassList("hover");
            }
        }

        /// <summary>
        /// Handles pointer leave events on map tiles.  When the pointer
        /// exits a tile, remove the hover styling and restore the
        /// description of the current location.  Without this callback the
        /// preview text would persist even when the pointer is no longer
        /// hovering over any tile.
        /// </summary>
        /// <param name="evt">Pointer leave event.</param>
        private void OnTileExited(PointerLeaveEvent evt)
        {
            var tile = evt.currentTarget as VisualElement;
            if (tile == null) return;
            // Remove hover class
            tile.RemoveFromClassList("hover");
            // Restore the description to the current location
            UpdateDescription();
        }

        /// <summary>
        /// Changes the current location if the destination is directly connected
        /// to the player's current location.  This prevents teleporting to
        /// arbitrary tiles that are not neighbours.  When the move succeeds
        /// the map is re-rendered and the description is updated.
        /// </summary>
        /// <param name="id">The identifier of the destination location.</param>
        private void MoveTo(string id)
        {
            if (currentLocation == null || !locations.ContainsKey(id)) return;
            if (currentLocation.id == id) return;
            var destination = locations[id];
            // Ensure the destination is connected via one of the cardinal directions
            bool isConnected = false;
            if (currentLocation.north == id) isConnected = true;
            if (currentLocation.south == id) isConnected = true;
            if (currentLocation.east == id) isConnected = true;
            if (currentLocation.west == id) isConnected = true;
            if (!isConnected)
            {
                // Do not allow moves to unconnected locations
                return;
            }
            currentLocation = destination;

            // Update the player's location, if a player exists
            var player = PlayerState.Current;
            if (player != null)
            {
                // Use the existing GameLocation system if available.  Here we
                // simply update the player's currentLocation.  You could look
                // up an existing GameLocation instance from a database instead
                // of creating a new one on the fly.
                // Use fully qualified name because GameLocation lives in the global namespace
                player.currentLocation = new global::GameLocation(destination.id, destination.name, string.Empty, destination.description);
            }
            UpdateDescription();
            RenderMap();
            // Optionally notify other systems.  For example, you could raise
            // a GameEventSystem event here:
            // GameEventSystem.Instance?.RaiseGameFlagSet("location", destination.id);
        }

        /// <summary>
        /// Updates the descriptive text below the map to reflect the current location.
        /// </summary>
        private void UpdateDescription()
        {
            if (mapDescription == null) return;
            if (currentLocation == null)
            {
                mapDescription.text = string.Empty;
            }
            else
            {
                mapDescription.text = $"{currentLocation.name}\n{currentLocation.description}";
            }
        }

        /// <summary>
        /// Internal representation of a map tile.  Each instance occupies a
        /// discrete cell on the grid.  For simplicity all coordinates use
        /// zero-based indexing (row, col) where (0,0) is the top-left.
        /// </summary>
        private class MapLocation
        {
            public string id;
            public string name;
            public string description;
            public int row;
            public int col;
            public string north;
            public string south;
            public string east;
            public string west;
        }
    }
}