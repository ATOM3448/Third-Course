using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
// Authorization Code Flow (PKCE)
// Client credentials flow Auth2.0

namespace Lab5
{
    class Server
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
            string user = "Atom";
            string myId = "4245";
            string mySecret = "skdgmgslfbnslskf";
            string pass = "12345";
            string token = "NzU3NTYzMDQ2MjU1MjYzNzU0.GWNObb.wRdHivgATxo3ngoNPCvPXCpqZmDE5M6JVvtSbY";
            string endMes = "\r\n";

            using (Socket authServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                authServer.Bind(new IPEndPoint(IPAddress.Any, 9090));
                authServer.Listen(1000);

                Console.WriteLine("Сервер авторизации запущен успешно. Ожидание подключений...");

                using (Socket client = authServer.Accept())
                {
                    Console.WriteLine($"К серверу авторизации подключился клиент {client.RemoteEndPoint}");
                    Console.WriteLine($"Клиент должен получить токен: {token}");

                    // Получаем метод авторизации
                    byte mode = Convert.ToByte(Recive(client).Replace("\r\n", ""));

                    switch (mode)
                    {
                        // Authorization Code Flow (PKCE)
                        case 1:
                            // Читаем код авторизации, испытания, метод преобразывания
                            string responce = Recive(client);

                            var parts = responce.Split("\r\n");
                            while (parts.Length < 4)
                            {
                                responce += Recive(client);

                                parts = responce.Split("\r\n");
                            }

                            int authCode = Convert.ToInt32(parts[0]);

                            string codeChallenge = parts[1];

                            string method = parts[2];

                            // Запрашиваем логин и пароль
                            Console.WriteLine("Введите логин:");
                            string inUser = Console.ReadLine();

                            Console.WriteLine("Введите пароль:");
                            string inPass = Console.ReadLine();

                            if ((user != inUser) || (pass != inPass))
                            {
                                Console.WriteLine("Неверный логин или пароль");
                                Console.ReadKey();
                                return;
                            }

                            // Отправляем код авторизации
                            string message = authCode.ToString() + endMes;

                            client.Send(Encoding.UTF8.GetBytes(message));

                            // Получаем код верификации
                            responce = Recive(client);
                            parts = responce.Split("\r\n");
                            while (parts.Length < 2)
                            {
                                responce += Recive(client);

                                parts = responce.Split("\r\n");
                            }

                            authCode = Convert.ToInt32(parts[0]);

                            string codeVerifer = parts[1];

                            // Сравнение кода верификации и кода испытания
                            string codeToCheck;
                            byte[] bytes;

                            using (var sha256 = SHA256.Create())
                            {
                                bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifer));
                            }

                            codeToCheck = Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').Replace('=', ' ');

                            if (codeToCheck != codeChallenge)
                            {
                                Console.WriteLine("Неверный код верификации");
                                Console.ReadKey();
                                return;
                            }

                            // Отправка токена
                            message = token + endMes;
                            client.Send(Encoding.UTF8.GetBytes(message));
                            break;

                        // Client credentials flow Auth2.0
                        case 2:
                            // Получаем id и секрет
                            responce = Recive(client);

                            parts = responce.Split("\r\n");
                            while (parts.Length < 2)
                            {
                                responce += Recive(client);

                                parts = responce.Split("\r\n");
                            }

                            // Сравниваем полученные и хранимые id и секрет
                            if (!((parts[0] == myId) && (parts[1] == mySecret)))
                            {
                                Console.WriteLine("Неверные id или секрет");
                                Console.ReadKey();
                                return;
                            }

                            // Отправка токена
                            message = token + endMes;
                            client.Send(Encoding.UTF8.GetBytes(message));
                            break;

                        default:
                            Console.WriteLine("Wrong input");
                            break;
                    }

                    Console.ReadKey();
                }
            }
        }
    }
}