using System;

namespace Minesweeper.WPF
{
    public class Plate : IPlate
    {

        //автоматические свойства (поля)
        public MinesGrid GameGrid { get; private set; }
        public int RowPosition { get; private set; }
        public int ColPosition { get; private set; }
        public bool IsFlagged { get; set; }
        public bool IsMined { get; set; }
        public bool IsRevealed { get; private set; }

        //constructor
        public Plate(MinesGrid grid, int rowPosition, int colPosition)
        {
            this.GameGrid = grid;
            this.RowPosition = rowPosition;
            this.ColPosition = colPosition;
        }

        //метод подсчета мин вокруг текущей ячейки и присвоения ей номера в зависимости от количества ячеек
        //если вокруг нее нет мин, перенаправьте на метод MinesGrid.RevealPlate, чтобы проверить все ячейки вокруг на наличие мин вокруг них
        public int Check()
        {
            int counter = 0;

            if (!IsRevealed && !IsFlagged)
            {
                IsRevealed = true;

                for (int i = 0; i < 9; i++) // проверьте всех соседей на наличие бомб 
                {
                    if (i == 4) continue; // не проверяйте себя
                    if (GameGrid.IsBomb(RowPosition + i / 3 - 1, ColPosition + i % 3 - 1)) counter++; // если есть бомба, подсчитайте ее
                }

                if (counter == 0)
                {
                    for (int i = 0; i < 9; i++) // проверьте всех соседей на наличие бомб
                    {
                        if (i == 4) continue; // не проверяйте себя
                        GameGrid.OpenPlate(RowPosition + i / 3 - 1, ColPosition + i % 3 - 1); // выявить всех соседей
                    }
                }
            }

            return counter;
        }
    }
}
