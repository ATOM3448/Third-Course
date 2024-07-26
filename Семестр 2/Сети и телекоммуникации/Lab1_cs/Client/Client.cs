using System.Net;
using System.Net.Sockets;
using CommonModule;

namespace Client
{
    internal class Client
    {
        private static int QuickSortInit(in Communicate _iam)
        {
            Console.WriteLine("Введите кол-во элементов:");
                    int len = Math.Abs(Convert.ToInt32(Console.ReadLine()));
                    Console.WriteLine("Введите элементы:");

                    int[] content = new int[len];
                    
                    for (int i = 0; i < len; i++)
                    {
                        content[i] = Convert.ToInt32(Console.ReadLine());
                    }

                    _iam.Send(_iam.contacts[0], 20, _iam.myEndPoint.Address, (ushort)_iam.myEndPoint.Port, 0, (ushort)len, content, null);

                    return len;
        }

        private static int partition(ref int[] _data)
        {
            int pivot = _data[_data.Length/2];
            int i = 0;
            int j = _data.Length-1;

            while(true)
            {
                while (_data[i] < pivot)
                    i++;
                while (_data[j] > pivot)
                    j--;
                
                if (i > j)
                    return i;
                
                int buffer = _data[i];
                _data[i++] = _data[j];
                _data[j--] = buffer;
            }
        }
        private static void QuickSort(in Communicate _iam)
        {
            int[] data = _iam.parsedPacket.Item6;
            
            int p = partition(ref data);

            int[] low = new int[p];
            int[] hight = new int[data.Length-p];

            for (int i = 0; i < p; i++)
                low[i] = data[i];
            for (int j = p; j < data.Length; j++)
                hight[j-p] = data[j];

            if (low.Length > 0)
                _iam.Send(_iam.contacts[0], 20, _iam.parsedPacket.Item2, _iam.parsedPacket.Item3, _iam.parsedPacket.Item4, (ushort)low.Length, low, null);
            if (hight.Length > 0)
                _iam.Send(_iam.contacts[0], 20, _iam.parsedPacket.Item2, _iam.parsedPacket.Item3, (byte)(_iam.parsedPacket.Item4+low.Length), (ushort)hight.Length, hight, null);
        }

        public static void Main(string[] args)
        {
            // Определяем роль клиента
            byte type = 0;
            int[] recived_data = new int[0];
            int recived_counter = 0;
            while(true)
            {
                Console.WriteLine("Кто я?\n\t0 - хост.\n\t1 - расчетчик.");
                type = Convert.ToByte(Console.ReadLine());

                if ((type != 0) && (type != 1))
                {
                    Console.WriteLine("Ввод находится за областью допустимых значений, попробуйте ещё раз.");
                    continue;
                }

                break;
            }

            Communicate iam = new Communicate();

            iam.AddContact(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9090));

            iam.Send(iam.contacts[0], 10);
            iam.Recive();

            while (true)
            {
                if (type == 0)
                    recived_data = new int[QuickSortInit(in iam)];

                while (true)
                {
                    if ((type == 0) && (recived_counter == recived_data.Length))
                        break;

                    if (iam.Recive())
                        continue;
                    switch (iam.parsedPacket.Item1)
                    {
                        case 20:
                            if ((type == 0) && (((iam.parsedPacket.Item5 == 2) && (iam.parsedPacket.Item6[0] <= iam.parsedPacket.Item6[1])) || (iam.parsedPacket.Item5 == 1)))
                            {
                                for (int i = (int)iam.parsedPacket.Item4; i < ((int)iam.parsedPacket.Item4 + (int)iam.parsedPacket.Item5); i++)
                                    recived_data[i] = iam.parsedPacket.Item6[i-(int)iam.parsedPacket.Item4];
                                recived_counter += (int)iam.parsedPacket.Item5;
                                break;
                            }
                            QuickSort(in iam);
                            break;
                        default:
                            break;
                    }
                }

                if (type == 1)
                    continue;

                foreach (int elem in recived_data)
                    Console.Write($"{elem} ");
                
                Console.WriteLine();
                recived_counter = 0;
            }
        }
    }
}