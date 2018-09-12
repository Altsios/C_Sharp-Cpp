//Program.cs

using System;
using System.Diagnostics;

namespace Lab4Csh
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch st = new Stopwatch();

            int[] a;
            int[] b;
            Console.WriteLine(string.Format("С#:\n{0,-16}|{1,-16}|{2,-16}","Алгоритм","Элементов","Время"));
            Console.WriteLine(new string('-', 48));

            for (int i = 100; i <= 100000; i += 5000)
            {
                a = Utils.GenerArray(i);

                st.Start();
                b = a.SortSeq();
                st.Stop();
                Console.WriteLine(string.Format("{0,-16}|{1,-16}|{2,-16}", "Последовательный", i.ToString(),
                    st.ElapsedMilliseconds.ToString()));

                st.Restart();
                b = a.SortParall();
                st.Stop();
                Console.WriteLine(string.Format("{0,-16}|{1,-16}|{2,-16}", "Параллельный", i.ToString(),
    st.ElapsedMilliseconds.ToString()));
            }

            Console.WriteLine("Готово!");
            Console.ReadKey();
        }
    }

}

//Utils.cs

using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Lab4Csh
{
    static class Utils
    {
        //заполнение числами от 0 до n по возрастанию
        public static int[] GenerArray(int n)
        {
            var a = new int[n];
            for (int i = 0; i < n; i++)
                a[n-i-1] = i;
            return a;
        }

        public static int[] SortSeq(this int[] a)
        {
            int n = a.Length;
            int[] b = new int[n];
            int x;
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
            return b;
        }

        public static int[] SortParall(this int[] a)
        {
            int n = a.Length;
            int[] b = new int[n];
            Parallel.ForEach(Partitioner.Create(0, n), range =>
            {
                int x;
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    x = 0;
                    // вычисляем ранг элемента
                    for (int j = 0; j < n; j++)
                        if (a[i] > a[j] || (a[i] == a[j] && j > i))
                            x++;
                    b[x] = a[i]; // записываем в результирующую
                }
            }
            );
            return b;
        }
    }
}
