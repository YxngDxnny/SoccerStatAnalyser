namespace SoccerStatAnalyser.Models
{
    ///<summary>class that handles all available competitions</summary>
    public class Competition
    {
        ///<summary>use to get a competitions name from its id</summary>
        ///<param name="competitionId">id of the competition</param>
        ///<returns>name of the competion or All if id doesn't match any available competion</returns>
        public static string GetCompetitionName(int competitionId)
        {
            switch (competitionId)
            {
                case -1: return "All";
                case 0: return "Premier League";
                case 1: return "La Liga";
                case 2: return "Serie A";
                case 3: return "Bundesliga";
                case 4: return "Ligue 1";
                default: return "All";
            }
        }

        ///<summary>use to get a competitions logo's path from its id</summary>
        ///<param name="competitionId">id of the competition</param>
        ///<returns>Logo path of the competion</returns>
        public static string GetCompetitionLogo(int competitionId)
        {
            switch (competitionId)
            {
                case 0: return "/images/Premier_League_Logo.png";
                case 1: return "/images/La_Liga_Logo.png";
                case 2: return "/images/Serie_A_Logo.png";
                case 3: return "/images/Bundesliga_Logo.png";
                case 4: return "/images/Ligue_1_Logo.png";
                default: return "/images/CompetitionLogo.png";
            }
        }

        ///<summary>use to get a competitions Fbref link from its id</summary>
        ///<param name="competitionId">id of the competition</param>
        ///<returns>Fbref link of the competion</returns>
        public static string GetCompetitionLink(int competitionId)
        {
            switch (competitionId)
            {
                case 0: return "https://fbref.com/en/comps/9/Premier-League-Stats";
                case 1: return "https://fbref.com/en/comps/12/La-Liga-Stats";
                case 2: return "https://fbref.com/en/comps/11/Serie-A-Stats";
                case 3: return "https://fbref.com/en/comps/20/Bundesliga-Stats";
                case 4: return "https://fbref.com/en/comps/13/Ligue-1-Stats";
                default: return "https://fbref.com/en/comps/9/Premier-League-Stats";
            }
        }

    }
}
