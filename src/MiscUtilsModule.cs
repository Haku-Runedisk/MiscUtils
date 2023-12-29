using System;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using MonoMod.ModInterop;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.MiscUtils {
    public class MiscUtilsModule : EverestModule {
        private bool WasPaused;
        //private int counter;

        private bool FrozenEngine => Engine.FreezeTimer > 0f;

        public static MiscUtilsModule Instance { get; private set; }

        public override Type SettingsType => typeof(MiscUtilsModuleSettings);
        public static MiscUtilsModuleSettings Settings => (MiscUtilsModuleSettings)Instance._Settings;

        public override Type SessionType => typeof(MiscUtilsModuleSession);
        public static MiscUtilsModuleSession Session => (MiscUtilsModuleSession)Instance._Session;

        public MiscUtilsModule() {
            Instance = this;
#if DEBUG
            // debug builds use verbose logging
            Logger.SetLogLevel(nameof(MiscUtilsModule), LogLevel.Verbose);
#else
            // release builds use info logging to reduce spam in log files
            Logger.SetLogLevel(nameof(MiscUtilsModule), LogLevel.Info);
#endif
        }

        public override void Unload() {
            //Engine.Commands?.Log("unloaded MiscUtils");
            On.Celeste.LevelLoader.ctor -= LevelLoader_ctor;
            On.Celeste.OverworldLoader.ctor -= OverworldLoader_ctor;

            // TODO: unapply any hooks applied in Load()
            On.Celeste.Level.Update -= HookLevelUpdate;
            On.Monocle.Entity.Added -= Q.Entity_Added;
            Everest.Events.Player.OnSpawn -= OnPlayerSpawn;
            On.Celeste.Level.LoadLevel -= Q.Level_LoadLevel;
            On.Monocle.Scene.AfterUpdate -= HookSceneAfterUpdate;
            On.Monocle.MInput.Update -= HookMInputUpdate;
        }
        public override void Load() {
            //Engine.Commands?.Log("loaded MiscUtils");
            //typeof(MiscUtilsExports).ModInterop(); // TODO: delete this line if you do not need to export any functions

            On.Celeste.LevelLoader.ctor += LevelLoader_ctor;
            On.Celeste.OverworldLoader.ctor += OverworldLoader_ctor;

            // TODO: apply any hooks that should always be active
            //On.Celeste.Celeste.Update += UtilityMethods.Update;
            On.Celeste.Level.Update += HookLevelUpdate;
            On.Monocle.Entity.Added += Q.Entity_Added;
            Everest.Events.Player.OnSpawn += OnPlayerSpawn;
            On.Celeste.Level.LoadLevel += Q.Level_LoadLevel;
            using (new DetourContext { Before = new() { "CelesteTAS" } }) {
                On.Monocle.Scene.AfterUpdate += HookSceneAfterUpdate;
            }
            using (new DetourContext { After = new() { "CelesteTAS" } }) {
                On.Monocle.MInput.Update += HookMInputUpdate;
            }

            //IL.Celeste.Player.Update += Player_Update;
        }

        //private void Player_Update(MonoMod.Cil.ILContext il) {
        //    ILCursor cursor = new ILCursor(il);
        //    cursor.EmitDelegate(() => Logger.Log(LogLevel.Debug, "test", "hello from lambda, this should trigger a warning"));
        //}

        private void HookMInputUpdate(On.Monocle.MInput.orig_Update orig) {
            if (Settings.Enabled) {
                if (Engine.Scene is Level level) {
                    //Engine.Commands.Log("checking press");
                    if (Settings.ShowRoundingErrorBB.Pressed) {
                        //Engine.Commands.Log("toggle");
                        Settings.ShowRoundingError ^= true;
                    }
                }
            }

            orig();
        }

        private void HookSceneAfterUpdate(On.Monocle.Scene.orig_AfterUpdate orig, Monocle.Scene self) {
            orig(self);

            if (Settings.Enabled) {
                Q.HookSceneAfterUpdate(self);
            }
        }

        public void LoadBeforeLevel() {
            //On.Celeste.Mod.AssetReloadHelper.ReloadLevel += AssetReloadHelper_ReloadLevel;

            // TODO: apply any hooks that should only be active while a level is loaded
        }

        public void UnloadAfterLevel() {
            //On.Celeste.Mod.AssetReloadHelper.ReloadLevel -= AssetReloadHelper_ReloadLevel;

            // TODO: unapply any hooks applied in LoadBeforeLevel()
        }

        private void HookLevelUpdate(On.Celeste.Level.orig_Update orig, Level self) {
            if (Settings.Enabled) {
                //WasPaused = Engine.Scene is Level level && Engine.FreezeTimer <= 0f && level.unpauseTimer <= 0f && !level.FrozenOrPaused && !level.PauseMainMenuOpen;
                WasPaused = Engine.Scene is Level level && Engine.FreezeTimer <= 0f && !level.wasPaused;
            }

            orig(self);

        }

        //private void AssetReloadHelper_ReloadLevel(On.Celeste.Mod.AssetReloadHelper.orig_ReloadLevel orig) {
        //    orig();

        //    // TODO: anything that should happen after assets are reloaded with F5
        //}

        private void OverworldLoader_ctor(On.Celeste.OverworldLoader.orig_ctor orig, OverworldLoader self, Overworld.StartMode startmode, HiresSnow snow) {
            orig(self, startmode, snow);
            if (startmode != (Overworld.StartMode)(-1)) {
                UnloadAfterLevel();
            }
        }

        private void LevelLoader_ctor(On.Celeste.LevelLoader.orig_ctor orig, LevelLoader self, Session session, Vector2? startposition) {
            orig(self, session, startposition);
            LoadBeforeLevel();
        }
        public override void Initialize() {
            Q.Initialize();
        }

        private void OnPlayerSpawn(Player player) {
        }
    }
}