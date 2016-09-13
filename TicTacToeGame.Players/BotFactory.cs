using TicTacToeGame.Players.Bots;
using TicTacToeGame.Players.Bots.BrainTvs;
using TicTacToeGame.Players.Bots.Nibbler;
using TicTacToeGame.Players.Bots.Trivial;
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
                case BotKind.BrainTvs:
                    return new BrainTvsBot();
                default:
                    return null;
            }
        }
    }
}
