namespace SoccerStatAnalyser.Models
{
    ///<summary>Model for scraped teams from Fbref link</summary>
    public class Team
    {
        public string Name { get; set; }
        public string FbrefLink { get; set; }
        public string LogoPath { get; set; }
    }
}
