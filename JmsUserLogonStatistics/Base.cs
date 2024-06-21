using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JmsUserLogonStatistics
{
    internal class Base
    {
        internal static void PrintColoredString(string value, ConsoleColor color)
        {
            var lastColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(value);
            Console.ForegroundColor = lastColor;
        }
        internal static void PrintColoredString(string key, string value, ConsoleColor color)
        {
            var lastColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine("{0}\t\t{1} ", key, value);
            Console.ForegroundColor = lastColor;
        }
        internal static void PrintColoredString(string key, string value1, string value2, ConsoleColor color)
        {
            var lastColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine("{0}\t\t{1}\t\t{2} ", key, value1, value2);
            Console.ForegroundColor = lastColor;
        }
    }
    internal class ServerInfo
    {
        public string name { get; set; }
        public string baseUri { get; set; }
        public string token { get; set; }
    }
    internal class UserLogonLog : UserInfo
    {
        //[DataMember]
        public string reason { get; set; }
        //public string backend { get; set; }
        public bool status { get; set; }
        //public string datetime { get; set; }
    }
    internal class UserLogonStatistic : UserInfo
    {
        public int totalCount { get; set; } = 0;
        public int failCount { get; set; } = 0;
    }

    internal class FailLogonStatistic
    {
        public string reason { get; set; }
        public int count { get; set; } = 0;
    }
    internal class UserInfo
    {
        public string username { get; set; }
    }

    internal class FullStatistic
    {
        public string hostname { get; set; }
        public string year { get; set; }
        public string month { get; set; }
        public int logonCount { get; set; }
        public int userCount { get; set; }
        public int failCount { get; set; }
        public List<UserLogonLog> logs { get; set; } = [];
        public List<UserLogonStatistic> userStats { get; set; } = [];
        public List<FailLogonStatistic> failStats { get; set; } = [];
    }
}
