using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
            Console.WriteLine("Enter Task Name");
            string taskname = Console.ReadLine();
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
                Console.WriteLine("Creating user...");

                // Créer un compte
                using HttpResponseMessage response = await httpClient.PostAsync("api/id/signup", queryBody); 

                if (response.IsSuccessStatusCode) // Vérifie que le serveur renvoie un status 200
                {
                    Console.Clear();
                    Console.WriteLine("Account creation Success");
                    Thread.Sleep(600);
                    Console.Clear();
                    Console.WriteLine("How many tries?");
                    string tries = Console.ReadLine();
                    int nbTries = 0;
                    try { nbTries = int.Parse(tries); }catch { Console.WriteLine("Input is not a number"); Console.WriteLine("resetting..."); await start(); }
                    
                    Console.WriteLine("Finding task id...");

                    int? id = await getTaskIdFromName(nbTries,0, taskname, httpClient);
                    if (id == null)
                    {
                        Console.WriteLine("Task does not exist");
                        Thread.Sleep(200);
                        Console.WriteLine("Recommencer? (Y/n)");
                        string retry = Console.ReadLine();
                        if (retry.ToLower() == "y")
                        {
                            await start();
                        }
                        else
                        {
                            Console.WriteLine("Shutting Down");
                            Thread.Sleep(5000);
                            System.Environment.Exit(0);
                        }
                    }
                    Console.WriteLine("Insert new task value:"); // Nouvelle pourcentage tâche
                    int progress = 0;
                    try
                    {
                        progress = int.Parse(Console.ReadLine());
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
                        // Get JSESSIONID
                        var cookie = cookies.First();
                        cookieContainer.Add(httpClient.BaseAddress, new Cookie("JSESSIONID", cookies.Where(s=>s.Name == "JSESSIONID").First().Value)); // idk if i need to do this but put cookie in requête
                        
                        // Modifie pourcentage tâche
                        var result = await client.GetAsync("/api/progress/" + id + "/" + progress);
                        Console.Clear();
                        Console.WriteLine("Success");
                        Thread.Sleep(3000);
                        Console.WriteLine("Recommencer? (Y/n)");
                        string retry = Console.ReadLine();
                        if (retry.ToLower() == "y")
                        {
                            await start();
                        }
                        else
                        {
                            Console.WriteLine("Shutting Down");
                            Thread.Sleep(5000);
                            System.Environment.Exit(0);
                        }
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
        static async Task<int?> getTaskIdFromName(int noTries,int startingPoint, string taskname,HttpClient httpClient)
        {
            if (startingPoint < noTries)
            {
                Console.WriteLine("finding tasks with name: " + taskname);
                HttpClientHandler httpClientHander = new HttpClientHandler();
                using HttpResponseMessage response = await httpClient.GetAsync("api/detail/"+startingPoint);
                if (response.IsSuccessStatusCode)
                {
                    string id = await response.Content.ReadAsStringAsync();
                    JsonDocument jsonObject = JsonDocument.Parse(id);
                    if (jsonObject.RootElement.GetProperty("name").ToString().Contains(taskname))
                    {
                        string realid = jsonObject.RootElement.GetProperty("id").ToString();
                        Console.WriteLine("Found task id: " + realid);
                        return int.Parse(realid);
                    }
                    else
                    {
                        Console.WriteLine("wrong task... retrying");
                        Thread.Sleep(30);
                        Console.Clear();
                        return await getTaskIdFromName(noTries, startingPoint + 1, taskname, httpClient);
                    }
                }
                else
                {
                    Console.WriteLine("wrong task... retrying");
                    Thread.Sleep(30);
                    Console.Clear();
                    return await getTaskIdFromName(noTries,startingPoint+1, taskname, httpClient);
                }
            }
            else
            {
                return null;
            }
        }

    }
}
