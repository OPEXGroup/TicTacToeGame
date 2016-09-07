using TicTacToeGame.Players.Bots;
using TicTacToeGame.Players.Enums;

namespace TicTacToeGame.Players
{
    public static class BotFactory
    {
        public static BotPlayer BuildBot(BotKind kind)
        {
            switch (kind)
            {
                case BotKind.Trivial:
                    return new TrivialBot();
                case BotKind.Nibbler:
                    return new NibblerBot();
                default:
                    return null;
            }
        }
    }
}
