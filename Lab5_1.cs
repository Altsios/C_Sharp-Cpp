//Последовательная версия
//Program.cs
using System;
using System.Diagnostics;

namespace GA
{

    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch st = new Stopwatch();
            for (int genes = 10; genes <= 100; genes += 30)
            {
                st.Restart();
                Genetic GA = new Genetic(1000, genes, -100, 100, 10000);
                var solution = GA.Seq();
                st.Stop();
                Console.WriteLine(st.ElapsedMilliseconds);
                foreach (var item in solution)
                {
                    Console.WriteLine(item);
                }
           }

            Console.ReadKey();
        }
    }
}

//Genetic.cs
using System;
using System.IO;
using System.Linq;

namespace GA
{
    //класс-особь
    class Individuo
    {
        public double[] Genes { get; set; }
        public double Fitness {
            get
            {
                return 1 / (1 + Genes.Sum(x => (x - 1)*(x-1)));
            }

        }

        public Individuo(int genes)
        {
            Genes = new double[genes];
        }


        public void Clone(Individuo source)
        {
            Genes = (double[])source.Genes.Clone();
        }

    }

    class Genetic
    {
        //посмотреть, что используется из полей после конструктора
        readonly int _genes;
        readonly int _population;
        readonly int _minValgen;
        readonly int _maxValgen;
        readonly int _n_max;

        const double _mutProp = 0.45d;
        const double _crossover = 0.4d;
        const double _maxMutation = 0.5d;
        const double _minMutation = -0.5d;

        private Individuo[] _individuos;
        private static readonly Random Rand = new Random();


        public Genetic(int population, int genes, int min, int max, int N_MAX = 10000)
        {
            _genes = genes;
            _population = population;
            _minValgen = min;
            _maxValgen = max;
            _n_max = N_MAX;
            //формирование начальной популяции
            _individuos = new Individuo[population];
            for (var i = 0; i < population; i++)
            {
                _individuos[i] = new Individuo(genes);
                //формирование генов
                for (var j = 0; j < genes; j++)
                {
                    _individuos[i].Genes[j] = Rand.NextDouble() * (max - min) + min;
                }
            }

        }

        public double[] Seq()
        {
            var generation = 1; // номер поколения
            double NewavegFit = 0d;
           // NewavegFit = _individuos.Select(x => x.Fitness).Average();
          //  Console.WriteLine("Поколение#{0,-5}|Cредняя приспособленность: {1:0.####################}", 
          //      generation, NewavegFit);
     
          //  using (StreamWriter sw = new StreamWriter(@"C:\Users\Alenka\OneDrive\Parall2\GA\GA\output.txt", true))
          //  {
          //      sw.WriteLine("{0:0.####################}", NewavegFit);
                while ((generation < _n_max))
                {
                    generation++;
                    // Отбор
                    for (int i = 0; i < _population / 2; i++)
                    {
                        Individuo ind1, ind2;
                        GetRandomPair(out ind1, out ind2);
                        Select(ind1, ind2);
                    }
                    // Скрещивание
                    for (int i = 0; i < _population / 2; i++)
                    {
                        Individuo ind1, ind2;
                        GetRandomPair(out ind1, out ind2);
                        Crossover(ind1, ind2);
                    }
                    // Мутация
                    foreach (var ind in _individuos)
                    {
                        Mutate(ind);
                    }
                    // Оценка качества популяции
                   // NewavegFit = _individuos.Select(x => x.Fitness).Average();
                    //Console.WriteLine("Поколение#{0,-5}|Cредняя приспособленность: {1:0.####################}", 
                    //    generation, NewavegFit);
                    //запись в файл для графика
                  //  sw.WriteLine("{0:0.####################}", NewavegFit);
                //}
            }
            return _individuos.OrderByDescending(ind => ind.Fitness).Select(ind => ind.Genes).First();
         
        }

        //отбор
        void Select(Individuo ind1, Individuo ind2)
        {
            if (ind1.Fitness > ind2.Fitness)
                ind2.Clone(ind1);
            else
                ind1.Clone(ind2);
        }
        //скрещивание(равномерное)
        void Crossover(Individuo ind1, Individuo ind2)
        {
            for (int i = 0; i < _genes; i++)
            {
                if (Rand.Next(0, 100) > _crossover)
                    continue;
                Swap(ref ind1.Genes[i], ref ind2.Genes[i]);
            }

        }
        //обмен
        static void Swap(ref double a,ref double b)
        {
            var tmp = a;
            a = b;
            b = tmp;
        }
        //мутация
        void Mutate(Individuo ind)
        {
            for (int i = 0; i < _genes; i++)
            {
                // Вероятность мутации каждого гена не велика (5 - 20%)
                if (Rand.NextDouble() < _mutProp)
                     ind.Genes[i] += Rand.NextDouble() * (_maxMutation - _minMutation) + _minMutation;
            }
        }
        //получаем 2 случаные особи из популяции
        void GetRandomPair(out Individuo first, out Individuo second)
        {
            //подбираем рандомно 2 индекса
            int idx1;
            int idx2;
            do
            {
                idx1 = Rand.Next(0, _population);
                idx2 = Rand.Next(0, _population);
            } while (idx1 == idx2);

            first = _individuos[idx1];
            second = _individuos[idx2];
        }

    }
}

//MPI
//Programs.cs
using MPI;
using System;
using System.Diagnostics;

namespace GA_MPI
{
    class Program
    {
        static void Main(string[] args)
        {
            using (new MPI.Environment(ref args))
            {

                Stopwatch st= new Stopwatch(); ;
                if (Communicator.world.Rank == 0)
                    st.Start();

                Genetic GA = new Genetic(1000/ Communicator.world.Size, 10, -100, 100,10000);     
                 var solution = GA.MPI();

                if(Communicator.world.Rank==0)
                {
                    st.Stop();
                    Console.WriteLine(st.ElapsedMilliseconds);
                    /*
                foreach (var item in solution)
                {
                    Console.WriteLine(item);
                }*/
                }
                //Console.WriteLine();
            }
        }
    }
}

//Gentic.cs
using MPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GA_MPI
{
    //класс-особь
    [Serializable]
    class Individuo
    {
        public double[] Genes { get; set; }
        public double Fitness
        {
            get
            {
                return 1 / (1 + Genes.Sum(x => (x - 1) * (x - 1)));
            }

        }

        public Individuo(int genes)
        {
            Genes = new double[genes];
        }


        public void Clone(Individuo source)
        {
            Genes = (double[])source.Genes.Clone();
        }

    }
    class Genetic
    {
        //посмотреть, что используется из полей после конструктора
        readonly int _genes;
        readonly int _population;
        readonly int _minValgen;
        readonly int _maxValgen;
        readonly int _n_max;

        const double _mutProp = 0.45d;
        const double _crossover = 0.4d;
        const double _maxMutation = 0.5d;
        const double _minMutation = -0.5d;

        private Individuo[] _individuos;
        private static readonly Random Rand = new Random();


        public Genetic(int population, int genes, int min, int max, int N_MAX = 10000)
        {
            _genes = genes;
            _population = population;
            _minValgen = min;
            _maxValgen = max;
            _n_max = N_MAX;

            //формирование начальной популяции
            _individuos = new Individuo[population];
            for (var i = 0; i < population; i++)
            {
                _individuos[i] = new Individuo(genes);
                //формирование генов
                for (var j = 0; j < genes; j++)
                {
                    _individuos[i].Genes[j] = Rand.NextDouble() * (max - min) + min;
                }
            }

        }

        public double[] MPI()
        {
            const int MIGRATION = 100;
            int cnt = (int)(_population * 0.3);
            Intracommunicator comm = Communicator.world;
            var rank = comm.Rank;
            var size = comm.Size;
            var generation = 1; // номер поколения
            double NewavegFit = 0d;
            double GeneralFit = 0d;
           // NewavegFit = _individuos.Select(x => x.Fitness).Average();

           // GeneralFit = comm.Reduce(NewavegFit, Operation<double>.Add, 0);
            /*
            comm.Barrier();
            if (rank == 0)
            {
                Console.WriteLine("Поколение#{0}",generation);
                double avgFitness = GeneralFit / size;
                using (StreamWriter sw = new StreamWriter(@"C:\Users\Alenka\OneDrive\Parall2\GA_MPI\GA_MPI\bin\Debug\output.txt", true))
                {
                    sw.WriteLine("{0:0.####################}", avgFitness);
                }
            }
            */
            while ((generation < _n_max))
            {
                generation++;
                // Отбор
                for (int i = 0; i < _population / 2; i++)
                {
                    Individuo ind1, ind2;
                    GetRandomPair(out ind1, out ind2);
                    Select(ind1, ind2);
                }
                // Скрещивание
                for (int i = 0; i < _population / 2; i++)
                {
                    Individuo ind1, ind2;
                    GetRandomPair(out ind1, out ind2);
                    Crossover(ind1, ind2);
                }
                // Мутация
                foreach (var ind in _individuos)
                {
                    Mutate(ind);
                }
                // Оценка качества популяции
               // NewavegFit = _individuos.Select(x => x.Fitness).Average();
                //GeneralFit = comm.Reduce(NewavegFit, Operation<double>.Add, 0);
/*
                comm.Barrier();
                if (rank == 0)
                {
                    Console.WriteLine("Поколение#{0}", generation);
                    double avgFitness = GeneralFit/ size;
                        using (StreamWriter sw = new StreamWriter(@"C:\Users\Alenka\OneDrive\Parall2\GA_MPI\GA_MPI\bin\Debug\output.txt", true))
                    {
                        sw.WriteLine("{0:0.####################}", avgFitness);
                    }
                }*/
                //GeneralFit = 0;
                //время миграции
                if ((generation % MIGRATION == 0))
                {
                    //узнать индексы лучших, чтобы потом воткнуть на их место вновь прибывших особей
                    //берем лучшие особи(10%)
                    var bestIdxs = _individuos.Select((item, index) => new { Item = item, Index = index }).Where(n => _individuos.
                    OrderByDescending(t => t.Fitness).Take(cnt).Contains(n.Item))
                    .Select(n => n.Index).ToArray();
                    //берем лучших особей
                    var bestIndividuos = bestIdxs.Select(index => _individuos[index]).ToArray();

                    //если не центральный остров*/
                    if (rank != 0)
                    {
                        //послали лучших 
                        comm.Send(bestIndividuos, 0, rank);
                        //получили лучших
                        var newIndiv = comm.Receive<List<Individuo>>(0, rank);
                        for (int i = 0; i < bestIdxs.Length; i++)
                            ////копируем ссылки на места отданых элементов
                            _individuos[bestIdxs[i]] = newIndiv[i];
                    }

                    else
                    {
                        //берем элементы со всех массивов
                        //добавляем все полученные особи(массив отобранных особей) на центральный процесс
                        List<Individuo> allIndividuos = new List<Individuo>();
                        for (int i = 1; i < size; i++)
                            allIndividuos.AddRange(comm.Receive<Individuo[]>(i, i));

                        //+элементы корня
                        allIndividuos.AddRange(bestIndividuos);

                        for (int i = 1; i < size; i++)
                        {
                            var randomIndividuos = new List<Individuo>();
                            for (int j = 0; j < cnt; j++)
                            {
                                int rnd = Rand.Next(0, allIndividuos.Count);
                                randomIndividuos.Add(allIndividuos[rnd]);
                                allIndividuos.RemoveAt(rnd);
                            }
                            comm.Send(randomIndividuos, i, i);
                        }

                        //оставшиеся главному отдаем
                        for (int i = 0; i < bestIdxs.Length; i++)
                            //копируем ссылки на места отданых элементов
                            _individuos[bestIdxs[i]] = allIndividuos[i];
                    }
                }
            }
            //итерации кончились, берем самую лучшую особь у всех процессоров
            Individuo best = _individuos.OrderByDescending(ind => ind.Fitness).First();
            Individuo[] bests = comm.Gather(best, 0);
            if (rank==0)
            {
                return bests.OrderByDescending(ind=>ind.Fitness).Select(ind => ind.Genes).First();
            }
            return null;
        }

        //отбор
        void Select(Individuo ind1, Individuo ind2)
        {
            if (ind1.Fitness > ind2.Fitness)
                ind2.Clone(ind1);
            else
                ind1.Clone(ind2);
        }
        //скрещивание(равномерное)
        void Crossover(Individuo ind1, Individuo ind2)
        {
            for (int i = 0; i < _genes; i++)
            {
                if (Rand.Next(0, 100) > _crossover)
                    continue;
                Swap(ref ind1.Genes[i], ref ind2.Genes[i]);
            }

        }
        //обмен
        static void Swap(ref double a, ref double b)
        {
            var tmp = a;
            a = b;
            b = tmp;
        }
        //мутация
        void Mutate(Individuo ind)
        {
            for (int i = 0; i < _genes; i++)
            {
                // Вероятность мутации каждого гена не велика (5 - 20%)
                if (Rand.NextDouble() < _mutProp)
                    ind.Genes[i] += Rand.NextDouble() * (_maxMutation - _minMutation) + _minMutation;
            }
        }
        //получаем 2 случаные особи из популяции
        void GetRandomPair(out Individuo first, out Individuo second)
        {
            //подбираем рандомно 2 индекса
            int idx1;
            int idx2;
            do
            {
                idx1 = Rand.Next(0, _population);
                idx2 = Rand.Next(0, _population);
            } while (idx1 == idx2);

            first = _individuos[idx1];
            second = _individuos[idx2];
        }

    }
}
