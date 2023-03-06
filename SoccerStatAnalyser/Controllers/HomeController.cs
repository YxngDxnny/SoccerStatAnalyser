using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using SoccerStatAnalyser.Models;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using static System.Net.WebRequestMethods;

namespace SoccerStatAnalyser.Controllers
{
    ///<summary>Controller class for main page</summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        ///<summary>Creates pagemodel and returns index page view</summary>
        ///<param name="competitionId">Selected competition's id to build pagemodel</param>
        ///<returns>index page view</returns>
        public IActionResult Index(int competitionId=0)
        {
            var model = new PageModel(competitionId);
            if(model.AllTeams==null)
            {
                return Problem();
            }
            return View(model);
        }

        ///<summary>Creates pagemodel and returns index page view</summary>
        ///<param name="homeTeamLink">Home team's Fbref Link</param>
        ///<param name="awayTeamLink">Away team's Fbref Link</param>
        ///<param name="duration">number of previous matches to filter</param>
        ///<param name="competitionId">competition to filter for</param>
        ///<returns>index page view passing page model with analyser stats</returns>
        public IActionResult Analyse(int competitionId, string homeTeamLink, string awayTeamLink, int duration, int competitionFilterId)
        {
            string competitionFilter= Competition.GetCompetitionName(competitionFilterId);
            var regModel = new Analyser.FixtureStats(homeTeamLink, awayTeamLink, duration, competitionFilter);
            var homeAwayModel = new Analyser.FixtureStats(homeTeamLink, awayTeamLink, duration, competitionFilter, true);
            var allStatsModel = new Analyser.AllFixtureStats(regModel, homeAwayModel);

            var pageModel = new PageModel(competitionId, allStatsModel);
            if (pageModel.AllTeams == null)
            {
                return Problem();
            }
            return View("~/Views/Home/Index.cshtml", pageModel);
        }

        ///<summary>Class that contains all info to build index page view</summary>
        public class PageModel
        {
            public string[] LogoLinks { get; set; }
            public int CompetitionID { get; set; }
            public List<Team> AllTeams
            {
                get { return _allTeams; }
                set
                {
                    List<Team> orderedAtoZ = value.OrderBy(s => s.Name).ToList();
                    _allTeams = orderedAtoZ;
                }
            }
            public List<Team> _allTeams;
            public Analyser.AllFixtureStats AllStats { get; set; }
            public int HomeTeamId { get; set; }
            public int AwayTeamId { get; set; }

            public PageModel(int competitionId, Analyser.AllFixtureStats allStats =null)
            {
                string url = Competition.GetCompetitionLink(competitionId);
                var response = HtmlHandler.GetHtmlAsync(url);
                List<Team> teams;
                try
                {
                     teams = HtmlHandler.ParseTeamsFromCompetitionHtml(response.Result);
                }
                catch(AggregateException)
                {
                    teams = null;
                }

                if(teams != null)
                {
                    AllTeams = teams;
                    CompetitionID = competitionId;

                    LogoLinks = new string[5];
                    int i = 0;
                    while (i < LogoLinks.Length)
                    {
                        LogoLinks[i] = Competition.GetCompetitionLogo(i);
                        i++;
                    }

                    AllStats = allStats;

                    if (AllStats != null)
                    {
                        var query = from t in AllTeams where(t.Name == AllStats.RegularStats.HomeTeamStats.Team.Name) select t;
                        Team homeTeam = AllTeams.Where(t => t.FbrefLink == AllStats.RegularStats.HomeTeamStats.Team.FbrefLink).First();
                        HomeTeamId = AllTeams.IndexOf(homeTeam);
                        Team awayTeam = AllTeams.Where(t => t.FbrefLink == AllStats.RegularStats.AwayTeamStats.Team.FbrefLink).First();
                        AwayTeamId = AllTeams.IndexOf(awayTeam);
                    }
                    else
                    {
                        HomeTeamId = 0;
                        AwayTeamId = 1;
                    }
                } 
            }

            public PageModel()
            {

            }
        }
    }
}