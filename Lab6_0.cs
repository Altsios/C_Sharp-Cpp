PROGRAM
using System;

namespace Lsb6_Tsios
{
    static class Program
    {
        public static bool parallel=false;
        public static DateTime d1, d2;
        static void Main(string[] args)
        {
            //FileHandler используется для работы с 3мя файлами программы
            FileHandler h = new FileHandler();
            //g макс число при генерации,количество ребер
            //проверяем переданные флаги. Если -s-последовательный, если -p -параллельный, если есть -g-генерация графа
            //массив выбранных флагов. Ввод в формате -режим, -генерация. Если введено что-то непонятное, поведение по умолчанию -s
            //Элементы: 0-p,0-s,1-g макс число при генерации,количество ребер
            //выбираем режим запуска
            try
            {
                if (args.Length == 4)//с генерацией
                {
                    if (args[0] == "-p" && args[1] == "-g")//паралл+генерация
                    {
                        parallel = true;
                        h.GenerateAndWrite(int.Parse(args[2]), int.Parse(args[3]));
                    }
                    else if (args[0] == "-s" && args[1] == "-g")//не допускаю запуска параллельного и последовательного варианта
                    {
                        h.GenerateAndWrite(int.Parse(args[2]), int.Parse(args[3]));
                    }
                }
                else if(args[0] == "-p") parallel = true;//только паралл
                else if (args[0] == "-g")//генерация
                {
                    h.GenerateAndWrite(int.Parse(args[1]), int.Parse(args[2]));
                    Console.WriteLine("Готово!");
                    Console.ReadKey();
                    return;
                }
            }
            catch(IndexOutOfRangeException)
            {
                Console.WriteLine("Введенное число ребер не может быть построено при таком количестве вершин");
                return;
            }
            catch(Exception)
            {
                Console.WriteLine("Введены неверные параметры");
                return;
            }
            //иначе-поведение по умолчанию: последовательный, без генерации
            try
            {
                Graph A = new Graph(h);
                d1 = DateTime.Now;
                int[,] ans = Search.Run(A.getCopyMtx());
                d2 = DateTime.Now;
                h.Write(ans);//копия поля->функция поиска и ответ в файл
            }
            catch(NullReferenceException)
            {
                Console.WriteLine("Файл IN.txt пуст!");
                return;
            }
            Console.WriteLine("Готово!");
            Console.ReadKey();
        }
    }
}


FILEHANDLER
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Lsb6_Tsios
{

    class FileHandler
    {
        readonly string IN;
        readonly string OUT;
        readonly string SUMMARY;
        int CNT;

        public void GenerateAndWrite(int max, int cnt)//cnt-кол-во ребер
        {
            
            CNT = cnt;//присваиваем полю значение
            //для избежания зависания, используем формулу кол-ва ребер от вершин для графов без петель
            if (max * (max - 1) / 2 < cnt) throw new IndexOutOfRangeException();
            List<string> nums = new List<string>();
            while (nums.Count < CNT)
            {
                //генерация ребра
                Random r = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
                int a = r.Next(1, max + 1);
                int b = r.Next(1, max + 1);
                //не допускаем дублей, в том числе вида 2 1 и 1 2 и одинаковых значений (1 1)
                if ((a == b) || nums.Contains(a.ToString() + " " + b.ToString()) || nums.Contains(b.ToString() + " " + a.ToString()))
                    continue;
                else nums.Add(a.ToString() + " " + b.ToString());
            }
            Random rr = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            using (StreamWriter sr = new StreamWriter(IN, true))
            {
                foreach (var line in nums)
                    sr.WriteLine(line + " " + rr.Next(1, 20));
                sr.WriteLine("@");
            };
        }

        public FileHandler()
        {
            string str = Assembly.GetExecutingAssembly().Location;
            //часть пути к текстовым файлам 
            string path = str.Substring(0, str.IndexOf("bin"));
            IN = path + "In.txt";
            OUT = path + "out.txt";
            SUMMARY = path + "summary.txt";
        }

        public List<int[]> Read()
        {
            List<int[]> lst = new List<int[]>();
            //Чтение из файла
            using (StreamReader rfile = new StreamReader(IN))
            {
                string line = "";

                    while ((line = rfile.ReadLine()) != "@")
                    {
                        int[] nums = line.Split(new char[] { ' ', }, StringSplitOptions.RemoveEmptyEntries).Select(n => int.Parse(n)).ToArray();
                        lst.Add(nums);
                    }
           }
            return lst;
        }

        public void Write(int[,] ans)
        {
            int len = ans.GetLength(0);

            using (StreamWriter sr = new StreamWriter(OUT, true))
            {
                if (Program.parallel) sr.WriteLine("Паралелльная версия");
                else sr.WriteLine("Последовательная версия");
                for (int i = 0; i < len; i++)
                {
                    for (int j = 0; j < len; j++)
                    {
                        if (ans[i, j] == Graph.INF) sr.Write("oo\t");
                        else sr.Write(ans[i, j].ToString() + "\t");
                    }
                    sr.WriteLine();
                }
            }
            using (StreamWriter sr = new StreamWriter(SUMMARY, true))
            {
                if (Program.parallel) sr.WriteLine("Паралелльная версия");
                else sr.WriteLine("Последовательная версия");
                sr.WriteLine((Program.d2 - Program.d1).TotalMilliseconds+" мс");
            }
        }
    }
}

GRAPH

using System;
using System.Collections.Generic;

namespace Lsb6_Tsios
{
    class Graph
    {
        //значение бесконечности
        public static readonly int INF = int.MaxValue;
        //поле-матрица
        private readonly int [,]Mtx;
        public int[,] getCopyMtx()
        {
            return Mtx;
        }

        static int Max(ref List< int[] > lst)
        {
            int num=0;
            int max=0;
            foreach (var item in lst)
            {
                if (item[0] > item[1]) num = item[0];
                else num = item[1];
                if (num > max) max = num;
            }
            return max;
        }

        public Graph(FileHandler h)
        {
            //класс с содержимым файла и n-число записей про ребра 
            List < int[] > lst= h.Read();

                if (lst.Count == 0) return;
         
            int n = Max(ref lst);//количество элементов в матрице от первой вершины до n-й, ищем максимум
            Mtx = new int[n,n];
            //заполняем самыми большими значениями
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Mtx[i, j] = INF;
                }
            }

            foreach (var elem in lst)
            {
                Mtx[elem[0] - 1, elem[1] - 1] = elem[2];//берем значение. Используем симметрию
                Mtx[elem[1]-1, elem[0]-1] = elem[2];
            }

        }
    }
}


SEARCH
using System.Threading;

namespace Lsb6_Tsios
{
    class Search
    {

        static int[,] Lnew;//для записи расстояний
        static int[,] CopySymm(ref int[,] A)
        {
            int n = A.GetLength(0);
            int[,] B = new int[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    B[i, j] = A[i, j];
                    B[j, i] = A[i, j];
                }
            }
            return B;
        }

        static void F(object obj)
        {
            int n = Lnew.GetLength(0);
            int i = (int)((object[])obj)[0];//вытаскиваем i
            CountdownEvent ev = ((object[])obj)[1] as CountdownEvent;//состояние(сигнал)
            int[,] L = (int[,])((object[])obj)[2];

            for (int j = i + 1; j < n; j++)//используем симметрию
                {
                    if ((i == j) || Lnew[i, j] <= 2) continue;//не меняем значений для элементов диагонали и значений 1 и 2)

                    for (int k = 0; k < n; k++)
                    {
                        int sum = (L[i, k] == Graph.INF || L[k, j] == Graph.INF) ? Graph.INF : L[i, k] + L[k, j];//если превысим макс инт, будет записано что-то с минусом-ошибка в расчетах
                        if (Lnew[i, j] > sum)
                        {
                            Lnew[i, j] = sum;
                            Lnew[j, i] = sum;//меняем у симметричного
                        }
                    }
                }
            ev.Signal();//регистрируем сигнал с CountdownEvent. Уменьшаем currentCount 
        }

        static int[,] SHORTEST_PATHS_PARALL(ref int[,] L)//вычисляет матрицу следующей степени
        {
            int n = L.GetLength(0);
            Lnew = CopySymm(ref L);
            CountdownEvent events = new CountdownEvent(n);//выдадим i пулу потоков

           for(int i = 0; i < n; i++)//распараллеливаем внешний цикл
           ThreadPool.QueueUserWorkItem(F, new object[] { i, events,L});
            events.Wait();//блок, пока Signal не вызовется n раз
            return Lnew;
        }

        static int[,] SHORTEST_PATHS(ref int[,] L)//вычисляет матрицу следующей степени
        {
            int n = L.GetLength(0);
            int[,] Lnew = CopySymm(ref L);
            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)//используем симметрию
                {
                    if ((i == j) || Lnew[i, j] <= 2) continue;//не меняем значений для элементов диагонали и значений 1 и 2)

                    for (int k = 0; k < n; k++)
                    {
                        int sum = (L[i, k] == Graph.INF || L[k, j] == Graph.INF) ? Graph.INF : L[i, k] + L[k, j];//если превысим макс инт, будет записано что-то с минусом-ошибка в расчетах
                        if (Lnew[i, j] > sum)
                        {
                            Lnew[i, j] = sum;
                            Lnew[j, i] = sum;//меняем у симметричного
                        }
                    }
                }
            }
            return Lnew;
        }
        //многократное возведение в квадрат
        public static int[,] Run(int[,] W)
        {
            int n = W.GetLength(0);
            int[,] L = CopySymm(ref W);//L^1
            int m = 1;
            //выбор последовательной или паралелльной версии 
            if(!Program.parallel)
            while (m < n - 1)//только до n-1 степени!!! 
            {
                L = SHORTEST_PATHS(ref L);
                m *= 2;
            }
            else
                while (m < n - 1)
                {
                    L = SHORTEST_PATHS_PARALL(ref L);
                    m *= 2;
                }
            return L;
        }

    }
}
