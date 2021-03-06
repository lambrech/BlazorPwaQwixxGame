namespace BlazorPwaQwixxGame.Pages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using BlazorPro.BlazorSize;
    using Microsoft.AspNetCore.Components;
    using Microsoft.JSInterop;

    public partial class Index : IDisposable
    {
        // We can also capture the browser's width / height if needed. We hold the value here.
        private BrowserWindowSize browser = new();

        public Index()
        {
            this.Game = new Game();
        }

        [Inject]
        public ResizeListener Listener { get; private set; } = null!;

        [Inject]
        public IJSRuntime JsRuntime { get; private set; } = null!;

        public Game Game { get; }

        public int Size { get; set; }

        public double? DevicePixelRatio { get; set; }

        void IDisposable.Dispose()
        {
            // Always use IDisposable in your component to unsubscribe from the event.
            // Be a good citizen and leave things how you found them. 
            // This way event handlers aren't called when nobody is listening.
            this.Listener.OnResized -= this.WindowResized;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                this.DevicePixelRatio = await this.JsRuntime.InvokeAsync<double>("getWindowDevicePixelRatio");

                // Subscribe to the OnResized event. This will do work when the browser is resized.
                this.Listener.OnResized += this.WindowResized;
            }
        }

        // This method will be called when the window resizes.
        // It is ONLY called when the user stops dragging the window's edge. (It is already throttled to protect your app from perf. nightmares)
        private void WindowResized(object? _, BrowserWindowSize window)
        {
            // Get the browser's width / height
            this.browser = window;
            this.CalcSize();
        }

        private void CalcSize()
        {
            var maxPartWidth = (this.browser.Width / 13) - 8;
            var maxPartHeight = (int)((this.browser.Height * 0.18) - 16);

            this.Size = maxPartWidth < maxPartHeight ? maxPartWidth : maxPartHeight;
            this.StateHasChanged();
        }
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

        public int RowScore => this.CalcRowScore();

        public void ToggleClose()
        {
            this.Closed = !this.Closed;
            if (!this.Closed)
            {
                this.GotClosingPoint = false;
                this.RowNumbers.Last().Checked = false;
            }

            this.Game.CheckGameFinished();
        }

        private int CalcRowScore()
        {
            var score = 0;

            var checkedNumbers = this.RowNumbers.Where(x => x.Checked).ToList();

            for (var i = 0; i < checkedNumbers.Count; i++)
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

        public bool ShowScoreOnDialog { get; set; }

        public int TotalScore => this.CalcTotalScore();

        public int TotalFailScore => this.FailRolls.Count(x => x.Checked) * 5;

        public void ToggleScoreVisibility()
        {
            this.ScoreVisible = !this.ScoreVisible;
        }

        public void CheckGameFinished()
        {
            if ((this.FailRolls.Count(x => x.Checked) == 4) || (this.GameRows.Count(x => x.Closed) > 1))
            {
                this.OpenDialog(true);
            }
        }

        public void OpenDialog(bool showScore)
        {
            if (showScore)
            {
                this.ShowScoreOnDialog = true;
            }

            this.ShowDialog = true;
        }

        public void CloseDialog(bool reset)
        {
            this.ShowDialog = false;

            if (reset)
            {
                this.ResetGame();
            }
        }

        public void ResetGame()
        {
            var redRow = new GameRow(this, "Red", new List<int>(new[] { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }));
            var yellowRow = new GameRow(this, "Orange", new List<int>(new[] { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }));
            var greenRow = new GameRow(this, "Green", new List<int>(new[] { 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 }));
            var blueRow = new GameRow(this, "Blue", new List<int>(new[] { 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 }));

            this.GameRows = new List<GameRow>(new[] { redRow, yellowRow, greenRow, blueRow });

            this.FailRolls = new List<FailRoll>(new[] { new FailRoll(this), new FailRoll(this), new FailRoll(this), new FailRoll(this) });
            this.ShowScoreOnDialog = false;
        }

        private int CalcTotalScore()
        {
            var sum = this.GameRows.Sum(x => x.RowScore);
            var failRolls = this.TotalFailScore;

            return sum - failRolls;
        }
    }
}