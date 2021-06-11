namespace BlazorPwaQwixxGame.Pages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.JSInterop;

    public partial class Index
    {
        public Index()
        {
            this.Game = new Game();
        }

        public Game Game { get; set; }
    }

    public class RowNumber
    {
        public RowNumber(GameRow correspondingGameRow, int number)
        {
            this.CorrespondingGameRow = correspondingGameRow;
            this.Number = number;
        }

        public GameRow CorrespondingGameRow { get; }

        public int Number { get; }

        public bool Checked { get; set; }

        public bool CanBeToggled => this.CalcCanBeToggled();

        public void Toggle()
        {
            if (this.CanBeToggled)
            {
                this.Checked = !this.Checked;
            }
        }

        private bool CalcCanBeToggled()
        {
            if (this.Checked)
            {
                return this == this.CorrespondingGameRow.RowNumbers.LastOrDefault(x => x.Checked);
            }

            var index = this.CorrespondingGameRow.RowNumbers.IndexOf(this);

            if (index < 0)
            {
                return false;
            }

            // last number
            if (index + 1 == this.CorrespondingGameRow.RowNumbers.Count)
            {
                return this.CorrespondingGameRow.CanBeClosed;
            }

            // otherwise skip until index of this + 1, and check if any item after is already checked
            return !this.CorrespondingGameRow.RowNumbers.Skip(index + 1).Any(x => x.Checked);
        }
    }

    public class GameRow
    {
        public GameRow(string color, List<int> numbers)
        {
            this.Color = color;
            this.RowNumbers = numbers.Select(x => new RowNumber(this, x)).ToList();
        }

        public string Color { get; }

        public List<RowNumber> RowNumbers { get; }

        public bool Closed { get; set; }

        public bool GotClosingPoint { get; set; }

        public bool CanBeClosed => this.RowNumbers.Count(x => x.Checked) > 4;
    }

    public class Game
    {
        public Game()
        {
            var redRow = new GameRow("Red", new List<int>(new[] { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }));
            var yellowRow = new GameRow("Orange", new List<int>(new[] { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }));
            var greenRow = new GameRow("Green", new List<int>(new[] { 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 }));
            var blueRow = new GameRow("Blue", new List<int>(new[] { 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 }));

            this.GameRows = new List<GameRow>(new[] { redRow, yellowRow, greenRow, blueRow });
        }

        public List<GameRow> GameRows { get; }
    }
}
