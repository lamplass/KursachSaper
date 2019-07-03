using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Media;
using System.Windows.Controls.Primitives;
using System.Security;
using Microsoft.Win32;
using static System.IO.Path;

namespace WpfApp1
{
    class Generator_h
    {
        public int kolvomin1;
        public int[,] field;// двумерныфй массив 
        public bool isBroken(int x, int y)
        {/*проверка окружение мин*/
            bool res = true;

            if ((x < 0) || (x > field.GetLength(0) - 1))
                throw new ArgumentException("ВЫХОД ЗА ГРАНИЦУ");

            if ((y < 0) || (y > field.GetLength(1) - 1))
                throw new ArgumentException("ВЫХОД ЗА ГРАНИЦУ");

            int minx = x - 1;
            if (minx < 0) minx = 0;
            int miny = y - 1;
            if (miny < 0) miny = 0;

            int maxx = x + 1;
            if (maxx > field.GetLength(0) - 1) maxx = field.GetLength(0) - 1;
            int maxy = y + 1;
            if (maxy > field.GetLength(1) - 1) maxy = field.GetLength(1) - 1;

            for (int i = minx; i <= maxx; i++)
            {
                for (int j = miny; j <= maxy; j++)
                {
                    if (field[i, j] == 0)
                    {
                        res = false;
                        break;
                    }
                }
                if (res == false) break;
            }
            return res;
        }

        public int nummin(int x, int y)
        {
            int kolvo = 0;

            int minx = x - 1;
            if (minx < 0) minx = 0;
            int miny = y - 1;
            if (miny < 0) miny = 0;

            int maxx = x + 1;
            if (maxx > field.GetLength(0) - 1) maxx = field.GetLength(0) - 1;
            int maxy = y + 1;
            if (maxy > field.GetLength(1) - 1) maxy = field.GetLength(1) - 1;

            for (int i = minx; i <= maxx; i++)
            {
                for (int j = miny; j <= maxy; j++)
                {
                    if (field[i, j] == -1)
                    {
                        kolvo++;
                    }
                }
            }
            return kolvo;
        }

        public void init(int n)
        {/*создание поля*/
            field = new int[n, n];
        }

        public void plantMines(int n)
        {/*установка мин*/
            //field = new int[n, n];
            Random kuku = new Random();//функция генерирующая случайное значение
            if (n > 23)
                throw new ArgumentException("МНОГО МИН");

            if (n < kolvomin1)
                throw new ArgumentException("МАЛО МИН");

            for (int i = 0; i < n; i++)
            {
                int x = kuku.Next(field.GetLength(0));//ставим случайное значение по x
                int y = kuku.Next(field.GetLength(1));//ставим случайное значение по y

                if (field[x, y] == -1)
                {
                    i--;//возвращаемся на шаг назад
                }
                else
                    field[x, y] = -1;// иначе ставим мину

                if (nummin(x, y) >= 4)
                {
                    field[x, y] = 0;
                    i--;
                }

                for (int i1 = 0; i1 < field.GetLength(0); i1++)
                {
                    for (int j1 = 0; j1 < field.GetLength(1); j1++)
                        if (isBroken(x, y) == true)
                        {/*если есть рядом мина*/
                            field[x, y] = 0;//выбранное поле зануляем
                            i--;//возвращаемся на шаг назад
                            break;//выход из цикла
                        }
                    if (field[x, y] == 0) break;//выход из цикла если поле равно 0
                }
            }
        }

        public void calculate()
        {/*считает сколько вокруг мин*/
            for (int i = 0; i < field.GetLength(0); i++)
                for (int j = 0; j < field.GetLength(1); j++)
                {
                    if (field[i, j] == 0)
                    {
                        int minx = i - 1;
                        if (minx < 0) minx = 0;
                        int miny = j - 1;
                        if (miny < 0) miny = 0;

                        int maxx = i + 1;
                        if (maxx > field.GetLength(0) - 1) maxx = field.GetLength(0) - 1;
                        int maxy = j + 1;
                        if (maxy > field.GetLength(1) - 1) maxy = field.GetLength(1) - 1;

                        int sum = 0;//количество мин

                        for (int i1 = minx; i1 <= maxx; i1++)
                        {
                            for (int j1 = miny; j1 <= maxy; j1++)
                            {
                                if (field[i1, j1] == -1)
                                {/*если поле мина*/
                                    sum++;//количество мин увеличиваем
                                }
                            }

                        }
                        field[i, j] = sum;//заполняем поле числом равным количеству мин вокруг
                    }
                }

        }

        public int getCell(int u, int h)
        {/*возвращаем знначение поля*/
         
            return field[u, h];
        }

        public void reveal(int i, int j)
        {/*функция открывает вокруг поля без мин*/
          
            if (i >= 0 && j >= 0 && i < (field.GetLength(0)) && j < (field.GetLength(1)))/**/
                if (field[i, j] == 0)
                {/*вокруг мин */
                    field[i, j] = 20;

                    reveal(i, j - 1);
                    reveal(i - 1, j);
                    reveal(i, j + 1);
                    reveal(i + 1, j);

                    reveal(i + 1, j + 1);
                    reveal(i - 1, j + 1);
                    reveal(i + 1, j - 1);
                    reveal(i - 1, j - 1);

                }
                else if (field[i, j] == 1)
                {
                    field[i, j] = 21;
                }
                else if (field[i, j] == 2)
                {
                    field[i, j] = 22;
                }
                else if (field[i, j] == 3)
                {
                    field[i, j] = 23;
                }
                else if (field[i, j] == 4)
                {
                    field[i, j] = 24;
                }
                else if (field[i, j] == 5)
                {
                    field[i, j] = 25;
                }
                else if (field[i, j] == 6)
                {
                    field[i, j] = 26;
                }
                else if (field[i, j] == 7)
                {
                    field[i, j] = 27;
                }
                else if (field[i, j] == 8)
                {
                    field[i, j] = 28;
                }
        }
    }
}

