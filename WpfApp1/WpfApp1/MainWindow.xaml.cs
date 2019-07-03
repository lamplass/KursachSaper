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
using System.Media;
using System.Windows.Controls.Primitives;
using System.Security;
using Microsoft.Win32;
using static System.IO.Path;



namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        BitmapImage mine; // картнка мины 
        Generator gen = new Generator(); // генерирует поле 
        Generator_h gen1 = new Generator_h();
        Generator_l gen2 = new Generator_l();
        int kolvopust = 0; // количество пустых
        public int kolvomin ;// нормальные мины
        public int kolvomin1 ;// сложные ины

        public int kolvomin2; // легкие мины 
        
        System.Windows.Threading.DispatcherTimer Timer;
        DateTime start;  // ТАЙМЕР 
        SoundPlayer sp;  // WIN
        SoundPlayer sp2; // LOSER
        SoundPlayer sp3; // START 
        SoundPlayer sp4; // GAME  

        SQLiteConnection m_dbConnection;

        private int _currentLevel = 0; //Выбранный уровень сложности
        private int _elapsedSeconds; //Сколько секунд прошло с начала игры

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Timer.Stop();
            lbtimer.Visibility = Visibility.Hidden;
            lbname.Visibility = Visibility;
            tbname.Visibility = Visibility;
            setka.IsEnabled = false;
        }


        public class CTest 
        {
            public string Name { get; set; } //Имя
            public int Level { get; set; } //Уровень (число от 1 до 3)
            public int Time { get; set; } //Количество секунд
            


        //Отформатированное время, получаем время из секунд и преобразовываем в строку
        public string FormattedTime => TimeSpan.FromSeconds(Time).ToString(@"mm\:ss");

            //Отформатированный уровень, т.к. в базе уровень хранится в числах, а мы хотим всё красиво отображать,
            //требуется преобразование
            public string FormattedLevel => Level == 1 ? "Легкий" : Level == 2 ? "Средний" : "Сложный";
        }

        private void RefreshData() 
        {
            try //Пытаемся получить данные
            {
                //Формируем запрос
                string sql = "SELECT * FROM igroki "; //Получаем всех игроков из таблицы
                if (_currentLevel > 0) //Если уровень выбран
                    sql += $"WHERE Level = {_currentLevel} "; //Добавляем фильтрацию по уровню
                sql += "ORDER BY Time"; //Добавляем сортировку по времени

                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection); //Создаём команду, используя наш запрос и открытое соединение
                SQLiteDataReader reader = command.ExecuteReader(); //Выполняем запрос и получаем поток для чтения данных
                var list = new List<CTest>(); //Создаём список наших объектов
                while (reader.Read()) //И читаем поток, пока можно
                {
                    //Каждая итерация этого цикла обрабатывает очередную строку из таблицы
                    var record = new CTest //Создаём экземпляр класса
                    {
                        Name = reader["Name"].ToString(), //Заполняем имя
                        Level = int.Parse(reader["Level"].ToString()), //Уровень
                        Time = int.Parse(reader["Time"].ToString()) //Время
                    };
                    list.Add(record); //Добавляем экземпляр в список
                }
                lbshow.AutoGenerateColumns = false; //Отключаем генерацию столбцов у датагрида
                lbshow.ItemsSource = list; //Устанавливаем источник данных
            }
            catch (Exception exception) //Если не удалось получить данные
            {
                MessageBox.Show("Не удалось получить данные");
            }
        }

        private void UpdateData() 
        {
            try
            {
                var name = tbname.Text; //Получаем имя пользователя
                //Сейчас мы должны проверить, есть ли у этого пользователя на этом уровне рекорд лучше, чем текущий
                //Т.е. если я когда-то прошел игру за 10 секунд, а сейчас за 15, то обновлять таблицу не нужно
                var command = new SQLiteCommand("SELECT COUNT(*) FROM igroki " + //Получаем из таблицы количество таких записей
                                                $"WHERE Name = \"{name}\" " + //В которых имя такое, как у пользователя
                                                $"AND Level = {_currentLevel} " + //И уровень такой, какой сейчас выбран
                                                $"AND Time < {_elapsedSeconds}", m_dbConnection); //И времени было затрачено меньше, чем сейчас

                var count = int.Parse(command.ExecuteScalar().ToString()); //Выполняем команду, на выходе получаем число - количество таких записей

                if (count > 0) return; //Если такая запись есть, то обновлять ничего не нужно, выходим из метода
                //Если не вышли, значит нужно обновлять таблицу и изменять/добавлять рекорд
                //Для этого воспользуемся конструкцией INSERT OR REPLACE, она вставит (если этот пользователь не был в таблице на этом уровне)
                //Или заменит (если этот пользователь уже есть в таблице на этом уровне)
                string sql = $"INSERT OR REPLACE INTO igroki (Name, Level, Time) VALUES (\"{tbname.Text}\", {_currentLevel}, {_elapsedSeconds})";
                command = new SQLiteCommand(sql, m_dbConnection); //Формируем команду, используем запрос и подключение
                command.ExecuteNonQuery(); //Выполняем команду
                RefreshData(); //Обновляем датагрид
            }
            catch (Exception exception)
            {
                MessageBox.Show("Не удалось загрузить данные");
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                kolvomin2 = Int32.Parse(tbmin.Text);
                if (kolvomin2 > 6)
                {
                    MessageBox.Show("Слишком много мин, max - 6");
                    return;
                }
            }
            catch
            { MessageBox.Show("Введите цифры!"); }

            try
            {
                kolvomin1 = Int32.Parse(tbmin1.Text);
                if (kolvomin1 > 23)
                {
                    MessageBox.Show("Слишком много мин, max - 23");
                    return;
                }
            }

            catch
            { MessageBox.Show("Введите цифры!"); }

            try
            {
                kolvomin = Int32.Parse(tbmin2.Text);
                if (kolvomin > 13)
                {
                    MessageBox.Show("Слишком много мин, max - 13");
                    return;
                }
            }

            catch
            { MessageBox.Show("Введите цифры!"); }


            mine = new BitmapImage(new Uri(@"images\bomb.jpg", UriKind.Relative)); // ставим изображение мины
            sp = new SoundPlayer(Properties.Resources.win);//адрес звукового файла
            sp2 = new SoundPlayer(Properties.Resources.loser);//адрес звукового файла №2
            sp3 = new SoundPlayer(Properties.Resources.start);//адрес звукового файла №3
            sp4 = new SoundPlayer(Properties.Resources.game);//адрес звукового файла №4
            ////Разворачивается как C:\KursachSaper-master2\WpfApp1\WpfApp1\bin\Debug\records.db, например
            //var databasePath = $@"{Environment.CurrentDirectory}\records.db";
           // _mDbConnection = new SQLiteConnection($"Data Source=records.db;Version=3;"); //Здесь указывается путь к базе
            m_dbConnection = new SQLiteConnection($"Data Source=records.db;Version=3;");
            m_dbConnection.Open();
            RefreshData(); //Обновляем грид
        }

        public void gena()
        {
            if ((25 - kolvopust) == kolvomin)
            {
                Timer.Stop();
                sp.Play();
                MessageBox.Show("Вы выйграли");
                UpdateData();
            }
        }

        public void gena1()
        {
            if ((49 - kolvopust) == kolvomin1)
            {
                Timer.Stop();
                sp.Play();
                MessageBox.Show("Вы выйграли");
                UpdateData();
            }
        }

        public void gena2()
        {
            if ((9 - kolvopust) == kolvomin1)
            {
                Timer.Stop();
                sp.Play();
                MessageBox.Show("Вы выйграли");
                UpdateData();
            }
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            string st = "";
            TimeSpan ts = ts = DateTime.Now - start;
            st += ts.Minutes.ToString() + ":" + ts.Seconds.ToString();
            _elapsedSeconds = (int) ts.TotalSeconds;
            lbtimer.Content = st;
        }

        private void b1_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                kolvomin = Int32.Parse(tbmin2.Text);
                if (kolvomin > 13)
                {
                    MessageBox.Show("Слишком много мин, max - 13");
                    return;
                }
            }

            catch
            { MessageBox.Show("Введите цифры!"); }

            _currentLevel = 2;
            RefreshData();
            lbshow.Visibility = Visibility;
            bb.Visibility = Visibility;
            lbname.Visibility = Visibility.Hidden;
            tbname.Visibility = Visibility.Hidden;
            lbtimer.Visibility = Visibility;
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
            this.Width = 845;//задаем размер окошка по ширене
            this.Height = 630;//задаем размер по высоте
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

        private void b2_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                kolvomin1 = Int32.Parse(tbmin1.Text);
                if (kolvomin1 > 23)
                {
                    MessageBox.Show("Слишком много мин, max - 23");
                    return;
                }
            }

            catch
            { MessageBox.Show("Введите цифры!"); }

            _currentLevel = 3;
            RefreshData();
            lbshow.Visibility = Visibility;
            bb.Visibility = Visibility;
            lbname.Visibility = Visibility.Hidden;
            tbname.Visibility = Visibility.Hidden;
            lbtimer.Visibility = Visibility;
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
            gen1.init(7);// размер поля в нашем случае 5 на 5 
            gen1.plantMines(kolvomin1);// количество мин 
            gen1.calculate();// раставляет мины
            //указыается количество строк и столбцов в сетке
            setka.Rows = 7;
            setka.Columns = 7;
            //указываются размеры сетки (число ячеек * (размер кнопки в ячейки + толщина её границ))
            setka.Width = 7 * (50 + 4);// по ширене 
            setka.Height = 7 * (50 + 4);// по высоте
            //толщина границ сетки
            setka.Margin = new Thickness(3);//расстояние между ними
            this.Width = 845;//задаем размер окошка по ширене
            this.Height = 630;//задаем размер по высоте
            for (int i = 0; i < 7 * 7; i++)
            {
                //создание кнопки
                Button bth = new Button();
                //запись номера кнопки
                bth.Tag = i;
                //установка размеров кнопки
                bth.Width = 50;
                bth.Height = 50;
                //текст на кнопке
                bth.Content = " ";
                //толщина границ кнопки
                bth.Margin = new Thickness(2);
                //при нажатии кнопки, будет вызываться метод Bth_Click
                bth.Click += Bth_Click;
                //добавление кнопки в сетку
                setka.Children.Add(bth);
            }
        }

        private void b3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                kolvomin2 = Int32.Parse(tbmin.Text);
                if (kolvomin2 > 6)
                {
                    MessageBox.Show("Слишком много мин, max - 6");
                    return;
                }
            }
            catch
            { MessageBox.Show("Введите цифры!"); }

            _currentLevel = 1;
            RefreshData();
            lbshow.Visibility = Visibility;
            bb.Visibility = Visibility;
            lbname.Visibility = Visibility.Hidden;
            tbname.Visibility = Visibility.Hidden;
            lbtimer.Visibility = Visibility;
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
            gen2.init(3);// размер поля в нашем случае 5 на 5 
            gen2.plantMines(kolvomin2);// количество мин 
            gen2.calculate();// раставляет мины
            //указыается количество строк и столбцов в сетке
            setka.Rows = 3;
            setka.Columns = 3;
            //указываются размеры сетки (число ячеек * (размер кнопки в ячейки + толщина её границ))
            setka.Width = 3 * (50 + 4);// по ширене 
            setka.Height = 3 * (50 + 4);// по высоте
            //толщина границ сетки
            setka.Margin = new Thickness(3);//расстояние между ними
            this.Width = 845;//задаем размер окошка по ширене
            this.Height = 630;//задаем размер по высоте
            for (int i = 0; i < 3 * 3; i++)
            {
                //создание кнопки
                Button l = new Button();
                //запись номера кнопки
                l.Tag = i;
                //установка размеров кнопки
                l.Width = 50;
                l.Height = 50;
                //текст на кнопке
                l.Content = " ";
                //толщина границ кнопки
                l.Margin = new Thickness(2);
                //при нажатии кнопки, будет вызываться метод Btn_Click
                l.Click += Btl_Click;
                //добавление кнопки в сетку
                setka.Children.Add(l);
            }
        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                kolvomin = Int32.Parse(tbmin2.Text);
                if (kolvomin > 13)
                {
                    MessageBox.Show("Слишком много мин, max - 13");
                    return;
                }
            }

            catch
            { MessageBox.Show("Введите цифры!"); }

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
                        gena();
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
                        gena();
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
                        gena();
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
                        gena();
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
                        gena();
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
                        gena();
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
                gena();
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
                sp2.Play();
                MessageBox.Show("Вы проиграли");
                setka.IsEnabled = false;
            }
        }

        private void Bth_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                kolvomin1 = Int32.Parse(tbmin1.Text);
                if (kolvomin1 > 23)
                {
                    MessageBox.Show("Слишком много мин, max - 23");
                    return;
                }
            }
            catch
            { MessageBox.Show("Введите цифры!"); }

            ////получение значения лежащего в Tag
            int n = (int)((Button)sender).Tag;// узнаем номер кнопки
            if (gen1.getCell(n % 7, n / 7) == 0)
            {/*если вокруг мин нету*/
                gen1.reveal(n % 7, n / 7);//открывает вокруг поля
                Button[] buts = new Button[setka.Children.Count];// создаем массив кнопок
                setka.Children.CopyTo(buts, 0);// создаем пустое поле
                for (int i = 0; i < buts.Length; i++)
                {
                    int ind = (int)(buts[i]).Tag;//узнаем номер кнопки
                    if (gen1.getCell(ind % 7, ind / 7) == 20)
                    {/*проверка на победу */
                        //установка фона нажатой кнопки, цвета и размера шрифта
                        (buts[i]).Background = Brushes.White;//фон кнопки
                        (buts[i]).Foreground = Brushes.Red;//цвет цифр
                        (buts[i]).FontSize = 23;//раз шрифта
                        //запись в нажатую кнопку её номера
                        (buts[i]).Content = 0;
                        kolvopust++;//количество открытых увеличиваем
                        gena1();
                    }
                    if (gen1.getCell(ind % 7, ind / 7) == 21)
                    {
                        //установка фона нажатой кнопки, цвета и размера шрифта
                        (buts[i]).Background = Brushes.White;
                        (buts[i]).Foreground = Brushes.Red;
                        (buts[i]).FontSize = 23;
                        //запись в нажатую кнопку её номера
                        (buts[i]).Content = 1;
                        kolvopust++;
                        gena1();
                    }
                    if (gen1.getCell(ind % 7, ind / 7) == 22)
                    {
                        //установка фона нажатой кнопки, цвета и размера шрифта
                        (buts[i]).Background = Brushes.White;
                        (buts[i]).Foreground = Brushes.Red;
                        (buts[i]).FontSize = 23;
                        //запись в нажатую кнопку её номера
                        (buts[i]).Content = 2;
                        kolvopust++;
                        gena1();
                    }
                    if (gen1.getCell(ind % 7, ind / 7) == 23)
                    {
                        //установка фона нажатой кнопки, цвета и размера шрифта
                        (buts[i]).Background = Brushes.White;
                        (buts[i]).Foreground = Brushes.Red;
                        (buts[i]).FontSize = 23;
                        //запись в нажатую кнопку её номера
                        (buts[i]).Content = 3;
                        kolvopust++;
                        gena1();
                    }
                    if (gen1.getCell(ind % 7, ind / 7) == 24)
                    {
                        //установка фона нажатой кнопки, цвета и размера шрифта
                        (buts[i]).Background = Brushes.White;
                        (buts[i]).Foreground = Brushes.Red;
                        (buts[i]).FontSize = 23;
                        //запись в нажатую кнопку её номера
                        (buts[i]).Content = 4;
                        kolvopust++;
                        gena1();
                    }
                    if (gen1.getCell(ind % 7, ind / 7) == 25)
                    {
                        //установка фона нажатой кнопки, цвета и размера шрифта
                        (buts[i]).Background = Brushes.White;
                        (buts[i]).Foreground = Brushes.Red;
                        (buts[i]).FontSize = 23;
                        //запись в нажатую кнопку её номера
                        (buts[i]).Content = 5;
                        kolvopust++;
                        gena1();
                    }
                    if (gen1.getCell(ind % 7, ind / 7) == 26)
                    {
                        //установка фона нажатой кнопки, цвета и размера шрифта
                        (buts[i]).Background = Brushes.White;
                        (buts[i]).Foreground = Brushes.Red;
                        (buts[i]).FontSize = 23;
                        //запись в нажатую кнопку её номера
                        (buts[i]).Content = 6;
                        kolvopust++;
                        gena1();
                    }
                    if (gen1.getCell(ind % 7, ind / 7) == 27)
                    {
                        //установка фона нажатой кнопки, цвета и размера шрифта
                        (buts[i]).Background = Brushes.White;
                        (buts[i]).Foreground = Brushes.Red;
                        (buts[i]).FontSize = 23;
                        //запись в нажатую кнопку её номера
                        (buts[i]).Content = 7;
                        kolvopust++;
                        gena1();
                    }
                    if (gen1.getCell(ind % 7, ind / 7) == 28)
                    {
                        //установка фона нажатой кнопки, цвета и размера шрифта
                        (buts[i]).Background = Brushes.White;
                        (buts[i]).Foreground = Brushes.Red;
                        (buts[i]).FontSize = 23;
                        //запись в нажатую кнопку её номера
                        (buts[i]).Content = 8;
                        kolvopust++;
                        gena1();
                    }
                }
            }
            else

            if (gen1.getCell(n % 7, n / 7) > 0)
            {
                //установка фона нажатой кнопки, цвета и размера шрифта
                ((Button)sender).Background = Brushes.White;
                ((Button)sender).Foreground = Brushes.Red;
                ((Button)sender).FontSize = 23;
                //запись в нажатую кнопку её номера
                ((Button)sender).Content = gen1.getCell(n % 7, n / 7);
                kolvopust++;
                gena1();
            }
            else

            if (gen1.getCell(n % 7, n / 7) == -1)
            {/*если нажали на мину*/

                Button[] buts = new Button[setka.Children.Count];//массив кнопок
                setka.Children.CopyTo(buts, 0);

                for (int j = 0; j < buts.Length; j++)
                {
                    int ind = (int)(buts[j]).Tag;//узнаем номер кнопки
                    if (gen1.getCell(ind % 7, ind / 7) == -1)
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
                        (buts[j]).Content = minePnl;
                    } 
                }
                Timer.Stop();
                sp2.Play();
                MessageBox.Show("Вы проиграли");
                setka.IsEnabled = false;
            }
        }

        private void Btl_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                kolvomin2 = Int32.Parse(tbmin.Text);
                if (kolvomin2 > 6)
                {
                    MessageBox.Show("Слишком много мин, max - 6");
                    return;
                }
            }
            catch
            { MessageBox.Show("Введите цифры!"); }

            ////получение значения лежащего в Tag
            int n = (int)((Button)sender).Tag;// узнаем номер кнопки
            if (gen2.getCell(n % 3, n / 3) == 0)
            {/*если вокруг мин нету*/
                gen2.reveal(n % 3, n / 3);//открывает вокруг поля
                Button[] buts = new Button[setka.Children.Count];// создаем массив кнопок
                setka.Children.CopyTo(buts, 0);// создаем пустое поле
                for (int i = 0; i < buts.Length; i++)
                {
                    int ind = (int)(buts[i]).Tag;//узнаем номер кнопки
                    if (gen2.getCell(ind % 3, ind / 3) == 6)
                    {/*проверка на победу */
                        //установка фона нажатой кнопки, цвета и размера шрифта
                        (buts[i]).Background = Brushes.White;//фон кнопки
                        (buts[i]).Foreground = Brushes.Red;//цвет цифр
                        (buts[i]).FontSize = 23;//раз шрифта
                        //запись в нажатую кнопку её номера
                        (buts[i]).Content = 0;
                        kolvopust++;//количество открытых увеличиваем
                        if ((9 - kolvopust) == kolvomin2)
                        {
                            Timer.Stop();
                            sp.Play();
                            MessageBox.Show("Вы выйграли");
                            UpdateData();
                        }
                    }
                    if (gen2.getCell(ind % 3, ind / 3) == 7)
                    {
                        //установка фона нажатой кнопки, цвета и размера шрифта
                        (buts[i]).Background = Brushes.White;
                        (buts[i]).Foreground = Brushes.Red;
                        (buts[i]).FontSize = 23;
                        //запись в нажатую кнопку её номера
                        (buts[i]).Content = 1;
                        kolvopust++;
                        if ((9 - kolvopust) == kolvomin2)
                        {
                            Timer.Stop();
                            sp.Play();
                            MessageBox.Show("Вы выйграли");
                            UpdateData();

                        }
                    }
                    if (gen2.getCell(ind % 3, ind / 3) == 8)
                    {
                        //установка фона нажатой кнопки, цвета и размера шрифта
                        (buts[i]).Background = Brushes.White;
                        (buts[i]).Foreground = Brushes.Red;
                        (buts[i]).FontSize = 23;
                        //запись в нажатую кнопку её номера
                        (buts[i]).Content = 2;
                        kolvopust++;
                        if ((9 - kolvopust) == kolvomin2)
                        {
                            Timer.Stop();
                            sp.Play();
                            MessageBox.Show("Вы выйграли");
                            UpdateData();

                        }
                    }
                    if (gen2.getCell(ind % 3, ind / 3) == 9)
                    {
                        //установка фона нажатой кнопки, цвета и размера шрифта
                        (buts[i]).Background = Brushes.White;
                        (buts[i]).Foreground = Brushes.Red;
                        (buts[i]).FontSize = 23;
                        //запись в нажатую кнопку её номера
                        (buts[i]).Content = 3;
                        kolvopust++;
                        if ((9 - kolvopust) == kolvomin)
                        {
                            Timer.Stop();
                            sp.Play();
                            MessageBox.Show("Вы выйграли");
                            UpdateData();
                        }
                    }
                    if (gen2.getCell(ind % 3, ind / 3) == 10)
                    {
                        //установка фона нажатой кнопки, цвета и размера шрифта
                        (buts[i]).Background = Brushes.White;
                        (buts[i]).Foreground = Brushes.Red;
                        (buts[i]).FontSize = 23;
                        //запись в нажатую кнопку её номера
                        (buts[i]).Content = 4;
                        kolvopust++;
                        if ((9 - kolvopust) == kolvomin)
                        {
                            Timer.Stop();
                            sp.Play();
                            MessageBox.Show("Вы выйграли");
                            UpdateData();
                        }
                    }
                    if (gen2.getCell(ind % 3, ind / 3) == 11)
                    {
                        //установка фона нажатой кнопки, цвета и размера шрифта
                        (buts[i]).Background = Brushes.White;
                        (buts[i]).Foreground = Brushes.Red;
                        (buts[i]).FontSize = 23;
                        //запись в нажатую кнопку её номера
                        (buts[i]).Content = 5;
                        kolvopust++;
                        if ((9 - kolvopust) == kolvomin2)
                        {
                            Timer.Stop();
                            sp.Play();
                            MessageBox.Show("Вы выйграли");
                            UpdateData();
                        }
                    }
                }
            }
            else

            if (gen2.getCell(n % 3, n / 3) > 0)
            {
                //установка фона нажатой кнопки, цвета и размера шрифта
                ((Button)sender).Background = Brushes.White;
                ((Button)sender).Foreground = Brushes.Red;
                ((Button)sender).FontSize = 23;
                //запись в нажатую кнопку её номера
                ((Button)sender).Content = gen2.getCell(n % 3, n / 3);
                kolvopust++;
                if ((9 - kolvopust) == kolvomin2)
                {
                    Timer.Stop();
                    sp.Play();
                    MessageBox.Show("Вы выйграли");
                    UpdateData();
                }
            }
            else

            if (gen2.getCell(n % 3, n / 3) == -1)
            {/*если нажали на мину*/

                Button[] buts = new Button[setka.Children.Count];//массив кнопок
                setka.Children.CopyTo(buts, 0);

                for (int i = 0; i < buts.Length; i++)
                {
                    int ind = (int)(buts[i]).Tag;//узнаем номер кнопки

                    if (gen2.getCell(ind % 3, ind / 3) == -1)
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
                sp2.Play();
                MessageBox.Show("Вы проиграли");
                setka.IsEnabled = false;
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            sp4.Play();
            sp4.PlayLooping();//метод повторного воспроизведения WAV-файла.
            stop.Visibility = Visibility;
            bstart.Visibility = Visibility.Hidden;
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            sp4.Stop();
            stop.Visibility = Visibility.Hidden;
            bstart.Visibility = Visibility;
        }
    }
}