using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Intrinsics.Arm;

namespace CommonModule
{
    public class Communicate
    {
        public IPEndPoint myEndPoint;
        private Socket mySocket;

        private byte[] packet = new byte[4096];
        public Tuple<byte, IPAddress?, UInt16?, byte?, UInt16?, int[]?, byte?> parsedPacket;

        public EndPoint lastContact = new IPEndPoint(IPAddress.Any, 0);

        public List<EndPoint> contacts = new List<EndPoint>();

        private Dictionary<EndPoint, byte[]> packetsBuffer = new Dictionary<EndPoint, byte[]>();

        public Communicate(IPAddress _ip, UInt16 _port)
        {
            myEndPoint = new IPEndPoint(_ip, _port);

            mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            mySocket.Bind(myEndPoint);
            Console.WriteLine("Сокет забинжен.");
        }
        public Communicate()
        {
            mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            mySocket.Bind(new IPEndPoint(IPAddress.Any, 0));

            myEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), Convert.ToInt32(mySocket.LocalEndPoint.ToString().Split(':')[1]));
            Console.WriteLine("Сокет забинжен.");
        }

        private void Send(EndPoint _target, byte[] _packet)
        {
            packetsBuffer[_target] = _packet;
            mySocket.SendTo(_packet, _target);
            Console.WriteLine("Сообщение отправлено.");
        }
        public void Send(EndPoint _target, byte _code, IPAddress? _address, UInt16? _port, byte? _index, UInt16? _len, int[]? _data, byte? _crc)
        {
            byte[] loc_packet = new byte[4096];
            int current_index = 0;
            loc_packet[current_index++] = _code;

            if (_address == null)
            {
                packetsBuffer[_target] = loc_packet;
                Send(_target, loc_packet);
                Console.WriteLine("Сообщение отправлено.");
                return;
            }

            byte[] buffer_bytes = new byte[4];
            int buffer_size = 0;
            _address.TryWriteBytes(buffer_bytes, out buffer_size);
            foreach (byte elem in buffer_bytes)
                loc_packet[current_index++] = elem;

            foreach (byte elem in BitConverter.GetBytes((UInt16)_port))
                loc_packet[current_index++] = elem;

            loc_packet[current_index++] = (byte)_index;

            foreach (byte elem in BitConverter.GetBytes((UInt16)_len))
                loc_packet[current_index++] = elem;
            
            foreach (int value in _data)
                foreach (byte elem in BitConverter.GetBytes(value))
                    loc_packet[current_index++] = elem;

            if (_crc == null)
                _crc = CRC(in loc_packet, (UInt16)_len);
            
            loc_packet[current_index++] = (byte)_crc;
            
            packetsBuffer[_target] = loc_packet;
            Send(_target, loc_packet);
        }
        public void Send(EndPoint _target, Tuple<byte, IPAddress?, UInt16?, byte?, UInt16?, int[]?, byte?> _packet)
        => Send(_target, _packet.Item1, _packet.Item2, _packet.Item3, _packet.Item4, _packet.Item5, _packet.Item6, _packet.Item7);
        public void Send(EndPoint _target, byte _code)
        => Send(_target, [_code]);

        public bool Recive()
        {
            mySocket.ReceiveFrom(packet, ref lastContact);
            ParsePacket();

            if (parsedPacket.Item1 == 01)
            {
                Send(lastContact, packetsBuffer[lastContact]);
                Console.WriteLine("Сообщение получено. Ошибка контрольной суммы. Повторная отправка.");
                return true;
            }

            if ((parsedPacket.Item1 == 20) && !CheckCRC())
            {
                Send(lastContact, 01);
                Console.WriteLine("Сообщение получено. Ошибка контрольной суммы.");
                return true;
            }

            if (parsedPacket.Item1 == 00)
            {
                Console.WriteLine("Сообщение получено целью.");
                return false;
            }

            Console.WriteLine("Сообщение получено.");
            Send(lastContact, 00);

            return false;
        }
        private bool CheckCRC()
        {
            byte myCRC = CRC(in packet, parsedPacket);
            if (myCRC != parsedPacket.Item7)
                return false;

            return true;
        }

        private byte CRC(in byte[] _packet, UInt16 _len)
        {
            byte crc = 0;
            int content_len = _len*4+9;
            for (int i = 0; i < content_len; i++)
            {
                byte data = _packet[i];
                for (int j = 8; j > 0; j--)
                {
                    int buffer = crc ^ data;
                    // Вместо (crc ^ data) & 1
                    if ((crc ^ data) == 1)
                        crc = (byte)((crc >> 1) ^ 0x8C);
                    else
                        crc = (byte)(crc >> 1);
                    data >>= 1;
                }
            }
            return crc;
        }
        private byte CRC(in byte[] _packet_bytes, Tuple<byte, IPAddress?, UInt16?, byte?, UInt16?, int[]?, byte?> _packet)
        => CRC(_packet_bytes, (UInt16)_packet.Item5);

        public void ParsePacket()
        {
            byte code = packet[0];

            if (code != 20)
            {
                parsedPacket = new Tuple<byte, IPAddress?, UInt16?, byte?, UInt16?, int[]?, byte?>(code, null, null, null, null, null, null);
                return;
            }

            IPAddress ip = new IPAddress(new byte[]{packet[1], packet[2], packet[3], packet[4]});

            UInt16 port = BitConverter.ToUInt16(new byte[]{packet[5], packet[6]});

            byte start = packet[7];

            UInt16 len = BitConverter.ToUInt16(new byte[]{packet[8], packet[9]});

            int[] data = new int[len];

            for (int i = 0; i < len; i++)
            {
                data[i] = BitConverter.ToInt32(new byte[]{packet[10 + 4*i], packet[11 + 4*i], packet[12 + 4*i], packet[13 + 4*i]});
            }

            byte crc = packet[len*4+10];

            parsedPacket = new Tuple<byte, IPAddress?, UInt16?, byte?, UInt16?, int[]?, byte?>(code, ip, port, start, len, data, crc);
        }

        public void AddContact(EndPoint _contact)
        {
            if (contacts.Any(c => c.Equals(_contact)))
                return;
            
            contacts.Add(_contact);
            Console.WriteLine("Контакт добавлен.");
        }
        public void AddContact()
        => AddContact(lastContact);
    }
}

/*
Структура пакета:
1b - Код сообщения:
    00 - OK (клиент/сервер)
    01 - Ошибка контрольной суммы (клиент/сервер)
    10 - Регистрация узла (клиент)
    20 - Отсылается задача (клиент/сервер)
(далее при 20)
4b - IP Изначального отправителя задачи
2b - Порт Изначального отправителя задачи

1b - Стартовый индекс рассматриваемой части

2b - Длинна содержимого (N)

Nb - Содержимое

(Nb+2+7)/2 - Контрольная сумма
*/