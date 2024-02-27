using System;
using System.Windows;
using System.Windows.Threading;

namespace Minesweeper.WPF
{
    public class MinesGrid : IGame
    {
        //события, связанные с делегатами EventHandler
        public event EventHandler CounterChanged;
        public event EventHandler TimerThresholdReached;
        public event EventHandler<PlateEventArgs> ClickPlate;

        //поля/свойства
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Mines { get; private set; }
        public int TimeElapsed { get; private set; }
        private Plate[,] plates;
        private int correctFlags;
        private int wrongFlags;
        public int FlaggedMines { get { return (this.correctFlags + this.wrongFlags); } }
        private DispatcherTimer gameTimer;

        //конструктор
        public MinesGrid(int width, int height, int mines)
        {
            this.Width = width;
            this.Height = height;
            this.Mines = mines;
        }
        //способ проверить, находится ли текущая позиция внутри сетки
        public bool IsInGrid(int rowPosition, int colPosition)
        {
            return ((rowPosition >= 0) && (rowPosition < this.Width) && (colPosition >= 0) && (colPosition < this.Height));
        }

        //способ проверить, заминирована ли текущая позиция
        public bool IsBomb(int rowPosition, int colPosition)
        {
            if (this.IsInGrid(rowPosition, colPosition))
            {
                return this.plates[rowPosition, colPosition].IsMined;
            }
            return false;
        }

        //способ проверить, заминирована ли текущая позиция
        public bool IsFlagged(int rowPosition, int colPosition)
        {
            if (this.IsInGrid(rowPosition, colPosition))
            {
                return this.plates[rowPosition, colPosition].IsFlagged;
            }
            return false;
        }
        //метод определения статуса текущей ячейки
        //перенаправляет на табличку.Проверьте(), чтобы определить, заминирована ли ячейка или сколько мин вокруг нее
        public int RevealPlate(int rowPosition, int colPosition)
        {
            if (this.IsInGrid(rowPosition, colPosition))
            {
                int result = this.plates[rowPosition, colPosition].Check(); // проверяет количество окружающих шахт
                CheckFinish(); // проверяет окончание игры
                return result;
            }
            throw new MinesweeperException("Invalid MinesGrid reference call [row, column] on reveal");
        }

        //способ установки или удаления флага, если выбрана какая-либо ячейка
        public void FlagMine(int rowPosition, int colPosition)
        {
            if (!this.IsInGrid(rowPosition, colPosition))
            {
                throw new MinesweeperException("Invalid MinesGrid reference call [row, column] on flag");
            }

            Plate currPlate = this.plates[rowPosition, colPosition];
            if (!currPlate.IsFlagged)
            {
                if (currPlate.IsMined)
                {
                    this.correctFlags++;
                }
                else
                {
                    this.wrongFlags++;
                }
            }
            else
            {
                if (currPlate.IsMined)
                {
                    this.correctFlags--;
                }
                else
                {
                    this.wrongFlags--;
                }
            }

            currPlate.IsFlagged = !currPlate.IsFlagged; // обновляет помеченное значение
            CheckFinish(); // проверяет окончание игры
            // Вызывает событие CounterChanged
            this.OnCounterChanged(new EventArgs());
        }

        //способ вскрытия точной одиночной пластины
        public void OpenPlate(int rowPosition, int colPosition)
        {
            // Проверяет, не открыта ли еще табличка
            if (this.IsInGrid(rowPosition, colPosition) && !this.plates[rowPosition, colPosition].IsRevealed)
            {
                // затем вызывает событие ClickPlate с данными о положении пластины
                this.OnClickPlate(new PlateEventArgs(rowPosition, colPosition));
            }
        }

        //способ проверить, полностью ли решена проблема с доской
        private void CheckFinish()
        {
            bool hasFinished = false; // предполагается, что игра еще не закончена
            if (this.wrongFlags == 0 && this.FlaggedMines == this.Mines) // у нас больше нет никаких флагов, которые можно было бы поставить
            {
                hasFinished = true; // предполагается, что все пластины раскрыты
                foreach (Plate item in this.plates)
                {
                    if (!item.IsRevealed && !item.IsMined)
                    {
                        hasFinished = false; // если тарелка не обнаружена, то игра не закончена
                        break;
                    }
                }
                MessageBox.Show("Вы выиграли");
            }

            if (hasFinished) 
                gameTimer.Stop(); // когда игра закончится, таймер должен быть немедленно остановлен
            
        }

        //способ создания игры
        public void Run()
        {
            this.correctFlags = 0;
            this.wrongFlags = 0;
            this.TimeElapsed = 0;

            this.plates = new Plate[Width, Height];

            for (int row = 0; row < Width; row++)
            {
                for (int col = 0; col < Height; col++)
                {
                    Plate cell = new Plate(this, row, col);
                    this.plates[row, col] = cell;
                }
            }

            int minesCounter = 0;
            Random minesPosition = new Random();

            while (minesCounter < Mines)
            {
                int row = minesPosition.Next(Width);
                int col = minesPosition.Next(Height);

                Plate cell = this.plates[row, col];

                if (!cell.IsMined)
                {
                    cell.IsMined = true;
                    minesCounter++;
                }
            }

            gameTimer = new DispatcherTimer();
            gameTimer.Tick += new EventHandler(OnTimeElapsed);
            gameTimer.Interval = new TimeSpan(0, 0, 1);
            gameTimer.Start();            
        }

        // способ остановить игру
        public void Stop()
        {
            gameTimer.Stop();
        }

        // Организатор мероприятия "Счетчик флагов изменен"
        protected virtual void OnCounterChanged(EventArgs e)
        {
            EventHandler handler = CounterChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        // Организатор мероприятия "Счетчик времени изменен"
        protected virtual void OnTimeElapsed(object sender, EventArgs e)
        {
            this.TimeElapsed++;
            EventHandler handler = TimerThresholdReached;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        // Организатор мероприятия "Нажмите, чтобы открыть тарелку" - используется для автоматического открытия всех пустых тарелок в регионе
        protected virtual void OnClickPlate(PlateEventArgs e)
        {
            EventHandler<PlateEventArgs> handler = ClickPlate;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
