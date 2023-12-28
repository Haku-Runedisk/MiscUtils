using Mono.Cecil.Cil;
using Monocle;
using MonoMod.ModInterop;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Reflection;
using System.Numerics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using System.Linq;

namespace Celeste.Mod.MiscUtils {
    public static class Q {
        public static MiscUtilsModuleSettings Settings => MiscUtilsModule.Settings;
        public static Player player => Engine.Scene.Tracker.GetEntity<Player>();
        public static Level level => Engine.Scene as Level;
        public static double realDeltaTime = 0.0166666992008686065673828125d;
        public static double intendedDeltaTime = 1d / 60d;
        public static double deltaTimeRatio = realDeltaTime / intendedDeltaTime;
        public static double xGrid = 0;
        public static double yGrid = 0;
        public static string[] xSpeedIncrements = new string[] {
            // ground
            "(6)",
            "8.(3)",
            // air
            "4.(3)",
            "10.8(3)",
            // misc
            "1",
        };
        public static string[] ySpeedIncrements = new string[] {
            "5",
            "7.5",
            "15",
            "52.5",
            "160", 
            // misc
            "1",
        };

        public static double XDrift { get; internal set; }
        public static double YDrift { get; internal set; }
        public static double XDriftDiff { get; internal set; }
        public static double YDriftDiff { get; internal set; }
        public static Vector2 PrevSpeed { get; internal set; }
        public static Vector2 Accel { get; internal set; }
        public static bool ShowStunningInfo {
            get {
                if (Settings.ShowStunningInfo) {
                    return true;
                }
                switch (Settings.AutoShowStunningInfo) {
                    case AutoShowStunningInfoMode.None:
                        break;
                    case AutoShowStunningInfoMode.Room:
                        return Engine.Scene.Tracker.Entities.Keys.Intersect(StunnableEntities).FirstOrDefault((type) => Engine.Scene.Tracker.Entities[type].Any()) != null;
                    case AutoShowStunningInfoMode.Map:
                        break;
                    default:
                        break;
                }
                return Settings.ShowStunningInfo;
            }
        }

        public static HashSet<Type> StunnableEntities { get; private set; } = new() {
            typeof(CrystalStaticSpinner),
            typeof(Lightning),
        };

        public static string CustomInfoStr() {
            string res2;
            try {
                List<string> lines = new();
                if (Settings.Enabled) {
                    if (Settings.ShowMainCustomInfo) {
                        if (player != null) {
                            lines.Add($"a:{Accel.X:0.00},DashDir:{ValToCIString(player.DashDir, "f2")}");
                            lines.Add($"AutoJump:{FrameValToCIStr(player.AutoJumpTimer)},DAttack:{FrameValToCIStr(player.dashAttackTimer)}");
                            lines.Add($"ForceMoveX:{FrameValToCIStr(player.forceMoveXTimer)},JumpTimer:{FrameValToCIStr(player.varJumpTimer)}");
                        }
                        lines.Add($"Respawn:{ValToCIString(level.Session.RespawnPoint, "f1")}");
                        if (player != null) {
                            lines.Add(LineStr("aDepth", player.actualDepth));
                            lines.Add(LineStr("Depth", player.Depth));
                        }
                    }
                    if (ShowStunningInfo) {
                        lines.Add(LineStr("TimeActive", Engine.Scene.TimeActive));
                        lines.Add(LineStr("Cam", level.Camera.Position));
                        lines.Add(LineStr("CamOfs", level.CameraOffset));
                    }
                    if (Settings.ShowRoundingError) {
                        lines.Add($"reX:{GetXDriftStr()}");
                        lines.Add($"reY:{GetYDriftStr()}");
                    }
                    if (Settings.ShowMainCustomInfo) {
                        if (Engine.TimeRate != 1f) {
                            lines.Add(LineStr("TimeRate", Engine.TimeRate));
                        }
                        if (player != null) {
                            AddLineStrConditional(lines, "wsDir", player.wallSlideDir);
                        }
                    }
                }
                res2 = string.Join("\n", lines);
            } catch (Exception e) {
                res2 = "exception occurred:\n" + e;
            }
            return res2;
        }

        private static string FrameValToCIStr(float v, string format = null) {
            if (format == null) {
                format = "00";
            }
            return (ConvertToFrames(v)).ToString(format);
        }

        private static void AddLineStrConditional(List<string> lines, string label, int value) {
            if (value != 0) {
                lines.Add(LineStr(label, value));
            }
        }

        private static string LineStr(string label, string value) {
            return $"{label}:{value}";
        }
        private static string LineStr(string label, object value, string format = null) {
            return LineStr(label, ValToCIString(value, format));
        }

        private static string ValToCIString(object value, string format = null) {
            switch (value) {
                case Vector2 v:
                    return $"{ValToCIString(v.X, format)},{ValToCIString(v.Y, format)}";
                case double v:
                    if (format != null) {
                        return v.ToString(format);
                    }
                    return v.ToString("f8");
                case float v:
                    if (format != null) {
                        return v.ToString(format);
                    }
                    return v.ToString("f8");
                default:
                    return value.ToString();
            }
        }

        public static double GetXDrift() {
            double res = player.movementCounter.X + 0.5d;
            res = GetDrift(res, xGrid) / (realDeltaTime - intendedDeltaTime);
            return res;
        }
        public static double GetYDrift() {
            double res = player.movementCounter.Y + 0.5d;
            res = GetDrift(res, yGrid) / (realDeltaTime - intendedDeltaTime);
            return res;
        }

        private static double GetDrift(double val, double resolution) {
            double res = val % resolution;
            if (res > resolution / 2) {
                res -= resolution;
            }
            return res;
        }

        public static List<BigInteger> GetFractionFromDecimal(string input) {
            List<BigInteger> res = new List<BigInteger>();
            string valStr = "";
            string truncatedValStr = "";
            bool seenParen = false;
            bool seenPeriod = false;
            int prelimMultExp = 0;
            int postlimMultExp = 0;
            foreach (char c in input) {
                if (c == '(' || c == ')') {
                    seenParen = true;
                } else {
                    valStr += c;
                    if (char.IsDigit(c)) {
                        if (seenParen) {
                            truncatedValStr += "0";
                            postlimMultExp++;
                            continue;
                        } else {
                            if (seenPeriod) {
                                prelimMultExp += 1;
                            }
                        }
                    } else if (c == '.') {
                        seenPeriod = true;
                    }
                    truncatedValStr += c;
                }
            }
            if (!seenParen) {
                res.Add((BigInteger)(decimal.Parse(valStr) * (decimal)BigInteger.Pow(10, prelimMultExp)));
                res.Add(BigInteger.Pow(10, prelimMultExp));
            } else {
                decimal val = decimal.Parse(valStr) * (decimal)BigInteger.Pow(10, prelimMultExp) * (decimal)BigInteger.Pow(10, postlimMultExp);
                decimal truncatedVal = decimal.Parse(truncatedValStr) * (decimal)BigInteger.Pow(10, prelimMultExp);
                res.Add((BigInteger)(val - truncatedVal));
                res.Add(BigInteger.Pow(10, prelimMultExp + postlimMultExp) - BigInteger.Pow(10, prelimMultExp));
            }
            //Engine.Commands.Log(input + " -> " + res[0] + "/" + res[1]);
            return res;
        }
        public static List<BigInteger> FractionalGCD(params string[] decimalStrs) {
            List<List<BigInteger>> fractions = new();
            foreach (string decimalStr in decimalStrs) {
                fractions.Add(GetFractionFromDecimal(decimalStr));
            }
            return FractionalGCD(fractions);
        }

        private static List<BigInteger> FractionalGCD(List<List<BigInteger>> fractions) {
            if (fractions.Count < 1) {
                throw new NotSupportedException("fraction count less than 1");
            }
            List<BigInteger> res = new(fractions[0]);
            SimplifyFraction(res);
            if (fractions.Count == 1) {
                return res;
            }
            foreach (List<BigInteger> fraction in fractions.GetRange(1, fractions.Count - 1)) {
                List<BigInteger> working = new(fraction);
                res = FractionalGCD(res, working);
                //Engine.Commands.Log(res[0] + "/" + res[1]);
            }
            return res;
        }

        private static List<BigInteger> FractionalGCD(List<BigInteger> a, List<BigInteger> b) {
            List<BigInteger> res = new();
            res.Add(BigInteger.GreatestCommonDivisor(a[0] * b[1], a[1] * b[0]));
            res.Add(a[1] * b[1]);
            SimplifyFraction(res);
            return res;
        }

        private static void SimplifyFraction(List<BigInteger> res) {
            BigInteger gcd = BigInteger.GreatestCommonDivisor(res[0], res[1]);
            res[0] /= gcd;
            res[1] /= gcd;
        }

        private static void ValidateDivisibility(List<BigInteger> divident, List<BigInteger> divisor) {
            List<BigInteger> res = Divide(divident, divisor);
            if (res[1] != 1) {
                throw new Exception($"{FractionStr(divident)} not divisible by {FractionStr(divisor)}");
            }
        }

        private static string FractionStr(List<BigInteger> a) {
            return $"{a[0]}/{a[1]}";
        }

        private static List<BigInteger> Divide(List<BigInteger> divident, List<BigInteger> divisor) {
            List<BigInteger> res = new() { divident[0] * divisor[1], divident[1] * divisor[0] };
            SimplifyFraction(res);
            return res;
        }

        public static void HookSceneAfterUpdate(Scene self) {
            if (self is Level level && !level.wasPaused) {
                if (player != null) {
                    Accel = player.Speed - PrevSpeed;
                    PrevSpeed = player.Speed;

                    double prevXDrift = XDrift;
                    double prevYDrift = YDrift;
                    XDrift = GetXDrift();
                    YDrift = GetYDrift();
                    XDriftDiff = XDrift - prevXDrift;
                    YDriftDiff = YDrift - prevYDrift;
                }
            }
        }
        public static void Initialize() {
            List<BigInteger> xGCD = FractionalGCD(xSpeedIncrements);
            List<BigInteger> yGCD = FractionalGCD(ySpeedIncrements);
            List<BigInteger> one = new() { 1, 1 };
            ValidateDivisibility(one, xGCD);
            ValidateDivisibility(one, yGCD);
            xGrid = ((double)xGCD[0]) / ((double)xGCD[1]) / 60d;
            yGrid = ((double)yGCD[0]) / ((double)yGCD[1]) / 60d;
        }

        internal static string GetXDriftStr() {
            return GetDriftStr(XDrift, XDriftDiff);
        }

        internal static string GetYDriftStr() {
            return GetDriftStr(YDrift, YDriftDiff);
        }

        private static string GetDriftStr(double drift, double driftDiff) {
            return $"{drift:0.00},{driftDiff:0.00}";
        }

        // Following 3 functions based on https://github.com/EverestAPI/CelesteTAS-EverestInterop/blob/0deeb0af3ec7e8a3adce4471702d9931274d0cac/CelesteTAS-EverestInterop/Source/TAS/GameInfo.cs#L497
        private static int ToCeilingFrames(this float seconds) {
            return (int)Math.Ceiling(seconds / Engine.RawDeltaTime / Engine.TimeRateB);
        }
        private static int ToFloorFrames(this float seconds) {
            return (int)Math.Floor(seconds / Engine.RawDeltaTime / Engine.TimeRateB);
        }
        public static int ConvertToFrames(float seconds) {
            return seconds.ToCeilingFrames();
        }
        private static void AdjustTimeActive(decimal frames, double timeRate = 1d) {
            decimal absFrames = Math.Abs(frames);
            bool isPos = frames > 0;
            double taIncr = realDeltaTime * timeRate * (isPos ? 1d : -1d);
            for (long k = 0L; k < absFrames; k++) {
                level.TimeActive += (float)taIncr;
            }
        }
        private static void SetLiftboost(Vector2 liftSpeed) {
            player.currentLiftSpeed = liftSpeed;
            player.lastLiftSpeed = liftSpeed;
        }
        private static void SetLiftboost(Vector2 liftSpeed, float liftSpeedTimer) {
            SetLiftboost(liftSpeed);
            player.liftSpeedTimer = liftSpeedTimer;
        }
        private static void SetLiftboost(Vector2 liftSpeed, decimal liftSpeedFrames) {
            SetLiftboost(liftSpeed, (float)((double)(liftSpeedFrames - 0.5m) * realDeltaTime));
        }
        private static void ata(decimal frames, double timeRate) => AdjustTimeActive(frames, timeRate);
        private static void slb(Vector2 liftSpeed, decimal liftSpeedFrames) => SetLiftboost(liftSpeed, liftSpeedFrames);
        private static void i1(decimal frames) => AdjustTimeActive(frames);
        private static void i12(decimal frames, double timeRate) => AdjustTimeActive(frames, timeRate);
        private static void i2(Vector2 liftSpeed) => SetLiftboost(liftSpeed);
        private static void i22(Vector2 liftSpeed, decimal liftSpeedFrames) => SetLiftboost(liftSpeed, liftSpeedFrames);
    }
}