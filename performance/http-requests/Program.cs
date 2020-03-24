using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace http_requests
{
    public class Program
    {
        [Argument(0, "urls", "List of target URLs")]
        [Required(AllowEmptyStrings = false)]
        public string[] Urls { get; }

        [Option("--rps", "Requests per second", CommandOptionType.SingleValue)]
        [Required]
        public int RequestsPerSecond { get; }

        public static Task<int> Main(string[] args)
            => CommandLineApplication.ExecuteAsync<Program>(args);

        private async Task OnExecuteAsync()
        {
            Console.WriteLine("Press CTRL+C to exit...");
            var tasks = new List<Task>(Urls.Length);

            foreach (string url in Urls)
            {
                Console.WriteLine($"Sending {RequestsPerSecond} requests per second to {url}.");
                var callbackArgs = new HttpRequestGenerator(url);
                Task task = callbackArgs.StartAsync(RequestsPerSecond);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }

        public class HttpRequestGenerator
        {
            private readonly Stopwatch _stopwatch = new Stopwatch();

            private readonly string _url;

            private readonly HttpClient _httpClient = new HttpClient();

            public HttpRequestGenerator(string url)
            {
                _url = url;
                _stopwatch.Start();
            }

            public async Task StartAsync(int requestsPerSecond)
            {
                // give server time to boot up
                await Task.Delay(TimeSpan.FromSeconds(5));

                // send first request to warm up the server
                (await _httpClient.GetAsync(_url)).EnsureSuccessStatusCode();

                // seconds between each request
                var period = TimeSpan.FromSeconds(1.0 / requestsPerSecond);

                var stopwatch = new Stopwatch();

                while (true)
                {
                    stopwatch.Restart();

                    try
                    {
                        (await _httpClient.GetAsync(_url)).EnsureSuccessStatusCode();
                    }
                    catch (Exception ex)
                    {
                        Environment.FailFast(null, ex);
                    }

                    TimeSpan timeLeft = period - stopwatch.Elapsed;

                    if (timeLeft > TimeSpan.Zero)
                    {
                        Thread.Sleep(timeLeft);
                    }
                }
            }
        }
    }
}
