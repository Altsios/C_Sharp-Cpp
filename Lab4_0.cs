//PROGRAM
using System;

namespace Lab4PP
{
    class Program
    {
        static void Main(string[] args)
        {
            //последовательный алгоритм(Алгоритм 1.0)
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("                               Последовательный алгоритм");
            Console.ResetColor();
            Console.WriteLine("-------------------------------------------------------------------------------------");
            Sequential.Start();
            //Декомпозиция по файлам
            //параллельный алгоритм(Алгоритм 1.1-локальный буффер)
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("                           Параллельный алгоритм(Локальный буффер)");
            Console.ResetColor();
            Console.WriteLine("-------------------------------------------------------------------------------------");
            Local thr2 = new Local(2);
            thr2.Start();
            Local thr4 = new Local(4);
            thr4.Start();
            Local thr6 = new Local(6);
            thr6.Start();
            Local thr8 = new Local(8);
            thr8.Start();
            Local thr10 = new Local(10);
            thr10.Start();
            Local thr12 = new Local(12);
            thr12.Start();
            //параллельный алгоритм(Алгоритм 1.2-глобальный буффер)
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("                           Параллельный алгоритм(Глобальный буффер)");
            Console.ResetColor();
            Console.WriteLine("-------------------------------------------------------------------------------------");
            Global dthr2 = new Global(2);
            dthr2.Start();
            Global dthr4 = new Global(4);
            dthr4.Start();
            Global dthr6 = new Global(6);
            dthr6.Start();
            Global dthr8 = new Global(8);
            dthr8.Start();
            Global dthr10 = new Global(10);
            dthr10.Start();
            Global dthr12 = new Global(12);
            dthr12.Start();
            //Декомпозиция по задачам
            //параллельный алгоритм(Алгоритм 2.0)
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("                        Параллельный алгоритм(Декомпозиция по задачам)");
            Console.ResetColor();
            Console.WriteLine("-------------------------------------------------------------------------------------");
            Decomposition dec2 = new Decomposition(1,1);
            dec2.Start();
            Decomposition dec4 = new Decomposition(2,2);
            dec4.Start();
            Decomposition dec6 = new Decomposition(3,3);
            dec6.Start();
            Decomposition dec8 = new Decomposition(4, 4);
            dec8.Start();
            Decomposition dec10 = new Decomposition(5, 5);
            dec10.Start();
            Decomposition dec12 = new Decomposition(6, 6);
            dec12.Start();
            Console.ReadKey();
        }
    }
}

//SEQUENTIAL
using System;
using System.Collections.Generic;
using System.IO;

namespace Lab4PP
{
    class Sequential
    {
        //Словарь-слова. <слово,количество>
        static Dictionary<string, int> Words = new Dictionary<string, int>();
        //Словарь-символ. <символ,количество>
        static Dictionary<char, int> Symbols = new Dictionary<char, int>();
        //Количество цифр
        static Dictionary<int, int> Dig = new Dictionary<int, int>();
        //количество вхождений данного слова в текст
        static int cnt;
        static int cnt2;

        public static void Start()
        {
            DateTime dt, dt2;
            dt = DateTime.Now;
            //массив, содержащий имена всех текстовых файлов с указанием пути к ним в указанной директории
            string[] files = Directory.GetFiles(@"C:\Users\Аленка\Documents\Texts_lab_4");
            string line;
            int res;
            foreach (var fname in files)
            {

         
            StreamReader R = new StreamReader(fname);
            while ((line = R.ReadLine()) != null)
            {
                //записываем в массив все слова в строчке, разделенные знаками удаляя строки из 2+ пробелов также(с ' ' игнор лишь 1го пробела).
                string[] ArrWords = line.Split(new char[] { ' ', ',', '.', '?', '!', '-', '"', ':', '\t', '/', '\\', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
            //переводим строчку в массив char-ов
                    char[] ArrSymbols = line.ToCharArray();
                    foreach (char ch in ArrSymbols)
                    {
                        //есть в словаре такой символ? увеличиваем счетчик
                        if (Symbols.ContainsKey(ch)) Symbols[ch]++;
                        //нет такого символа? добавляем
                        else Symbols.Add(ch, 1);
                    }
                    foreach (var word in ArrWords)
                    {
                        //есть в словаре такое слово? увеличиваем счетчик
                        if (Words.ContainsKey(word))Words[word]++;
                        //нет такого слова? добавляем
                        else Words.Add(word, 1);
                        // Кол-во цифр
                        bool isInt = int.TryParse(word, out res);
                        if(isInt)
                        if (Dig.ContainsKey(res))

                            Dig[res]++;

                        else
                            Dig.Add(res, 1);
                    }
            }
            R.Close();
            }
            dt2 = DateTime.Now;
            foreach (var word in Words)
            {
                cnt2 += word.Value;
            }
            Console.Write("Уникальных слов " + Words.Count + ", общее количество слов: " + cnt2);
            cnt2 = 0;
            foreach (var ch in Symbols)
            {
                cnt2 += ch.Value;
            }
            foreach (var dig in Dig)
            {
                cnt += dig.Value;
           }


            Console.WriteLine("     \t|Время = " + (dt2 - dt).TotalMilliseconds+
                "мс\nУникальных символов: " + Symbols.Count + ", общее количество символов: " + cnt2 +
                "\t|\nРазличных цифр: "  + Dig.Count + ", общее количество цифр: " + cnt+
                "             \t|\n-------------------------------------------------------------------------------------\n");
        }

    }
}

//LOCAL

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Lab4PP
{
    class Local
    {
        //конкурентные коллекции- ConcurrentBag. Потокобезопасная неупорядоченная коллекция
        //Преимущество использования ConcurrentBag<T> по сравнению с ConcurrentQueue<T> и ConcurrentStack<T> состоит в том,
        //что при вызове метода Add из нескольких потоков одновременно не происходит практически никакой конкуренции.
        ConcurrentBag<Dictionary<string, int>> ConcWords = new ConcurrentBag<Dictionary<string, int>>();
        ConcurrentBag<Dictionary<char, int>> ConcSymbols = new ConcurrentBag<Dictionary<char, int>>();
        ConcurrentBag<Dictionary<int, int>> ConcDig = new ConcurrentBag<Dictionary<int, int>>();
        //обычные словари, объединяющие результаты поиска
        //Словарь-слова. <слово,количество>
        Dictionary<string, int> CntWords = new Dictionary<string, int>();
        //Словарь-символ. <символ,количество>
        Dictionary<char, int> CntSymbols = new Dictionary<char, int>();
        //Количество цифр
        Dictionary<int, int> CntDig = new Dictionary<int, int>();
        string[] files;
        //количество вхождений данного слова в текст
        int cnt;
        int cnt2;
        //кол-во потоков
        int M;

        //конструктор
        public Local(int m)
        {
            M = m;
        }

        public void Start()
        {
            Console.WriteLine("Число потоков: "+M+"\n");
            DateTime dt, dt2;
            int FilesCnt;
            dt = DateTime.Now;
            //массив, содержащий имена всех текстовых файлов с указанием пути к ним в указанной директории
            files = Directory.GetFiles(@"C:\Users\Аленка\Documents\Texts_lab_4");
            //количество файлов. будем по нему распределять читателей
            FilesCnt = files.Length;
            Thread[] thrs = new Thread[M];
            //инициализируем каждый поток в массиве
            for (int i = 0; i < M; i++)
            {
                //локальный индекс. иначе i-ссылка. когда поток начнет работу, i убежит далеко
                int thr = i;
                thrs[i] = new Thread(() =>
                {
                    //Словарь-слова. <слово,количество>
                    Dictionary<string, int> Words = new Dictionary<string, int>();
                    Dictionary<char, int> Symbols = new Dictionary<char, int>();
                    Dictionary<int, int> Dig = new Dictionary<int, int>();
                    string line;
                    int res;
                    for (int j = thr; j < FilesCnt; j += M)//выдаем определенный файл
                    {
                        StreamReader reader = new StreamReader(files[j]);
                        while ((line = reader.ReadLine()) != null)
                        {
                            //записываем в массив все слова в строчке, разделенные знаками удаляя строки из 2+ пробелов также(с ' ' игнор лишь 1го пробела).
                            string[] ArrWords = line.Split(new char[] { ' ', ',', '.', '?', '!', '-', '"', ':', '\t', '/', '\\', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                            char[] ArrSymbols = line.ToCharArray();
                            foreach (char ch in ArrSymbols)
                            {
                                //есть в словаре такой символ? увеличиваем счетчик
                                if (Symbols.ContainsKey(ch)) Symbols[ch]++;
                                //нет такого символа? добавляем
                                else Symbols.Add(ch, 1);
                            }
                            foreach (var word in ArrWords)
                            {
                                //есть в словаре такое слово? увеличиваем счетчик
                                if (Words.ContainsKey(word)) Words[word]++;
                                //нет такого слова? добавляем
                                else Words.Add(word, 1);
                                // Кол-во цифр
                                bool isInt = int.TryParse(word, out res);
                                if (isInt)
                                    if (Dig.ContainsKey(res))

                                        Dig[res]++;

                                    else
                                        Dig.Add(res, 1);
                            }
                        }
                    }
                    //добавляем в общую коллекцию
                    ConcWords.Add(Words);
                    //добавляем в общую коллекцию
                    ConcSymbols.Add(Symbols);
                    //добавляем в общую коллекцию
                    ConcDig.Add(Dig);
                });
                thrs[i].Start();
            }
            //останавливаем потоки
            for (int i = 0; i < M; i++) thrs[i].Join();
            dt2 = DateTime.Now;
            foreach (var dict in ConcWords)
            {
                foreach (var file in dict)
                {
                    if (CntWords.ContainsKey(file.Key)) CntWords[file.Key]+=file.Value;
                    //нет такого слова? добавляем
                    else
                        CntWords.Add(file.Key, file.Value);
                }
            }
  
            foreach (var dict in ConcSymbols)
            {
                foreach (var file in dict)
                {
                    if (CntSymbols.ContainsKey(file.Key)) CntSymbols[file.Key] += file.Value;
                    //нет такого символа? добавляем
                    else
                        CntSymbols.Add(file.Key, file.Value);
                }
            }

            foreach (var dict in ConcDig)
            {
                foreach (var file in dict)
                {
                    if (CntDig.ContainsKey(file.Key)) CntDig[file.Key] += file.Value;
                    //нет такого символа? добавляем
                    else
                        CntDig.Add(file.Key, file.Value);
                }
            }

            foreach (var word in CntWords)
            {
                cnt2 += word.Value;
            }
            Console.Write("Уникальных слов " + CntWords.Count + ", общее количество слов: " + cnt2);
            cnt2 = 0;
            foreach (var ch in CntSymbols)
            {
                cnt2 += ch.Value;
            }
            cnt = 0;
            foreach (var dig in CntDig)
            {
                cnt += dig.Value;
            }
            Console.WriteLine("     \t|Время = " + (dt2 - dt).TotalMilliseconds +
                "мс\nУникальных символов: " + CntSymbols.Count + ", общее количество символов: " + cnt2 +
                "\t|\nРазличных цифр: " + ConcDig.Count + ", общее количество цифр: " + cnt +
                "             \t|\n-------------------------------------------------------------------------------------\n");
        }
    }
}

//GLOBAL

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lab4PP
{
    class Global
    {
        //конкурентные коллекции- ConcurrentDictionary. Потокобезопасный словарь
         ConcurrentDictionary<string, int> Words = new ConcurrentDictionary<string, int>();
         ConcurrentDictionary<char, int> Symbols = new ConcurrentDictionary<char, int> ();
         ConcurrentDictionary<int, int> Dig = new ConcurrentDictionary<int, int>();
         string[] files;
        //искомое слово
         string SearchedWorld = "она";
        //количество вхождений данного слова в текст
         int cnt;
         int cnt2;
        int FilesCnt;
        //кол-во потоков
        int M;

        //конструктор
        public Global(int m)
        {
            M = m;
        }

        void Job(object obj)
        {
            int thr = (int)obj;
            string line;
            int res;
            for (int i = thr; i < FilesCnt; i += M)//выдаем определенный файл
            {
                StreamReader reader = new StreamReader(files[i]);
                while ((line = reader.ReadLine()) != null)
                {
                    //записываем в массив все слова в строчке, разделенные знаками удаляя строки из 2+ пробелов также(с ' ' игнор лишь 1го пробела).
                    string[] ArrWords = line.Split(new char[] { ' ', ',', '.', '?', '!', '-', '"', ':', '\t', '/', '\\', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                    //переводим строчку в массив char-ов
                    char[] ArrSymbols = line.ToCharArray();
                    foreach (char ch in ArrSymbols)
                        Symbols.AddOrUpdate(ch, 1, (k, v) => v + 1);
                    foreach (var word in ArrWords)
                    {
                        Words.AddOrUpdate(word, 1, (k, v) => v + 1);
                        // Кол-во цифр
                        bool isInt = int.TryParse(word, out res);
                        if (isInt)
                            Dig.AddOrUpdate(res, 1, (k, v) => v + 1);
                    }
                }
            }
        }

        public  void Start()
        {
            Console.WriteLine("Число потоков: " + M + "\n");
            DateTime dt, dt2;
            dt = DateTime.Now;
            //массив, содержащий имена всех текстовых файлов с указанием пути к ним в указанной директории
            files = Directory.GetFiles(@"C:\Users\Аленка\Documents\Texts_lab_4");
            //количество файлов. будем по нему распределять читателей
            FilesCnt = files.Length;
            Thread[] thrs = new Thread[M];
            //инициализируем каждый поток в массиве

            {
                thrs[i] = new Thread(Job);
                thrs[i].Start(i);
            }
            //останавливаем потоки
            for (int i = 0; i < M; i++) thrs[i].Join();
            dt2 = DateTime.Now;
     
            foreach (var word in Words)
            {
                cnt2 += word.Value;
            }
            Console.Write("Уникальных слов " + Words.Count + ", общее количество слов: " + cnt2);
            cnt2 = 0;
            foreach (var ch in Symbols)
            {
                cnt2 += ch.Value;
            }
            cnt = 0;
            foreach (var dig in Dig)
            {
                cnt += dig.Value;
            }
            Console.WriteLine("     \t|Время = " + (dt2 - dt).TotalMilliseconds +
                 "мс\nУникальных символов: " + Symbols.Count + ", общее количество символов: " + cnt2 +
                 "\t|\nРазличных цифр: " + Dig.Count + ", общее количество цифр: " + cnt +
                 "             \t|\n-------------------------------------------------------------------------------------\n");
        }
    }
}

//DECOMPOSITION

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;

namespace Lab4PP
{
    class Decomposition
    {
        BlockingCollection<string> Lines = new BlockingCollection<string>();
        //конкурентные коллекции- ConcurrentDictionary. Потокобезопасный словарь
        ConcurrentDictionary<string, int> Words = new ConcurrentDictionary<string, int>();
        ConcurrentDictionary<char, int> Symbols = new ConcurrentDictionary<char, int>();
        ConcurrentDictionary<int, int> Dig = new ConcurrentDictionary<int, int>();
        string[] files;
        //количество вхождений данного слова в текст
        int cnt2;
        int FilesCnt;
        int cnt = 0;
        //потоки
        int N;
        int M;

        //конструктор
        public Decomposition(int n,int m)
        {
            N = n;
            M = m;
        }

        //обработка
        void WordReader()
        {
            string line;
            int res;
            while (!Lines.IsCompleted)
            {
                if (Lines.TryTake(out line,-1))//-1-неограниченное время ожидания
                {
                    //записываем в массив все слова в строчке, разделенные знаками удаляя строки из 2+ пробелов также(с ' ' игнор лишь 1го пробела).
                    string[] ArrWords= line.Split(new char[] { ' ', ',', '.', '?', '!', '-', '"', ':', '\t', '/', '\\', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                    //переводим строчку в массив char-ов
                    char[] ArrSymbols = line.ToCharArray();
                    foreach (char ch in ArrSymbols)
                        Symbols.AddOrUpdate(ch, 1, (k, v) => v + 1);
                    foreach (var word in ArrWords)
                    {
                        Words.AddOrUpdate(word, 1, (k, v) => v + 1);
                        // Кол-во цифр
                        bool isInt = int.TryParse(word, out res);
                        if (isInt)
                            Dig.AddOrUpdate(res, 1, (k, v) => v + 1);
                    }
                  
                }
            }
        }


        //записываем в общую коллекцию
        void ToQueue(object obj)
        {
            //узнаем номер потока
            int thr = (int)obj;
            for (int i = thr; i < FilesCnt; i+=N)//выдаем определенный файл
            {
                StreamReader reader = new StreamReader(files[i]);
                string line;
                while ((line = reader.ReadLine()) != null)Lines.Add(line);
            }
        }


        public void Start()
        {
            Console.WriteLine("Число писателей: {0}, число читателей: {1}\n", N, M);
            DateTime dt, dt2;
            dt = DateTime.Now;
            //читающие(писатели
            Thread[] Writers = new Thread[M];
            //читатели
            Thread[] Readers = new Thread[N];
            //массив, содержащий имена всех текстовых файлов с указанием пути к ним в указанной директории
            files = Directory.GetFiles(@"C:\Users\Аленка\Documents\Texts_lab_4");
            FilesCnt = files.Length;
            //инициализируем потоки
            for (int i = 0; i < N; i++)
            {
                Writers[i] = new Thread(ToQueue);
                Writers[i].Start(i);
            }
            for (int i = 0; i < M; i++)
            {
                Readers[i] = new Thread(WordReader);
                Readers[i].Start();
            }
            for (int i = 0; i < N; i++) Writers[i].Join();
            //закончили читать
            Lines.CompleteAdding();//пометили, как законченную коллекцию
            for (int i = 0; i < M; i++) Readers[i].Join();
            dt2 = DateTime.Now;

            foreach (var word in Words)
            {
                cnt2 += word.Value;
            }
            Console.Write("Уникальных слов " + Words.Count + ", общее количество слов: " + cnt2);
            cnt2 = 0;
            foreach (var ch in Symbols)
            {
                cnt2 += ch.Value;
            }
            foreach (var dig in Dig)
            {
                cnt += dig.Value;
            }
            Console.WriteLine("     \t|Время = " + (dt2 - dt).TotalMilliseconds +
                 "мс\nУникальных символов: " + Symbols.Count + ", общее количество символов: " + cnt2 +
                 "\t|\nРазличных цифр: " + Dig.Count + ", общее количество цифр: " + cnt +
                 "             \t|\n-------------------------------------------------------------------------------------\n");
        }
    }
}    


}
