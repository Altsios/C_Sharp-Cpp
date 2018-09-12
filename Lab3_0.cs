//Реализация программы без каких-либо средств синхронизации
using System;
using System.Collections.Generic;
using System.Threading;
namespace Lab3
{
    class Program
    {
        static DateTime dt1, dt2;
        static int R = 2; // Параметр N - число писателей
        static int W = 2; // Параметр M - число читателей
        static int n = 1000000; // Параметр NumMessages - количество сообщений
        static string buffer;
        static Thread[] Writers = new Thread[W];
        static Thread[] Readers = new Thread[R];

        //список дял проверки массивов писателей
        //static List<string[]> ResultWri = new List<string[]>();
        //список для проверки массивов читателей
        //static List<List<string>> ResultRea = new List<List<string>>();

        static bool bEmpty = true;
        static bool finish = false;
        static void Read()
        {
            List<string> MyMessagesRead = new List<string>();//локальный массив читателя
            while (!finish)
                if (!bEmpty)
                {
                    MyMessagesRead.Add(buffer);
                    bEmpty = true;
                }
            //заносим в статический список, чтобы проверить содержимое
     //       ResultRea.Add(MyMessagesRead);
        }
        static void Write()
        {
            string[] MyMessagesWri = new string[n];//локальный массив писателя
            for (int j = 0; j < n; j++)
                MyMessagesWri[j] = j.ToString();
            // MyMessagesWri[j] = "Thread WRI #" + Thread.CurrentThread.Name + ", Message: " + j.ToString();//заменить
            int i = 0;
            while (i < n)
                if (bEmpty)
                {
                    buffer = MyMessagesWri[i++];
                    bEmpty = false;
                }
            //заносим в статический список, чтобы проверить содержимое
       //     ResultWri.Add(MyMessagesWri);
        }
        static void Main()
        {
            dt1 = DateTime.Now;
            for (int i = 0; i < W; i++)
            {
                Writers[i] = new Thread(Write);
             //   Writers[i].Name = i.ToString();
                Writers[i].Start();
            }
            for (int i = 0; i < R; i++)
            {
                Readers[i] = new Thread(Read);
                Readers[i].Start();
            }
            for (int i = 0; i < W; i++)
                Writers[i].Join();
            finish = true;//завершаем работу читателей
            for (int i = 0; i < R; i++)
                Readers[i].Join();
            dt2 = DateTime.Now;
            Console.WriteLine((dt2 - dt1).TotalMilliseconds);
  /*         
            int cnt = 0;
            for (int i = 0; i < ResultWri.Count; i++)
            {
                    cnt += ResultWri[i].GetLength(0);
            }
            Console.WriteLine("Всего сообщений отправлено:{0}", cnt);
            cnt = 0;
            for (int i = 0; i < ResultRea.Count; i++)
            {
                if (ResultRea[i] != null)
                    cnt+= ResultRea[i].Count;
              
            }
            Console.WriteLine("Получено сообщений: {0}",cnt);*/
            Console.ReadKey();
        }
    }
}
//Реализация с использованием средств синхронизации
using System;
using System.Collections.Generic;
using System.Threading;
namespace Lab3
{
    class Program
    {
        static DateTime dt1, dt2;
        static int R = 4; // Параметр N - число писателей
        static int W = 4; // Параметр M - число читателей
        static int n = 1000000; // Параметр NumMessages - количество сообщений
        static string buffer;
        static Thread[] Writers = new Thread[W];
        static Thread[] Readers = new Thread[R];

        //список дял проверки массивов писателей
        //static List<string[]> ResultWri = new List<string[]>();
        //список для проверки массивов читателей
        //static List<List<string>> ResultRea = new List<List<string>>();

        static bool bEmpty = true;
        static bool finish = false;
        static void Read()
        {
            List<string> MyMessagesRead = new List<string>();//локальный массив читателя
            while (!finish)
                if (!bEmpty)
                {
                    lock ("read")
                    {
                        if (!bEmpty)
                        {
                            bEmpty = true;
                            MyMessagesRead.Add(buffer);                        
                        }
                    }
                }
            //заносим в статический список, чтобы проверить содержимое
            //ResultRea.Add(MyMessagesRead);
        }
        static void Write()
        {
            string[] MyMessagesWri = new string[n];//локальный массив писателя
            for (int j = 0; j < n; j++)
             MyMessagesWri[j] = j.ToString();
            int i = 0;
            while (i < n)
                lock("write")
                {
                    if (bEmpty)
                    {
                        buffer = MyMessagesWri[i++];
                        bEmpty = false;
                    }
                }             
            //заносим в статический список, чтобы проверить содержимое
            //ResultWri.Add(MyMessagesWri);
        }
        static void Main()
        {
            dt1 = DateTime.Now;
            for (int i = 0; i < W; i++)
            {
                Writers[i] = new Thread(Write);
                Writers[i].Start();
            }
            for (int i = 0; i < R; i++)
            {
                Readers[i] = new Thread(Read);
                Readers[i].Start();
            }
            for (int i = 0; i < W; i++)
                Writers[i].Join();
            finish = true;//завершаем работу читателей
            for (int i = 0; i < R; i++)
                Readers[i].Join();
            dt2 = DateTime.Now;
            Console.WriteLine((dt2 - dt1).TotalMilliseconds);
     /*       int cnt = 0;
            for (int i = 0; i < ResultWri.Count; i++)
            {
                    cnt += ResultWri[i].GetLength(0);
            }
            Console.WriteLine("Всего сообщений отправлено:{0}", cnt);
            cnt = 0;
            for (int i = 0; i < ResultRea.Count; i++)
            {
                if (ResultRea[i] != null)
                    cnt+= ResultRea[i].Count;
              
            }
            Console.WriteLine("Получено сообщений: {0}",cnt);*/
            Console.ReadKey();
        }
    }
}
//Сигнальные сообщения
using System;
using System.Collections.Generic;
using System.Threading;
namespace Lab3
{
    class Program
    {
        static DateTime dt1, dt2;
        static int R = 2; // Параметр N - число писателей
        static int W = 6; // Параметр M - число читателей
        static int n = 1000000; // Параметр NumMessages - количество сообщений
        static string buffer;
        static Thread[] Writers = new Thread[W];
        static Thread[] Readers = new Thread[R];
        //сигнальные сообщения
        static AutoResetEvent evFull; 
        static AutoResetEvent evEmpty;
        //список для проверки массивов писателей
        //static List<string[]> ResultWri = new List<string[]>();
        //список для проверки массивов читателей
        //static List<List<string>> ResultRea = new List<List<string>>();

        static bool bEmpty = true;
        static bool finish = false;
        static void Read(object state)
        {
            var evFull = ((object[])state)[0] as AutoResetEvent;
            var evEmpty = ((object[])state)[1] as AutoResetEvent;
            List<string> MyMessagesRead = new List<string>();//локальный массив читателя
            while (!finish)
                        {   
                            evFull.WaitOne();//блокируем, ждем сигнала от писателей
                            if (finish)//пока ждали, нам уже сказали прекратить работу
                            {
                                evFull.Set();//даем сигналы следующим читателям
                                break;
                            }
                            MyMessagesRead.Add(buffer);
                            evEmpty.Set();//буфер пуст(прочитали)
                        }
                    
            //заносим в статический список, чтобы проверить содержимое
            //ResultRea.Add(MyMessagesRead);
        }
        static void Write(object state)
        {
            var evFull = ((object[])state)[0] as AutoResetEvent;
            var evEmpty = ((object[])state)[1] as AutoResetEvent;
            string[] MyMessagesWri = new string[n];//локальный массив писателя
            for (int j = 0; j < n; j++)
            MyMessagesWri[j] = j.ToString();
            int i = 0;
            while (i < n)
            {   
                        evEmpty.WaitOne();//блокируем, ждем сигнала от читателей
                        buffer = MyMessagesWri[i++];
                        evFull.Set();//буфер заполнен(можно читать)
                    }
            //заносим в статический список, чтобы проверить содержимое
            //ResultWri.Add(MyMessagesWri);
        }
        static void Main()
        {
            
            dt1 = DateTime.Now;
    evFull = new AutoResetEvent(false);//изначально буфер не полон
            evEmpty = new AutoResetEvent(true);//изначально буфер пуст

            for (int i = 0; i < W; i++)
            {
                Writers[i] = new Thread(Write);
                Writers[i].Start(new object[] { evFull, evEmpty });
            }
            for (int i = 0; i < R; i++)
            {
                Readers[i] = new Thread(Read);
                Readers[i].Start(new object[] { evFull,evEmpty});
            }
            for (int i = 0; i < W; i++)
                Writers[i].Join();
            finish = true;//завершаем работу читателей
            evFull.Set();//если читатели не успели прочитать и ждут.
            for (int i = 0; i < R; i++)
                Readers[i].Join();
            dt2 = DateTime.Now;
            Console.WriteLine((dt2 - dt1).TotalMilliseconds);
                /*   int cnt = 0;
                   for (int i = 0; i < ResultWri.Count; i++)
                   {
                           cnt += ResultWri[i].GetLength(0);
                   }
                   Console.WriteLine("Всего сообщений отправлено:{0}", cnt);
                   cnt = 0;
                   for (int i = 0; i < ResultRea.Count; i++)
                   {
                       if (ResultRea[i] != null)
                           cnt+= ResultRea[i].Count;

                   }
                   Console.WriteLine("Получено сообщений: {0}",cnt);*/
            Console.ReadKey();
        }
    }
}
//Семафоры(как критическая секция)
using System;
using System.Collections.Generic;
using System.Threading;
namespace Lab3
{
    class Program
    {
        static DateTime dt1, dt2;
        static int R = 6; // Параметр N - число писателей
        static int W = 4; // Параметр M - число читателей
        static int n = 1000000; // Параметр NumMessages - количество сообщений
        static string buffer;
        static Thread[] Writers = new Thread[W];
        static Thread[] Readers = new Thread[R];
        //семафор
        static SemaphoreSlim ssEmpty;

        //список для проверки массивов писателей
        //static List<string[]> ResultWri = new List<string[]>();
        //список для проверки массивов читателей
        //static List<List<string>> ResultRea = new List<List<string>>();

        static bool bEmpty = true;
        static bool finish = false;
        static void Read(object o)
        {
            var ssRead = o as SemaphoreSlim;
            List<string> MyMessagesRead = new List<string>();//локальный массив читателя
            while (!finish)
                if (!bEmpty)
                {
                    ssRead.Wait();
                        if (!bEmpty)
                        {
                            bEmpty = true;
                            MyMessagesRead.Add(buffer);
                        }
                    ssRead.Release();
                }
            //заносим в статический список, чтобы проверить содержимое
            //ResultRea.Add(MyMessagesRead);
        }
        static void Write(object o)
        {
            var ssWrit = o as SemaphoreSlim;
            string[] MyMessagesWri = new string[n];//локальный массив писателя
            for (int j = 0; j < n; j++)
                MyMessagesWri[j] = j.ToString();
            int i = 0;
            while (i < n)
                if(bEmpty)
                { 
                ssWrit.Wait();
                    if (bEmpty)
                    {
                        buffer = MyMessagesWri[i++];
                        bEmpty = false;
                    }
            ssWrit.Release();
                    }
            //заносим в статический список, чтобы проверить содержимое
            //ResultWri.Add(MyMessagesWri);
        }
        static void Main()
        {
            dt1 = DateTime.Now;
            ssEmpty = new SemaphoreSlim(1);//только один запрос может выполняться одновременно
            for (int i = 0; i < W; i++)
            {
                Writers[i] = new Thread(Write);
                Writers[i].Start(ssEmpty);
            }
            for (int i = 0; i < R; i++)
            {
                Readers[i] = new Thread(Read);
                Readers[i].Start(ssEmpty);
            }
            for (int i = 0; i < W; i++)
                Writers[i].Join();
            finish = true;//завершаем работу читателей
            for (int i = 0; i < R; i++)
                Readers[i].Join();
            dt2 = DateTime.Now;
            Console.WriteLine((dt2 - dt1).TotalMilliseconds);
              /*     int cnt = 0;
                   for (int i = 0; i < ResultWri.Count; i++)
                   {
                           cnt += ResultWri[i].GetLength(0);
                   }
                   Console.WriteLine("Всего сообщений отправлено:{0}", cnt);
                   cnt = 0;
                   for (int i = 0; i < ResultRea.Count; i++)
                   {
                       if (ResultRea[i] != null)
                           cnt+= ResultRea[i].Count;

                   }
                   Console.WriteLine("Получено сообщений: {0}",cnt);*/
            Console.ReadKey();
        }
    }
}
//Атомарные операторы
using System;
using System.Collections.Generic;
using System.Threading;
namespace Lab3
{
    class Program
    {
        static DateTime dt1, dt2;
        static int R = 2; // Параметр N - число писателей
        static int W = 2; // Параметр M - число читателей
        static int n = 1000000; // Параметр NumMessages - количество сообщений
        static string buffer;
        static Thread[] Writers = new Thread[W];
        static Thread[] Readers = new Thread[R];

        //список дял проверки массивов писателей
        //static List<string[]> ResultWri = new List<string[]>();
        //список для проверки массивов читателей
        //static List<List<string>> ResultRea = new List<List<string>>();

        static int intFull = 0;//буфер сначала не наполнен
        static int intEmpty = 1;//буфер сначала пуст

        static bool bEmpty = true;
        static bool finish = false;
        static void Read()
        {
            List<string> MyMessagesRead = new List<string>();//локальный массив читателя
            while (!finish)

                        if (Interlocked.CompareExchange(ref intFull, 0, 1) == 1)
                        {
                            MyMessagesRead.Add(buffer);
                            intEmpty = 1;//буффер пуст
                        }

            //заносим в статический список, чтобы проверить содержимое
            //ResultRea.Add(MyMessagesRead);
        }
        static void Write()
        {
            string[] MyMessagesWri = new string[n];//локальный массив писателя
            for (int j = 0; j < n; j++)
                MyMessagesWri[j] = j.ToString();
            //MyMessagesWri[j] = "Thread WRI #" + Thread.CurrentThread.Name + ", Message: " + j.ToString();//заменить
            int i = 0;
            while (i < n)
                    if (Interlocked.CompareExchange(ref intEmpty,0,1)==1)//если пуст
                    {
                        buffer = MyMessagesWri[i++];
                        intFull = 1;//буффер полон
                    }
            //заносим в статический список, чтобы проверить содержимое
           // ResultWri.Add(MyMessagesWri);
        }
        static void Main()
        {
            dt1 = DateTime.Now;
            for (int i = 0; i < W; i++)
            {
                Writers[i] = new Thread(Write);
                Writers[i].Name = i.ToString();
                Writers[i].Start();
            }
            for (int i = 0; i < R; i++)
            {
                Readers[i] = new Thread(Read);
                Readers[i].Start();
            }
            for (int i = 0; i < W; i++)
                Writers[i].Join();
            finish = true;//завершаем работу читателей
            for (int i = 0; i < R; i++)
                Readers[i].Join();
            dt2 = DateTime.Now;
            Console.WriteLine((dt2 - dt1).TotalMilliseconds);
           /*        int cnt = 0;
                   for (int i = 0; i < ResultWri.Count; i++)
                   {
                           cnt += ResultWri[i].GetLength(0);
                   }
                   Console.WriteLine("Всего сообщений отправлено:{0}", cnt);
                   cnt = 0;
                   for (int i = 0; i < ResultRea.Count; i++)
                   {
                       if (ResultRea[i] != null)
                           cnt+= ResultRea[i].Count;

                   }
                   Console.WriteLine("Получено сообщений: {0}",cnt);*/
            Console.ReadKey();
        }
    }
}