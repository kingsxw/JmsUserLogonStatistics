using Sharprompt;
using System.Text.Json;
using System.Text.Json.Nodes;

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
            FullStatistic full = new FullStatistic();
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
                if (File.Exists(file))
                {
                    var fileString = File.ReadAllText(file);
                    var json = JsonNode.Parse(fileString).AsArray();
                    if (json.Count != 0)
                    {
                        serverList = json.Deserialize<List<ServerInfo>>();
                        foreach (var i in serverList)
                        {
                            serverNameList.Add(i.name);
                        }
                        serverNameList.Add("手动输入");
                        serverName = Prompt.Select("选择Jumpserver服务器", serverNameList, defaultValue: serverNameList[0]);
                    }
                    else
                    {
                        serverName = "手动输入";
                    }
                }
                else
                {
                    serverName = "手动输入";
                }

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
            full = s.GetFullStatistic();

            Console.WriteLine("\r\n\r\n");
            var isSave = Prompt.Confirm("是否保存Excel文件?", defaultValue: true);
            if (isSave)
            {
                Excel.OutXls(full);
            }

            Console.WriteLine("\r\n\r\n");
            var isQuit = Prompt.Confirm("是否退出?", defaultValue: false);
            if (isQuit)
            {
                Environment.Exit(0);
            }
            else
            {
                Console.Clear();
                await Program.Main(Array.Empty<string>());
            }
            //Base.PrintColoredString("\r\n\r\n按y/Y保存excel文件，任意键忽略", ConsoleColor.Cyan);
            //var key1 = Console.ReadKey(true);
            //if (key1.KeyChar == ('y' | 'Y'))
            //{
            //    var title = (month.ToString().Length == 1) ? (year + "-0" + month) : (year + "-" + month);
            //    var uri = new Uri(serverBaseUri);
            //    Excel.OutXls(full.userStats, title, uri.Host + $"-{title}.xlsx");
            //}

            //Base.PrintColoredString("\r\n\r\n按q/Q退出，任意键回到主菜单", ConsoleColor.Cyan);
            //var key2 = Console.ReadKey(true);
            //if (key2.KeyChar == ('q' | 'Q'))
            //{
            //    Environment.Exit(0);
            //}
            //else
            //{
            //    Console.Clear();
            //    await Program.Main(Array.Empty<string>());
            //}
        }
    }
}
