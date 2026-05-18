<<<<<<< HEAD
﻿namespace ChessLogic
{
    public class Queen : Piece
=======
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessLogic
{
    internal class Queen : Piece
>>>>>>> 6c8b6daa2c76f00f816c303e9a2655ec152eea04
    {
        public override PieceType Type => PieceType.Queen;
        public override Player Color { get; }

<<<<<<< HEAD
        private static readonly Direction[] dirs = new Direction[]
        {
            Direction.North,
            Direction.South,
            Direction.East,
            Direction.West,
            Direction.NorthWest,
            Direction.NorthEast,
            Direction.SouthWest,
            Direction.SouthEast,
        };

=======
>>>>>>> 6c8b6daa2c76f00f816c303e9a2655ec152eea04
        public Queen(Player color)
        {
            Color = color;
        }

        public override Piece Copy()
        {
            Queen copy = new Queen(Color);
            copy.HasMoved = HasMoved;
            return copy;
        }
<<<<<<< HEAD

        public override IEnumerable<Move> GetMoves(Position from, Board board)
        {
            return MovePositionInDirs(from, board, dirs).Select(to => new NormalMove(from, to));
        }
=======
>>>>>>> 6c8b6daa2c76f00f816c303e9a2655ec152eea04
    }
}
