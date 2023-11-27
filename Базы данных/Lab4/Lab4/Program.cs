using Microsoft.EntityFrameworkCore;
using Lab4.Db;

namespace Lab4
{
    class Program
    {
        public static void Main(string[] args)
        {
            // Список атрибутов таблиц, для удобства в дальнейшем
            string[][] attributes = { new string[] {"Art", "Name_G", "Meas", "Price_G" },
                                     new string[] { "Code_C", "FIO", "Addr_C", "Tel" },
                                     new string[] { "Code_Z", "Code_C", "Acc_DT", "Addr_D", "Del_DT", "Price_D" },
                                     new string[] { "Code_Z", "Art", "Qt" }
                                   };

            bool flag = true;
            while (flag)
            {
                // Запрашиваем тип действия
                byte mode;
                Console.WriteLine("Выберите действие:\n\t1. Внести новые данные.\n\t2. Вывести отчёт за неделю.\n\tОстальное - выйти");
                try
                {
                    mode = Convert.ToByte(Console.ReadLine());
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка ввода");
                    flag = false;
                    break;
                }

                // Выполняем запрошенное действие
                Console.WriteLine();
                switch (mode)
                {
                    case 1:
                        // Заправшиваем целевую таблицу
                        Console.WriteLine("Выберите тиблицу:\n\t1. GOODS.\n\t2. CLIENT.\n\t3. CLIENT_ORDER\n\t4. ORD_GD\n\tОстальное - вернуться");
                        try
                        {
                            mode = Convert.ToByte(Console.ReadLine());
                        }
                        catch
                        {
                            break;
                        }

                        Console.WriteLine();

                        // Утанавливаем количество полей в указанной таблице
                        byte count_atttr = 0;

                        switch (mode)
                        {
                            case 1:
                            case 2:
                                count_atttr = 4;
                                break;
                            case 3:
                                count_atttr = 6;
                                break;
                            case 4:
                                count_atttr = 3;
                                break;
                            default:
                                break;
                        }

                        if (count_atttr == 0)
                            break;

                        // Даем пользователю ввести значения
                        string[] input = new string[count_atttr];
 
                        for (byte i = 0; i < count_atttr; i++)
                        {
                            Console.WriteLine($"Введите значение атриубта {attributes[mode-1][i]}:");
                            input[i] = Console.ReadLine();
                        }
                        try
                        {
                            // Вносим значения
                            using (LabsContext labs = new LabsContext())
                            {
                                switch (mode)
                                {
                                    case 1:
                                        labs.Goods.Add(new Good
                                        {
                                            Art = input[0],
                                            NameG = input[1],
                                            Meas = input[2],
                                            PriceG = Convert.ToDecimal(input[3].Replace('.', ','))
                                        });
                                        break;
                                    case 2:
                                        labs.Clients.Add(new Client
                                        {
                                            CodeC = Convert.ToInt32(input[0]),
                                            Fio = input[1],
                                            AddrC = input[2],
                                            Tel = input[3]
                                        });
                                        break;
                                    case 3:
                                        labs.ClientOrders.Add(new ClientOrder
                                        {
                                            CodeZ = input[0],
                                            CodeC = Convert.ToInt32(input[1]),
                                            AccDt = DateOnly.Parse(input[2]),
                                            AddrD = input[3],
                                            DelDt = DateOnly.Parse(input[4]),
                                            PriceD = Convert.ToDecimal(input[5])
                                        });
                                        break;
                                    case 4:
                                        labs.OrdGds.Add(new OrdGd
                                        {
                                            CodeZ = input[0],
                                            Art = input[1],
                                            Qt = Convert.ToInt32(input[2])
                                        });
                                        break;
                                }
                                labs.SaveChanges();
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }

                        break;
                    case 2:
                        Console.WriteLine("Введите дату - начало отчёта");
                        DateOnly startDate;
                        try
                        {
                            startDate = DateOnly.Parse(Console.ReadLine());
                        }
                        catch 
                        {
                            Console.WriteLine("Ошибка чтения даты");
                            flag = false;
                            break;
                        }

                        DateOnly endDate = startDate.AddDays(60);

                        //Фомриурем отчёт
                        using (LabsContext labs = new LabsContext())
                        {
                            var allTable = labs.ClientOrders.Where(x => (x.DelDt >= startDate) && (x.DelDt <= (endDate)))
                                                            .Join(labs.OrdGds, f => f.CodeZ, s => s.CodeZ, (f, s) => new { f.CodeC, f.CodeZ, f.PriceD, s.Art, s.Qt })
                                                            .Join(labs.Goods, f => f.Art, s => s.Art, (f, s) => new { f.CodeC, f.CodeZ, f.Qt, s.PriceG, f.PriceD })
                                                            .Join(labs.Clients, f => f.CodeC, s => s.CodeC, (f, s) => new { f.CodeC, s.Fio, f.CodeZ, f.Qt, f.PriceG, f.PriceD })
                                                            .Select(x => new { ClientCode = x.CodeC, FIO = x.Fio, OrderCode = x.CodeZ, OrderPrice = Convert.ToDecimal(x.Qt.Value) * Convert.ToDecimal(x.PriceG.Value), DelPrice = x.PriceD })
                                                            .OrderBy(x => x.ClientCode)
                                                            .ThenBy(x => x.OrderCode);

                            var finalTable = allTable.Join(allTable.GroupBy(x => x.OrderCode).Select(x => new { code = x.Key, sum = x.Sum(y => y.OrderPrice) }),
                                                           f => f.OrderCode,
                                                           s => s.code,
                                                           (f, s) => new { f.ClientCode, f.FIO, f.OrderCode, OrderPrice = Convert.ToDecimal(s.sum) + Convert.ToDecimal(f.DelPrice.Value) })
                                                     .Distinct();

                            var tableInLst = finalTable.ToList();

                            // Локальные переменные для каждого клиента
                            int codeC = (int)tableInLst[0].ClientCode;
                            int _count = 0;
                            decimal _sum = 0;

                            // Для конца отчёта
                            int count = 0;
                            decimal sum = 0;

                            Console.WriteLine($"\t\t\t\t\tОТЧЁТ\n\t\tоб исполненных заказах клиентов с {startDate} по {endDate}");

                            foreach (var row in tableInLst)
                            {
                                if ((count == 0) && (_count == 0))
                                {
                                    Console.WriteLine($"\n{row.FIO} ({row.ClientCode})\n\tЗаказ #\t\tна сумму");
                                }

                                if ((row.ClientCode != codeC) && (_count != 0))
                                {
                                    Console.WriteLine($"\tВсего заказов\t{_count}\n\tна сумму\t{_sum}р");
                                    Console.WriteLine($"\n{row.FIO} ({row.ClientCode})\n\tЗаказ #\t\tна сумму");

                                    count += _count;
                                    _count = 0;
                                    sum += _sum;
                                    _sum = 0;
                                }

                                Console.WriteLine($"\t{row.OrderCode}\t{row.OrderPrice}р");

                                _count++;
                                _sum += row.OrderPrice;
                            }
                            Console.WriteLine($"\tВсего заказов\t{_count}\n\tна сумму\t{_sum}р");
                            count += _count;
                            sum += _sum;

                            Console.WriteLine($"\nВсего заказов за период\t{count}\nна общую сумму\t\t{sum}р\n");
                        }
                        break;
                    default:
                        flag = false;
                        break;
                }
            }
            
        }
    }
}