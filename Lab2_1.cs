//Form1.cs
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab2
{
    public partial class Form1 : Form
    {
        //здесь файлы для обработки
        static string[] files;
        double time=-1;
        //сколько раз встречается каждое слово
        static Dictionary<string, int> words;
        static Dictionary<string, int> top10;

        static ConcurrentDictionary<string, int> parwords;
        static ConcurrentDictionary<string, int> ptop10;

        static Dictionary<int, int> length;
        static ConcurrentDictionary<int, int> parlength;


        public Form1()
        {
            InitializeComponent();
            //откл кнопок при запуске формы
            Sequentilal.Enabled = false;
            LINQ.Enabled = false;
            ParallelFor.Enabled = false;
            PLINQ.Enabled = false;
            start.Enabled = false;
            Cancel.Enabled = false;
            TaskA.Enabled = false;
            TaskB.Enabled = false;
            TaskC.Enabled = false;
            //работа воркера
            BW.DoWork += do_work;
            //по завершению работы
            BW.RunWorkerCompleted += RunWorkerCompleted;
        }

        private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //вывод информации в форму
            //если ошибка
            if (e.Error != null)
            {
                MessageBox.Show(string.Format("Ошибка: {0}", e.Error.Message));
                OK.Enabled = true;
                Cancel.Enabled = false;
            }
            else if (e.Cancelled)
            {
                MessageBox.Show("Действие отменено");
                Cancel.Enabled = false;
                time = -1;
            }

            else if (Sequentilal.Checked || LINQ.Checked || PLINQ.Checked)
                if (TaskA.Checked)
                {
                    Controls.Add(dataGridView);
                    dataGridView.DataSource = words.ToArray();
                }
                else if (TaskB.Checked)
                {
                    Controls.Add(dataGridView);
                    dataGridView.DataSource = top10.ToArray();
                }
                else
                {
                    Controls.Add(dataGridView);
                    Controls.Add(chart1);
                    dataGridView.DataSource = length.ToArray();
                    chart1.DataSource = length;
                    chart1.Series[0].XValueMember = "Key";
                    chart1.Series[0].YValueMembers = "Value";
                    chart1.ChartAreas[0].AxisX.Interval = 1;
                    chart1.DataBind();
                }
            else if (ParallelFor.Checked)
                if (TaskA.Checked)
                {
                    Controls.Add(dataGridView);
                    dataGridView.DataSource = parwords.ToArray();
                }

                else if (TaskB.Checked)
                {
                    Controls.Add(dataGridView);
                    dataGridView.DataSource = ptop10.ToArray();
                }
                    else
                {
                    Controls.Add(dataGridView);
                    Controls.Add(chart1);
                    dataGridView.DataSource = parlength.ToArray();
                    chart1.DataSource = parlength;
                    chart1.Series[0].XValueMember = "Key";
                    chart1.Series[0].YValueMembers = "Value";
                    chart1.ChartAreas[0].AxisX.Interval = 1;
                    chart1.DataBind();
                }

            //включаем отрубленные кнопки
            if (time != -1)
            {
                Time.Text = time.ToString();
                cnt.Text = dataGridView.RowCount.ToString();
            }
            OK.Enabled = true;
            //else if (ParallelFor.Checked)
            if (files != null)
            {
                Sequentilal.Enabled = true;
                LINQ.Enabled = true;
                ParallelFor.Enabled = true;
                PLINQ.Enabled = true;
                start.Enabled = true;
                TaskA.Enabled = true;
                TaskB.Enabled = true;
                TaskC.Enabled = true;
                Cancel.Enabled = false;
            }
   
        }

        private void do_work(object sender, DoWorkEventArgs e)
        {
            string action = e.Argument.ToString();
            //разбираем, какую процедуру вызывать
            if (action == "OK")
                files = Directory.GetFiles(path.Text, "*.txt");
            else
            {
                if (Sequentilal.Checked)
                    if (TaskA.Checked)
                        seqA();
                    else if
                        (TaskB.Checked)
                        seqB();
                    else seqC();
                else if (LINQ.Checked)
                    if (TaskA.Checked)
                        LINQA();
                    else if (TaskB.Checked)
                        LINQB();
                    else LINQC();
                else if (ParallelFor.Checked)
                    if (TaskA.Checked)
                        PFA();
                    else if
                        (TaskB.Checked)
                        PFB();
                    else PFC();
                else
                    if (TaskA.Checked)
                    PLINQA();
                else if (TaskB.Checked)
                    PLINQB();
                else PLINQC();
            }
            if (BW.CancellationPending)
            {
                e.Cancel = true;
                return;
            }
        }

        private void PLINQC()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            length = new Dictionary<int, int>();
            length = files.AsParallel().SelectMany(f => File.ReadAllText(f).Split(new char[] { ' ', ',', '.', '?', '!', '-', '"', ':', '\t', '/', '\\', '(', ')', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                        .GroupBy(s => s.Length).ToDictionary(s => s.Key, s => s.Count());
            sw.Stop();
            time = sw.ElapsedMilliseconds;
        }
        private void PLINQB()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            top10 = files.AsParallel().SelectMany(f => File.ReadAllText(f).Split(new char[] { ' ', ',', '.', '?', '!', '-', '"', ':', '\t', '/', '\\', '(', ')', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                        .GroupBy(s => s).ToDictionary(s => s.Key, s => s.Count())
                        .OrderByDescending(pair => pair.Value).Take(10).ToDictionary(pair => pair.Key, pair => pair.Value);
            sw.Stop();
            time = sw.ElapsedMilliseconds;
        }

        private void PLINQA()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            words = new Dictionary<string, int>();
            words = files.AsParallel().SelectMany(f => File.ReadAllText(f).Split(new char[] { ' ', ',', '.', '?', '!', '-', '"', ':', '\t', '/', '\\', '(', ')', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                        .GroupBy(s => s).ToDictionary(s => s.Key, s => s.Count());
            sw.Stop();
            time = sw.ElapsedMilliseconds;
        }

        private void PFA()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            parwords = new ConcurrentDictionary<string, int>();
            Parallel.ForEach(files, (fname) =>
            {
                string line;
                using (StreamReader R = new StreamReader(fname))
                {
                    while ((line = R.ReadLine()) != null)
                    {
                        string[] ArrWords = line.Split(new char[] { ' ', ',', '.', '?', '!', '-', '"', ':', '\t', '/', '\\', '(', ')', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var word in ArrWords)
                        {
                            parwords.AddOrUpdate(word, 1, (key, val) => val + 1);
                            if (BW.CancellationPending) return;
                        }
                    }
                }
            });
            sw.Stop();
            time = sw.ElapsedMilliseconds;
        }

        private void PFB()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            PFA();
            ptop10 = new ConcurrentDictionary<string, int>();

            var myList = parwords.ToList();
            myList.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));
            int counter = 0;

            Parallel.ForEach(myList, (kvp) =>
                 {
                     if (counter >= 10)return;
                     ptop10.AddOrUpdate(kvp.Key, kvp.Value,(k,v)=>kvp.Value);
                     Interlocked.Add(ref counter, 1);
                 }
                );

            sw.Stop();
            time = sw.ElapsedMilliseconds;

        }

        private void PFC()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            parlength = new ConcurrentDictionary<int, int>();
            Parallel.ForEach(files, (fname) =>
            {
                    string line;
                    using (StreamReader R = new StreamReader(fname))
                    {
                        while ((line = R.ReadLine()) != null)
                        {
                            string[] ArrWords = line.Split(new char[] { ' ', ',', '.', '?', '!', '-', '"', ':', '\t', '/', '\\', '(', ')', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var word in ArrWords)
                            {
                            parlength.AddOrUpdate(word.Length,1,(key,val)=>val+1);
                                if (BW.CancellationPending) return;
                            }
                        }
                    }
            });
            
            sw.Stop();
            time = sw.ElapsedMilliseconds;
        }

        private void LINQC()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            length = new Dictionary<int, int>();
            length = files.SelectMany(f => File.ReadAllText(f).Split(new char[] { ' ', ',', '.', '?', '!', '-', '"', ':', '\t', '/', '\\', '(', ')', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                        .GroupBy(s => s.Length).ToDictionary(s => s.Key, s => s.Count());
            sw.Stop();
            time = sw.ElapsedMilliseconds;
        }

        private void LINQB()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            top10 = files.SelectMany(f => File.ReadAllText(f).Split(new char[] { ' ', ',', '.', '?', '!', '-', '"', ':', '\t', '/', '\\', '(', ')', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                        .GroupBy(s => s).ToDictionary(s => s.Key, s => s.Count())
                        .OrderByDescending(pair=>pair.Value).Take(10).ToDictionary(pair => pair.Key, pair => pair.Value);
            sw.Stop();
            time = sw.ElapsedMilliseconds;
        }

        private void LINQA()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            words = new Dictionary<string, int>();
            words = files.SelectMany(f => File.ReadAllText(f).Split(new char[] { ' ', ',', '.', '?', '!', '-', '"', ':', '\t', '/', '\\', '(', ')', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                        .GroupBy(s=>s).ToDictionary(s=>s.Key,s=>s.Count());
            sw.Stop();
            time = sw.ElapsedMilliseconds;
        }

        void seqA()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            words = new Dictionary<string, int>();
            foreach (var fname in files)
            {
                string line;
                using (StreamReader R = new StreamReader(fname))
                {
                    while ((line = R.ReadLine()) != null)
                    {
                        string[] ArrWords = line.Split(new char[] { ' ', ',', '.', '?', '!', '-', '"', ':', '\t', '/', '\\', '(', ')','\r','\n' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var word in ArrWords)
                        {
                            //есть в словаре такое слово? увеличиваем счетчик
                            if (words.ContainsKey(word)) words[word]++;
                            //нет такого слова? добавляем
                            else words.Add(word, 1);
                            if (BW.CancellationPending) return;
                        }
                    }
                }
            }
            sw.Stop();
            time = sw.ElapsedMilliseconds;
        }

        void seqB()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            seqA();
            top10 = new Dictionary<string, int>();
            //создали список
            var myList = words.ToList();
            myList.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));
            int counter = 0;
            foreach (var kvp in myList)
            {
                if (counter >= 10)  break; 
                else top10.Add(kvp.Key, kvp.Value);
                counter++;
            }
            sw.Stop();
            time = sw.ElapsedMilliseconds;
        }


        void seqC()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            length = new Dictionary<int, int>();
            foreach (var fname in files)
            {
                string line;
                using (StreamReader R = new StreamReader(fname))
                {
                    while ((line = R.ReadLine()) != null)
                    {
                        string[] ArrWords = line.Split(new char[]  { ' ', ',', '.', '?', '!', '-', '"', ':', '\t', '/', '\\', '(', ')', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var word in ArrWords)
                        {
                            //есть в словаре такое слово? увеличиваем счетчик
                            if (length.ContainsKey(word.Length)) length[word.Length]++;
                            //нет такого слова? добавляем
                            else length.Add(word.Length, 1);
                            if (BW.CancellationPending) return;
                        }
                    }
                }
            }
            sw.Stop();
            time = sw.ElapsedMilliseconds;
        }

        private void start_Click(object sender, EventArgs e)
        {
            //выкл кнопку
            Cancel.Enabled = true;
            start.Enabled = false;
            Controls.Remove(chart1);
            Controls.Remove(dataGridView);
            BW.RunWorkerAsync("START");//запуск кода в обработчике do_work, выполняется в отдельном потоке
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            //отмена фоновой операции
            BW.CancelAsync();
        }

        private void OK_Click(object sender, EventArgs e)
        {
            OK.Enabled = false;
            Controls.Remove(chart1);
            Controls.Remove(dataGridView);
            BW.RunWorkerAsync("OK");
        }
    }
}

//Программа для оценки PLINQ
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace Lab2_console
{
    class Program
    {
        static string[] files;
        static Dictionary<string, int> words;
        static Dictionary<string, int> top10;
        static Dictionary<int, int> length;
        static ConcurrentBag<int> thrs;

        static void Main(string[] args)
        {
            files = Directory.GetFiles(@"C:\Users\Alenka\OneDrive\Parall2\MyTexts", "*.txt");
            double time;
            Stopwatch sw = new Stopwatch();
            ////
            Console.WriteLine("Автобуфферизация:");
            ////
            thrs = new ConcurrentBag<int>();
            words = new Dictionary<string, int>();
            sw.Start();
            words = files.AsParallel().SelectMany(  f => {
                thrs.Add(Thread.CurrentThread.ManagedThreadId);
                return File.ReadAllText(f)
                .Split(new char[] { ' ', ',', '.', '?', '!', '-', '"', ':', '\t', '/', '\\', '(', ')', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries); ;})
                .GroupBy(s => s).ToDictionary(s => s.Key, s => s.Count());
            sw.Stop();
            time = sw.ElapsedMilliseconds;
            Console.WriteLine("TaskA:\n Время {0} мс, число потоков {1}",time.ToString(),thrs.Distinct().Count());
            ////
            thrs = new ConcurrentBag<int>();
            top10 = new Dictionary<string, int>();
            sw.Restart();
            top10 = files.AsParallel().SelectMany(f => {
                thrs.Add(Thread.CurrentThread.ManagedThreadId);
                return File.ReadAllText(f)
                .Split(new char[] { ' ', ',', '.', '?', '!', '-', '"', ':', '\t', '/', '\\', '(', ')', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries); ;
            })
                .GroupBy(s => s).ToDictionary(s => s.Key, s => s.Count())
                .OrderByDescending(pair => pair.Value).Take(10).ToDictionary(pair => pair.Key, pair => pair.Value); ;
            sw.Stop();
            time = sw.ElapsedMilliseconds;
            Console.WriteLine("TaskB:\n Время {0} мс, число потоков {1}", time.ToString(), thrs.Distinct().Count());
            /////
            thrs = new ConcurrentBag<int>();
            length = new Dictionary<int, int>();
            sw.Restart();
            length = files.AsParallel().SelectMany(f =>
            {
                thrs.Add(Thread.CurrentThread.ManagedThreadId);
                return File.ReadAllText(f)
                .Split(new char[] { ' ', ',', '.', '?', '!', '-', '"', ':', '\t', '/', '\\', '(', ')', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries); ;
            })
               .GroupBy(s => s.Length).ToDictionary(s => s.Key, s => s.Count());
            sw.Stop();
            Console.WriteLine("TaskC:\n Время {0} мс, число потоков {1}", time.ToString(), thrs.Distinct().Count());
            /////
            


            Console.WriteLine("Полная буфферизация:");
            ////
            thrs = new ConcurrentBag<int>();
            words = new Dictionary<string, int>();
            sw.Restart();
            words = files.AsParallel().WithMergeOptions(ParallelMergeOptions.FullyBuffered).SelectMany(f => {
                thrs.Add(Thread.CurrentThread.ManagedThreadId);
                return File.ReadAllText(f)
                .Split(new char[] { ' ', ',', '.', '?', '!', '-', '"', ':', '\t', '/', '\\', '(', ')', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries); ;
            })
                .GroupBy(s => s).ToDictionary(s => s.Key, s => s.Count());
            sw.Stop();
            time = sw.ElapsedMilliseconds;
            Console.WriteLine("TaskA:\n Время {0} мс, число потоков {1}", time.ToString(), thrs.Distinct().Count());
            ////
            thrs = new ConcurrentBag<int>();
            top10 = new Dictionary<string, int>();
            sw.Restart();
            top10 = files.AsParallel().WithMergeOptions(ParallelMergeOptions.FullyBuffered).SelectMany(f => {
                thrs.Add(Thread.CurrentThread.ManagedThreadId);
                return File.ReadAllText(f)
                .Split(new char[] { ' ', ',', '.', '?', '!', '-', '"', ':', '\t', '/', '\\', '(', ')', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries); ;
            })
                .GroupBy(s => s).ToDictionary(s => s.Key, s => s.Count())
                .OrderByDescending(pair => pair.Value).Take(10).ToDictionary(pair => pair.Key, pair => pair.Value); ;
            sw.Stop();
            time = sw.ElapsedMilliseconds;
            Console.WriteLine("TaskB:\n Время {0} мс, число потоков {1}", time.ToString(), thrs.Distinct().Count());
            /////
            thrs = new ConcurrentBag<int>();
            length = new Dictionary<int, int>();
            sw.Restart();
            length = files.AsParallel().WithMergeOptions(ParallelMergeOptions.FullyBuffered).SelectMany(f =>
            {
                thrs.Add(Thread.CurrentThread.ManagedThreadId);
                return File.ReadAllText(f)
                .Split(new char[] { ' ', ',', '.', '?', '!', '-', '"', ':', '\t', '/', '\\', '(', ')', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries); ;
            })
               .GroupBy(s => s.Length).ToDictionary(s => s.Key, s => s.Count());
            sw.Stop();
            Console.WriteLine("TaskC:\n Время {0} мс, число потоков {1}", time.ToString(), thrs.Distinct().Count());
            /////



            Console.WriteLine("Без буфферизации:");
            ////
            thrs = new ConcurrentBag<int>();
            words = new Dictionary<string, int>();
            sw.Restart();
            words = files.AsParallel().WithMergeOptions(ParallelMergeOptions.NotBuffered).SelectMany(f => {
                thrs.Add(Thread.CurrentThread.ManagedThreadId);
                return File.ReadAllText(f)
                .Split(new char[] { ' ', ',', '.', '?', '!', '-', '"', ':', '\t', '/', '\\', '(', ')', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries); ;
            })
                .GroupBy(s => s).ToDictionary(s => s.Key, s => s.Count());
            sw.Stop();
            time = sw.ElapsedMilliseconds;
            Console.WriteLine("TaskA:\n Время {0} мс, число потоков {1}", time.ToString(), thrs.Distinct().Count());
            ////
            thrs = new ConcurrentBag<int>();
            top10 = new Dictionary<string, int>();
            sw.Restart();
            top10 = files.AsParallel().WithMergeOptions(ParallelMergeOptions.NotBuffered).SelectMany(f => {
                thrs.Add(Thread.CurrentThread.ManagedThreadId);
                return File.ReadAllText(f)
                .Split(new char[] { ' ', ',', '.', '?', '!', '-', '"', ':', '\t', '/', '\\', '(', ')', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries); ;
            })
                .GroupBy(s => s).ToDictionary(s => s.Key, s => s.Count())
                .OrderByDescending(pair => pair.Value).Take(10).ToDictionary(pair => pair.Key, pair => pair.Value); ;
            sw.Stop();
            time = sw.ElapsedMilliseconds;
            Console.WriteLine("TaskB:\n Время {0} мс, число потоков {1}", time.ToString(), thrs.Distinct().Count());
            /////
            thrs = new ConcurrentBag<int>();
            length = new Dictionary<int, int>();
            sw.Restart();
            length = files.AsParallel().WithMergeOptions(ParallelMergeOptions.NotBuffered).SelectMany(f =>
            {
                thrs.Add(Thread.CurrentThread.ManagedThreadId);
                return File.ReadAllText(f)
                .Split(new char[] { ' ', ',', '.', '?', '!', '-', '"', ':', '\t', '/', '\\', '(', ')', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries); ;
            })
               .GroupBy(s => s.Length).ToDictionary(s => s.Key, s => s.Count());
            sw.Stop();
            Console.WriteLine("TaskC:\n Время {0} мс, число потоков {1}", time.ToString(), thrs.Distinct().Count());
            /////
            Console.ReadKey();
        }
    }
}
