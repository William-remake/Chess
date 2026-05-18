using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessLogic
{
    public abstract class Piece
    {
        public abstract PieceType Type { get; }
        
        public abstract Player Color { get; }

        public bool HasMoved { get; set; } = false;

        public abstract Piece Copy();
<<<<<<< HEAD

        public abstract IEnumerable<Move> GetMoves(Position from, Board board);

        protected IEnumerable<Position> MovePositionInDir(Position from, Board board, Direction dir)
        {
            for (Position pos = from + dir; Board.IsInside(pos); pos += dir)
            {
                if (board.IsEmpty(pos))
                {
                    yield return pos;
                    continue;
                }

                Piece piece = board[pos];

                if (piece.Color != Color)
                {
                    yield return pos;
                }

                yield break;
            }
        }

        protected IEnumerable<Position> MovePositionInDirs(Position from , Board board, Direction[] dirs)
        {
            return dirs.SelectMany(dir => MovePositionInDir(from, board, dir));
        }
=======
>>>>>>> 6c8b6daa2c76f00f816c303e9a2655ec152eea04
    }
}
