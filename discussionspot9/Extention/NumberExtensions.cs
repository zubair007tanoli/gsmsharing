namespace discussionspot9.Extensions
{
    public static class NumberExtensions
    {
        public static string ToShortString(this int number)
        {
            if (number >= 1000000)
                return $"{number / 1000000.0:0.#}M";
            if (number >= 1000)
                return $"{number / 1000.0:0.#}k";
            return number.ToString();
        }

        public static string ToShortString(this long number)
        {
            if (number >= 1000000)
                return $"{number / 1000000.0:0.#}M";
            if (number >= 1000)
                return $"{number / 1000.0:0.#}k";
            return number.ToString();
        }
    }
}