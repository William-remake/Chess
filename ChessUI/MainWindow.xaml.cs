using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;
using System.Windows.Threading;
using ChessLogic;

namespace ChessUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private readonly Image[,] pieceImages = new Image[8, 8];
        private readonly Rectangle[,] highlights = new Rectangle[8, 8];
        private readonly Dictionary<Position, Move> moveCache = new Dictionary<Position, Move>();

        private readonly Stack<GameStateMemento> undoStack = new();
        private readonly Stack<GameStateMemento> redoStack = new();

        private GameState gameState;
        private Position selectedPos = null;

        private DispatcherTimer timer;
        private int whiteSeconds = 600;
        private int blackSeconds = 600;

        private readonly List<string> moveHistory = new();

        private readonly List<Piece> whiteCaptured = new();
        private readonly List<Piece> blackCaptured = new();

        public MainWindow()
        {
            InitializeComponent();
            InitializeBoard();

            Hide();
            OpenGameMenu menu = new OpenGameMenu();
            menu.ShowDialog();
            Show();

            gameState = new GameState(Player.White, Board.Initial());
            DrawBoard(gameState.Board);
            SetCursor(gameState.CurrentPlayer);

            StartClock();
            UpdateClockUI();
            UpdateStatusText();
        }

        private void InitializeBoard()
        {
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    Image image = new Image();
                    pieceImages[r, c] = image;
                    PiecesGrid.Children.Add(image);

                    Rectangle highlight = new Rectangle();
                    highlights[r, c] = highlight;
                    HighlightGrid.Children.Add(highlight);
                }
            }
        }

        private void DrawBoard(Board board)
        {
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    Piece piece = board[r, c];
                    pieceImages[r, c].Source = Images.GetImage(piece);
                }
            }
        }

        private void BoardGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(IsMenuOnSreen())
            {
                return;
            }

            Point point = e.GetPosition(BoardGrid);
            Position pos = ToSquarePosition(point);

            if(selectedPos == null)
            {
                OnFromPositionSelected(pos);
            }
            else
            {
                OnToPositionSelected(pos);
            }
        }

        private Position ToSquarePosition(Point point)
        {
            double squareSize = BoardGrid.ActualWidth / 8;
            int row = (int)(point.Y /  squareSize);
            int col = (int)(point.X / squareSize);
            return new Position(row, col);
        }

        private void OnFromPositionSelected(Position pos)
        {
            IEnumerable<Move> moves = gameState.LegalMovesForPiece(pos);

            if(moves.Any())
            {
                selectedPos = pos;
                CacheMoves(moves);
                ShowHighlights();
            }
        }

        private void OnToPositionSelected(Position pos)
        {
            selectedPos = null;
            HideHighlights();

            if(moveCache.TryGetValue(pos, out Move move))
            {
                if (move.Type == MoveType.PawnPromotion)
                {
                    HandlePromotion(move.FromPos, move.ToPos);
                }
                else
                {
                    HandleMove(move);
                }                
            }
        }

        private void HandlePromotion(Position from, Position to)
        {
            pieceImages[to.Row, to.Column].Source = Images.GetImage(gameState.CurrentPlayer, PieceType.Pawn);
            pieceImages[from.Row, from.Column].Source = null;

            PromotionMenu promMenu = new PromotionMenu(gameState.CurrentPlayer);
            MenuContainer.Content = promMenu;

            promMenu.PieceSelected += type =>
            {
                MenuContainer.Content = null;
                Move promMove = new PawnPromotion(from, to, type);
                HandleMove(promMove);
            };
        }

        private void HandleMove(Move move)
        {
            undoStack.Push(new GameStateMemento(
                gameState.Board,
                gameState.CurrentPlayer));

            redoStack.Clear();

            Piece capturedPiece = gameState.Board[move.ToPos];

            gameState.MakeMove(move);

            if (capturedPiece != null)
            {
                AddCapturedPiece(capturedPiece);
            }

            AddMoveToHistory(move);
            DrawBoard(gameState.Board);
            SetCursor(gameState.CurrentPlayer);
            UpdateStatusText();

            if (gameState.IsGameOver())
            {
                ShowGameOver();
            }
        }

        private void UndoMove()
        {
            if (undoStack.Count == 0)
            {
                return;
            }

            redoStack.Push(new GameStateMemento(
                gameState.Board,
                gameState.CurrentPlayer));

            GameStateMemento previous = undoStack.Pop();

            gameState = new GameState(
                previous.CurrentPlayer,
                previous.Board);

            DrawBoard(gameState.Board);
            SetCursor(gameState.CurrentPlayer);
            UpdateStatusText();
            selectedPos = null;
            HideHighlights();
            moveCache.Clear();
        }

        private void RedoMove()
        {
            if (redoStack.Count == 0)
            {
                return;
            }

            undoStack.Push(new GameStateMemento(
                gameState.Board,
                gameState.CurrentPlayer));

            GameStateMemento next = redoStack.Pop();

            gameState = new GameState(
                next.CurrentPlayer,
                next.Board);

            DrawBoard(gameState.Board);
            SetCursor(gameState.CurrentPlayer);
            UpdateStatusText();
            selectedPos = null;
            HideHighlights();
            moveCache.Clear();
        }

        private void AddMoveToHistory(Move move)
        {
            string moveText = move.ToString();

            if (gameState.CurrentPlayer == Player.Black)
            {
                int moveNumber = (moveHistory.Count / 2) + 1;

                moveHistory.Add($"{moveNumber}. {moveText}");
            }
            else
            {
                moveHistory[moveHistory.Count - 1] += $"   {moveText}";
            }

            MoveHistoryText.Text = string.Join("\n", moveHistory);
        }

        private void AddCapturedPiece(Piece piece)
        {
            Image image = new Image();

            image.Source = Images.GetImage(piece);

            image.Width = 30;
            image.Height = 30;

            if (piece.Color == Player.White)
            {
                blackCaptured.Add(piece);

                BlackCapturedPanel.Children.Add(image);
            }
            else
            {
                whiteCaptured.Add(piece);

                WhiteCapturedPanel.Children.Add(image);
            }
        }

        private void CacheMoves(IEnumerable<Move> moves)
        {
            moveCache.Clear();

            foreach (Move move in moves)
            {
                moveCache[move.ToPos] = move;
            }
        }

        private void ShowHighlights()
        {
            Color normalColor = Color.FromArgb(150, 125, 255, 125);
            Color captureColor = Color.FromArgb(180, 255, 80, 80);

            foreach (Move move in moveCache.Values)
            {
                if (gameState.Board[move.ToPos] == null)
                {
                    highlights[move.ToPos.Row, move.ToPos.Column].Fill =
                        new SolidColorBrush(normalColor);
                }
                else
                {
                    highlights[move.ToPos.Row, move.ToPos.Column].Fill =
                        new SolidColorBrush(captureColor);
                }
            }
        }

        private void HideHighlights()
        {
            foreach (Position to in moveCache.Keys)
            {
                highlights[to.Row, to.Column].Fill = Brushes.Transparent;
            }
        }

        private void SetCursor(Player player)
        {
            if (player == Player.White)
            {
                Cursor = ChessCursors.WhiteCursor;
            }
            else
            {
                Cursor = ChessCursors.BlackCursor;
            }
        }

        private void StartClock()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (gameState.CurrentPlayer == Player.White)
            {
                whiteSeconds--;
            }
            else
            {
                blackSeconds--;
            }

            UpdateClockUI();

            if (whiteSeconds <= 0)
            {
                timer.Stop();

                MessageBox.Show("Black wins by time!");

                ShowGameOver();
            }

            if (blackSeconds <= 0)
            {
                timer.Stop();

                MessageBox.Show("White wins by time!");

                ShowGameOver();
            }
        }

        private void UpdateClockUI()
        {
            TimeSpan white = TimeSpan.FromSeconds(whiteSeconds);
            TimeSpan black = TimeSpan.FromSeconds(blackSeconds);

            WhiteTimerText.Text = $"White: {white:mm\\:ss}";
            BlackTimerText.Text = $"Black: {black:mm\\:ss}";
        }

        private void UpdateStatusText()
        {
            if (gameState.IsGameOver())
            {
                if (gameState.Board.IsInCheck(gameState.CurrentPlayer))
                {
                    StatusText.Text = "Checkmate";
                }
                else
                {
                    StatusText.Text = "Stalemate";
                }

                return;
            }

            if (gameState.Board.IsInCheck(gameState.CurrentPlayer))
            {
                StatusText.Text = $"{gameState.CurrentPlayer} in check";
            }
            else
            {
                StatusText.Text = $"{gameState.CurrentPlayer} to move";
            }
        }

        private bool IsMenuOnSreen()
        {
            return MenuContainer.Content != null;
        }

        private void ShowGameOver()
        {
            GameOverMenu gameOverMenu = new GameOverMenu(gameState);
            MenuContainer.Content = gameOverMenu;

            gameOverMenu.OptionSelected += option =>
            {
                if (option == Option.Restart)
                {
                    MenuContainer.Content = null;
                    RestartGame();
                }
                else
                {
                    Application.Current.Shutdown();
                }
            };
        }

        private void RestartGame()
        {
            selectedPos = null;
            HideHighlights();
            moveCache.Clear();
            gameState = new GameState(Player.White, Board.Initial());
            DrawBoard(gameState.Board);
            SetCursor(gameState.CurrentPlayer);

            whiteSeconds = 600;
            blackSeconds = 600;
            UpdateClockUI();
            timer.Start();

            moveHistory.Clear();
            MoveHistoryText.Text = "";

            UpdateStatusText();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Z)
            {
                UndoMove();
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Y)
            {
                RedoMove();
            }

            if (!IsMenuOnSreen() && e.Key == Key.Escape)
            {
                ShowPauseMenu();
            }
        }

        private void ShowPauseMenu()
        {
            PauseMenu pauseMenu = new PauseMenu();
            MenuContainer.Content = pauseMenu;

            pauseMenu.OnOptionSelected += option =>
            {
                MenuContainer.Content = null;

                if (option == Option.Restart)
                {
                    RestartGame();
                }
            };

        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            UndoMove();
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            RedoMove();
        }
    }
}