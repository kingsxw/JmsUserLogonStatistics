using System.ComponentModel.Design;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Nodes;
using Sharprompt;

namespace JmsUserLogonStatistics
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Prompt.ColorSchema.Answer = ConsoleColor.Yellow;
            Prompt.ColorSchema.Select = ConsoleColor.Green;
            Prompt.ColorSchema.PromptSymbol = ConsoleColor.Blue;
            Prompt.ColorSchema.Hint = ConsoleColor.DarkGreen;
            Prompt.ColorSchema.DisabledOption = ConsoleColor.DarkCyan;
            Prompt.ColorSchema.DoneSymbol = ConsoleColor.DarkGreen;
            Prompt.Symbols.Prompt = new Symbol("🤔", "?");
            Prompt.Symbols.Done = new Symbol("😎", "V");
            Prompt.Symbols.Error = new Symbol("😱", ">>");

            string serverBaseUri, serverUri, serverName = default, serverToken = default;
            int year, month;
            string uriSuffix = "/api/v1/audits/login-logs/";
            var defaultDate = DateTime.Now.AddMonths(-1);

            if (args.Length == 0)
            {
                List<ServerInfo> serverList = new List<ServerInfo>();
                List<string> serverNameList = new List<string>();

#if DEBUG
                var file = "servers.dev.json";
#else
                var file = "servers.json";
#endif

                var fileString = File.ReadAllText(file);
                var json = JsonNode.Parse(fileString).AsArray();
                if (json.Count != 0)
                {
                    serverList = json.Deserialize<List<ServerInfo>>();
                    foreach (var i in serverList)
                    {
                        serverNameList.Add(i.name);
                    }
                }

                serverNameList.Add("手动输入");
                serverName = Prompt.Select("选择Jumpserver服务器", serverNameList, defaultValue: serverNameList[0]);

                if (serverName == "手动输入")
                {
                    serverBaseUri = Prompt.Input<string>("输入Jumpserver服务器URL地址，例如https://jms.sumpay.cn或http://192.168.11.11", validators: new[] { Validators.Required(), Validators.RegularExpression("^http[s]{0,1}://[A-Za-z0-9.]{1,}$") });

                    serverToken = Prompt.Input<string>("输入Token", validators: new[]
                    {
                        Validators.Required(), Validators.MinLength(40),Validators.MaxLength(40),Validators.RegularExpression("^[a-z0-9]{40}$")
                    });
                }

                else
                {
                    var server =
                        from serverInfo in serverList
                        where serverInfo.name == serverName
                        select serverInfo;

                    //ServerInfo server = (ServerInfo)serverList.Where(x => x.name == serverName);
                    serverBaseUri = server.ToList()[0].baseUri;
                    serverToken = server.ToList()[0].token;
                }
                year = Prompt.Input<int>("输入要导出的年份", defaultValue: defaultDate.Year);
                month = Prompt.Input<int>("输入要导出的月份", defaultValue: defaultDate.Month);

            }
            else
            {
                serverBaseUri = args[0];
                serverToken = args[1];
                year = int.Parse(args[2]);
                month = int.Parse(args[3]);
               
            }
            serverUri = serverBaseUri + uriSuffix;
            var s = new Jms(serverUri, serverToken);
            await s.GetLogonStatisticsByMonth(year, month);
            Console.ReadKey();
        }
    }
    internal class ServerInfo
    {
        public string name { get; set; }
        public string baseUri { get; set; }
        public string token { get; set; }
    }
}
