using System;
using System.Collections.Generic;
using System.Linq;
using TicTacToeGame.Players.Enums;

namespace TicTacToeGame.BotCompetition.Utils
{
    internal static class BotPairBuilder
    {
        #region public

        public static IEnumerable<BotPair> GetAllBotPairs()
        {
            var botTypes = GetAllBotKinds();
            var innerBotTypes = GetAllBotKinds();
            foreach (var botType in botTypes)
            {
                /**
                 * We should have:
                 * 1) Only one of (a, b) and (b, a) pairs
                 * 2) Only pairs (a, b) with a != b
                 */
                foreach (var innerBotType in innerBotTypes.Where(innerBotType => botType < innerBotType))
                {
                    yield return new BotPair
                    {
                        FirstPlayer = botType,
                        SecondPlayer = innerBotType
                    };
                }
            }
        }

        public static List<BotKind> GetAllBotKinds() => Enum.GetValues(typeof(BotKind)).Cast<BotKind>().ToList();

        #endregion
    }
}
