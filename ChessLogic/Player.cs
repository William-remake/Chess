using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessLogic
{
    //Note: Enum là một kiểu dữ liệu tham trị (kiểu dữ liệu tham trị đã được trình bày trong bài KIỂU DỮ LIỆU)
    //      Enum không được phép kế thừa(khái niệm về kế thừa sẽ trình bày trong bài KẾ THỪA TRONG C#).
    public enum Player
    {
        None,
        White,
        Black,
    }

    public static class PlayerExtension
    {
        public static Player Opponent(this Player player)
        {
            switch (player)
            {
                case Player.White:
                    return Player.Black;
                case Player.Black:
                    return Player.White;
                default:
                    return Player.None;
            }
        }
    }
}
