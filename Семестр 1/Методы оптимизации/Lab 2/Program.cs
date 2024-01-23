using System;
using System.Linq.Expressions;
using PolStrLib;

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

        public static void Main(string[] args)
        {
            string expr = "4*x*x*x+2*x-3*x*x+e^(x/2)";
            PolStr.StrToPolStr(expr, out expr, 0);

            double a;
            double b;

            uint mode;

            RequestInput("a", out a);
            RequestInput("b", out b);

            if (a > b)
            {
                Console.WriteLine("\"a\" can't be bigger than \"b\"");
                return;
            }

            double exp = 0.0001;
            RequestInput("exp", out exp);

            RequestInput("mode (0, 1)", out mode);

            switch (mode)
            {
                case 0:
                    Console.WriteLine($"Answer: x(min) = {Newton(a, b, exp, expr)}"); // 1.9 - предельная точность (1.814)

                    break;
                case 1:
                    Console.WriteLine($"Answer: x(min) = {MidPoint(a, b, exp, expr)}");

                    break;
                default:
                    Console.WriteLine("Wrong input");
                    break;
            }
        }

        public static double Newton(double _a, double _b, double _exp, string _expr)
        {
            double x = _exp;
            do
                x = x - PolStr.EvalPolStr(_expr, x, 1) / PolStr.EvalPolStr(_expr, x, 2);
            while (Math.Abs(PolStr.EvalPolStr(_expr, x, 1)) > _exp);

            return x;
        }

        public static double MidPoint(double _a, double _b, double _exp, string _expr)
        {
            double x;
            double fx;
            do
            {
                x = (_a + _b) / 2;
                fx = PolStr.EvalPolStr(_expr, x, 1);
                if (fx <= 0)
                    _a = x;
                else
                    _b = x;
            } while (Math.Abs(fx) > _exp);
            
            return x;
        }
    }
}