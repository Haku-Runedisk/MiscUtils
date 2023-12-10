﻿using System;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.ModInterop;

namespace Celeste.Mod.MiscUtils {
    public class MiscUtilsModule : EverestModule {
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

        public override void Load() {
            //typeof(MiscUtilsExports).ModInterop(); // TODO: delete this line if you do not need to export any functions

            On.Celeste.LevelLoader.ctor += LevelLoader_ctor;
            On.Celeste.OverworldLoader.ctor += OverworldLoader_ctor;

            // TODO: apply any hooks that should always be active
        }

        public override void Unload() {
            On.Celeste.LevelLoader.ctor -= LevelLoader_ctor;
            On.Celeste.OverworldLoader.ctor -= OverworldLoader_ctor;

            // TODO: unapply any hooks applied in Load()
        }

        public void LoadBeforeLevel() {
            //On.Celeste.Mod.AssetReloadHelper.ReloadLevel += AssetReloadHelper_ReloadLevel;

            // TODO: apply any hooks that should only be active while a level is loaded
            if (Settings.Enabled) {
                //On.Celeste.Celeste.Update += UtilityMethods.Update;
                Everest.Events.Player.OnSpawn += OnPlayerSpawn;
            }
            //Scene scene = Engine.Scene;
            //if (scene is Level level) {
            //    UtilityMethods.player = level.Tracker.GetEntity<Player>();
            //}
        }

        public void UnloadAfterLevel() {
            //On.Celeste.Mod.AssetReloadHelper.ReloadLevel -= AssetReloadHelper_ReloadLevel;

            // TODO: unapply any hooks applied in LoadBeforeLevel()
            Everest.Events.Player.OnSpawn -= OnPlayerSpawn;
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

        private void OnPlayerSpawn(Player player) {
            UtilityMethods.player = player;
        }
    }
}