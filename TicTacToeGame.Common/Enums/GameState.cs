using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToeGame.Common.Enums
{
    public enum GameState
    {
        XTurn,
        OTurn,
        XWon,
        OWon,
        XInvalidTurn,
        OInvalidTurn,
        Draw
    }
}
