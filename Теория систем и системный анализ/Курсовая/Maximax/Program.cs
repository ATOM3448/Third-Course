using System;
using System.Runtime.InteropServices;

namespace Maximax
{                          
   class Program
   {
        private static void ErrorMEs(string mes) 
        {
            Console.Write($"{mes}, повторите ввод, когда окно очистится.");
            System.Threading.Thread.Sleep(5000);
        }
        public static void Main(string[] args)
        {
            double mantis = 0.00001;
            bool flag = true;
            string[] skills = ["Продвинутый", "Средний", "Низкий"];

            // Запрос количества альтернатив
            int alternativesCount = 0;
            while (flag)
            {
                Console.Clear();
                try
                {
                    Console.Write("Введите количество альтерантив: ");
                    alternativesCount = Convert.ToInt32(Console.ReadLine());
                    flag = false;
                }
                catch (Exception e)
                {
                    ErrorMEs("Что-то пошло не так");
                    continue;
                }
                if (alternativesCount < 2)
                {
                    ErrorMEs("Количество альтернатив должно быть больше двух");
                    continue;
                }
            }

            flag = true;

            // Запрос количества состояний
            int modsCount = 0;
            while (flag)
            {
                Console.Clear();
                try
                {
                    Console.Write("Введите количество состояний: ");
                    modsCount = Convert.ToInt32(Console.ReadLine());
                    flag = false;
                }
                catch (Exception e)
                {
                    ErrorMEs("Что-то пошло не так");
                    continue;
                }
                if (modsCount < 1)
                {
                    ErrorMEs("Количество состояний должно быть больше нуля");
                    continue;
                }
            }

            flag = true;

            // Запрос количества экспертов
            int exCount = 0;
            while (flag)
            {
                Console.Clear();
                try
                {
                    Console.Write("Введите количество экспертов: ");
                    exCount = Convert.ToInt32(Console.ReadLine());
                    flag = false;
                }
                catch (Exception e)
                {
                    ErrorMEs("Что-то пошло не так");
                    continue;
                }
                if (exCount < 1)
                {
                    ErrorMEs("Количество экспертов должно быть больше нуля");
                    continue;
                }
            }

            flag = true;

            // Запрос компетентности экспертов
            double[] exSkil = new double[exCount];
            while (flag)
            {
                double timesum = 0.0;
                Console.Clear();
                try
                {
                    for (int i = 0; i < exCount; i++)
                    {
                        Console.Write($"Введите компетентность эксперта #{i + 1}: ");
                        exSkil[i] = Convert.ToDouble(Console.ReadLine().Replace(".", ","));
                        timesum += exSkil[i];
                    }
                }
                catch (Exception e)
                {
                    ErrorMEs("Что-то пошло не так");
                    continue;
                }
                if((timesum < (1 - mantis)) || (timesum > (1 + mantis)))
                {
                    ErrorMEs("Сумма компетенций должна быть равна 1");
                    continue;
                }
                flag = false;
            }

            flag = true;


            // Запрос имен альтернатив
            string[] alternativesNames = new string[alternativesCount];
            while (flag)
            {
                Console.Clear();
                try
                {
                    for (int i = 0; i < alternativesCount; i++)
                    {
                        Console.Write($"Введите название альтернативы #{i+1}: ");
                        alternativesNames[i] = Console.ReadLine();
                        if (alternativesNames[i].Length < 1)
                        {
                            Console.WriteLine("Имя должно иметь хотябы 1 символ, повторите ввод для ЭТОЙ альтернативы");
                            i--;
                        }
                    }
                }
                catch (Exception e)
                {
                    ErrorMEs("Что-то пошло не так");
                    continue;
                }
                flag = false;
            }

            flag = true;

            // Запрос имен состояний
            string[] modsNames = new string[modsCount];
            while (flag)
            {
                Console.Clear();
                try
                {
                    for (int i = 0; i < modsCount; i++)
                    {
                        Console.Write($"Введите название состояния #{i + 1}: ");
                        modsNames[i] = Console.ReadLine();
                        if (modsNames[i].Length < 1)
                        {
                            Console.WriteLine("Имя должно иметь хотябы 1 символ, повторите ввод для ЭТОГО состояния");
                            i--;
                        }
                    }
                }
                catch (Exception e)
                {
                    ErrorMEs("Что-то пошло не так");
                    continue;
                }
                flag = false;
            }

            flag = true;

            // Запрос значений долей
            double[,,] skillsParts = new double[exCount,3, modsCount];
            while (flag)
            {
                Console.Clear();
                try
                {
                    for(int e = 0; e < exCount; e++)
                    {
                        for (int i = 0; i < modsCount; i++)
                        {
                            double sum = 0;
                            for (int j = 0; j < 3; j++)
                            {
                                Console.Write($"Введите долю кандидатов с навыком \"{skills[j]}\", при состоянии #{i + 1}, по эксперту #{e+1}, в формате \"0,xxx\": ");
                                skillsParts[e, j, i] = Convert.ToDouble(Console.ReadLine().Replace(".", ","));
                                if (skillsParts[e, j, i] < mantis)
                                {
                                    Console.WriteLine("Значение должно быть больше 0.000000001, повторите этот ввод");
                                    j--;
                                    continue;
                                }
                                sum += skillsParts[e, j, i];
                            }
                            if ((sum > 1.0 + mantis) || (sum < 1 - mantis))
                            {
                                Console.WriteLine("Сумма долей кандидатов должна быть равна единице, повторите ввод для состояния #{i+1}");
                                i--;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    ErrorMEs("Что-то пошло не так");
                    continue;
                }
                flag = false;
            }

            flag = true;

            // Запрос значений вероятностей
            double[,] chances = new double[3, alternativesCount];
            while (flag)
            {
                Console.Clear();
                try
                {
                    for (int i = 0; i < alternativesCount; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            Console.Write($"Введите шанс найти информацию с навыком \"{skills[j]}\", при альтернативе #{i + 1}, в формате \"0,xxx\": ");
                            chances[j, i] = Convert.ToDouble(Console.ReadLine().Replace(".", ","));
                            if (chances[j, i] < mantis)
                            {
                                Console.WriteLine($"Значение должно быть больше {mantis}, повторите этот ввод");
                                j--;
                                continue;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    ErrorMEs("Что-то пошло не так");
                    continue;
                }
                flag = false;
            }

            // Подсчёт матрицы
            double[,] final = new double[alternativesCount, modsCount+1];
            int bestIndex = 0;
            for (int i = 0; i < alternativesCount; i++)
            {
                double max = -1.0;
                for (int j = 0; j < modsCount; j++)
                {
                    final[i, j] = 0.0;
                    for (int k = 0; k < skills.Length; k++)
                    {
                        double skils = 0.0;
                        for (int e = 0; e < exCount; e++)
                        {
                            skils += exSkil[e]*skillsParts[e, k, j];
                        }
                        final[i, j] += skils * chances[k, i];
                    }

                    if (final[i, j] > max)
                        max = final[i, j];
                }
                final[i, modsCount] = max;
                if (max > final[bestIndex, modsCount])
                    bestIndex = i;
            }

            // Вывод матрицы
            Console.Clear();
            for (int i = 0; i < alternativesCount; i++)
                Console.WriteLine($"u{i+1} - {alternativesNames[i]}");

            Console.WriteLine();

            for (int i = 0; i < modsCount; i++)
                Console.WriteLine($"w{i + 1} - {modsNames[i]}");

            Console.WriteLine();

            for (int i = -1; i < alternativesCount; i++)
            {
                Console.WriteLine();
                if (i == -1)
                {
                    Console.Write("Варианты");
                    for (int j = 0; j < modsCount; j++)
                    {
                        Console.Write($"\tw{j+1}");
                    }
                    Console.Write("\tМаксимакс");
                    continue;
                }
                for (int j = -1; j < modsCount+1; j++)
                {
                    if (j == -1)
                    {
                        Console.Write($"u{i+1}\t");
                        continue;
                    }
                    Console.Write($"\t{final[i,j].ToString("0.000")}");
                }
            }
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine($"Лучшая альтернатива: {alternativesNames[bestIndex]}");

            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("Нажмите любую клавишу для выхода из программы");
            Console.ReadKey();
        }
   }
}