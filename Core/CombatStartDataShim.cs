namespace MyGameNamespace
{
    /// <summary>
    /// Shim class to expose the global <see cref="global::CombatStartData"/> type under the
    /// <c>MyGameNamespace</c> namespace.  Some components in this project live in
    /// <c>MyGameNamespace</c> and reference <c>CombatStartData</c> without a fully
    /// qualified namespace.  Without this shim those references would not resolve
    /// once <c>CombatStartData</c> was moved into the global namespace.  This
    /// derived class inherits all members from the global type and adds no new
    /// behaviour.
    /// </summary>
    public class CombatStartData : global::CombatStartData
    {
        // No additional fields or methods; this class simply bridges the global
        // CombatStartData into the MyGameNamespace for ease of use.
    }
}