using TicTacToeGame.BotCompetition.Enums;
using TicTacToeGame.Players.Enums;

namespace TicTacToeGame.BotCompetition.Utils
{
    internal class BotPair
    {
        public BotKind FirstPlayer { get; set; }
        public BotKind SecondPlayer { get; set; }
        public MatchResult MatchResult { get; set; }
    }
}
