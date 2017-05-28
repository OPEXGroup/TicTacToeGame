using System.Text;
using TicTacToeGame.Players.Enums;

namespace TicTacToeGame.BotCompetition.Competition
{
    internal class CompetitionResult
    {
        public BotKind FirstPlayer { get; set; }
        public BotKind SecondPlayer { get; set; }
        public int RunCount { get; set; }

        public WinLossDistribution FirstPlayerXStats { get; set; }
        public WinLossDistribution FirstPlayerOStats { get; set; }
        public bool FailedToCreate { get; set; }
        public BotKind? Winner { get; set; }
        public BotKind? Loser { get; set; }

        public override string ToString()
        {
            const int padding = 6;

            var builder = new StringBuilder();

            builder.AppendLine($"\n{FirstPlayer} vs {SecondPlayer} stats:");
            builder.AppendLine($"\t{FirstPlayer} as X:");
            builder.AppendLine($"\t\tWins   : {FirstPlayerXStats.Wins,padding} of {FirstPlayerXStats.Total}");
            builder.AppendLine($"\t\tLosses : {FirstPlayerXStats.Losses,padding} of {FirstPlayerXStats.Total}");
            builder.AppendLine($"\t\tDraws  : {FirstPlayerXStats.Draws,padding} of {FirstPlayerXStats.Total}");
            builder.AppendLine($"\t{FirstPlayer} as O:");
            builder.AppendLine($"\t\tWins   : {FirstPlayerOStats.Wins,padding} of {FirstPlayerOStats.Total}");
            builder.AppendLine($"\t\tLosses : {FirstPlayerOStats.Losses,padding} of {FirstPlayerOStats.Total}");
            builder.AppendLine($"\t\tDraws  : {FirstPlayerOStats.Draws,padding} of {FirstPlayerOStats.Total}");
            var flawless = (FirstPlayerXStats.Losses == 0 && FirstPlayerOStats.Losses == 0)
                || (FirstPlayerXStats.Losses == RunCount && FirstPlayerOStats.Losses == RunCount);
            var winnerString = Winner == null ? "Draw" : $"{Winner} won.";
            if (flawless && Winner != null)
                winnerString += " FLAWLESS VICTORY";
            builder.AppendLine($"Result: {winnerString}");

            return builder.ToString();
        }
    }
}
