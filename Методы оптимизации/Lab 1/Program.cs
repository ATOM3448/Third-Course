using Lab_1;
using System;

namespace Lab1
{
    class Program
    {
        internal static void RequestInput(string _nameOfVal, out double _val)
        {
            Console.Write($"Input \"{_nameOfVal}\": ");
            string answer = Console.ReadLine().Replace('.', ',');
            _val = Convert.ToDouble(answer);
        }

        internal static void RequestInput(string _nameOfVal, out uint _val)
        {
            Console.Write($"Input \"{_nameOfVal}\": ");
            _val = Convert.ToUInt32(Console.ReadLine());
        }

        internal static double ResultExpression(double x)
        {
            return 4 * x * x * x + 2 * x - 3 * x * x + Math.Pow(Math.E, x / x);
        }

        public static void Main(string[] args)
        {
            string expr = "4*x*x*x+2*x-3*x*x+e^(x/2)";

            double a;
            double b;

            uint mode;

            RequestInput("a", out a);
            RequestInput("b", out b);

            RequestInput("mode (0, 1, 2)", out mode);

            switch(mode)
            {
                case 0:
                    Console.WriteLine($"Answer: x(min) = {Uniform(a, b)}");
                    
                    break;
                case 1:
                    Console.WriteLine($"Answer: x(min) = {Dichotomy(a, b)}");

                    break;
                case 2:
                    Console.WriteLine($"Answer: x(min) = {GoldRatio(a, b)}");

                    break;
                default:
                    Console.WriteLine("Wrong input");
                    break;
            }
        }

        public static double Uniform(double _a, double _b)
        {
            uint n;
            RequestInput("n", out n);

            double[] x = new double[n];
            for (uint i = 0; i < n; i++)
                x[i] = _a + i * ((_b - _a) / (n + 1));

            double[] fx = new double[n];
            for (uint i = 0; i < n; i++)
                fx[i] = ResultExpression(x[i]);

            return x[Array.IndexOf(fx, fx.Min())];
        }

        public static double Dichotomy(double _a, double _b)
        {
            uint maxIterations = 10000;

            double accuracy;
            RequestInput("accuracy", out accuracy);

            double e = 0.2 * accuracy;

            uint k = 0;

            do
            {
                double y = (_a + _b - e) / 2;
                double z = (_a + _b + e) / 2;

                double fy = ResultExpression(y);
                double fz = ResultExpression(z);

                if(fy <= fz)
                {
                    _b = z;
                }
                else
                {
                    _a = y;
                }

                k++;
            }while((Math.Abs(_b - _a) > accuracy) && (k < maxIterations));

            Console.WriteLine("Count of iterations: " + k);

            return (_a + _b) / 2;
        }

        public static double GoldRatio(double _a, double _b)
        {
            uint maxIterations = 10000;

            double accuracy;
            RequestInput("accuracy", out accuracy);

            double k = 0;

            double y0 = _a + (3 - Math.Sqrt(5)) / 2;
            double z0 = _a + _b - y0;

            double yk = y0;
            double zk = z0;
            do
            {
                double fy = ResultExpression(yk);
                double fz = ResultExpression(zk);

                if (fy <= fz)
                {
                    _b = zk;
                }
                else
                {
                    _a = yk;
                    yk = zk;
                    zk = _a + _b - zk;
                }

                k++;
            } while ((Math.Abs(_b - _a) > accuracy) && (k < maxIterations));

            Console.WriteLine("Count of iterations: " + k);

            return (_a + _b) / 2;
        }
    }
}