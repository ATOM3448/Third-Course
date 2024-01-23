using PolStrLib;
using System;
using System.ComponentModel;

namespace Lab1
{
	class Program
	{
		public static void Main(string[] args)
		{
			// Выражение: 3x-x*x*x+3*y*y+4*y
			// Ответ: x = -1, y = -2/3
			string expression = "(3*x1)-(x1*x1*x1)+(3*x2*x2)+(4*x2)";
			string exprInPol = PolStr.CreatePolStr(expression, 2);

			double[] x0 = { 0.78, 1 };

			Console.Write("1. Метод Хука-Дживса\n2. Симплексный метод\nВыберите метод: ");
			int mode = Convert.ToInt32(Console.ReadLine());

			double[] answ = null;
			double a;
			double ex;

			switch (mode)
			{
				case 1:
					a = 1.7;
					ex = 0.001;
					answ = HookJeeves(exprInPol, x0, 1, a, ex, ex);
					break;
				case 2:
					a = 0.7;
					ex = 0.001;

					answ = Simplex(exprInPol, x0, 1, a, ex, ex);

					break;
				default:
					Console.WriteLine("Ошибочный ввод");
					break;
			}

			for (int i = 0; i < answ.Length; i++)
				Console.WriteLine(answ[i]);
		}

		// Метод Хука-Дживса
		static double[] HookJeeves(string _exprPol, double[] _x0, double _step, double _a, double _exS, double _exF)
		{
			double check = PolStr.EvalPolStr(_exprPol, _x0, 0, 0)+(_exF*2);

			while ((_step > _exS) || (Math.Abs(PolStr.EvalPolStr(_exprPol, _x0, 0, 0) - check) > _exF))
			{
				check = PolStr.EvalPolStr(_exprPol, _x0, 0, 0);

				double[] x = { _x0[0], _x0[1] };

				double xValue = PolStr.EvalPolStr(_exprPol, x, 0, 0);

				bool notFound = true;

				// Исследующий поиск
				for (double i = (x[0] - _step); i <= (x[0] + _step); i += _step)
					for (double j = x[1] - _step; j <= (x[1] + _step); j += _step)
					{
						double localSolution = PolStr.EvalPolStr(_exprPol, new double[] { i, j }, 0, 0);
						if (!(xValue > localSolution))
							continue;

						x[0] = i;
						x[1] = j;
						xValue = localSolution;
						notFound = false;
					}

				if (notFound)
				{
					_step /= _a;
					continue;
				}

				// Поиск по образцу
				double[] xSample = new double[2];

				for (byte i = 0; i < 2; i++)
					xSample[i] = x[i] + (x[i] - _x0[i]);

				if (PolStr.EvalPolStr(_exprPol, xSample, 0, 0) < xValue)
					for (byte i = 0; i < 2; i++)
						_x0[i] = xSample[i];
				else
					for (byte i = 0; i < 2; i++)
						_x0[i] = x[i];
			}

			return _x0;
		}

		// Метод симплекса
		static double[] Simplex(string _exprPol, double[] _x0, double _len, double _a, double _exS, double _exF)
		{
			int n = 2; // Размерность

			// Находим p и g
			double p = _len * (Math.Sqrt(n + 1) + n - 1) / (n * Math.Sqrt(2));
			double g = p - _len * Math.Sqrt(2) / 2;
			
			// Построение симплекса
			double[,] xS = { { _x0[0], _x0[1] }, { _x0[0] + p, _x0[1] + g }, { _x0[0] + g, _x0[1] + p } };

            // Начальное приближение (геометрический центр симплекса)
            double[] x = new double[2];

            for (byte i = 0; i < 2; i++)
            {
                double sum = 0;
                for (byte j = 0; j < 3; j++)
                    sum += xS[j, i];

                x[i] = 1 / (n + 1) * sum;
            }

			double[] lastX = { x[0], x[1] };
			double lastVal = PolStr.EvalPolStr(_exprPol, lastX, 0, 0);

			do
			{
				// Сохраняем прошлое приближение
				lastX[0] = x[0];
				lastX[1] = x[1];
				lastVal = PolStr.EvalPolStr(_exprPol, lastX, 0, 0);

				// Поиск значений ЦФ для вершин симплекса
				double[] fS = new double[3];

				for (byte i = 0; i < 3; i++)
					fS[i] = PolStr.EvalPolStr(_exprPol, new double[] { xS[i, 0], xS[i, 1] }, 0, 0);

				// Ищем максимальное значение ЦФ среди вершин симплекса
				int indOfMax = Array.IndexOf(fS, fS.Max());

				// Ищем отражение вершины с максимальным значением ЦФ
				double[] mirror = new double[2];

				for (byte i = 0; i < 2; i++)
				{
					mirror[i] = 0;
					for (byte j = 0; j < 3; j++)
						mirror[i] += xS[j, i];

					mirror[i] -= xS[indOfMax, i];
					mirror[i] = mirror[i] * (2 / n) - xS[indOfMax, i];
				}

				// Если значение ЦФ отражения меньше значения ЦФ максимальной вершины, то меняем максимальную на отражение
				if (PolStr.EvalPolStr(_exprPol, new double[] { mirror[0], mirror[1] }, 0, 0) <= fS[indOfMax])
				{
					xS[indOfMax, 0] = mirror[0];
					xS[indOfMax, 1] = mirror[1];
				}
				else
				{
					// Иначе производим сжатие
					_len *= _a;

					int indOfMin = Array.IndexOf(fS, fS.Min());

					for (byte i = 0; i < 2; i++)
						for (byte j = 0; j < 3; j++)
						{
							if (j == indOfMin)
								continue;

							xS[j, i] = _a * xS[j, i] + (1 - _a) * xS[indOfMin, i];
						}
				}

				// Новое приближение точки оптиума
				for (byte i = 0; i < 2; i++)
				{
					double sum = 0;
					for (byte j = 0; j < 3; j++)
						sum += xS[j, i];

					x[i] = 1.0 / (n + 1) * sum;
				}


			}while ((Math.Abs(x[0] - lastX[0]) > _exS) || (Math.Abs(x[1] - lastX[1]) > _exS) || (Math.Abs(PolStr.EvalPolStr(_exprPol, x, 0, 0) - lastVal) > _exF));

			return x;
		}
	}
}