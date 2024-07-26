using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Server
{
    class Server
    {
        async static Task Main(string[] args)
        {
            using (var client = new HttpClient())
            {
                using (var server = new HttpListener())
                {
                    server.Prefixes.Add("http://127.0.0.1:8888/");
                    server.Start();
                    while (true)
                    {
                        var context = await server.GetContextAsync();

                        var request = context.Request;
                        var response = context.Response;

                        string test = Regex.Replace(request.RawUrl, @"http:", "https:").TrimStart('/');
                        try
                        {
                            using var result = await client.GetAsync(test);
                            
                            Console.WriteLine(result.StatusCode);
                            Console.WriteLine(Encoding.UTF8.GetString(await result.Content.ReadAsByteArrayAsync()));
                            foreach (var i in result.Headers)
                                foreach(var s in i.Value)
                                    Console.WriteLine($"{i.Key}: {s}");
                            
                            // Переносим заголовки и код
                            response.StatusCode = (int)result.StatusCode;
                            int counter = 0;
                            foreach (var i in result.Headers)
                            {
                                counter += 1;
                                string header_value = "";
                                foreach (var j in i.Value)
                                    header_value += ", " + j;
                                if (i.Key == "Transfer-Encoding")
                                    continue;
                                response.Headers[i.Key.ToString()] = header_value.TrimStart(',').TrimStart(' ');
                            }

                            // Переносим содержимое
                            byte[] response_content = await result.Content.ReadAsByteArrayAsync();

                            response.ContentLength64 = response_content.Length;
                            using var output = response.OutputStream;
                            if (response_content.Length > 0)
                                await output.WriteAsync(response_content);
                            await output.FlushAsync();
                        }
                        catch (Exception ex)
                        {
                            continue;
                        }
                    }
                }
            }
        }
    }
}