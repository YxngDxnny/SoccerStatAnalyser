using HtmlAgilityPack;
using System.Text;

namespace SoccerStatAnalyser.Models
{
    ///<summary>Class for handling http requests and scraping fbref HTML pages for data</summary>
    public static class HtmlHandler
    {
        ///<summary>Use to process request for an HTML pages</summary>
        ///<param name="url">Url of requested page</param>
        ///<returns>Http response for request</returns>
        public static async Task<string> GetHtmlAsync(string url)
        {
            HttpClient client = new HttpClient();

            var response = await client.GetStringAsync(url);
            return response;
        }

        ///<summary>Use to get a list of teams by scraping competition page</summary>
        ///<param name="html">html page to scrape</param>
        ///<returns>list of teams</returns>
        public static List<Team> ParseTeamsFromCompetitionHtml(string html)
        {
            List<Team> result = new List<Team>();

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var teamNodes = htmlDoc.DocumentNode.Descendants("table")
                .First().Descendants("td")
                .Where(node => node.GetAttributeValue("data-stat", "") == "team")
                .ToList();

            foreach (var tn in teamNodes)
            {
                var team = new Team();
                team.Name = GetTeamName(tn.InnerText);
                team.FbrefLink = "https://fbref.com" + tn.Descendants("a").First().GetAttributeValue("href", "");

                string miniImgSrc = tn.Descendants("img").First().GetAttributeValue("src", "");
                team.LogoPath = GetLogoPath(miniImgSrc);

                result.Add(team);
            }

            return result;

            ///<summary>Parse team logo link from mini logo in Competition Page</summary>
            ///<param name="miniImgSrc">Mini image link</param>
            ///<returns>link to large team logo</returns>
            string GetLogoPath(string miniImgSrc)
            {
                StringBuilder stringBuilder = new StringBuilder();
                int count = 0;
                string mini = "/mini.";

                foreach (var ch in miniImgSrc)
                {
                    bool hasCounted = count == mini.Length;

                    if (!hasCounted && ch == mini[count])
                    {
                        count++;

                        if (count == mini.Length)
                            stringBuilder.Append(mini[0]);

                        continue;
                    }

                    if (!hasCounted && count != 0)
                    {
                        int reWriteCount = 0;
                        while (reWriteCount < count)
                        {
                            stringBuilder.Append(mini[reWriteCount]);
                            reWriteCount++;
                        }

                        count = 0;
                        stringBuilder.Append(ch);
                        continue;
                    }

                    stringBuilder.Append(ch);
                }

                return stringBuilder.ToString();
            }

            ///<summary>Parse team name from html inner text on Competition Page</summary>
            ///<param name="innerText">HTML innerText</param>
            ///<returns>team name</returns>
            string GetTeamName(string innerText)
            {
                StringBuilder stringBuilder = new StringBuilder();

                foreach(var ch in innerText)
                {
                    if(ch==' ' && stringBuilder.Length == 0)
                    {
                        continue;
                    }

                    stringBuilder.Append(ch);
                }

                return stringBuilder.ToString();
            }
        }

        ///<summary>Parse team from team Page</summary>
        ///<param name="html">HTML page of the team</param>
        ///<param name="teamLink">team Fbref's link</param>
        ///<returns>link to large team logo</returns>
        public static Team ParseTeamFromTeamHtml(string html, string teamLink)
        {
            var team = new Team();
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var titleInnerText = htmlDoc.DocumentNode.Descendants("title")
                .First().InnerText;

            var logoPath = htmlDoc.DocumentNode.Descendants("img")
                .First().GetAttributeValue("src", "");

            team.Name = GetTeamName(titleInnerText);
            team.FbrefLink = teamLink;
            team.LogoPath = logoPath;

            ///<summary>Parse team name from html inner text on Team Page</summary>
            ///<param name="innerText">HTML innerText</param>
            ///<returns>team name</returns>
            string GetTeamName(string innerText)
            {
                StringBuilder stringBuilder = new StringBuilder();
                int count = 0;
                string stats = " Stats";

                foreach (var ch in innerText)
                {
                    bool hasCounted = count == stats.Length;
                    if (hasCounted) break;

                    if (!hasCounted && ch == stats[count])
                    {
                        count++;
                        continue;
                    }

                    if (!hasCounted && count != 0)
                    {
                        int reWriteCount = 0;
                        while (reWriteCount < count)
                        {
                            stringBuilder.Append(stats[reWriteCount]);
                            reWriteCount++;
                        }

                        count = 0;
                        stringBuilder.Append(ch);
                        continue;
                    }

                    stringBuilder.Append(ch);
                }

                return stringBuilder.ToString();
            }

            return team;
        }

        ///<summary>Parse all matches of current season from team Page</summary>
        ///<param name="html">HTML page of the team</param>
        ///<returns>List of all matches from current season</returns>
        public static List<TeamMatch> ParseMatchesFromTeamHtml(string html)
        {
            List<TeamMatch> result = new List<TeamMatch>();

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var matchNodes = htmlDoc.DocumentNode.Descendants("table")
                .Where(node => node.GetAttributeValue("id", "") == "matchlogs_for")
                .First().Descendants("tbody").First().Descendants("tr").ToList();

            foreach (var matchNode in matchNodes)
            {
                if (matchNode.Descendants("td").Where(node => node.GetAttributeValue("data-stat", "") == "result").First().InnerText == "")
                    continue;

                var teamMatch = new TeamMatch();
                teamMatch.Opponent = matchNode.Descendants("td").Where(node => node.GetAttributeValue("data-stat", "") == "opponent").First().InnerText;
                teamMatch.Venue = matchNode.Descendants("td").Where(node => node.GetAttributeValue("data-stat", "") == "venue").First().InnerText;
                teamMatch.Competition = matchNode.Descendants("td").Where(node => node.GetAttributeValue("data-stat", "") == "comp").First().InnerText;
                teamMatch.GoalsFor = Int32.Parse(GetGoals(matchNode.Descendants("td").Where(node => node.GetAttributeValue("data-stat", "") == "goals_for").First().InnerText));
                teamMatch.GoalsAgainst = Int32.Parse(GetGoals(matchNode.Descendants("td").Where(node => node.GetAttributeValue("data-stat", "") == "goals_against").First().InnerText));
                teamMatch.Result = matchNode.Descendants("td").Where(node => node.GetAttributeValue("data-stat", "") == "result").First().InnerText[0];
                result.Add(teamMatch);
            }

            return result;

            ///<summary>Get fulltime goal(s) from goal text parsed from page </summary>
            ///<param name="text">goal text parsed from page</param>
            ///<returns>fulltime goal(s) as string</returns>
            string GetGoals(string text)
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (var ch in text)
                {
                    if (!char.IsDigit(ch))
                        break;

                    stringBuilder.Append(ch);
                }

                return stringBuilder.ToString();
            }
        }

    }
}
