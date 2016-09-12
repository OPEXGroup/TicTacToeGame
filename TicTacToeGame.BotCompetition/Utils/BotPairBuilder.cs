using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicTacToeGame.Players;
using TicTacToeGame.Players.Enums;

namespace TicTacToeGame.BotCompetition.Utils
{
    internal static class BotPairBuilder
    {
        #region public

        public static IEnumerable<BotPair> GetAllBotPairs()
        {
            var botTypes = Enum.GetValues(typeof(BotKind)).Cast<BotKind>().ToList();
            var innerBotTypes = new List<BotKind>(botTypes);
            foreach (var botType in botTypes)
            {
                foreach (var innerBotType in innerBotTypes)
                {
                    if (botType == innerBotType)
                        continue;
                    yield return new BotPair
                    {
                        FirstPlayer = botType,
                        SecondPlayer = innerBotType
                    };
                }
            }
        }

        #endregion
    }
}
