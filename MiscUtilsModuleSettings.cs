namespace Celeste.Mod.MiscUtils {
    public class MiscUtilsModuleSettings : EverestModuleSettings {
        public bool Enabled { get; set; }
        [SettingSubHeader("Custom Info")]
        [SettingName("miscutils_show_rounding_error")]
        public bool ShowRoundingError { get; set; }
        [SettingName("miscutils_show_rounding_error")]
        public ButtonBinding ShowRoundingErrorBB { get; set; }
    }
}
