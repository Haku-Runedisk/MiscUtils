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

namespace Celeste.Mod.MiscUtils {
    public static class Q {
        public static Player player = null;
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
        public static string XDriftStr { get; internal set; }
        public static string YDriftStr { get; internal set; }

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

        public static void Update() {
            // Scene scene = Engine.Scene;
            // if (scene is Level level) {
            //     player = level.Tracker.GetEntity<Player>();
            // }
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
            return GetDriftStr(Q.XDrift, Q.XDriftDiff);
        }

        internal static string GetYDriftStr() {
            return GetDriftStr(Q.YDrift, Q.YDriftDiff);
        }

        private static string GetDriftStr(double drift, double driftDiff) {
            return $"{drift.ToString("0.00")},{driftDiff.ToString("0.00")}";
        }
    }
}