namespace MyGameNamespace
{
    public partial class PlayerCharacter
    {
        /// <summary>
        /// Path within the Resources folder to load the player's portrait. Set during
        /// character creation so the same image is used throughout the HUD and combat.
        /// Example: "Portraits/earthpony_female/Earth Pony Female".  Should not include
        /// file extension.
        /// </summary>
        public string portraitResourcePath;
    }
}