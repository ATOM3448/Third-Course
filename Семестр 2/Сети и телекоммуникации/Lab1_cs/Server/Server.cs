using System.Net;
using CommonModule;

namespace Server
{
    internal class Server
    {
        private static int queue = 0;
        public static void TaskManager(in Communicate _iam)
        {
            if (((_iam.parsedPacket.Item5 == 2) && (_iam.parsedPacket.Item6[0] <= _iam.parsedPacket.Item6[1])) || (_iam.parsedPacket.Item5 == 1))
            {
                _iam.Send(new IPEndPoint(_iam.parsedPacket.Item2, (int)_iam.parsedPacket.Item3), _iam.parsedPacket);
                return;
            }

            if (queue > (_iam.contacts.Count-1))
                queue = 0;

            _iam.Send(_iam.contacts[queue++], _iam.parsedPacket);
        }
        public static void Main(string[] args)
        {
            Communicate iam = new Communicate(IPAddress.Parse("127.0.0.1"), 9090);

            while(true)
            {
                if (iam.Recive())
                    continue;
                
                switch (iam.parsedPacket.Item1)
                {
                    case 10:
                        iam.AddContact();
                        break;
                    case 20:
                        TaskManager(in iam);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}