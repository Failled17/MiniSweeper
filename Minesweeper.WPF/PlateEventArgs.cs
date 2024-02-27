using System;

namespace Minesweeper.WPF
{
    public class PlateEventArgs : EventArgs
    {
        public int PlateRow { get; set; }
        public int PlateColumn { get; set; }

        public PlateEventArgs(int row, int col)
        {
            this.PlateRow = row;
            this.PlateColumn = col;
        }
    }
}
