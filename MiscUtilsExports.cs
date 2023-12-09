using MonoMod.ModInterop;
namespace Celeste.Mod.MiscUtils {
    /// <summary>
    /// Provides export functions for other mods to import.
    /// If you do not need to export any functions, delete this class and the corresponding call
    /// to ModInterop() in <see cref="MiscUtilsModule.Load"/>
    /// </summary>
    [ModExportName("MiscUtils")]
    public static class MiscUtilsExports {
        // TODO: add your mod's exports, if required
    }
}
