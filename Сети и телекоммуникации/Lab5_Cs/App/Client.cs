using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography;

namespace MyClient
{
    class Client
    {
        static private string mesEnd = "\r\n";
        static private string errMes = "Access denied";

        // Чтение из потока указанное количество сообщений
        private static string[] Reception(in Socket _soc, int _count = 1)
        {
            byte[] responceData = new byte[1024];
            StringBuilder responce = new StringBuilder();
            int bytes;

            do
            {
                bytes = _soc.Receive(responceData);
                responce.Append(Encoding.UTF8.GetString(responceData, 0, bytes));

                // Прекращаем прием, если доступ отклонен
                if (responce.ToString().Contains(errMes))
                {
                    Console.WriteLine("Что-то пошло не так. Доступ отклонен.");
                    _soc.Close();
                    Console.ReadKey();
                    return [errMes];
                }
            } while (responce.ToString().Split(mesEnd).Length < _count+1);

            return responce.ToString().Split(mesEnd).SkipLast(1).ToArray();
        }

        // Отправка в поток сообщений
        private static void Send(in Socket _soc, string[] _messages)
        {
            foreach (string message in _messages)
            {
                _soc.Send(Encoding.UTF8.GetBytes(message + mesEnd));
            }
        }

        public static void Main(string[] args)
        {
            using (Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                Console.WriteLine("Клиент запущен.");

                client.Connect("127.0.0.1", 6666);

                Console.WriteLine("Клиент подключился.");

                Console.WriteLine("\n1: Authorization Code Flow (PKCE).\n2: Client credentials flow.");
                byte method = Convert.ToByte(Console.ReadLine());
                string[] answers;

                if (method == 1)
                {
                    Console.WriteLine("Выбран первый метод.");
                    Send(client, [method.ToString()]);

                    // Ожидание готовности сервера
                    answers = Reception(in client);
                    Console.WriteLine("Сервер готов.");

                    // Создаем код авторизации, код верификации и код испытания

                    Random rnd = new Random();
                    int authCode = rnd.Next();

                    string verification;
                    string challenge;

                    byte[] bytes = new byte[32];
                    rnd.NextBytes(bytes);
                    verification = Convert.ToBase64String(bytes);

                    string conversion = "sha256";
                    using (SHA256 sha256 = SHA256.Create())
                    {
                        bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(verification));
                    }
                    challenge = Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').Replace('=', ' '); ;

                    // Отправляем код авторизации, код испытания и метод преобразования
                    Send(in client, [authCode.ToString(), challenge, conversion]);
                    Console.WriteLine("Отправлены код авторизации, код испытания и метод преобразования.");

                    // Получаем код авторизации
                    answers = Reception(in client);
                    if (answers[0] == errMes)
                        return;

                    if (answers[0] != authCode.ToString())
                    {
                        Send(in client, [errMes]);
                        client.Close();
                        return;
                    }

                    Console.WriteLine("Получен код авторизации.");

                    // Отправляем код верификации
                    Send(in client, [authCode.ToString(), verification]);
                    Console.WriteLine("Отправлен код верификации.");

                    // Получаем токен
                    answers = Reception(in client);
                    if (answers[0] == errMes)
                        return;

                    string token = answers[0];

                    Console.WriteLine($"Получен токен {token}");
                }
                else if (method == 2)
                {
                    Console.WriteLine("Выбран второй метод.");
                    Send(client, [method.ToString()]);

                    // Ожидание готовности сервера
                    answers = Reception(in client);
                    Console.WriteLine("Сервер готов.");

                    // Отправляем пользователя (id) и секрет
                    Send(in client, ["Poleno", "one"]);
                    Console.WriteLine("Отправлены id и секрет.");

                    // Получаем токен
                    string token = Reception(in client)[0];

                    if (token == errMes)
                        return;

                    Console.WriteLine($"Получен токен {token}");
                }
                else
                {
                    Console.WriteLine("Ошибочный ввод.");
                    Send(client, [errMes]);
                }
            }

            Console.ReadKey();
        }
    }
}