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
        public RowNumber(GameRow correspondingGameRow, int number, bool isLast = false)
        {
            this.CorrespondingGameRow = correspondingGameRow;
            this.Number = number;
            this.IsLast = isLast;
        }

        public bool IsLast { get; }

        public GameRow CorrespondingGameRow { get; }

        public int Number { get; }

        public bool Checked { get; set; }

        public bool CanBeToggled => this.CalcCanBeToggled();

        public void Toggle()
        {
            if (this.CanBeToggled)
            {
                this.Checked = !this.Checked;

                if (this.IsLast && this.Checked)
                {
                    this.CorrespondingGameRow.Closed = true;
                    this.CorrespondingGameRow.GotClosingPoint = true;
                    this.CorrespondingGameRow.Game.CheckGameFinished();
                }
            }
        }

        private bool CalcCanBeToggled()
        {
            if (this.CorrespondingGameRow.Closed)
            {
                return false;
            }

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
                return this.CorrespondingGameRow.CanBeClosedByLastNumber;
            }

            // otherwise skip until index of this + 1, and check if any item after is already checked
            return !this.CorrespondingGameRow.RowNumbers.Skip(index + 1).Any(x => x.Checked);
        }
    }

    public class GameRow
    {
        public GameRow(Game game, string color, List<int> numbers)
        {
            this.Game = game;
            this.Color = color;
            this.RowNumbers = numbers.Select(x => new RowNumber(this, x, numbers.IndexOf(x) == numbers.Count - 1)).ToList();
        }

        public Game Game { get; }

        public string Color { get; }

        public List<RowNumber> RowNumbers { get; }

        public bool Closed { get; set; }

        public bool GotClosingPoint { get; set; }

        public bool CanBeClosedByLastNumber => this.RowNumbers.Count(x => x.Checked) > 4;

        public void ToggleClose()
        {
            this.Closed = !this.Closed;
            if (!this.Closed)
            {
                this.GotClosingPoint = false;
            }

            this.Game.CheckGameFinished();
        }

        public int RowScore => this.CalcRowScore();

        private int CalcRowScore()
        {
            var score = 0;

            var checkedNumbers = this.RowNumbers.Where(x => x.Checked).ToList();

            for (int i = 0; i < checkedNumbers.Count; i++)
            {
                score += i + 1;
            }

            if (this.GotClosingPoint)
            {
                score += checkedNumbers.Count + 1;
            }

            return score;
        }
    }

    public class FailRoll
    {
        public FailRoll(Game game)
        {
            this.Game = game;
        }

        public Game Game { get; }

        public bool Checked { get; set; }

        public void ToggleChecked()
        {
            this.Checked = !this.Checked;
            this.Game.CheckGameFinished();
        }
    }

    public class Game
    {
        public Game()
        {
            this.ResetGame();
        }

        public bool ShowDialog { get; set; }

        public List<GameRow> GameRows { get; private set; } = null!;

        public List<FailRoll> FailRolls { get; private set; } = null!;

        public bool ScoreVisible { get; set; }

        public void ToggleScoreVisibility()
        {
            this.ScoreVisible = !this.ScoreVisible;
        }

        public void CheckGameFinished()
        {
        }

        public int TotalScore => this.CalcTotalScore();

        public int TotalFailScore => this.FailRolls.Count(x => x.Checked) * 5;

        private int CalcTotalScore()
        {
            var sum = this.GameRows.Sum(x => x.RowScore);
            var failRolls = this.TotalFailScore;

            return sum - failRolls;
        }

        public void ResetGame()
        {
            var redRow = new GameRow(this, "Red", new List<int>(new[] { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }));
            var yellowRow = new GameRow(this, "Orange", new List<int>(new[] { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }));
            var greenRow = new GameRow(this, "Green", new List<int>(new[] { 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 }));
            var blueRow = new GameRow(this, "Blue", new List<int>(new[] { 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 }));

            this.GameRows = new List<GameRow>(new[] { redRow, yellowRow, greenRow, blueRow });

            this.FailRolls = new List<FailRoll>(new[] { new FailRoll(this), new FailRoll(this), new FailRoll(this), new FailRoll(this) });
        }
    }
}
