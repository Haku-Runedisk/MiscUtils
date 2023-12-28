namespace Celeste.Mod.MiscUtils {
    public class MiscUtilsModuleSettings : EverestModuleSettings {
        public bool Enabled { get; set; }
        public AutoShowStunningInfoMode AutoShowStunningInfo { get; internal set; }
        [SettingSubHeader("Custom Info")]
        [SettingName("miscutils_show_main_custom_info")]
        public bool ShowMainCustomInfo { get; set; }
        [SettingName("miscutils_show_stunning_info")]
        public bool ShowStunningInfo { get; internal set; }
        public bool ShowRoundingError { get; set; }

        [SettingName("miscutils_show_rounding_error")]
        public ButtonBinding ShowRoundingErrorBB { get; set; }
    }

    public enum AutoShowStunningInfoMode {
        None = 0,
        Room = 1,
        Map = 2,
    }
}
