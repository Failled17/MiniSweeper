using System;

namespace Minesweeper.WPF
{
    public interface IGame
    {
        //события, связанные с игрой
        event EventHandler CounterChanged;
        event EventHandler TimerThresholdReached;
        event EventHandler<PlateEventArgs> ClickPlate;

        void Run(); // игра должна быть запущена
    }
}
