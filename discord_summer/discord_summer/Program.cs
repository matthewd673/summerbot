using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using DSharpPlus;
using Newtonsoft.Json;

namespace discord_summer
{
    class Program
    {
        static void Main(string[] args)
        {

            Run().GetAwaiter().GetResult();

        }

        public static async Task Run()
        {
            var discord = new DiscordClient(new DiscordConfig
            {
                AutoReconnect = true,
                DiscordBranch = Branch.Stable,
                LargeThreshold = 250,
                Token = "MzMzMDAwNTUxNzExMzc1MzYx.DEGS7g.u20_KSXwVSC8bI4znHxAf29hkUA",
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true
            });

            discord.GuildAvailable += e =>
            {
                discord.DebugLogger.LogMessage(LogLevel.Info, "discord bot", $"Guild available: {e.Guild.Name}", DateTime.Now);
                return Task.Delay(0);
            };

            discord.MessageCreated += async e =>
            {
                bool summarize = false;

                if (e.Message.Content.StartsWith("s: "))
                {
                    summarize = true;

                }

                if(summarize)
                {
                    string urlToSummarize = e.Message.Content.Remove(0, 3);

                    string response = "";

                    using (WebClient client = new WebClient())
                    {
                        //execute request and read response as string to console
                        using (StreamReader reader = new StreamReader(client.OpenRead("http://api.smmry.com/&SM_API_KEY=B7B52E7FD9&SM_LENGTH=2&SM_URL=" + urlToSummarize)))
                        {
                            response = reader.ReadToEnd();
                        }
                    }

                    SMMRYResponse sr = JsonConvert.DeserializeObject<SMMRYResponse>(response);

                    sr.sm_api_content = sr.sm_api_content.Replace("*", "\\*");
                    sr.sm_api_content = sr.sm_api_content.Replace("_", "\\_");
                    sr.sm_api_content = sr.sm_api_content.Replace("#", "\\#");
                    sr.sm_api_content = sr.sm_api_content.Replace("~", "\\~");
                    sr.sm_api_content = sr.sm_api_content.Replace("@", "\\@");

                    DiscordEmbed de = new DiscordEmbed();
                    de.Title = sr.sm_api_title;
                    de.Description = sr.sm_api_content;

                    DiscordEmbedFooter def = new DiscordEmbedFooter();
                    def.Text = "Cut down to " + sr.sm_api_character_count.ToString() + " characters from " + urlToSummarize;

                    de.Footer = def;

                    await e.Message.RespondAsync("", false, de);
                }

            };

            await discord.ConnectAsync();

            await Task.Delay(-1);
        }

    }
}
