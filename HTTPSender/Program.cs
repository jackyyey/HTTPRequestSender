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
            await StartAsync();
        }

        static async Task StartAsync()
        {
            Console.WriteLine("Enter Domain Name:");
            string domain = Console.ReadLine()?.Trim();

            Console.WriteLine("Enter Task Name:");
            string taskName = Console.ReadLine()?.Trim();

            Console.WriteLine("Is domain name a standard domain name? (y/n)");
            string isStandardDomainInput = Console.ReadLine()?.ToLower();
            bool isStandardDomain = isStandardDomainInput == "y";

            if (!isStandardDomain || Regex.IsMatch(domain, @"^[A-Za-z0-9]+\.[A-Za-z0-9]+$"))
            {
                using var httpClientHandler = new HttpClientHandler();
                using var httpClient = new HttpClient(httpClientHandler) { BaseAddress = new Uri($"http://{domain}/") };

                var userPayload = new
                {
                    username = $"MrPipo{new Random().Next(1, 9999)}",
                    password = "PIPO"
                };

                using var queryBody = new StringContent(
                    JsonSerializer.Serialize(userPayload),
                    Encoding.UTF8,
                    "application/json"
                );

                Console.WriteLine("Creating user...");
                using var response = await httpClient.PostAsync("api/id/signup", queryBody);

                if (response.IsSuccessStatusCode)
                {
                    Console.Clear();
                    Console.WriteLine("Account creation successful.");
                    Thread.Sleep(600);

                    Console.WriteLine("How many tries?");
                    if (!int.TryParse(Console.ReadLine(), out int maxTries))
                    {
                        Console.WriteLine("Invalid input. Resetting...");
                        await StartAsync();
                        return;
                    }

                    Console.WriteLine("Finding task ID...");
                    int? taskId = await GetTaskIdFromNameAsync(maxTries, 0, taskName, httpClient);

                    if (taskId == null)
                    {
                        Console.WriteLine("Task does not exist. Retry? (y/n)");
                        if (Console.ReadLine()?.ToLower() == "y")
                        {
                            Console.Clear();
                            await StartAsync();
                        }
                        else
                        {
                            Console.WriteLine("Shutting down...");
                            Thread.Sleep(5000);
                            Environment.Exit(0);
                        }
                        return;
                    }

                    Console.WriteLine("Insert new task value:");
                    if (!int.TryParse(Console.ReadLine(), out int progress))
                    {
                        Console.WriteLine("Invalid value. Retrying in 5 seconds...");
                        Thread.Sleep(5000);
                        await StartAsync();
                        return;
                    }

                    var cookies = httpClientHandler.CookieContainer.GetCookies(new Uri($"http://{domain}/"));
                    var sessionId = cookies.FirstOrDefault(c => c.Name == "JSESSIONID")?.Value;

                    if (sessionId != null)
                    {
                        using var handler = new HttpClientHandler();
                        handler.CookieContainer.Add(httpClient.BaseAddress, new Cookie("JSESSIONID", sessionId));

                        using var client = new HttpClient(handler) { BaseAddress = httpClient.BaseAddress };
                        var result = await client.GetAsync($"/api/progress/{taskId}/{progress}");

                        if (result.IsSuccessStatusCode)
                        {
                            Console.Clear();
                            Console.WriteLine("Task progress updated successfully.");
                            Thread.Sleep(3000);
                        }
                    }

                    Console.WriteLine("Retry? (y/n)");
                    if (Console.ReadLine()?.ToLower() == "y")
                    {
                        await StartAsync();
                    }
                    else
                    {
                        Console.WriteLine("Shutting down...");
                        Thread.Sleep(5000);
                        Environment.Exit(0);
                    }
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine("Account creation failed. Resetting...");
                    await StartAsync();
                }
            }
            else
            {
                Console.Clear();
                Console.WriteLine("Invalid domain name. Resetting...");
                await StartAsync();
            }
        }

        static async Task<int?> GetTaskIdFromNameAsync(int maxTries, int currentTry, string taskName, HttpClient httpClient)
        {
            if (currentTry >= maxTries) return null;

            Console.WriteLine($"Searching for task: {taskName} (Attempt {currentTry + 1}/{maxTries})");

            using var response = await httpClient.GetAsync($"api/detail/{currentTry}");
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                var jsonObject = JsonDocument.Parse(content);

                if (jsonObject.RootElement.TryGetProperty("name", out var nameProperty) &&
                    nameProperty.GetString()?.Contains(taskName) == true)
                {
                    if (jsonObject.RootElement.TryGetProperty("id", out var idProperty))
                    {
                        int taskId = idProperty.GetInt32();
                        Console.WriteLine($"Found task ID: {taskId}");
                        return taskId;
                    }
                }
            }

            Thread.Sleep(30);
            return await GetTaskIdFromNameAsync(maxTries, currentTry + 1, taskName, httpClient);
        }
    }
}
