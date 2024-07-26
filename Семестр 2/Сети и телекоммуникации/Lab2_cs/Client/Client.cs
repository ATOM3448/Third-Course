namespace Client
{
    class Client
    {
        async static Task Main(string[] args)
        {
            using (var client = new HttpClient())
            {
                while (true)
                {
                    using var result = await client.GetAsync("http://127.0.0.1:8888/https://example.org/");
                    Console.WriteLine(result.StatusCode);
                    Console.WriteLine(await result.Content.ReadAsStringAsync());
                    foreach (var i in result.Headers)
                        foreach(var s in i.Value)
                            Console.WriteLine($"{i.Key}: {s}");

                    return;
                }
            }
        }
    }
}