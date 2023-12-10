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

namespace Celeste.Mod.MiscUtils {
    public static class UtilityMethods {
        public static Player player = null;
        public static double xGrid = 0;
        public static double yGrid = 0;
        public static string[] xSpeedIncrements = new string[] {
            // ground
            "6.(6)",
            "8.(3)",
            // air
            "4.(3)",
            "10.8(3)",
            // misc
            "1",
        };
        //public static string[,] xSpeedIncrements = new string[,] {
        //    {"40",""},
        //    {"8","3"},
        //    {"10.8","3"},
        //    {"4","3"}
        //};
        public static string[,] ySpeedIncrements = new string[,] {
            {"5",""},
            {"7.5",""},
            {"15",""},
            {"160",""}
        };

        public static double GetXDrift() {
            double res;
            res = player.X;
            return res;
        }
        public static double GetYDrift() {
            double res;
            res = player.X;
            GetFractionFromDecimal("");
            return res;
        }
        public static BigInteger[] GetFractionFromDecimal(string input) {
            BigInteger[] res = new BigInteger[2];
            decimal val = decimal.Parse(input);
            string truncatedValStr = "";
            bool seenParen = false;
            foreach (char c in input) {
                if (c == '(' || c == ')') {
                    seenParen = true;
                } else {
                    if (Char.IsDigit(c)) {
                        if (seenParen) {
                            truncatedValStr += "0";
                            continue;
                        }
                    }
                    truncatedValStr += c;
                }
            }
            decimal truncatedVal = decimal.Parse(truncatedValStr);
            Console.WriteLine(val);
            Console.WriteLine(truncatedValStr);
            Console.WriteLine(truncatedVal);
            return res;
        }
        //public static BigInteger[] GetFractionFromDecimal(string input) {
        //    BigInteger[] res = new BigInteger[2];
        //    bool seenDecimalPoint = false;
        //    bool seenOpenParenthesis = false;
        //    BigInteger multipleExp1 = -1;
        //    string num1 = "";
        //    string num2 = "";
        //    string den1 = "1";
        //    string den2 = "";
        //    string mult2 = "1";
        //    foreach (char c in input) {
        //        if (c == '.') {
        //            if (num1.Length == 0 || num2.Length == 0) {

        //            }
        //            seenDecimalPoint = true;
        //        } else if (c == '(') {
        //            seenOpenParenthesis = true;
        //        } else if (Char.IsDigit(c)) {
        //            if (seenOpenParenthesis) {
        //                num2 += c;
        //                den2 += "9";
        //                if (seenDecimalPoint) {
        //                    multipleExp1 -= 1;
        //                } else {
        //                    mult2 += "0";
        //                }
        //            } else {
        //                num1 += c;
        //                if (seenDecimalPoint) {
        //                    den1 += "0";
        //                } else {

        //                }
        //            }
        //            if (seenDecimalPoint) {
        //                multipleExp1 -= 1;
        //            } else {
        //                if (seenOpenParenthesis) {

        //                } else {
        //                    multipleExp1 += 1;
        //                }
        //            }
        //        }
        //    }
        //    return res;
        //}
        public static void Update() {
            // Scene scene = Engine.Scene;
            // if (scene is Level level) {
            //     player = level.Tracker.GetEntity<Player>();
            // }
        }
    }
}