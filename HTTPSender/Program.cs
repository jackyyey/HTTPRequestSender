using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace HTTPSender
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await start();
        }

        static async Task start()
        {
            Console.WriteLine("Enter Domain Name");
            string domaine = Console.ReadLine();
            Console.WriteLine("Enter Task Id");
            string id = Console.ReadLine();
            if (true) //Regex.Match(domaine, "[A-Za-z0-9]+\\.[A-Za-z0-9]+").Success
            {
                HttpClient httpClient = new HttpClient() { BaseAddress = new Uri("http://"+domaine+"/") };
                using StringContent queryBody = new(
                    JsonSerializer.Serialize(new
                    {
                        username = "MrPipo23",
                        password = "PIPO"
                    }),
                    Encoding.UTF8,
                    "application/json"
                    );

                using HttpResponseMessage response = await httpClient.PostAsync("api/id/signup", queryBody);
                response.EnsureSuccessStatusCode();
                if (response.IsSuccessStatusCode)
                {
                    IEnumerable<string> cookies = response.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value;
                    var cookieContainer = new CookieContainer();
                    using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
                    using (var client = new HttpClient(handler) { BaseAddress = httpClient.BaseAddress })
                    {
                        cookieContainer.Add(httpClient.BaseAddress, new Cookie("JSESSIONID",cookies.First()));
                        var result = await client.GetAsync("/api/progres/"+id+"/100");

                    }
                    
                else
                {
                    Console.Clear();
                    Console.WriteLine("Account creation broke");
                    await start();
                }
            }
            else
            {
                Console.Clear();
                Console.WriteLine("Input is not a domain");
                await start();
                
            }
        }

    }
}
