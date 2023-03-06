namespace SoccerStatAnalyser.Models
{
    ///<summary>Class that contain classes and methods for processing and analysing match data</summary>
    public class Analyser
    {
        ///<summary>Class that contains a set of fixture stats </summary>
        public class AllFixtureStats
        {
            public FixtureStats RegularStats { get; set; }
            public FixtureStats HomeAwayStats { get; set; }
            public AllFixtureStats(FixtureStats regularStats, FixtureStats homeAwayStats)
            {

                RegularStats = regularStats;
                HomeAwayStats = homeAwayStats;
            }
        }

        ///<summary>Class that contains a Analyser stats for the home and away team</summary>
        public class FixtureStats
        {
            public AnalyserStats HomeTeamStats { get; set; }
            public AnalyserStats AwayTeamStats { get; set; }
            public int Duration { get; set; }
            public string Competition { get; set; }
            public FixtureStats(string homeTeamLink, string awayTeamLink, int duration, string competition, bool homeAwayOnly= false)
            {
                var homeHtmlresponse = HtmlHandler.GetHtmlAsync(homeTeamLink);
                var homeAllMatches = HtmlHandler.ParseMatchesFromTeamHtml(homeHtmlresponse.Result);
                var homeFilteredMatches = FilterMatches(homeAllMatches, duration, competition, homeAwayOnly ? "Home" : "None");

                var awayHtmlresponse = HtmlHandler.GetHtmlAsync(awayTeamLink);
                var awayAllMatches = HtmlHandler.ParseMatchesFromTeamHtml(awayHtmlresponse.Result);
                var awayFilteredMatches = FilterMatches(awayAllMatches, duration, competition, homeAwayOnly?"Away":"None");

                var homeTeamStats= new Analyser.AnalyserStats(HtmlHandler.ParseTeamFromTeamHtml(homeHtmlresponse.Result, homeTeamLink), homeFilteredMatches);
                var awayTeamStats= new Analyser.AnalyserStats(HtmlHandler.ParseTeamFromTeamHtml(awayHtmlresponse.Result, awayTeamLink), awayFilteredMatches);

                HomeTeamStats = homeTeamStats;
                AwayTeamStats = awayTeamStats;
                Duration = duration;
                Competition = competition;
            }

            public FixtureStats(AnalyserStats homeTeamStats, AnalyserStats awayTeamStats)
            {
                HomeTeamStats = homeTeamStats;
                AwayTeamStats = awayTeamStats;
            }
        }


        ///<summary>Used to filter a list of matches by duration, competition and venue</summary>
        ///<param name="matches">List of matches to be filtered</param>
        ///<param name="duration">filter number of previous matches to collate</param>
        ///<param name="competition">competition to filter matches for</param>
        ///<param name="venue">filter for Home and Away matches </param>
        ///<returns>a list filtered based on the parameters</returns>
        private static List<TeamMatch> FilterMatches(List<TeamMatch> matches, int duration, string competition, string venue = "None")
        {
            List<TeamMatch> result = matches;
            List<TeamMatch> tempResult = new List<TeamMatch>();

            if (competition != "All")
            {
                foreach (var m in result)
                {
                    if (m.Competition == competition)
                        tempResult.Add(m);
                }

                result = tempResult;
            }


            if (venue != "None")
            {
                tempResult = new List<TeamMatch>();

                foreach (var m in result)
                {
                    if (m.Venue == venue)
                        tempResult.Add(m);
                }

                result = tempResult;
            }

            int count = 0;
            tempResult = new List<TeamMatch>();
            for (int i = (result.Count() - 1); i >= 0; i--)
            {
                tempResult.Add(result[i]);
                count++;

                if (count == duration)
                    break;
            }

            result = tempResult;

            return result;
        }

        ///<summary>Class that contains a set of stats for a Team</summary>
        public class AnalyserStats
        {
            public Team Team { get; set; }
            public ResultStats ResultStats { get; set; }
            public GoalStats GoalStats { get; set; }
            public GoalForStats GoalForStats { get; set; }
            public GoalAgainstStats GoalAgainstStats { get; set; }
            public GoalMarginStats GoalMarginStats { get; set; }
            public List<Stat> AllStats { get; set; }
            public AnalyserStats(Team team, List<TeamMatch> matches)
            {
                Team = team;
                ResultStats = new ResultStats(matches);
                GoalStats = new GoalStats(matches);
                GoalForStats = new GoalForStats(matches);
                GoalAgainstStats = new GoalAgainstStats(matches);
                GoalMarginStats = new GoalMarginStats(matches);

                AllStats = new List<Stat>();
                AllStats.AddRange(ResultStats.AllStats);
                AllStats.AddRange(GoalStats.AllStats);
                AllStats.AddRange(GoalForStats.AllStats);
                AllStats.AddRange(GoalAgainstStats.AllStats);
                AllStats.AddRange(GoalMarginStats.AllStats);
            }

        }

        ///<summary>Win, Draw and Loss Stats</summary>
        public class ResultStats
        {
            public SingleStat Win { get; set; }
            public SingleStat Draw { get; set; }
            public SingleStat Loss { get; set; }
            public List<Stat> AllStats { get; set; }
            public ResultStats(List<TeamMatch> matches)
            {
                int totalWins = 0, totalDraws = 0, totalLosses = 0;
                foreach (var m in matches)
                {
                    switch (m.Result)
                    {
                        case 'W': totalWins++; break;
                        case 'D': totalDraws++; break;
                        case 'L': totalLosses++; break;
                    }
                }

                Win = new SingleStat("Win", RoundUp(((double)totalWins / (double)matches.Count()) * 100d, 2));
                Draw = new SingleStat("Draw", RoundUp(((double)totalDraws / (double)matches.Count()) * 100d, 2));
                Loss = new SingleStat("Loss", RoundUp(((double)totalLosses / (double)matches.Count()) * 100d, 2));

                AllStats = new List<Stat>();
                AllStats.Add(Win);
                AllStats.Add(Draw);
                AllStats.Add(Loss);
            }
        }

        ///<summary>Total Goals and Both teams to score stats</summary>
        public class GoalStats
        {
            public DoubleStat G0_5 { get; set; }
            public DoubleStat G1_5 { get; set; }
            public DoubleStat G2_5 { get; set; }
            public DoubleStat G3_5 { get; set; }
            public DoubleStat G4_5 { get; set; }
            public DoubleStat GG_NG { get; set; }
            public List<Stat> AllStats { get; set; }

            public GoalStats(List<TeamMatch> matches)
            {
                int g0_5 = 0, g1_5 = 0, g2_5 = 0, g3_5 = 0, g4_5 = 0, gg_ng = 0;

                foreach (var m in matches)
                {
                    int totalGoals = m.GoalsFor + m.GoalsAgainst;

                    switch (totalGoals)
                    {
                        case 1:
                            {
                                g0_5++;
                                break;
                            }
                        case 2:
                            {
                                g0_5++; g1_5++;
                                break;
                            }
                        case 3:
                            {
                                g0_5++; g1_5++; g2_5++;
                                break;
                            }
                        case 4:
                            {
                                g0_5++; g1_5++; g2_5++; g3_5++;
                                break;
                            }
                        case >= 5:
                            {
                                g0_5++; g1_5++; g2_5++; g3_5++; g4_5++;
                                break;
                            }
                    }

                    if (m.GoalsFor > 0 && m.GoalsAgainst > 0)
                    {
                        gg_ng++;
                    }

                }

                G0_5 = new DoubleStat("O/U O.5 Total Goals", RoundUp(((double)g0_5 / (double)matches.Count()) * 100d, 2));
                G1_5 = new DoubleStat("O/U 1.5 Total Goals", RoundUp(((double)g1_5 / (double)matches.Count()) * 100d, 2));
                G2_5 = new DoubleStat("O/U 2.5 Total Goals", RoundUp(((double)g2_5 / (double)matches.Count()) * 100d, 2));
                G3_5 = new DoubleStat("O/U 3.5 Total Goals", RoundUp(((double)g3_5 / (double)matches.Count()) * 100d, 2));
                G4_5 = new DoubleStat("O/U 4.5 Total Goals", RoundUp(((double)g4_5 / (double)matches.Count()) * 100d, 2));
                GG_NG = new DoubleStat("GG/NG", RoundUp(((double)gg_ng / (double)matches.Count()) * 100d, 2), "GG", "NG");


                AllStats = new List<Stat>();
                AllStats.Add(G0_5);
                AllStats.Add(G1_5);
                AllStats.Add(G2_5);
                AllStats.Add(G3_5);
                AllStats.Add(G4_5);
                AllStats.Add(GG_NG);
            }
        }

        ///<summary>Goals scored by a team stats</summary>
        public class GoalForStats
        {
            public DoubleStat G0_5 { get; set; }
            public DoubleStat G1_5 { get; set; }
            public DoubleStat G2_5 { get; set; }
            public DoubleStat G3_5 { get; set; }
            public DoubleStat G4_5 { get; set; }
            public List<Stat> AllStats { get; set; }
            public GoalForStats(List<TeamMatch> matches)
            {
                int g0_5 = 0, g1_5 = 0, g2_5 = 0, g3_5 = 0, g4_5 = 0;

                foreach (var m in matches)
                {
                    switch (m.GoalsFor)
                    {
                        case 1:
                            {
                                g0_5++;
                                break;
                            }
                        case 2:
                            {
                                g0_5++; g1_5++;
                                break;
                            }
                        case 3:
                            {
                                g0_5++; g1_5++; g2_5++;
                                break;
                            }
                        case 4:
                            {
                                g0_5++; g1_5++; g2_5++; g3_5++;
                                break;
                            }
                        case >= 5:
                            {
                                g0_5++; g1_5++; g2_5++; g3_5++; g4_5++;
                                break;
                            }
                    }
                }

                G0_5 = new DoubleStat("O/U O.5 Goals For", RoundUp(((double)g0_5 / (double)matches.Count()) * 100d, 2));
                G1_5 = new DoubleStat("O/U 1.5 Goals For", RoundUp(((double)g1_5 / (double)matches.Count()) * 100d, 2));
                G2_5 = new DoubleStat("O/U 2.5 Goals For", RoundUp(((double)g2_5 / (double)matches.Count()) * 100d, 2));
                G3_5 = new DoubleStat("O/U 3.5 Goals For", RoundUp(((double)g3_5 / (double)matches.Count()) * 100d, 2));
                G4_5 = new DoubleStat("O/U 4.5 Goals For", RoundUp(((double)g4_5 / (double)matches.Count()) * 100d, 2));

                AllStats = new List<Stat>();
                AllStats.Add(G0_5);
                AllStats.Add(G1_5);
                AllStats.Add(G2_5);
                AllStats.Add(G3_5);
                AllStats.Add(G4_5);
            }
        }

        ///<summary>Goals against by a team stats</summary>
        public class GoalAgainstStats
        {
            public DoubleStat G0_5 { get; set; }
            public DoubleStat G1_5 { get; set; }
            public DoubleStat G2_5 { get; set; }
            public DoubleStat G3_5 { get; set; }
            public DoubleStat G4_5 { get; set; }
            public List<Stat> AllStats { get; set; }
            public GoalAgainstStats(List<TeamMatch> matches)
            {
                int g0_5 = 0, g1_5 = 0, g2_5 = 0, g3_5 = 0, g4_5 = 0;

                foreach (var m in matches)
                {
                    switch (m.GoalsAgainst)
                    {
                        case 1:
                            {
                                g0_5++;
                                break;
                            }
                        case 2:
                            {
                                g0_5++; g1_5++;
                                break;
                            }
                        case 3:
                            {
                                g0_5++; g1_5++; g2_5++;
                                break;
                            }
                        case 4:
                            {
                                g0_5++; g1_5++; g2_5++; g3_5++;
                                break;
                            }
                        case >= 5:
                            {
                                g0_5++; g1_5++; g2_5++; g3_5++; g4_5++;
                                break;
                            }
                    }
                }

                G0_5 = new DoubleStat("O/U O.5 Goals Against", RoundUp(((double)g0_5 / (double)matches.Count()) * 100d, 2));
                G1_5 = new DoubleStat("O/U 1.5 Goals Against", RoundUp(((double)g1_5 / (double)matches.Count()) * 100d, 2));
                G2_5 = new DoubleStat("O/U 2.5 Goals Against", RoundUp(((double)g2_5 / (double)matches.Count()) * 100d, 2));
                G3_5 = new DoubleStat("O/U 3.5 Goals Against", RoundUp(((double)g3_5 / (double)matches.Count()) * 100d, 2));
                G4_5 = new DoubleStat("O/U 4.5 Goals Against", RoundUp(((double)g4_5 / (double)matches.Count()) * 100d, 2));

                AllStats = new List<Stat>();
                AllStats.Add(G0_5);
                AllStats.Add(G1_5);
                AllStats.Add(G2_5);
                AllStats.Add(G3_5);
                AllStats.Add(G4_5);
            }
        }

        ///<summary>Goals margin stats</summary>
        public class GoalMarginStats
        {
            public SingleStat Minus1 { get; set; }
            public SingleStat Minus2 { get; set; }
            public SingleStat Minus3 { get; set; }
            public SingleStat Zero { get; set; }
            public SingleStat Plus1 { get; set; }
            public SingleStat Plus2 { get; set; }
            public SingleStat Plus3 { get; set; }
            public List<Stat> AllStats { get; set; }
            public GoalMarginStats(List<TeamMatch> matches)
            {
                int minus1 = 0, minus2 = 0, minus3 = 0, zero = 0, plus1 = 0, plus2 = 0, plus3 = 0;

                foreach (var m in matches)
                {
                    int margin = m.GoalsFor - m.GoalsAgainst;
                    switch (margin)
                    {
                        case -1:
                            {
                                minus1++;
                                break;
                            }
                        case -2:
                            {
                                minus2++;
                                break;
                            }
                        case -3:
                            {
                                minus3++;
                                break;
                            }
                        case 0:
                            {
                                zero++;
                                break;
                            }
                        case 1:
                            {
                                plus1++;
                                break;
                            }
                        case 2:
                            {
                                plus2++;
                                break;
                            }
                        case 3:
                            {
                                plus3++;
                                break;
                            }
                    }
                }

                Minus1 = new SingleStat("Margin -1", RoundUp(((double)minus1 / (double)matches.Count) * 100d, 2));
                Minus2 = new SingleStat("Margin -2", RoundUp(((double)minus2 / (double)matches.Count) * 100d, 2));
                Minus3 = new SingleStat("Margin -3", RoundUp(((double)minus3 / (double)matches.Count) * 100d, 2));
                Zero = new SingleStat("Margin 0", RoundUp(((double)zero / (double)matches.Count) * 100d, 2));
                Plus1 = new SingleStat("Margin +1", RoundUp(((double)plus1 / (double)matches.Count) * 100d, 2));
                Plus2 = new SingleStat("Margin +2", RoundUp(((double)plus2 / (double)matches.Count) * 100d, 2));
                Plus3 = new SingleStat("Margin +3", RoundUp(((double)plus3 / (double)matches.Count) * 100d, 2));

                AllStats = new List<Stat>();
                AllStats.Add(Minus1);
                AllStats.Add(Minus2);
                AllStats.Add(Minus3);
                AllStats.Add(Zero);
                AllStats.Add(Plus1);
                AllStats.Add(Plus2);
                AllStats.Add(Plus3);
            }
        }

        ///<summary>type for storing over and under goal stats</summary>
        public struct OverUnder
        {
            public Double Over { get; }
            public Double Under { get; }
            public OverUnder(Double over)
            {
                this.Over = over;
                Under = RoundUp(100d - over, 2);
            }
        }

        ///<summary>interface for all stats</summary>
        public interface Stat
        {
            public string StatName { get; set; }
        }

        ///<summary>class for stats with a single value</summary>
        public class SingleStat : Stat
        {
            public string StatName { get; set; }
            public double Value { get; set; }

            public SingleStat(string statName, double value)
            {
                StatName = statName;
                Value = value;
            }
        }

        ///<summary>class for stats with over and under values</summary>
        public class DoubleStat : Stat
        {
            public string StatName { get; set; }
            public string OverStatName { get; set; }
            public string UnderStatName { get; set; }
            public OverUnder Value { get; set; }

            public DoubleStat(string statName, double value, string overStatName = "Over", string underStatName = "Under")
            {
                StatName = statName;
                Value = new OverUnder(value);
                OverStatName = overStatName;
                UnderStatName = underStatName;
            }
        }

        ///<summary>round up a double input to a specified number of places</summary>
        ///<param name="input">input to round up</param>
        ///<param name="places">number of places to round up input</param>
        ///<returns>a double rounded up to specified places </returns>
        public static double RoundUp(double input, int places)
        {
            double multiplier = Math.Pow(10, Convert.ToDouble(places));
            return Math.Ceiling(input * multiplier) / multiplier;
        }
    }
}
