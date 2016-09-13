using ITCC.Logging.Core;
using ITCC.Logging.Windows.Loggers;
using TicTacToeGame.BotCompetition.Competition;
using TicTacToeGame.BotCompetition.Utils;
using TicTacToeGame.Common;

namespace TicTacToeGame.BotCompetition
{
    internal class Program
    {
        private const int PairCompetitionCount = 10000;
        private const int FieldWidth = 30;
        private const int FieldHeight = 30;
        private const int BotTurnLength = 1000;

        private static void Main(string[] args)
        {
            Logger.Level = LogLevel.Debug;
            Logger.RegisterReceiver(new ColouredConsoleLogger());
            Game.Mute();

            foreach (var botPair in BotPairBuilder.GetAllBotPairs())
            {
                var competitionConfiguration = new CompetitionConfiguration
                {
                    BotTurnLength = BotTurnLength,
                    FirstBotKind = botPair.FirstPlayer,
                    SecondBotKind = botPair.SecondPlayer,
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

                var result = competitionRunner.Run();
                Logger.LogEntry("COMPETITION", LogLevel.Info, result.ToString());
            }
        }
    }
}
