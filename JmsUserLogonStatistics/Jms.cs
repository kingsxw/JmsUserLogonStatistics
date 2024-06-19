using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http.Json;
using System.Runtime.InteropServices.Marshalling;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace JmsUserLogonStatistics
{
    internal class UserLogonLog
    {
        [DataMember]
        public string username { get; set; }
        public string reason { get; set; }
        //public string backend { get; set; }
        public bool status { get; set; }
        //public string datetime { get; set; }
    }
    internal class Jms
    {
        private string _baseUri;
        private string _token;
        private string _result;
        private List<UserLogonLog> _LogonLogs = [];
        HttpClient _httpClient;

        internal Jms(string baseUri, string token)
        {
            _baseUri = baseUri;
            _token = token;
        }

        private void PrintColoredString(string value, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(value);
        }
        private void PrintColoredString(string key, string value, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine("{0}\t\t{1} ", key, value);
        }
        internal async Task GetLogonStatisticsByMonth(int year, int month)
        {
            DateTime firstDay = new DateTime(year, month, 1).AddMilliseconds(1);
            DateTime lastDay = firstDay.AddMonths(1).AddMilliseconds(-2);
            string dateFrom = HttpUtility.UrlEncode(firstDay.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
            string dateTo = HttpUtility.UrlEncode(lastDay.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
            var client = new HttpClient();
            var uri = new Uri(_baseUri + $"?date_from={dateFrom}" + $"&date_to={dateTo}");
            var headers = client.DefaultRequestHeaders;
            headers.Add("Accept", "application/json");
            headers.Add("Authorization", $"Token {_token}");
            var response = await client.GetAsync(uri);
            ConsoleColor lastColor = Console.ForegroundColor;

            _result = await response.Content.ReadAsStringAsync();
            JsonArray json = JsonNode.Parse(_result).AsArray();


            foreach (var node in json)
            {

                if (node["backend"].ToString() != "Password")
                {
                    _LogonLogs.Add(node.Deserialize<UserLogonLog>());
                }
            }

            var logons =
                from word in _LogonLogs
                group word.username by word.username
                into g
                //where g.Count() > 1
                orderby g.Count() descending
                select new { g.Key, Count = g.Count() };
            var errors =
                from logonLog in _LogonLogs
                where logonLog.status == false
                group logonLog.username by logonLog.username
                into c
                orderby c.Count() descending
                select new { c.Key, Count = c.Count() };
            var reasons =
                from logonLog in _LogonLogs
                where logonLog.status == false
                group logonLog.reason by logonLog.reason
                into r
                orderby r.Count() descending
                select new { r.Key, Count = r.Count() };
            Console.Clear();
            PrintColoredString($"{year}年{month}月Jumpserver({uri.Host})用户登录数据统计", ConsoleColor.Cyan);
            PrintColoredString("\r\n所有用户总计登录次数:", ConsoleColor.Yellow);
            PrintColoredString(_LogonLogs.Count.ToString(), ConsoleColor.DarkGreen);

            PrintColoredString("\r\n各用户登陆次数:", ConsoleColor.Yellow);
            foreach (var logon in logons)
            {
                PrintColoredString(logon.Key, logon.Count.ToString(), ConsoleColor.DarkGreen);
            }
            PrintColoredString("\r\n各用户登录失败次数:", ConsoleColor.Yellow);
            foreach (var error in errors)
            {
                PrintColoredString(error.Key, error.Count.ToString(), ConsoleColor.DarkGreen);
            }
            PrintColoredString("\r\n登录失败原因汇总:", ConsoleColor.Yellow);
            foreach (var reason in reasons)
            {
                PrintColoredString(string.IsNullOrWhiteSpace(reason.Key.Trim()) ? "Jumpserver未记录原因" : reason.Key, reason.Count.ToString(), ConsoleColor.DarkGreen);
            }
            Console.ForegroundColor = lastColor;
        }
    }
}
