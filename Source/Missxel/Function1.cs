using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace Missxel
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static void Run([TimerTrigger("1 0 0 * * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}
