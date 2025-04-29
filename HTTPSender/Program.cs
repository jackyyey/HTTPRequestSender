using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Console.WriteLine("Is domain name a standard domain name? (y/n)");
            if (Console.ReadLine().ToLower() == "y" ? Regex.Match(domaine, "[A-Za-z0-9]+\\.[A-Za-z0-9]+").Success : true) //Regex.Match(domaine, "[A-Za-z0-9]+\\.[A-Za-z0-9]+").Success
            {
                HttpClientHandler httpClientHander = new HttpClientHandler();
                HttpClient httpClient = new HttpClient(httpClientHander) { BaseAddress = new Uri("http://"+domaine+"/") };
                using StringContent queryBody = new(
                    JsonSerializer.Serialize(new
                    {
                        username = "MrPipo" + new Random().Next(1,9999),
                        password = "PIPO"
                    }),
                    Encoding.UTF8,
                    "application/json"
                    );

                using HttpResponseMessage response = await httpClient.PostAsync("api/id/signup", queryBody);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Insert new task value:");
                    try
                    {
                        int progress = int.Parse(Console.ReadLine());
                    }
                    catch
                    {
                        Console.WriteLine("Value not a number, retrying in 5 seconds");
                        Thread.Sleep(5000);
                        Console.Clear();
                        await start();
                    }
                    var cookies = httpClientHander.CookieContainer.GetCookies(new Uri("http://" + domaine + "/"));
                    var cookieContainer = new CookieContainer();
                    using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
                    using (var client = new HttpClient(handler) { BaseAddress = httpClient.BaseAddress })
                    {
                        var cookie = cookies.First();
                        cookieContainer.Add(httpClient.BaseAddress, new Cookie("JSESSIONID", cookies.Where(s=>s.Name == "JSESSIONID").First().Value));
                        var result = await client.GetAsync("/api/progress/" + id + "/100");
                        Console.WriteLine("Success");
                        System.Environment.Exit(0);
                    }
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
