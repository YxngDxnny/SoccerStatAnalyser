namespace SoccerStatAnalyser.Models
{
    ///<summary>Model for scraped team match from Fbref link</summary>
    public class TeamMatch
    {
        public string Opponent { get; set; }
        public string Venue { get; set; }
        public string Competition { get; set; }
        public int GoalsFor { get; set; }
        public int GoalsAgainst { get; set; }
        public char Result { get; set; }
    }
}
