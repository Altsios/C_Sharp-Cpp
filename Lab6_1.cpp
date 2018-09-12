//Lab6.cpp
// Lab6.cpp: определяет точку входа для консольного приложения.
//

#include "stdafx.h"

int* GenerArray(int n);

int main(int argc, char** argv)

{
	int MAXPROCESS, n, rank, root = 0, k=0;
	//если передали количество элементов
	n = argc > 1 ? atoi(argv[1]) : 6;
	/*Любая прикладная MPI-программа (приложение) должна начинаться
	с вызова функции инициализации MPI: функции MPI_Init. 
	В результате выполнения этой функции создается группа процессов, 
	в которую помещаются все процессы приложения, и создается область связи, 
	описываемая предопределенным коммуникатором MPI_COMM_WORLD*/
	MPI_Init(&argc, &argv);
	//узнать количесвто процессов
	MPI_Comm_size(MPI_COMM_WORLD, &MAXPROCESS);
	//узнать ранг текущего процесса
	MPI_Comm_rank(MPI_COMM_WORLD, &rank);

	if (MAXPROCESS > n)
	{
		if(rank == root)printf("\nThe number of proccess must be lower or equal to the number of array elements\n");
		MPI_Finalize();
		return 1;
	}

	int *genrank=nullptr, *b=nullptr, *rcounts=nullptr, *displs=nullptr;
	int* a = (int*)malloc(n * sizeof(int));
	//для вычисления части ранга в процессах
	//у последнего мб остаток
	int len= (rank == MAXPROCESS - 1) && (n % MAXPROCESS != 0)? n-round(float(n) / float(MAXPROCESS))*(MAXPROCESS-1)
		:round(float(n)/float(MAXPROCESS));

	int * arrrank = (int*)malloc(len * sizeof(int));

	if (rank == root)
		a = GenerArray(n);

	//начало работы алгоритма
	double t1, t2, dt;
	t1 = MPI_Wtime();
	//рассылка из корневого процесса всем процессам, включая себя
	MPI_Bcast(a, n, MPI_INT, root, MPI_COMM_WORLD);

	int start = rank*round((float(n) / float(MAXPROCESS)));
	//при неравном делении на последнем процессоре
	int end= rank == MAXPROCESS-1? n :start + round(float(n) / float(MAXPROCESS));

	for (int i = start; i<end; i++) {
		arrrank[k] = 0;
		for (int j = 0; j < n; j++) {
			if (a[i]>a[j]) arrrank[k]++;
		}
		k++;
	}

	if (rank == root)
	{
		genrank = (int*)malloc(n*sizeof(int));
		b= (int*)malloc(n * sizeof(int));
		//кол элементов, смещение
		rcounts = (int*)malloc(n * sizeof(int));
		displs= (int*)malloc(n * sizeof(int));
		//для gatherv
		for (size_t i = 0; i < MAXPROCESS-1; i++)
		{
			//предполагается, что рут не последний по номеру
			rcounts[i] = len;
			displs[i] = i *round(float(n) / float(MAXPROCESS));
		}
		rcounts[MAXPROCESS - 1] = (n % MAXPROCESS != 0) ?  n-round(float(n) / float(MAXPROCESS))*(MAXPROCESS-1)
		:round(float(n)/float(MAXPROCESS));
		displs[MAXPROCESS - 1] = (MAXPROCESS - 1)*round(float(n) / float(MAXPROCESS));
	}
	
	//сборка данных в корневой процесс
	MPI_Gatherv(arrrank, len, MPI_INT, genrank, rcounts, displs, MPI_INT, root, MPI_COMM_WORLD);
	
	if (rank == root)
	{
		for (int i = 0; i<n; i++) {
			b[genrank[i]] = a[i];
		}
	}
	//конец алгоритма
	t2 = MPI_Wtime();
	dt = (t2-t1)*1000;//миллисекунд
	if(rank==root)
	printf("\nn = %-7d| time = %f\n",n,dt);
	/*Функция закрывает все MPI-процессы
	и ликвидирует все области связи.*/
	MPI_Finalize();
	delete[] genrank;
	delete[] b;
	delete[] a;
	return 0;
}

//генерация исходного массива
int* GenerArray(int n)
{
	int* a = (int*)malloc(n * sizeof(int));
	for (int i = 0; i < n; i++)
		a[n - i - 1] = i;
	return a;
}

