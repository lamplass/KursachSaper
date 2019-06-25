using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    class Generator_l
    {
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


        public void init(int n)
        {/*создание поля*/
            field = new int[n, n];
        }

        public void plantMines(int n)
        {/*установка мин*/
            //field = new int[n, n];
            Random kuku = new Random();//функция генерирующая случайное значение
            if (n > 3)
                throw new ArgumentException("МНОГО МИН");

            if (n < 2)
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

        public int getCell(int i, int j)
        {/*возвращаем знначение поля*/
            return field[i, j];
        }

        public void reveal(int i, int j)
        {/*функция открывает вокруг поля без мин*/
            field = new int [i,j];

            if (i >= 0 && j >= 0 && i < (field.GetLength(0)) && j < (field.GetLength(1)))/**/
                if (field[i, j] == 0)
                {/*вокруг мин */
                    field[i, j] = 6;

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
                    field[i, j] = 7;
                }
                else if (field[i, j] == 2)
                {
                    field[i, j] = 8;
                }
                else if (field[i, j] == 3)
                {
                    field[i, j] = 9;
                }
                else if (field[i, j] == 4)
                {
                    field[i, j] = 10;
                }
                else if (field[i, j] == 5)
                {
                    field[i, j] = 11;
                }

        }
    }
}

