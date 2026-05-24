using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessLogic
{
    public class GameStateMemento
    {
        public Board Board { get; }
        public Player CurrentPlayer { get; }

        public GameStateMemento(Board board, Player currentPlayer)
        {
            Board = board.Copy();
            CurrentPlayer = currentPlayer;
        }
    }
}
