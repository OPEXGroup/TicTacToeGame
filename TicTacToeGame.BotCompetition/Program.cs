using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITCC.Logging.Core;
using ITCC.Logging.Windows.Loggers;
using TicTacToeGame.BotCompetition.Utils;
using TicTacToeGame.Common;
using TicTacToeGame.Common.Enums;
using TicTacToeGame.Common.Interfaces;
using TicTacToeGame.Players;

namespace TicTacToeGame.BotCompetition
{
    internal class Program
    {
        private const int PairCompetitionCount = 100;
        private const int FieldWidth = 30;
        private const int FieldHeight = 30;
        private const int BotTurnLength = 1000;

        private static void Main(string[] args)
        {
            Logger.Level = LogLevel.Debug;
            Logger.RegisterReceiver(new ColouredConsoleLogger());

            foreach (var botPair in BotPairBuilder.GetAllBotPairs())
            {
                LogMessage(LogLevel.Info, $"Playing {botPair.FirstPlayer} vs {botPair.SecondPlayer}");
                var firstWonAsX = 0;
                var firstWonAsO = 0;

                for (var i = 0; i < PairCompetitionCount; i++)
                {
                    var firstBot = BotFactory.BuildBot(botPair.FirstPlayer);
                    var secondBot = BotFactory.BuildBot(botPair.SecondPlayer);

                    var winningSign = RunOneGame(firstBot, secondBot);
                    if (winningSign == CellSign.X)
                        ++firstWonAsX;
                }
                Console.WriteLine($"First won as X: {firstWonAsX} lost: {firstWonAsO}");
            }
        }

        private static CellSign RunOneGame(IPlayer firstPlayer, IPlayer secondPlayer)
        {
            var result = CellSign.Empty;
            var gameConfig = new GameConfiguration
            {
                BotTurnLength = BotTurnLength,
                FirstPlayer = firstPlayer,
                Height = FieldHeight,
                SecondPlayer = secondPlayer,
                Width = FieldWidth
            };

            var game = Game.CreateNewGame(gameConfig);
            if (game == null)
            {
                LogMessage(LogLevel.Warning, "Failed to create game");
                return CellSign.Empty;
            }
            game.GameStateChanged += (sender, eventArgs) => game.ReportStepProcessed();
            game.GameEnded += (sender, eventArgs) =>
            {
                if (eventArgs.Winner == firstPlayer)
                    result = CellSign.X;
                else if (eventArgs.Winner == secondPlayer)
                    result = CellSign.O;
                else
                    result = CellSign.Empty;

            };
            game.Start();
            game.WaitGameCompleted();
            return result;
        }

        private static void LogMessage(LogLevel level, string message) => Logger.LogEntry("BOT TEST", level, message);
        private static void LogException(LogLevel level, Exception exception) => Logger.LogException("BOT TEST", level, exception);
    }
}
