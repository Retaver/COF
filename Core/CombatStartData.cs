// CombatStartData.cs
//
// This class encapsulates information about a newly started combat encounter.  It
// holds the identifier of the enemy being faced so that UI components (such
// as portrait panels and health bars) can load the appropriate assets.  The
// class resides in the global namespace so it can be referenced from both
// game systems and UI code without requiring a specific using directive.  A
// shim in the MyGameNamespace namespace (see CombatStartDataShim.cs) extends
// this class to provide compatibility for components that live under that
// namespace.

/// <summary>
/// Represents data passed to listeners when combat starts. Contains the enemy identifier.
/// </summary>
public class CombatStartData
{
    /// <summary>
    /// The ID of the enemy encountered. Used to load enemy portraits and lookup data.
    /// </summary>
    public string enemyId;
}