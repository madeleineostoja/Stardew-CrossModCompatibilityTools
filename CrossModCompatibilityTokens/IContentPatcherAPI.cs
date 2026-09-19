#nullable enable
using StardewModdingAPI;

namespace ContentPatcher;

/// <summary>
/// Minimal Content Patcher API surface used by CMCT.
/// Content Patcher recommends copying the API interface into consumer mods
/// so they don't need a compile-time reference to ContentPatcher.dll.
/// </summary>
public interface IContentPatcherAPI
{
    /// <summary>Whether Content Patcher's conditions API is ready.</summary>
    bool IsConditionsApiReady { get; }

    /// <summary>Register a complex custom token.</summary>
    void RegisterToken(IManifest mod, string name, object token);
}
