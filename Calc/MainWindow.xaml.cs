using System;
using System.Windows;
using System.Windows.Controls;

namespace WpfCalc
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string left = ""; 
        string operation = ""; 
        string right = ""; 

        public MainWindow()
        {
            InitializeComponent();
            //обработчик кнопок
            foreach (var c in LRoot.Children)
            {
                if (c is Button)
                {
                    ((Button)c).Click += BClick;
                }
            }
        }

        private void BClick(object sender, RoutedEventArgs e)
        {
            // Получаем текст кнопки
            string s = (string)((Button)e.OriginalSource).Content;
            // Добавляем его в текстовое поле
            field.Text += s;
            int num;
            // Пытаемся преобразовать его в число
            bool result = int.TryParse(s, out num);
            // Если текст - это число
            if (result == true)
            {
                // Если операция не задана
                if (operation == "")
                {
                    // Добавляем к левому операнду
                    left += s;
                }
                else
                {
                    // Иначе к правому операнду
                    right += s;
                }
            }
            // Если было введено не число
            else
            {
                // Если равно, то выводим результат операции
                if (s == "=")
                {
                    UpR();
                    field.Text += right;
                    operation = "";
                }
                // Очищаем поле и переменные
                else if (s == "CLEAR")
                {
                    left = "";
                    right = "";
                    operation = "";
                    field.Text = "";
                }
 
                else
                {

                    if (right != "")
                    {
                        UpR();
                        left= right;
                        right = "";
                    }
                    operation = s;
                }
            }
        }

        private void UpR()
        {
            int num1 = int.Parse(left);
            int num2 = int.Parse(right);

            switch (operation)
            {
                case "+":
                    right = (num1 + num2).ToString();
                    break;
                case "-":
                    right = (num1 - num2).ToString();
                    break;
                case "*":
                    right = (num1 * num2).ToString();
                    break;
                case "/":
                    right = (num1 / num2).ToString();
                    break;
            }
        }

    }
}
