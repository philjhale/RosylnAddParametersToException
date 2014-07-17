using System;
using System.Linq;

namespace AddParametersToException.Utils
{
    public static class StringExtensions
    {
        public static string FirstCharToUpper(this string input)
        {
            if (String.IsNullOrEmpty(input))
                return input;
            return input.First().ToString().ToUpper() + String.Join("", input.Skip(1));
        }
    }
}