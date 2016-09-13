using System;
using System.Threading.Tasks;
using ITCC.Logging.Core;
using TicTacToeGame.Common;
using TicTacToeGame.Common.Enums;
using TicTacToeGame.Common.Utils;
using TicTacToeGame.Players;
using TicTacToeGame.Players.Enums;

namespace TicTacToeGame.BotCompetition.Competition
{
    internal class CompetitionRunner
    {
        #region public

        public const string LogScope = @"RUNNER";

        public static CompetitionRunner CreateRunner(CompetitionConfiguration configuration)
        {
            if (configuration == null || !configuration.IsCorrect())
                return null;

            return new CompetitionRunner(configuration);
        }

        public CompetitionResult Run()
        {
            RunAll(CellSign.X);
            RunAll(CellSign.O);

            var firstTotalWins = _firstPlayerXDistribution.Wins + _firstPlayerODistribution.Wins;
            var firstTotalLosses = _firstPlayerXDistribution.Losses + _firstPlayerODistribution.Losses;
            BotKind? winner;
            BotKind? loser;
            if (firstTotalWins > firstTotalLosses)
            {
                winner = _firstBotKind;
                loser = _secondBotKind;
            }
            else if (firstTotalWins == firstTotalLosses)
            {
                winner = null;
                loser = null;
            }
            else
            {
                winner = _secondBotKind;
                loser = _firstBotKind;
            }

            return new CompetitionResult
            {
                FailedToCreate = _failedToCreate,
                FirstPlayer = _firstBotKind,
                SecondPlayer = _secondBotKind,
                FirstPlayerXStats = _firstPlayerXDistribution,
                FirstPlayerOStats = _firstPlayerODistribution,
                Winner = winner,
                Loser = loser
            };
        }
        #endregion

        #region private

        private void LogMessage(LogLevel level, string message)
            => Logger.LogEntry(LogScope, level, message);

        private void LogException(LogLevel level, Exception exception)
            => Logger.LogException(LogScope, level, exception);

        private CompetitionRunner(CompetitionConfiguration configuration)
        {
            _firstBotKind = configuration.FirstBotKind;
            _secondBotKind = configuration.SecondBotKind;
            _width = configuration.Width;
            _height = configuration.Height;
            _botTurnLength = configuration.BotTurnLength;
            _concurrencyLevel = configuration.ConcurrencyLevel;
            _runCount = configuration.RunCount;

            _firstPlayerXDistribution = new WinLossDistribution {Total = _runCount};
            _firstPlayerODistribution = new WinLossDistribution { Total = _runCount };
        }

        private void RunAll(CellSign firstPlayerSign)
        {
            var winLossDistribution = firstPlayerSign == CellSign.X
                ? _firstPlayerXDistribution
                : _firstPlayerODistribution;

            Parallel.For(0, _runCount, new ParallelOptions {MaxDegreeOfParallelism = _concurrencyLevel},
                i =>
                {
                    var sign = RunSingle(firstPlayerSign);
                    lock (_statsLock)
                    {
                        if (sign == firstPlayerSign)
                            winLossDistribution.Wins++;
                        else if (sign == CellSign.Empty)
                            winLossDistribution.Draws++;
                        else
                            winLossDistribution.Losses++;
                    }

                });
        }

        private CellSign RunSingle(CellSign firstPlayerSign)
        {
            var firstPlayerKind = firstPlayerSign == CellSign.X ? _firstBotKind : _secondBotKind;
            var secondPlayerKind = firstPlayerSign == CellSign.X ? _secondBotKind : _firstBotKind;

            try
            {
                var config = new GameConfiguration
                {
                    BotTurnLength = _botTurnLength,
                    FirstPlayer = BotFactory.BuildBot(firstPlayerKind),
                    SecondPlayer = BotFactory.BuildBot(secondPlayerKind),
                    Height = _height,
                    Width = _width
                };

                var game = Game.CreateNewGame(config);
                if (game == null)
                {
                    SetFailedToCreate();
                    return CellSign.Empty;
                }

                GameEndedEventArgs args = null;
                game.GameStateChanged += (sender, eventArgs) => game.ReportStepProcessed();
                game.GameEnded += (sender, eventArgs) => args = eventArgs;
                game.Start();
                game.WaitCompleted();

                return GameStateToSign(args.State);
            }
            catch (Exception ex)
            {
                LogException(LogLevel.Warning, ex);
                SetFailedToCreate();
                return CellSign.Empty;
            }
        }

        private void SetFailedToCreate()
        {
            lock (_lock)
            {
                _failedToCreate = true;
            }
        }

        private CellSign GameStateToSign(GameState state)
        {
            switch (state)
            {
                case GameState.XWon:
                    return CellSign.X;
                case GameState.OWon:
                    return CellSign.O;
                case GameState.XInvalidTurn:
                    return CellSign.O;
                case GameState.OInvalidTurn:
                    return CellSign.X;
                default:
                    return CellSign.Empty;
            }
        }

        private readonly WinLossDistribution _firstPlayerXDistribution;
        private readonly WinLossDistribution _firstPlayerODistribution;

        private readonly BotKind _firstBotKind;
        private readonly BotKind _secondBotKind;
        private readonly int _width;
        private readonly int _height;
        private readonly int _botTurnLength;
        private readonly int _concurrencyLevel;
        private readonly int _runCount;

        private bool _failedToCreate;
        private readonly object _lock = new object();
        private readonly object _statsLock = new object();

        #endregion
    }
}
