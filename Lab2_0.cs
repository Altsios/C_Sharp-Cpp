using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lab2_Tsios
{

    class Program
    {
        //задаем границу диапазона поиска
        const int N = 1000000;
        //список для хранения простых чисел
        static List<int> basePrime = new List<int>();
        //массив, где отмечено, делится ли число на другое число. N+1, т.к. от 0 до N 
        static int[] Nums = new int[N + 1];
        //число потоков
        static int M = 4;
        //количество элементов на поток
        static int cnt;
        //текущий индекс для 4-го алгоритма
        static int current_index = 0;
        //метод для заполнения списка базовых простых чисел
        static void Init()
        {
            for (int i = 2; i < Math.Sqrt(N); i++)
            {
                if (Nums[i] == 0)
                {
                    for (int j = i + 1; j < Math.Sqrt(N); j++)
                        if (j % i == 0)//если j делится нацело на i, оно составное
                            Nums[j] = 1;
                    basePrime.Add(i);//добавляем простое число(делитель) в список
                }
            }
        }
        //метод для модифицированного последовательного алгоритма
        static void Task1()
        {
            Init();
            for (int i = (int)(Math.Sqrt(N)); i < N + 1; i++)
                foreach (var item in basePrime)
                {
                    if (i % item == 0)//если i делится нацело на item, оно составное
                        Nums[i] = 1;
                }
        }
       
        //метод для параллельного алгоритма №1: декомпозиция по данным
        static void Task2()
        {
            Init();
            Thread[] arrThr = new Thread[M];//массив потоков
            for (int i = 0; i < M; i++)
            {

                arrThr[i] = new Thread(ThrFuncT2);
                arrThr[i].Start(i);
            }
            for (int i = 0; i < M; i++)
                arrThr[i].Join();

        }
        //метод, связанный с потоком
        static void ThrFuncT2(object obj)
        {
            int idx = (int)obj;//получаем номер потока
            int end;
            int Sqrt = (int)Math.Sqrt(N);
            cnt = (N - Sqrt) / M;//получаем кол. элементов,обрабат. потоком
            int start = Sqrt + cnt * idx;
            if (idx == M - 1) end = N + 1;//идем до конца массива, захватывая элементы, которые останутся в хвосте(деление 16/3)
            else end = start + cnt;
            for (int i = start; i < end; i++)
                foreach (var item in basePrime)
                {
                    if (i % item == 0)//если i делится нацело на item, оно составное
                        Nums[i] = 1;
                }
        }

        //метод для параллельного алгоритма №2: декомпозиция набора простых чисел

        static void Task3()
        {
            Init();
            Thread[] arrThr = new Thread[M];//массив потоков
            for (int i = 0; i < M; i++)
            {

                arrThr[i] = new Thread(ThrFuncT3);
                arrThr[i].Start(i);
            }
            for (int i = 0; i < M; i++)
                arrThr[i].Join();

        }
        //метод, связанный с потоком
        private static void ThrFuncT3(object obj)
        {
            int idx = (int)obj;//получаем номер потока
            int end;
            int Sqrt = (int)Math.Sqrt(N);
            int Len = basePrime.Count;
            cnt = Len / M;//получаем кол. элементов,обрабат. потоком
            int start = cnt * idx;
            if (idx == M - 1) end = Len;//идем до конца массива, захватывая элементы, которые останутся в хвосте(деление 16/3)
            else end = start + cnt;
            for (int i = Sqrt; i < N + 1; i++)
                for (int j = start; j < end; j++)
                {
                    if (i % basePrime[j] == 0)//если i делится нацело на basePrime[j], оно составное
                        Nums[i] = 1;
                }
        }
        //метод для параллельного алгоритма №3: применение пула потоков
        static void Task4()//использовался CountdownEvent, т.к при использовании WaiHandler с кол-м простых чисел>64 приводило к ошибке(число events не м.б>64)
        {
            Init();
            int Len = basePrime.Count;
            //Число потоков, которых мы будем дожидаться
            CountdownEvent events = new CountdownEvent(Len);
            // Добавляем в пул рабочие элементы с параметрами
            for (int i = 0; i < Len; i++)
            {
                ThreadPool.QueueUserWorkItem(F,new object[] { basePrime[i], events});
            }
            // Дожидаемся завершения
            events.Wait();//блок, пока Signal не вызовется len раз

        }
        //– метод обработки всех чисел диапазона sqrt(n) …n на разложимость простому числу basePrime[i]
        private static void F(object obj)
        {
            int Sqrt = (int)Math.Sqrt(N);
            int prime = (int)((object[])obj)[0];//вытаскиваем простое базовое число
            CountdownEvent ev = ((object[])obj)[1] as CountdownEvent;//состояние(сигнал)
            for (int i = Sqrt; i < N + 1; i++)
                if (i % prime == 0)//если i делится нацело на prime, оно составное
                    Nums[i] = 1;
            ev.Signal();//регистрируем сигнал с CountdownEvent. Уменьшаем currentCount
        }

        //метод для параллельного алгоритма №4: последовательный перебор простых чисел
        static void Task5()
        {
            Init();
            Thread[] arrThr = new Thread[M];//массив потоков
            for (int i = 0; i < M; i++)
            {

                arrThr[i] = new Thread(Func);
                arrThr[i].Start();
            }
            for (int i = 0; i < M; i++)
                arrThr[i].Join();
        }

        //метод, связанный с потоком
        private static void Func()
        {
            int current_prime;
            int Len = basePrime.Count;
            while (true)
            {
                if (current_index >= Len)//проверяем, не вышли ли за границу
                    break;
                //критическая_секция
                lock ("Critical")
                {
                    current_prime = basePrime[current_index];//берем текущее простое число
                    current_index++;
                }
                // Обработка текущего простого числа
                for (int i = (int)Math.Sqrt(N); i < N + 1; i++)
                    if (i % current_prime == 0)//если i делится нацело на current_prime, оно составное
                        Nums[i] = 1;
            }
        }

        static void Main(string[] args)
        {
            DateTime Start, End;
            Start = DateTime.Now;
            for (int i = 0; i < 10; i++)
            {
            //Task1();
            //Task2();
            //Task3();
            //Task4();
            Task5();
            }
            End = DateTime.Now;
            Console.WriteLine(((End - Start).TotalSeconds)/10);
           /* int cntV = 0, cntC = 0;
            //контрольная сумма значений чисел, контрольная сумма чисел
            for (int i = 0; i < N - 1; i++)
            {
                if (Nums[i] == 0)
                {
                    cntV += i;//1061
                    cntC++;//27
                }
            }
            Console.WriteLine(cntV.ToString() + " " + cntC.ToString());
            */
            Console.ReadKey();
        }
    }
}
