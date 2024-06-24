using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;

namespace JmsUserLogonStatistics
{
    internal class Jms
    {
        private string _baseUri;
        private string _token;
        private string _result;
        private string _title;
        private FullStatistic _fullStatistic = new FullStatistic();

        internal Jms(string baseUri, string token)
        {
            _baseUri = baseUri;
            _token = token;
        }
        public FullStatistic GetFullStatistic()
        {
            return _fullStatistic;
        }
        internal async Task GetLogonStatisticsByMonth(int year, int month)
        {
            DateTime firstDay = new DateTime(year, month, 1).AddMilliseconds(1);
            DateTime lastDay = firstDay.AddMonths(1).AddMilliseconds(-2);
            string dateFrom = HttpUtility.UrlEncode(firstDay.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
            string dateTo = HttpUtility.UrlEncode(lastDay.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
            var uri = new Uri(_baseUri + $"?date_from={dateFrom}" + $"&date_to={dateTo}");
            _fullStatistic.hostname = uri.Host;
            _fullStatistic.year = year.ToString();
            _fullStatistic.month = (month.ToString().Length == 1 ? "0" : "") + month.ToString();
            _title = $"{year}年{month}月Jumpserver({uri.Host})用户登录数据统计";

            using (var client = new HttpClient())
            {
                var headers = client.DefaultRequestHeaders;
                headers.Add("Accept", "application/json");
                headers.Add("Authorization", $"Token {_token}");
                var response = await client.GetAsync(uri);
                _result = await response.Content.ReadAsStringAsync();
            };

            JsonArray json = JsonNode.Parse(_result).AsArray();
            foreach (var node in json)
            {
                if (node["backend"].ToString() != "Password")
                {
                    _fullStatistic.logs.Add(node.Deserialize<UserLogonLog>());
                }
            }
            var logons =
                from word in _fullStatistic.logs
                group word.username by word.username
                into g
                //where g.Count() > 1
                orderby g.Count() descending
                select new { g.Key, Count = g.Count() };
            var errors =
                from logonLog in _fullStatistic.logs
                where logonLog.status == false
                group logonLog.username by logonLog.username
                into c
                orderby c.Count() descending
                select new { c.Key, Count = c.Count() };
            var reasons =
                from logonLog in _fullStatistic.logs
                where logonLog.status == false
                group logonLog.reason by logonLog.reason
                into r
                orderby r.Count() descending
                select new { r.Key, Count = r.Count() };

            Console.Clear();
            Base.PrintColoredString(_title, ConsoleColor.Cyan);

            _fullStatistic.logonCount = _fullStatistic.logs.Count;
            _fullStatistic.userCount = logons.Count();
            _fullStatistic.failCount = errors.Sum(error => error.Count);

            Base.PrintColoredString("\r\n总计登录用户人数:", ConsoleColor.Yellow);
            Base.PrintColoredString(_fullStatistic.userCount.ToString(), ConsoleColor.DarkGreen);

            Base.PrintColoredString("\r\n所有用户总计登录次数:", ConsoleColor.Yellow);
            Base.PrintColoredString(_fullStatistic.logonCount.ToString(), ConsoleColor.DarkGreen);

            Base.PrintColoredString("\r\n各用户登陆次数:", ConsoleColor.Yellow);
            foreach (var logon in logons)
            {
                _fullStatistic.userStats.Add(new UserLogonStatistic
                {
                    username = logon.Key,
                    totalCount = logon.Count
                });
                Base.PrintColoredString(logon.Key, logon.Count.ToString(), ConsoleColor.DarkGreen);
            }
            Base.PrintColoredString("\r\n各用户登录失败次数:", ConsoleColor.Yellow);
            foreach (var error in errors)
            {
                (_fullStatistic.userStats.FirstOrDefault(x => x.username == error.Key)).failCount = error.Count;
                Base.PrintColoredString(error.Key, error.Count.ToString(), ConsoleColor.DarkGreen);
            }

            Base.PrintColoredString("\r\n登录失败原因汇总:", ConsoleColor.Yellow);
            foreach (var reason in reasons)
            {
                var reasonString = string.IsNullOrWhiteSpace(reason.Key.Trim()) ? "Jumpserver未记录原因" : reason.Key;
                _fullStatistic.failStats.Add(new FailLogonStatistic
                {
                    reason = reasonString,
                    count = reason.Count
                });
                Base.PrintColoredString(reasonString, reason.Count.ToString(), ConsoleColor.DarkGreen);
            }
        }
    }
}
