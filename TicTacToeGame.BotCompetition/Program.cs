using ITCC.Logging.Core;
using ITCC.Logging.Windows.Loggers;
using TicTacToeGame.BotCompetition.Competition;
using TicTacToeGame.BotCompetition.Utils;
using TicTacToeGame.Common;

namespace TicTacToeGame.BotCompetition
{
    internal class Program
    {
        private const int PairCompetitionCount = 1000;
        private const int FieldWidth = 30;
        private const int FieldHeight = 30;
        private const int BotTurnLength = 1000;

        private static void Main(string[] args)
        {
            Logger.Level = LogLevel.Debug;
            Logger.RegisterReceiver(new ColouredConsoleLogger());
            Game.Mute();

            var score = new GlobalScore();

            foreach (var botPair in BotPairBuilder.GetAllBotPairs())
            {
                var firstBotKind = botPair.FirstPlayer;
                var secondBotKind = botPair.SecondPlayer;

                var competitionConfiguration = new CompetitionConfiguration
                {
                    BotTurnLength = BotTurnLength,
                    FirstBotKind = firstBotKind,
                    SecondBotKind = secondBotKind,
                    Height = FieldHeight,
                    Width = FieldWidth,
                    RunCount = PairCompetitionCount
                };

                var competitionRunner = CompetitionRunner.CreateRunner(competitionConfiguration);
                if (competitionRunner == null)
                {
                    Logger.LogEntry("COMPETITION", LogLevel.Error, "Failed to create competition");
                    continue;
                }

                var competitionResult = competitionRunner.Run();
                score.Add(botPair, competitionResult);
                Logger.LogEntry("COMPETITION", LogLevel.Info, competitionResult.ToString());
            }

            score.Done();
            score.Print();
        }
    }
}
