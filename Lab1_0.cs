using System;
using System.Threading;

namespace Lab1_Alecio
{
    class Program
    {
        static int[] a;
        static int[] b;
        /*k для 4го задания
        static int k;*/
        //функция обработки элементов вектора равными частями
        static void Eq(object o)
        {
            int Start = ((int[])o)[0];
            int End = ((int[])o)[1];
            for (int i = Start; i < End; i++)
                /*пункт 1-3) 
                b[i] = a[i] * 2;*/
                /*пункт 4
                for (int j=0;j<k;j++)
                пункт 5*/
                for (int j = 0; j <= i; j++)
                        b[i] += a[i] * 2;
        }

        static void Circ(object o)
        {
            int iThread = ((int[])o)[0];
            int M = ((int[])o)[1];
            for (int i = iThread; i < b.Length; i += M)
                for (int j = 0; j <= i; j++)
                    b[i] += a[i] * 2;
        }
        //обозначить равное количество элементов как N/M, при этом N>M.
        //использовать структуру
        static void Main(string[] args)
        {
            while (true)
            {
                int N, M;
                try
                {
                /*для 4го задания*/
                /*Console.Write("Ввод коэффициента сложности (k)>");
                k = int.Parse(Console.ReadLine());*/
                Console.Write("Ввод числа элементов вектора (N)>");
                N = int.Parse(Console.ReadLine());
                Console.Write("Ввод числа потоков (M)>");
                M = int.Parse(Console.ReadLine());
                 }
                catch (Exception)
                {
                    Console.WriteLine("Неверный ввод.");
                    continue;
                }
                //инициализация и заполнение массива a
                a = new int[N];
                b = new int[N];
                var Rand = new Random();
                for (int i = 0; i < N; i++)
                    a[i] = Rand.Next(0, 9);


                //при M=1 последовательная обработка элементов вектора(1 поток)
                if (M == 1)
                {
                    DateTime dt1 = DateTime.Now;
                    for (int i = 0; i < 1000; i++)
                    {
                        var thr = new Thread(Eq);
                        //вычислительная процедура
                        thr.Start(new int[] { 0, N });
                        thr.Join();
                    }
                    DateTime dt2 = DateTime.Now;
                    Console.WriteLine("Затрачено времени {0}", (dt2 - dt1).TotalMilliseconds / 1000);
                }
                else
                {

                    DateTime dt1 = DateTime.Now;
                    //массив потоков 
                    for (int i = 0; i < 1000; i++)
                    {
                        //стартовые и конечные позиции, шаг (равное кол-о элементов)
                        /*пункты 1-5
                        int Step = N / M;
                        int Start = -Step;
                        int End = 0;*/
                        Thread[] arrThr = new Thread[M];
                        //инициализация и запуск потоков в цикле
                        for (int j = 0; j < M; j++)
                        {
                            /*пункты 1-5
                            arrThr[j] = new Thread(Eq);
                            arrThr[j].Start(new int[] { Start += Step, End += Step });*/
                            /*пункт 6* j-номер потока*/
                            arrThr[j] = new Thread(Circ);
                            arrThr[j].Start(new int[] { j, M });
                        }
                        //последовательное завершение(блокировка потоков)
                        for (int j = 0; j < M; j++)
                            arrThr[j].Join();
                    }
                    DateTime dt2 = DateTime.Now;
                    Console.WriteLine("Затрачено времени {0}", (dt2 - dt1).TotalMilliseconds / 1000);
                    a = null;
                    b = null;
                }
            }
        }
    }
}
