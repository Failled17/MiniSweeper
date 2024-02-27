using System;

namespace Minesweeper.WPF
{
    interface IPlate
    {
        // Пластина должна иметь фиксаторы положения
        int RowPosition { get; }
        int ColPosition { get; }
    }
}
