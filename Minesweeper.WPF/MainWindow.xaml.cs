using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Minesweeper.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int nrMines;
        public MinesGrid Mines { get; private set; }
        private bool gameStarted;
        private Color[] mineText;


        public MainWindow()
        {
            InitializeComponent();
            gameStarted = false;
            this.nrMines = 15;
            mineText = new Color[] {Colors.White /* первое не имеет значения */, 
                                    Colors.Blue, Colors.DarkGreen, Colors.Red, Colors.DarkBlue, 
                                    Colors.DarkViolet, Colors.DarkCyan, Colors.Brown, Colors.Black };
            GameSetup();
        }

        private void MenuItem_Click_New(object sender, RoutedEventArgs e)
        {
            GameSetup();
        }

        private void GameSetup()
        {
            Mines = new MinesGrid(10, 10, nrMines);
            foreach (Button btn in ButtonsGrid.Children)
            {
                btn.Content = ""; // удаляет изображение флага или бомбы (если таковые имеются)
                btn.IsEnabled = true; // кнопка становится кликабельной
            }
            // Присоединяет событие индикатора мин
            Mines.CounterChanged += OnCounterChanged;
            MinesIndicator.Text = nrMines.ToString();

            // Прикрепляет щелчок кнопки, вызываемый тарелкой
            Mines.ClickPlate += OnClickPlate;

            // Присоединяет событие с истекшим пороговым значением времени
            Mines.TimerThresholdReached += OnTimeChanged;            
            TimeIndicator.Text = "0";

            Mines.Run();
            gameStarted = true;
        }

        private void MenuItem_Click_Exit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown(); // закрывает приложение
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender; // получает ссылку на нажатую кнопку
            int row = ParseButtonRow(btn);
            int col = ParseButtonColumn(btn);
            if (!Mines.IsInGrid(row, col)) throw new MinesweeperException("Invalid Button to MinesGrid reference on reveal"); // кнопка указывает на недопустимую табличку
            if (Mines.IsFlagged(row, col)) return; // помеченная табличка не может быть обнаружена

            btn.IsEnabled = false; // отключает кнопку
            if (Mines.IsBomb(row, col)) //была обнаружена бомба!!!
            {
                // прикрепляет изображение бомбы к кнопке
                StackPanel sp = new StackPanel();
                sp.Orientation = Orientation.Horizontal;
                Image bombImage = new Image();
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = new Uri(@"..\..\Bomb.png", UriKind.Relative);
                bi.EndInit();
                bombImage.Source = bi;
                sp.Children.Add(bombImage);
                btn.Content = sp;
                Mines.Stop();

                // заканчивает игру и открывает все плитки
                if (gameStarted)
                {
                    gameStarted = false;
                    foreach (Button butn in ButtonsGrid.Children)
                    {
                        if (butn.IsEnabled) this.Button_Click(butn, e); // вызывает все остальные нераскрытые кнопки
                    }
                    MessageBox.Show("Вы проиграли!");
                }
                
            }
            else // открылось пустое пространство
            {
                int count = Mines.RevealPlate(row, col); // открывает тарелку и проверяет, нет ли поблизости бомб
                if (count > 0) // поместите соответствующую метку на текущую кнопку
                {
                    btn.Foreground = new SolidColorBrush(mineText[count]);
                    btn.FontWeight = FontWeights.Bold;
                    btn.Content = count.ToString();
                }
            }
        }

        private void Right_Button_Click(object sender, MouseButtonEventArgs e)
        {
            Button btn = (Button)sender; // получает ссылку на нажатую кнопку
            int row = ParseButtonRow(btn);
            int col = ParseButtonColumn(btn);
            if (!Mines.IsInGrid(row, col)) throw new MinesweeperException("Invalid Button to MinesGrid reference on flag"); // кнопка указывает на недопустимую табличку

            if (Mines.IsFlagged(row, col)) // кнопка имеет дочернее изображение флага
            {
                btn.Content = ""; // очищает изображение флага
            }
            else
            {
                // прикрепляет изображение флага к кнопке
                StackPanel sp = new StackPanel();
                sp.Orientation = Orientation.Horizontal;
                Image flagImage = new Image();
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = new Uri(@"..\..\Flag.png", UriKind.Relative);
                bi.EndInit();
                flagImage.Source = bi;
                sp.Children.Add(flagImage);
                btn.Content = sp;
            }

            Mines.FlagMine(row, col);
        }

        private int ParseButtonRow(Button btn)
        {
            // Формат названия кнопки должен быть "ButtonXY" или "ButtonXXYY", где X и Y - числовые индексы ячейки mine
            if (btn.Name.IndexOf("Button") != 0) throw new MinesweeperException("Wrong button name in UI module"); // кнопка не та
            return int.Parse(btn.Name.Substring(6, (btn.Name.Length - 6) / 2));
        }

        private int ParseButtonColumn(Button btn)
        {
            // Формат названия кнопки должен быть "ButtonXY" или "ButtonXXYY", где X и Y - числовые индексы ячейки mine
            if (btn.Name.IndexOf("Button") != 0) throw new MinesweeperException("Wrong button name in UI module"); // кнопка не та
            return int.Parse(btn.Name.Substring(6 + (btn.Name.Length - 6) / 2, (btn.Name.Length - 6) / 2));
        }

        private void OnCounterChanged(object sender, EventArgs e)
        {
            // Обновляет поле MineIndicator в пользовательском интерфейсе
            MinesIndicator.Text = (this.nrMines - Mines.FlaggedMines).ToString();
        }

        private void OnTimeChanged(object sender, EventArgs e)
        {
            // Обновляет поле MineIndicator в пользовательском интерфейсе
            TimeIndicator.Text = Mines.TimeElapsed.ToString();
        }

        private void OnClickPlate(object sender, PlateEventArgs e)
        {
            //Открывает запрошенную табличку имитирующим нажатием кнопки
            string btnName = "Button";
            if (Mines.Width <= 10 && Mines.Height <= 10) btnName += String.Format("{0:D1}{1:D1}", e.PlateRow, e.PlateColumn); // однозначные координаты
            else btnName += String.Format("{0:D2}{1:D2}", e.PlateRow, e.PlateColumn); // двузначные координаты

            Button senderButton = (ButtonsGrid.FindName(btnName) as Button);
            if (senderButton == null) throw new MinesweeperException("Invalid Button to MinesGrid reference on multiple reveal"); //табличка указывает на недопустимую кнопку

            // вызывает соответствующий обработчик события "Нажатие кнопки"
            this.Button_Click(senderButton, new RoutedEventArgs());
        }
    }
}
