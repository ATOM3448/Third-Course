using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography;

// Authorization Code Flow (PKCE)
// Client credentials flow Auth2.0

namespace Lab5
{
    class App
    {
        // Метод чтения из потока
        private static string Recive(in Socket soc)
        {
            var stream = new NetworkStream(soc);
            var responceData = new byte[1024];
            var responce = new StringBuilder();

            int bytes;
            while (true)
            {
                bytes = stream.Read(responceData);
                responce.Append(Encoding.UTF8.GetString(responceData, 0, bytes));

                if (responce.ToString().EndsWith("\r\n"))
                    break;
            }

            return responce.ToString();
        }

        public static void Main(string[] args)
        {
            string endMes = "\r\n";

            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            Console.WriteLine("Клиент запущен. Подключение...");

            client.Connect("127.0.0.1", 9090);

            Console.WriteLine("Клиент подключился к серверу авторизации.");

            // Выбор метода

            Console.WriteLine("Введите\n\t1 - Для Authorization Code Flow (PKCE)\n\t2 - Для Client credentials flow Auth2.0");
            byte mode = Convert.ToByte(Console.ReadLine());

            client.Send(Encoding.UTF8.GetBytes(mode + endMes));

            switch (mode)
            {
                // Authorization Code Flow (PKCE)
                case 1:
                    // Генерация кода верификации
                    var bytes = new byte[32];

                    new Random().NextBytes(new byte[32]);

                    string codeVerifer = Convert.ToBase64String(bytes);

                    // Генерация кода испытания
                    using (var sha256 = SHA256.Create())
                    {
                        bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifer));
                    }

                    string codeChallenge = Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').Replace('=', ' ');

                    // Отправка кода авторизации, кода испытания, метода преобразования
                    int authCode = 1;

                    string message = authCode.ToString() + endMes + codeChallenge + endMes + "sha256" + endMes;
                    client.Send(Encoding.UTF8.GetBytes(message));

                    // Получение кода авторизации
                    string responce = Recive(client).Replace("\r\n", "");

                    if (Convert.ToInt32(responce) != authCode)
                    {
                        Console.WriteLine("Неверный код авторизации");
                        Console.ReadKey();
                        return;
                    }

                    // Отправка кода верификации
                    message = authCode.ToString() + endMes + codeVerifer + endMes;
                    client.Send(Encoding.UTF8.GetBytes(message));

                    // Получаем токен доступа
                    string token = Recive(client).Replace("\r\n", "");

                    Console.WriteLine($"Получен токен: {token}");
                    break;
                
                // Client credentials flow Auth2.0
                case 2:
                    string myId = "4245";
                    string mySecret = "skdgmgslfbnslskf";

                    // Отправляем id и секрет
                    message = myId + endMes + mySecret + endMes;
                    client.Send(Encoding.UTF8.GetBytes(message));

                    // Получаем токен
                    token = Recive(client).Replace("\r\n", "");

                    Console.WriteLine($"Получен токен: {token}");
                    break;
                
                default:
                    Console.WriteLine("Wrong input");
                    break;
            }

            Console.ReadKey();
        }
    }
}