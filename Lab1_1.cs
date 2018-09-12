//Program.cs
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Lab1
{
    class Program
    {

        public static int Thmain;
        public static int maxParall = Environment.ProcessorCount;

        static void Run(int N, string[] files)
        {
            //Последовательный алгоритм
            using (StreamWriter stream = new StreamWriter(@"D:\OneDrive\Parall2\Output.txt", true))
            {
                stream.WriteLine("Последовательный алгоритм");
            }
               DateTime d1 = DateTime.Now;
               foreach (var file in files)
                   Schemas.MakeBlack_White(file);
               DateTime d2 = DateTime.Now;
            using (StreamWriter stream = new StreamWriter(@"D:\OneDrive\Parall2\Output.txt", true))
            {
                stream.WriteLine("\nОбработано изображений: {0}\n", N.ToString());
                stream.WriteLine("Затраченное время: {0} мс\n", (d2 - d1).TotalMilliseconds.ToString());
            }
            Console.WriteLine("Выполнено: Последовательный алгоритм");

            //параллельные версии

            var cts = new CancellationTokenSource();
            Thread thr = new Thread(() => {
                while (true)
                {
                    Thread.Sleep(100);
                    //Возвращает значение, указывающее на событие нажатия клавиши (во входном потоке).
                    if (Console.KeyAvailable)
                    {
                        Console.ReadKey(true);
                        //планировщик отменяет еще не запланированные действия
                        cts.Cancel();
                    }
                }
            });

            thr.Start();
                var Options = new ParallelOptions()
                {
                    MaxDegreeOfParallelism = maxParall,
                    //получаем сигнал отмены
                    CancellationToken = cts.Token
                };
            //parallelFor
            using (StreamWriter stream = new StreamWriter(@"D:\OneDrive\Parall2\Output.txt", true))
            {
                stream.WriteLine("\nПараллельный алгоритм");
            }
            Schemas.Info = new ConcurrentBag<Schemas.TaskDetails>();
            Parallel.For(0, N,Options, i =>
            {
                Schemas.Work(files[i]);

            });

            Schemas.PrintThreadsInfo();
            Schemas.PrintShemaInfo();

            Console.WriteLine("Выполнено: Параллельный алгоритм");
            //стандартная схема
            using (StreamWriter stream = new StreamWriter(@"D:\OneDrive\Parall2\Output.txt",true))
            {
                stream.WriteLine("\nСтандартная схема");
            }
                Schemas.Info = new ConcurrentBag<Schemas.TaskDetails>();
                Parallel.ForEach(files, Options, Schemas.Work);
                Schemas.PrintThreadsInfo();
            Schemas.PrintTaskInfo();
            Schemas.PrintShemaInfo();

            Console.WriteLine("Выполнено: Cтандартная схема");
            //сбалансированная схема
            using (StreamWriter stream = new StreamWriter(@"D:\OneDrive\Parall2\Output.txt", true))
            {
                stream.WriteLine("\nСбалансированная схема");
            }
                Schemas.Info = new ConcurrentBag<Schemas.TaskDetails>();
                var PartedData = Partitioner.Create(files, true);
                Parallel.ForEach(PartedData, Options, Schemas.Work);
                Schemas.PrintThreadsInfo();
            Schemas.PrintTaskInfo();
            Schemas.PrintShemaInfo();
            Console.WriteLine("Выполнено: Сбалансированная схема");
            //Статическая схема
            using (StreamWriter stream = new StreamWriter(@"D:\OneDrive\Parall2\Output.txt", true))
            {
                stream.WriteLine("\nСтатическая схема");
            }
                 Schemas.Info = new ConcurrentBag<Schemas.TaskDetails>();
                 Parallel.ForEach(Partitioner.Create(0, N), Options, range => {
                     for (int i = range.Item1; i < range.Item2; i++)
                         Schemas.Work(files[i]);
                 });
                Schemas.PrintThreadsInfo();
                Schemas.PrintTaskInfo();
                Schemas.PrintShemaInfo();

            Console.WriteLine("Выполнено: Статическая схема");
            //Статическая схема с фиксированным размером блока
            using (StreamWriter stream = new StreamWriter(@"D:\OneDrive\Parall2\Output.txt", true))
            {
                stream.WriteLine("\nСтатическая схема с фиксированным размером блока");
            }
                 Schemas.Info = new ConcurrentBag<Schemas.TaskDetails>();
                 Parallel.ForEach(Partitioner.Create(0, N, N/(maxParall)), Options,
                 range => {
                  for (int i = range.Item1; i < range.Item2; i++)
                  Schemas.Work(files[i]);
                  });
                Schemas.PrintThreadsInfo();
                Schemas.PrintTaskInfo();
                Schemas.PrintShemaInfo();

            Console.WriteLine("Выполнено: Статическая схема с фиксированным размером блока");
        }

        static void Main(string[] args)
        {
            Thmain = Thread.CurrentThread.ManagedThreadId;
            //получаем имена всех изображений
            var Allfiles = Directory.GetFiles(@"D:\OneDrive\Parall2\ImagesBefore", "*.j*");

            string[] files;
            try
            {
                files = new string[10];
                Array.Copy(Allfiles, files, 10);
                Run(10, files);
                files = new string[20];
                Array.Copy(Allfiles, files, 20);
                Run(20, files);
                files = new string[50];
                Array.Copy(Allfiles, files, 50);
                Run(50, files);
                Run(100, Allfiles);
                Console.WriteLine("Готово!");

            }
            catch (OperationCanceledException o)
            {
                Console.WriteLine(o.Message);
            }
            Console.ReadKey();
        }

    }
}

//Schemas.cs
using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lab1
{
    static class Schemas
    {
        public struct TaskDetails
        {
            //номер потока, задача
            public int Thread, Task;
            public DateTime start, end;
            public string fileName;


            public TaskDetails(int Thread, int Task, string fileName, DateTime start, DateTime end)
            {
                this.Thread = Thread;
                this.Task = Task;
                this.fileName = fileName;
                this.start = start;
                this.end = end;
            }
        }

        //для сбора информации о работе потоков
        public static ConcurrentBag<TaskDetails> Info;

        //вывод требуемой по заданию информации
        //задачи
        static public void PrintTaskInfo()
        {
            var table = Info.GroupBy(list => new { Task = list.Task, Thread = list.Thread })
                             .OrderBy(t=>t.Key.Task)
                             .Select(Sample => new { Task = Sample.Key.Task, Thread = Sample.Key.Thread, Images = Sample.Count(), start = Sample.Min(x => x.start), end = Sample.Max(x => x.end) });
            using (StreamWriter stream = new StreamWriter(@"D:\OneDrive\Parall2\Output.txt", true))
            {
                stream.WriteLine(string.Format("{0,-9}|{1,-15}|{2,-19}|{3,-19}|{4,-8}", "Задача, №", "Число элементов", "Время старта", "Время завершения", "Поток, №") +
    "\n=========|===============|===================|===================|========");
                foreach (var item in table)
                    stream.WriteLine(string.Format("{0,-9}|{1,-15}|{2,-19}|{3,-19}|{4,-8}", item.Task.ToString(), item.Images.ToString(), item.start.ToString(), item.end.ToString(), item.Thread.ToString()));
                stream.WriteLine();
            }
        }
        //информация по схемам
        static public void PrintShemaInfo()
        {


            var threads= Info.Select(t => t.Thread).Distinct().Count();
            var time = (Info.Max(t => t.end) - Info.Min(t => t.end)).TotalMilliseconds;
            var tasks= Info.Select(t => t.Task).Distinct().Count();
            using (StreamWriter stream = new StreamWriter(@"D:\OneDrive\Parall2\Output.txt", true))
            {
                stream.WriteLine(string.Format("{0,-16}|{1,-13}|{2,-11}", "Время работы, мс", "Число потоков", "Число задач") +
"\n================|=============|============");
                stream.WriteLine(string.Format("{0,-16}|{1,-13}|{2,-11}", time.ToString(), threads.ToString(), tasks.ToString()));
                stream.WriteLine();
            }
        }
        //информация по потокам
        static public void PrintThreadsInfo()
        {

            var threads = Info.GroupBy(t => t.Thread).OrderBy(t =>t.Key).Select(Sample => new { thread=Sample.Key, start = Sample.Min(x => x.start), end= Sample.Max(x => x.end),Images = Sample.Count() });
            //есть ли среди номеров потоков, номер главного?
            using (StreamWriter stream = new StreamWriter(@"D:\OneDrive\Parall2\Output.txt", true))
            {
                stream.WriteLine("\nОбработано изображений: {0}\n", Info.Count.ToString());
                stream.WriteLine("Главный поток, №: " + Program.Thmain.ToString());
                stream.WriteLine("Участие главного потока: {0}\n", Info.Select(t => t.Thread).Contains(Program.Thmain) ? "Да" : "Нет");
                stream.WriteLine(string.Format("{0,-8}|{1,-15}|{2,-19}|{3,-19}", "Поток, №", "Число элементов", "Время старта", "Время завершения") +
    "\n========|===============|===================|===================");
                foreach (var item in threads)
                    stream.WriteLine(string.Format("{0,-8}|{1,-15}|{2,-19}|{3,-19}", item.thread.ToString(), item.Images.ToString(), item.start.ToString(), item.end.ToString()));
                stream.WriteLine();
            }
        }


        static public void MakeBlack_White(string file)
        {
            //создаём Bitmap из исходного изображения
            var input = new Bitmap(file);
            // создаём Bitmap для черно-белого изображения
            var output = new Bitmap(input.Width, input.Height);
            // перебираем в циклах все пиксели исходного изображения
            for (int x = 0; x < input.Width; x++)
                for (int y = 0; y < input.Height; y++)
                {
                    // получаем (i, j) пиксель
                    var pixel = input.GetPixel(x, y);
                    // получаем компоненты цветов пикселя
                    int R = pixel.R;// красный
                    int G = pixel.G;// зеленый
                    int B = pixel.B;// синий
                    // делаем цвет черно-белым (оттенки серого) - находим среднее арифметическое
                    R = G = B = (R + G + B) / 3;
                    // добавляем его в Bitmap нового изображения
                    output.SetPixel(x, y, Color.FromArgb(R, G, B));
                }
            output.Save(file.Replace("ImagesBefore", "ImagesAfter"));
        }


        public static void Work(string file)
        {
            DateTime start, end;

            start = DateTime.Now;

            //узнать id потока и id задачи
            var thrId = Thread.CurrentThread.ManagedThreadId;
            var taskId = Task.CurrentId.Value;//без value вылазит ошибка в конструкторе
            MakeBlack_White(file);
            end = DateTime.Now;
            //сохраняем данные о работе
            Info.Add(new TaskDetails(thrId, taskId, file, start, end));
        }

    }
}
