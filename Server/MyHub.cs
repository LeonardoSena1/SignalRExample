using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public class MyHub : Hub
    {
        public async IAsyncEnumerable<DateTime> Streaming(CancellationToken token)
        {
            while (true)
            {
                yield return DateTime.Now;
                await Task.Delay(1000, token);
            }
        }
    }
}