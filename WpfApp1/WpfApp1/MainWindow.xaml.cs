using System;
using System.Collections.Generic;
using System.Data.SQLite;
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

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BitmapImage mine; // картнка мины 
        Generator gen = new Generator(); // генерирует поле 
        int kolvopust = 0; // количество пустых
        int kolvomin = 1; // сколько мин в игре 
        System.Windows.Threading.DispatcherTimer Timer;
        DateTime start;
        string nameuser;
        SQLiteConnection m_dbConnection;

        string db_name = "C:\\Users\\Sergo\\Desktop\\сапер3\\trpo6\\polzovateli.db";

        public MainWindow()
        {
            InitializeComponent();
            load();
            mine = new BitmapImage(new Uri(@"C:\Users\Sergo\source\repos\pyat\KursachSaper\WpfApp1\WpfApp1\bomb\bomb.jpg", UriKind.Absolute)); // ставим изображение мины
        }

        public void load()
        {
            Lbshow.Items.Clear();
            m_dbConnection = new SQLiteConnection("Data Source=" + db_name + ";Version=3;");
            //открытие соединения с базой данных
            m_dbConnection.Open();
            string sqlschit = "SELECT * FROM igroki ";
            SQLiteCommand commandschit = new SQLiteCommand(sqlschit, m_dbConnection);
            SQLiteDataReader readerschit = commandschit.ExecuteReader();
            string st;
            while (readerschit.Read())
            {
                st = readerschit["Name"].ToString() + "\t" + readerschit["Time"];
                Lbshow.Items.Add(st);
            }
            m_dbConnection.Close();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            string st = "";
            TimeSpan ts = ts = DateTime.Now - start;
            st += ts.Minutes.ToString() + ":" + ts.Seconds.ToString();

            lbtimer.Content = st;
        }

        private void b1_Click(object sender, RoutedEventArgs e)
        {
            Lbshow.Visibility = Visibility;
            tabll.Visibility = Visibility;

            m_dbConnection = new SQLiteConnection("Data Source=" + db_name + ";Version=3;");
            //открытие соединения с базой данных
            m_dbConnection.Open();
            //выполнение запросов
            lbname.Visibility = Visibility.Hidden;
            tbname.Visibility = Visibility.Hidden;
            lbtimer.Visibility = Visibility;
            nameuser = tbname.Text;
            start = DateTime.Now;
            Timer = new System.Windows.Threading.DispatcherTimer();
            Timer.Tick += new EventHandler(dispatcherTimer_Tick);
            Timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            Timer.Start();
            //установка интервала между тиками
            //TimeSpan – переменная для хранения времени в формате часы/минуты/секунды
            //запуск таймера

            kolvopust = 0;// количество пустух клеток равно 0
            setka.Children.Clear();// 
            setka.IsEnabled = true;

            gen.init(5);// размер поля в нашем случае 5 на 5 
            gen.plantMines(kolvomin);// количество мин 
            gen.calculate();// раставляет мины

            //указыается количество строк и столбцов в сетке
            setka.Rows = 5;
            setka.Columns = 5;
            //указываются размеры сетки (число ячеек * (размер кнопки в ячейки + толщина её границ))
            setka.Width = 5 * (50 + 4);// по ширене 
            setka.Height = 5 * (50 + 4);// по высоте
            //толщина границ сетки
            setka.Margin = new Thickness(3);//расстояние между ними

            this.Width = 5 * 150;//задаем размер окошка по ширене
            this.Height = 6 * 72;//задаем размер по высоте

            for (int i = 0; i < 5 * 5; i++)
            {
                //создание кнопки
                Button btn = new Button();
                //запись номера кнопки
                btn.Tag = i;
                //установка размеров кнопки
                btn.Width = 50;
                btn.Height = 50;
                //текст на кнопке
                btn.Content = " ";
                //толщина границ кнопки
                btn.Margin = new Thickness(2);
                //при нажатии кнопки, будет вызываться метод Btn_Click
                btn.Click += Btn_Click;
                //добавление кнопки в сетку
                setka.Children.Add(btn);
            }
        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            ////получение значения лежащего в Tag
            int n = (int)((Button)sender).Tag;// узнаем номер кнопки


            if (gen.getCell(n % 5, n / 5) == 0)
            {/*если вокруг мин нету*/
                gen.reveal(n % 5, n / 5);//открывает вокруг поля

                Button[] buts = new Button[setka.Children.Count];// создаем массив кнопок
                setka.Children.CopyTo(buts, 0);// создаем пустое поле

                for (int i = 0; i < buts.Length; i++)
                {
                    int ind = (int)(buts[i]).Tag;//узнаем номер кнопки

                    if (gen.getCell(ind % 5, ind / 5) == 10)
                    {/*проверка на победу */

                        //установка фона нажатой кнопки, цвета и размера шрифта
                        (buts[i]).Background = Brushes.White;//фон кнопки
                        (buts[i]).Foreground = Brushes.Red;//цвет цифр
                        (buts[i]).FontSize = 23;//раз шрифта

                        //запись в нажатую кнопку её номера
                        (buts[i]).Content = 0;
                        kolvopust++;//количество открытых увеличиваем
                        if ((25 - kolvopust) == kolvomin)
                        {
                            Timer.Stop();
                            MessageBox.Show("Вы выйграли");
                            
                            string sqladd = "INSERT INTO igroki (Name,Time) VALUES ('" + nameuser + "','" + lbtimer.Content.ToString() + "')";
                            string st;
                            st = nameuser + "\t" + lbtimer.Content.ToString();
                            SQLiteCommand commandDobavlenieNovogo = new SQLiteCommand(sqladd, m_dbConnection);

                            //извлечение запроса
                            commandDobavlenieNovogo.ExecuteNonQuery();
                            //закрытие соединения с базой данных
                            m_dbConnection.Close();
                        }

                    }
                    if (gen.getCell(ind % 5, ind / 5) == 11)
                    {

                        //установка фона нажатой кнопки, цвета и размера шрифта
                        (buts[i]).Background = Brushes.White;
                        (buts[i]).Foreground = Brushes.Red;
                        (buts[i]).FontSize = 23;

                        //запись в нажатую кнопку её номера
                        (buts[i]).Content = 1;
                        kolvopust++;
                        if ((25 - kolvopust) == kolvomin)
                        {
                            Timer.Stop();
                            MessageBox.Show("Вы выйграли");
                            
                            string sqladd = "INSERT INTO igroki (Name,Time) VALUES ('" + nameuser + "','" + lbtimer.Content.ToString() + "')";
                            string st;
                            st = nameuser + "\t" + lbtimer.Content.ToString();
                            SQLiteCommand commandDobavlenieNovogo = new SQLiteCommand(sqladd, m_dbConnection);

                            //извлечение запроса
                            commandDobavlenieNovogo.ExecuteNonQuery();
                            //закрытие соединения с базой данных
                            m_dbConnection.Close();
                        }

                    }
                    if (gen.getCell(ind % 5, ind / 5) == 12)
                    {

                        //установка фона нажатой кнопки, цвета и размера шрифта
                        (buts[i]).Background = Brushes.White;
                        (buts[i]).Foreground = Brushes.Red;
                        (buts[i]).FontSize = 23;

                        //запись в нажатую кнопку её номера
                        (buts[i]).Content = 2;
                        kolvopust++;
                        if ((25 - kolvopust) == kolvomin)
                        {
                            Timer.Stop();
                            MessageBox.Show("Вы выйграли");
                            
                            string sqladd = "INSERT INTO igroki (Name,Time) VALUES ('" + nameuser + "','" + lbtimer.Content.ToString() + "')";
                            string st;
                            st = nameuser + "\t" + lbtimer.Content.ToString();
                            SQLiteCommand commandDobavlenieNovogo = new SQLiteCommand(sqladd, m_dbConnection);

                            //извлечение запроса
                            commandDobavlenieNovogo.ExecuteNonQuery();
                            //закрытие соединения с базой данных
                            m_dbConnection.Close();
                        }

                    }
                    if (gen.getCell(ind % 5, ind / 5) == 13)
                    {

                        //установка фона нажатой кнопки, цвета и размера шрифта
                        (buts[i]).Background = Brushes.White;
                        (buts[i]).Foreground = Brushes.Red;
                        (buts[i]).FontSize = 23;

                        //запись в нажатую кнопку её номера
                        (buts[i]).Content = 3;
                        kolvopust++;
                        if ((25 - kolvopust) == kolvomin)
                        {
                            Timer.Stop();
                            MessageBox.Show("Вы выйграли");
                           
                            string sqladd = "INSERT INTO igroki (Name,Time) VALUES ('" + nameuser + "','" + lbtimer.Content.ToString() + "')";
                            string st;
                            st = nameuser + "\t" + lbtimer.Content.ToString();
                            SQLiteCommand commandDobavlenieNovogo = new SQLiteCommand(sqladd, m_dbConnection);

                            //извлечение запроса
                            commandDobavlenieNovogo.ExecuteNonQuery();
                            //закрытие соединения с базой данных
                            m_dbConnection.Close();
                        }

                    }
                    if (gen.getCell(ind % 5, ind / 5) == 14)
                    {

                        //установка фона нажатой кнопки, цвета и размера шрифта
                        (buts[i]).Background = Brushes.White;
                        (buts[i]).Foreground = Brushes.Red;
                        (buts[i]).FontSize = 23;

                        //запись в нажатую кнопку её номера
                        (buts[i]).Content = 4;
                        kolvopust++;
                        if ((25 - kolvopust) == kolvomin)
                        {
                            Timer.Stop();
                            MessageBox.Show("Вы выйграли");
                            
                            string sqladd = "INSERT INTO igroki (Name,Time) VALUES ('" + nameuser + "','" + lbtimer.Content.ToString() + "')";
                            string st;
                            st = nameuser + "\t" + lbtimer.Content.ToString();
                            SQLiteCommand commandDobavlenieNovogo = new SQLiteCommand(sqladd, m_dbConnection);
                            //извлечение запроса
                            commandDobavlenieNovogo.ExecuteNonQuery();
                            //закрытие соединения с базой данных
                            m_dbConnection.Close();
                        }

                    }
                    if (gen.getCell(ind % 5, ind / 5) == 15)
                    {

                        //установка фона нажатой кнопки, цвета и размера шрифта
                        (buts[i]).Background = Brushes.White;
                        (buts[i]).Foreground = Brushes.Red;
                        (buts[i]).FontSize = 23;

                        //запись в нажатую кнопку её номера
                        (buts[i]).Content = 5;
                        kolvopust++;
                        if ((25 - kolvopust) == kolvomin)
                        {
                            Timer.Stop();
                            MessageBox.Show("Вы выйграли");
                            
                            string sqladd = "INSERT INTO igroki (Name,Time) VALUES ('" + nameuser + "','" + lbtimer.Content.ToString() + "')";
                            string st;
                            st = nameuser + "\t" + lbtimer.Content.ToString();
                            SQLiteCommand commandDobavlenieNovogo = new SQLiteCommand(sqladd, m_dbConnection);
                            //извлечение запроса
                            commandDobavlenieNovogo.ExecuteNonQuery();
                            //закрытие соединения с базой данных
                            m_dbConnection.Close();
                        }

                    }
                }
            }
            else


            if (gen.getCell(n % 5, n / 5) > 0)
            {
                //установка фона нажатой кнопки, цвета и размера шрифта
                ((Button)sender).Background = Brushes.White;
                ((Button)sender).Foreground = Brushes.Red;
                ((Button)sender).FontSize = 23;

                //запись в нажатую кнопку её номера
                ((Button)sender).Content = gen.getCell(n % 5, n / 5);

                kolvopust++;
                if ((25 - kolvopust) == kolvomin)
                {
                    Timer.Stop();
                    MessageBox.Show("Вы выйграли");
                    
                    string sqladd = "INSERT INTO igroki (Name,Time) VALUES ('" + nameuser + "','" + lbtimer.Content.ToString() + "')";
                    string st;
                    st = nameuser + "\t" + lbtimer.Content.ToString();
                    SQLiteCommand commandDobavlenieNovogo = new SQLiteCommand(sqladd, m_dbConnection);
                    //извлечение запроса
                    commandDobavlenieNovogo.ExecuteNonQuery();
                    //закрытие соединения с базой данных
                    m_dbConnection.Close();
                }

            }
            else

            if (gen.getCell(n % 5, n / 5) == -1)
            {/*если нажали на мину*/

                Button[] buts = new Button[setka.Children.Count];//массив кнопок
                setka.Children.CopyTo(buts, 0);

                for (int i = 0; i < buts.Length; i++)
                {
                    int ind = (int)(buts[i]).Tag;//узнаем номер кнопки

                    if (gen.getCell(ind % 5, ind / 5) == -1)
                    {

                        Image img = new Image();
                        img.Source = mine;
                        //создание переменной для отображения изображения мины
                        StackPanel minePnl;
                        //инициализация и установка ориентации, можно вызвать в методе инициализации формы
                        minePnl = new StackPanel();
                        // minePnl.Orientation = Orientation.Vertical;
                        //установка толщины границы объекта
                        minePnl.Margin = new Thickness(1);
                        //добавление в объект изображения
                        minePnl.Children.Add(img);

                        (buts[i]).Content = minePnl;
                    }

                }
                Timer.Stop();
                MessageBox.Show("Вы проиграли");
                setka.IsEnabled = false;


            }
        }

        private void Butrecords_Click(object sender, RoutedEventArgs e)
        {
            //    Lbshow.Items.Clear();
            //    m_dbConnection = new SQLiteConnection("Data Source=" + db_name + ";Version=3;");
            //    //открытие соединения с базой данных
            //    m_dbConnection.Open();
            //    string sqlschit = "SELECT * FROM igroki ";
            //    SQLiteCommand commandschit = new SQLiteCommand(sqlschit, m_dbConnection);
            //    SQLiteDataReader readerschit = commandschit.ExecuteReader();
            //    string st;
            //    while (readerschit.Read())
            //    {
            //        st = readerschit["Name"].ToString() + "\t" + readerschit["Time"];
            //        Lbshow.Items.Add(st);
            //    }
            //    m_dbConnection.Close();
        }


    }
}
