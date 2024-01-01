using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace MyServer
{
    class Server
    {
        static private string mesEnd = "\r\n";
        static private string errMes = "Access denied";
        static private string[] tokens = ["hbogmlakjg.jgbksfmgldmslmlbmslm.hkjgbkvahjghrkja",
                                          "flmbmjslgj.dedlbldmkhfdfgafhlfl.hbsglfkjgioj5oti"];
        static private string[] users = ["User1",
                                         "User2"];
        static private string[] passwords = ["5567",
                                             "2234"];
        static private string[] secrets = ["one",
                                           "two"];
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
            using (Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                server.Bind(new IPEndPoint(IPAddress.Any, 6666));
                server.Listen(10);

                Console.WriteLine("Сервер запущен.");

                using (Socket client = server.Accept())
                {
                    Console.WriteLine($"Подключился: {client.RemoteEndPoint}");

                    string[] answers = Reception(in client);
                    if (answers[0] == errMes)
                    {
                        server.Close();
                        return;
                    }

                    byte method = Convert.ToByte(answers[0]);

                    // Отправляем сообщение о готовности
                    Send(in client, ["OK"]);
                    Console.WriteLine("Отправлено сообщение о готовности.");

                    if (method == 1)
                    {
                        Console.WriteLine("Выбран первый метод.");

                        // Получаем код авторизации, код испытания и метод преобразования
                        answers = Reception(in client, 3);

                        int authCode = Convert.ToInt32(answers[0]);
                        string challenge = answers[1];
                        Console.WriteLine("Получены код авторизации, код испытания и метод преобразования.");

                        if (answers[2] != "sha256")
                        {
                            Console.WriteLine("Получен неизвестный метод преобразования");
                            Send(in client, [errMes]);
                            client.Close();
                            server.Close();
                            Console.ReadKey();
                            return;
                        }

                        // Запрашиваем пользователя и пароль
                        Console.WriteLine("Введите логин:");
                        string user = Console.ReadLine();

                        Console.WriteLine("Введите пароль:");
                        string password = Console.ReadLine();

                        int userIndex = Array.IndexOf(users, user);

                        if (!((userIndex == Array.IndexOf(passwords, password)) && (userIndex != -1)))
                        {
                            Console.WriteLine("Введен неверные пользователь или пароль.");
                            Send(in client, [errMes]);
                            client.Close();
                            server.Close();
                            Console.ReadKey();
                            return;
                        }

                        // Отправляем код авторизации
                        Send(in client, [authCode.ToString()]);
                        Console.WriteLine("Отправлен код авторизации.");

                        // Получаем код верификации
                        answers = Reception(in client, 2);

                        if (answers[0] == errMes)
                        {
                            server.Close();
                            return;
                        }

                        if (answers[0] != authCode.ToString())
                        {
                            Send(in client, [errMes]);
                            client.Close();
                            server.Close();
                            Console.ReadKey();
                            return;
                        }

                        Console.WriteLine("Получен код верификации.");

                        // Проверяем корректности кода
                        byte[] bytes;
                        using (var sha256 = SHA256.Create())
                        {
                            bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(answers[1]));
                        }

                        if (Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').Replace('=', ' ') != challenge)
                        {
                            Console.WriteLine("Коды не совпадают.");
                            Send(in client, [errMes]);
                            client.Close();
                            server.Close();
                            Console.ReadKey();
                            return;
                        }

                        //Отправляем токен
                        Send(in client, [tokens[userIndex]]);
                        Console.WriteLine("Токен отправлен.");
                    }
                    else
                    {
                        Console.WriteLine("Выбран второй метод.");

                        // Получаем пользователя (id) и секрет
                        answers = Reception(in client, 2);
                        Console.WriteLine("Получены id и секрет.");

                        // Проверяем пользователя (id) и секрет
                        int userIndex = Array.IndexOf(users, answers[0]);

                        if (!((userIndex == Array.IndexOf(secrets, answers[1])) && (userIndex != -1)))
                        {
                            Console.WriteLine("Получены неверные id или секрет.");
                            Send(in client, [errMes]);
                            client.Close();
                            server.Close();
                            Console.ReadKey();
                            return;
                        }

                        // Отправляем токен
                        Send(in client, [tokens[userIndex]]);
                        Console.WriteLine("Токен отправлен.");
                    }
                }
            }
            Console.ReadKey();
        }
    }
}