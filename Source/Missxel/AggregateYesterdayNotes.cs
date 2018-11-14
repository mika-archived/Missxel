using System;
using System.Linq;
using System.Threading.Tasks;

using Disboard.Misskey;
using Disboard.Models;

using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

using Pixela;
using Pixela.Enums;

namespace Missxel
{
    public static class AggregateYesterdayNotes
    {
        [FunctionName(nameof(AggregateYesterdayNotes))]
        public static async Task Run([TimerTrigger("0 5 15 * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"{nameof(AggregateYesterdayNotes)} executed at {DateTime.Now}");
            var credential = new Credential
            {
                Domain = GetEnvironmentVariable("MISSKEY_DOMAIN"),
                ClientSecret = GetEnvironmentVariable("MISSKEY_CLIENT_SECRET"),
                AccessToken = GetEnvironmentVariable("MISSKEY_ACCESS_TOKEN")
            };

            var misskey = new MisskeyClient(credential);
            var me = await misskey.IAsync();
            var data = await misskey.Charts.Users.NotesAsync(me.Id, "hour", 24 * 2); // 2 days

            var notes = data.Inc.Skip(1).Take(24).Sum();

            var pixela = new PixelaClient(GetEnvironmentVariable("PIXELA_USER"), GetEnvironmentVariable("PIXELA_ACCESS_TOKEN"));

            var graphs = await pixela.Graphs.ListAsync();
            if (graphs.All(w => w.Id != "misskey"))
                await pixela.Graphs.CreateAsync("misskey", "Misskey Notes", "note", GraphType.Int, GraphColor.Sora, "Asia/Tokyo");
            await pixela.Pixel.CreateAsync("misskey", DateTime.Today, notes); // Azure Function's timezone is UTC

            log.LogInformation($"{nameof(AggregateYesterdayNotes)} ended at {DateTime.Now}");
        }

        private static string GetEnvironmentVariable(string variable)
        {
            return Environment.GetEnvironmentVariable(variable, EnvironmentVariableTarget.Process);
        }
    }
}