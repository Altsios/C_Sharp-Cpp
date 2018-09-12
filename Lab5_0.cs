//PROGRAM
using System;
using System.IO;
using System.Linq;

namespace Enumeration_sort
{
    class Program
    {
        //заполнение случайными числами от 0 до кол-ва элементов
        static void GenerateRandArray(int n)
        {
            Random Rand = new Random();
            using (StreamWriter file = new StreamWriter("C:\\Users\\Alenka\\OneDrive\\Параллельное программирование\\Enumeration sort\\Enumeration sort\\In.txt", true))
            {
                for (int i = 0; i < n; i++)
                    file.Write(Rand.Next(0, n) + " ");
                file.Write(Environment.NewLine);
            }
        }
        //заполнение числами от 0 до n по возрастанию
        static void GenerateAscArray(int n)
        {
            using (StreamWriter file = new StreamWriter("C:\\Users\\Alenka\\OneDrive\\Параллельное программирование\\Enumeration sort\\Enumeration sort\\In.txt", true))
            {
                for (int i = 0; i < n; i++)
                    file.Write(i + " ");
                file.Write(Environment.NewLine);
            }
        }
        //заполнение числами от 0 до n по убыванию
        static void GenerateDescArray(int n)
        {
            using (StreamWriter file = new StreamWriter("C:\\Users\\Alenka\\OneDrive\\Параллельное программирование\\Enumeration sort\\Enumeration sort\\In.txt", true))
            {
                for (int i = n-1; i >=0; i--)
                    file.Write(i + " ");
                file.Write(Environment.NewLine);
            }
        }
        static void Main(string[] args)
        {
            //неупорядоченный набор данных
             GenerateRandArray(100);
             GenerateRandArray(1000);
             GenerateRandArray(10000);
             GenerateRandArray(50000);
            GenerateRandArray(80000);
            GenerateRandArray(100000);
            //упорядоченный набор данных по возрастанию
            GenerateAscArray(100);
            GenerateAscArray(1000);
            GenerateAscArray(10000);
            GenerateAscArray(50000);
            GenerateAscArray(80000);
            GenerateAscArray(100000);
            //упорядоченный набор данных по убыванию
            GenerateDescArray(100);
            GenerateDescArray(1000);
            GenerateDescArray(10000);
            GenerateDescArray(50000);
            GenerateDescArray(80000);
            GenerateDescArray(100000);

            string str = "";

            //чтение из файла. Каждую строчку преобразуем массив, применяем последовательную и параллельную сортировку.
            using (StreamReader file = new StreamReader("C:\\Users\\Alenka\\OneDrive\\Параллельное программирование\\Enumeration sort\\Enumeration sort\\In.txt"))
            {
                string line;
                DateTime d1, d2;
                while((line=file.ReadLine())!=null)
                {
                    //получаем массив из файла
                    int[] a = line.Split(new char[] { ' ', }, StringSplitOptions.RemoveEmptyEntries).Select(n => int.Parse(n)).ToArray();
                    //последовательная сортировка 
                    d1 = DateTime.Now;
                    str = EnumeSort.Sequential(ref a);
                    d2 = DateTime.Now;
                    Console.WriteLine("Seq Complete");
                    //сразу записываем в файл строку - остортированный по рангам исходный массив
                    File.AppendAllText("C:\\Users\\Alenka\\OneDrive\\Параллельное программирование\\Enumeration sort\\Enumeration sort\\Out.txt",
                        str + Environment.NewLine);
                    File.AppendAllText("C:\\Users\\Alenka\\OneDrive\\Параллельное программирование\\Enumeration sort\\Enumeration sort\\summary.txt",
                        (d2 - d1).TotalMilliseconds + Environment.NewLine);
                      //перед использованием статического элемента нужно обнуление
                      EnumeSort.b = null;
                      EnumeSort.b = new int[a.Length];//переопределяем элемент
                      d1 = DateTime.Now;
                      str = EnumeSort.Parallel(a);
                      d2 = DateTime.Now;
                      Console.WriteLine("Par Complete");
                      //сразу записываем в файл строку - остортированный по рангам исходный массив
                      File.AppendAllText("C:\\Users\\Alenka\\OneDrive\\Параллельное программирование\\Enumeration sort\\Enumeration sort\\Out.txt",
                          str + Environment.NewLine);
                      File.AppendAllText("C:\\Users\\Alenka\\OneDrive\\Параллельное программирование\\Enumeration sort\\Enumeration sort\\summary.txt",
                          (d2 - d1).TotalMilliseconds + Environment.NewLine);
                }
            }
            Console.WriteLine("Done!");
            Console.ReadKey();
        }
    }
}

//ENUMESORT

using System.Threading;

namespace Enumeration_sort
{
    static class EnumeSort
    {
        public static int[] b;//результирующая последовательность
        public static string Sequential(ref int[] a)
        {
            int n = a.Length;
            int[] b = new int[n];
            int x;
            string str = "";
            // по всем элементам
            for (int i = 0; i < n; i++)
            {
                x = 0;
                // вычисляем ранг элемента
                for (int j = 0; j < n; j++)
                    if (a[i] > a[j] || (a[i] == a[j] && j > i))
                        x++;
                b[x] = a[i]; // записываем в результирующую
            }
            //запись в строку элементов массива
            foreach (var item in b)
                str += item + " ";
            return str;
        }

        public static string Parallel(int[] a)
        {
            int M = 4;//основываясь на предыдущих работах, берем оптимальное число потоков
            Thread[] thrs = new Thread[M];
            int n = a.Length;
            string str = "";
            //инициализируем каждый поток в массиве
            for (int i = 0; i < M; i++)
            {
                //локальный индекс. иначе i-ссылка. когда поток начнет работу, i убежит далеко
                int thr = i;
                thrs[i] = new Thread(() =>
                {
                    for (int j = thr; j < n; j+=M)//даем потоку обработать определенные элементы(примерно равные порции)
                    {
                        int x = 0;
                        // вычисляем ранг элемента
                        for (int k = 0; k < n; k++)
                            if (a[j] > a[k] || (a[j] == a[k] && k > j))
                                    x++;
                        b[x] = a[j]; // записываем в результирующую
                    }
                });
                //запускаем потоки
                thrs[i].Start();
            }

            //останавливаем потоки
            for (int i = 0; i < M; i++) thrs[i].Join();
            //запись в строку элементов массива
            foreach (var item in b)
                str += item + " ";
            return str;
        }
    }
}

