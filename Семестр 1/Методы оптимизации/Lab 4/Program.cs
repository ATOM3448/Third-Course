using System;
using System.Linq.Expressions;
using PolStrLib;


namespace Lab4
{
    class Program
    {
        static void Main(string[] args)
        {
            // f(x) = 3*x - x^3 + 3*y^2 + 4*y
            // Ответ: x = -1, y = -2/3
            string expression = "(3 * x1) - (x1 * x1 * x1) + (3 * (x2 * x2)) + (4 * x2)";

            double[] x0 = { 0.78, 1 };
            double ex = 0.0001;

            double[] x = Cauchy(expression, x0, ex, ex);

            foreach (double answ in x)
            {
                Console.WriteLine(answ);
            }
        }

        // Нахождение градиента
        static double[] Gradient(double[] _x, string _polstExpr)
        {
            
            return new double[] { PolStr.EvalPolStr(_polstExpr, _x, 1, 1), PolStr.EvalPolStr(_polstExpr, _x, 1, 2) };
        }

        // Метод Коши
        static double[] Cauchy(string _expression, double[] _x0, double _exX, double _exF)
        {
            double[] x = new double[2];
            _x0.CopyTo(x, 0);
            double[] gradX = new double[2];
            do
            {
                x.CopyTo(_x0, 0);

                string polstrExpr = PolStr.CreatePolStr(_expression, 2);

                double[] fGrad = Gradient(_x0, polstrExpr);
                double[] d = { -fGrad[0], -fGrad[1] };

                string alphaExpr = _expression.Replace("x1", $"(({_x0[0]}) + x * ({d[0]}))")
                                              .Replace("x2", $"(({_x0[1]}) + x * ({d[1]}))")
                                              .Replace(',', '.')
                                              .Replace("x", "x1");

                // Ищем значение альфа с помощью метода дихотомии (если не получается можно изменить границы)
                double alpha = Dichotomy(-2, 2, _exX, PolStr.CreatePolStr(alphaExpr, 1));

                x[0] = _x0[0] + alpha * d[0];
                x[1] = _x0[1] + alpha * d[1];

                gradX = Gradient(x, polstrExpr);
            } while ((Math.Abs(x[0] - _x0[0]) > _exX) || (Math.Abs(x[1] - _x0[1]) > _exX) || (Math.Abs(gradX[0]) > _exF) || (Math.Abs(gradX[1]) > _exF));


            return x;
        }

        // Метод дихотомии
        public static double Dichotomy(double _a, double _b, double _ex, string _expr)
        {
            uint maxIterations = 10000;

            double e = 0.2 * _ex;

            uint k = 0;

            do
            {
                double y = (_a + _b - e) / 2;
                double z = (_a + _b + e) / 2;

                double fy = PolStr.EvalPolStr(_expr, y, 0);
                double fz = PolStr.EvalPolStr(_expr, z, 0);

                if (fy <= fz)
                {
                    _b = z;
                }
                else
                {
                    _a = y;
                }

                k++;
            } while ((Math.Abs(_b - _a) > _ex) && (k < maxIterations));

            return (_a + _b) / 2;
        }
    }
}