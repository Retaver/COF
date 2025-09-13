// PlayerState.cs
// Simple static holder for the current PlayerCharacter.
// This allows the UI to retrieve the player without relying on GameManager.GetPlayer().

using MyGameNamespace;

namespace MyGameNamespace
{
    public static class PlayerState
    {
        /// <summary>
        /// Gets or sets the current player character.
        /// </summary>
        public static PlayerCharacter Current { get; set; }
    }
}