using System.Collections.Generic;
using System.Linq;
using System.Text;
using ITCC.Logging.Core;
using TicTacToeGame.BotCompetition.Enums;
using TicTacToeGame.BotCompetition.Utils;
using TicTacToeGame.Players.Enums;

namespace TicTacToeGame.BotCompetition.Competition
{
    internal class GlobalScore
    {
        #region public

        public const int LoseBonus = 0;
        public const int DrawBonus = 1;
        public const int WinBonus = 3;

        public void Add(BotPair pair, CompetitionResult result)
        {
            if (_finalized)
            {
                LogMessage(LogLevel.Warning, "Score is already finalized");
                return;
            }

            if (result.Winner == null)
                pair.MatchResult = MatchResult.Draw;
            else if (result.Winner == pair.FirstPlayer)
                pair.MatchResult = MatchResult.FirstWon;
            else
                pair.MatchResult = MatchResult.SecondWon;

            _matchResults.Add(pair);
        }

        public void Done()
        {
            _finalized = true;
            
            _scores = new Dictionary<BotKind, int>(BotPairBuilder.GetAllBotKinds().ToDictionary(bk => bk, bk => 0));
            foreach (var botPair in _matchResults)
            {
                switch (botPair.MatchResult)
                {
                    case MatchResult.Draw:
                        _scores[botPair.FirstPlayer] += DrawBonus;
                        _scores[botPair.SecondPlayer] += DrawBonus;
                        break;
                    case MatchResult.FirstWon:
                        _scores[botPair.FirstPlayer] += WinBonus;
                        _scores[botPair.SecondPlayer] += LoseBonus;
                        break;
                    case MatchResult.SecondWon:
                        _scores[botPair.FirstPlayer] += LoseBonus;
                        _scores[botPair.SecondPlayer] += WinBonus;
                        break;
                }
            }
        }

        public void Print()
        {
            if (!_finalized)
            {
                LogMessage(LogLevel.Warning, "Score is not finalized");
                return;
            }

            var builder = new StringBuilder();
            builder.AppendLine();
            builder.AppendLine("Final scores");
            var place = 1;
            foreach (var score in _scores.OrderByDescending(kv => kv.Value))
            {
                builder.AppendLine($"  {place,3}. {score.Key, -12} ({score.Value} points)");
                place++;
            }

            LogMessage(LogLevel.Info, builder.ToString());
        }
        #endregion

        #region private

        private bool _finalized;
        private readonly List<BotPair> _matchResults = new List<BotPair>();
        private Dictionary<BotKind, int> _scores;

        private void LogMessage(LogLevel level, string message)
            => Logger.LogEntry("SCORE", level, message);
        #endregion
    }
}
