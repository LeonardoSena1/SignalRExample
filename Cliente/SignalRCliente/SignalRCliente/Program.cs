using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace SignalRCliente
{
    class Program
    {
        const string Url = "http://localhost:49778/chat";
        static async Task Main(string[] args)
        {
            await using var connection = new HubConnectionBuilder().WithUrl(Url).Build();

            await connection.StartAsync();

            await foreach(var date in connection.StreamAsync<DateTime>("Streaming"))
            {
                Console.WriteLine(date);
            }
        }
    }
}