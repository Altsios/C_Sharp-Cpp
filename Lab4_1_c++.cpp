//Lab4Cpp.cpp
// Lab4Cpp.cpp: определяет точку входа для консольного приложения.
//

#include "stdafx.h"

using namespace std;
using namespace chrono;


int main()
{
	setlocale(LC_ALL, "Russian");
	int* a=nullptr;
	int* b= nullptr;

	cout << left << setw(16) << "С++:" << endl << left << setw(16) << "Алгоритм" << "|"
		<< left << setw(16) << "Элементов" << "|" << left << setw(16) << "Время" <<endl<< string(48, '-') <<endl;

	for (int i = 100; i <= 100000; i += 5000)
	{
		a = GenerArray(i);

		auto begin = steady_clock::now();
		b = SortSeq(a);
		auto end = steady_clock::now();
		auto elapsed_ms = duration_cast<std::chrono::milliseconds>(end - begin);
		cout <<left<< setw(16)<< "Последовательный"<<"|"<<left<<setw(16)<< i << "|" << left << setw(16) << elapsed_ms.count()<< endl;

		begin = steady_clock::now();
		b = SortPar(a);
		end = steady_clock::now();
		elapsed_ms = duration_cast<std::chrono::milliseconds>(end - begin);
		cout << left << setw(16)<< "Параллельный"<<"|"<< left << setw(16) << i << "|" << left << setw(16) << elapsed_ms.count() << endl;
			
	}
	
	//освобождение памяти
	delete[] a;
	delete[] b;
	cout << "Готово!" << endl;
	system("pause");
    return 0;
}


//Utils.cpp
#include "stdafx.h"
#include "Utils.h"


 int* GenerArray(int n)
{
	 int* a = new int[n];
	 for (int i = 0; i < n; i++)
		 a[n - i - 1] = i;
	 return a;
}

 int * SortSeq(int * a)
 {
	 //_msize Функция возвращает размер в байтах блока памяти, выделенной с помощью вызова calloc, malloc, или realloc.
	 int n = _msize(a) / sizeof(int);
	 int* b = new int[n];
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

 int * SortPar(int * a)
 {
	 int n = _msize(a) / sizeof(int);
	 int* b = new int[n];
#pragma omp parallel for
		 for (int i = 0; i < n; i++)
		 {
			 //частная переменная
			 int x = 0;
			 // вычисляем ранг элемента
			 for (int j = 0; j < n; j++)
				 if (a[i] > a[j] || (a[i] == a[j] && j > i))
					 x++;
			 b[x] = a[i]; // записываем в результирующую
		 }
		 return b;
 }



