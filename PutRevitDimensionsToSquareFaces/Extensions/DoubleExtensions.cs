using System;

namespace PutRevitDimensionsToSquareFaces.Extensions
{
    public static class DoubleExtensions
    {
        private const double DefaultTolerance = 0.00000001;

        public static bool IsAlmostEqualToOrMoreThan(this double x, double value)
        {
            return IsAlmostEqualTo(x, value) || x > value;
        }

        public static bool IsAlmostEqualToOrMoreThan(this double x, double value, double tolerance)
        {
            return IsAlmostEqualTo(x, value, tolerance) || x > value;
        }

        public static bool IsAlmostEqualToOrLessThan(this double x, double value)
        {
            return IsAlmostEqualToOrLessThan(x, value, DefaultTolerance);
        }

        public static bool IsAlmostEqualToOrLessThan(this double x, double value, double tolerance)
        {
            return IsAlmostEqualTo(x, value, tolerance) || x < value;
        }
        
        public static bool IsMoreThan(this double x, double value)
        {
            return !IsAlmostEqualTo(x, value) && x > value;
        }

        public static bool IsMoreThan(this double x, double value, double tolerance)
        {
            return !IsAlmostEqualTo(x, value, tolerance) && x > value;
        }

        public static bool IsMoreThanZero(this double x)
        {
            return IsMoreThan(x, 0);
        }

        public static bool IsLessThan(this double x, double value)
        {
            return !IsAlmostEqualTo(x, value) && x < value;
        }

        public static bool IsLessThan(this double x, double value, double tolerance)
        {
            return !IsAlmostEqualTo(x, value, tolerance) && x < value;
        }

        public static bool IsAlmostEqualTo(this double x, double value)
        {
            return IsAlmostEqualTo(x, value, DefaultTolerance);
        }

        public static bool IsAlmostEqualTo(this double x, double value, double tolerance)
        {
            return Math.Abs(x - value) < tolerance;
        }

        public static bool IsAlmostEqualToZero(this double x)
        {
            return IsAlmostEqualTo(x, 0);
        }

        public static bool IsAlmostEqualToZero(this double x, double tolerance)
        {
            return IsAlmostEqualTo(x, 0, tolerance);
        } 

        public static bool IsAlmostBetween(this double x, double min, double max)
        {
            return IsAlmostEqualToOrMoreThan(x, min) && IsAlmostEqualToOrLessThan(x, max);
        }

        public static bool IsAlmostBetween(this double x, double min, double max, double tolerance)
        {
            return IsAlmostEqualToOrMoreThan(x, Math.Min(min, max), tolerance) && IsAlmostEqualToOrLessThan(x, Math.Max(max, min), tolerance);
        }
    }
}