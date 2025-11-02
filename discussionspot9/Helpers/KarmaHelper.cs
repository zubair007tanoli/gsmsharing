namespace discussionspot9.Helpers
{
    public static class KarmaHelper
    {
        public static string GetKarmaLevel(int karmaPoints)
        {
            return karmaPoints switch
            {
                >= 10000 => "👑 Legend",
                >= 5000 => "🏅 Master",
                >= 2000 => "🌳 Expert",
                >= 500 => "🌿 Contributor",
                >= 50 => "🌱 Newbie",
                _ => "🌱 Newbie"
            };
        }

        public static string GetKarmaLevelName(int karmaPoints)
        {
            return karmaPoints switch
            {
                >= 10000 => "Legend",
                >= 5000 => "Master",
                >= 2000 => "Expert",
                >= 500 => "Contributor",
                >= 50 => "Newbie",
                _ => "Newbie"
            };
        }

        public static int GetKarmaProgressPercentage(int karmaPoints)
        {
            var nextLevel = GetNextLevelKarma(karmaPoints);
            var currentLevel = GetCurrentLevelKarma(karmaPoints);
            var range = nextLevel - currentLevel;
            if (range == 0) return 100;
            
            var progress = karmaPoints - currentLevel;
            return (int)((progress / (double)range) * 100);
        }

        public static int GetCurrentLevelKarma(int karmaPoints)
        {
            return karmaPoints switch
            {
                >= 10000 => 10000,
                >= 5000 => 5000,
                >= 2000 => 2000,
                >= 500 => 500,
                >= 50 => 50,
                _ => 0
            };
        }

        public static int GetNextLevelKarma(int karmaPoints)
        {
            return karmaPoints switch
            {
                >= 10000 => int.MaxValue,
                >= 5000 => 10000,
                >= 2000 => 5000,
                >= 500 => 2000,
                >= 50 => 500,
                _ => 50
            };
        }

        public static int GetKarmaNeededForNextLevel(int karmaPoints)
        {
            return GetNextLevelKarma(karmaPoints) - karmaPoints;
        }
    }
}

