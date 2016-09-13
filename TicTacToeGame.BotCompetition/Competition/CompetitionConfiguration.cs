using System;
using TicTacToeGame.Common;
using TicTacToeGame.Players.Enums;

namespace TicTacToeGame.BotCompetition.Competition
{
    internal class CompetitionConfiguration
    {
        /// <summary>
        ///     MUST be != SecondBotKind
        /// </summary>
        public BotKind FirstBotKind { get; set; }
        /// <summary>
        ///     MUST be != FirstBotKind
        /// </summary>
        public BotKind SecondBotKind { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int RunCount { get; set; }
        public int BotTurnLength { get; set; }
        public int ConcurrencyLevel { get; set; } = Environment.ProcessorCount;

        public bool IsCorrect() => Width >= Game.VictoryLength
                                   && Height >= Game.VictoryLength
                                   && RunCount > 0
                                   && BotTurnLength > 1
                                   && ConcurrencyLevel > 0;
    }
}
