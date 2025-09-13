using System;

namespace MyGameNamespace
{
    public static class RaceExtensions
    {
        public static RaceType ParseRace(string raceString)
        {
            if (Enum.TryParse<RaceType>(raceString, true, out var rt))
                return rt;
            return RaceType.EarthPony;
        }

        // Removed the ambiguous InitializeWithRaceSafe method - let PlayerCharacterCompat handle it
    }
}