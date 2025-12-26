using static CastlingRights;
using static Side;
using static Square;

public static class Board
{
    public const int P = 0, N = 1, B = 2, R = 3, Q = 4, K = 5;
    public const int p = 6, n = 7, b = 8, r = 9, q = 10, k = 11;

    public static readonly char[] asciiPieces = "PNBRQKpnbrqk".ToCharArray();
    public static readonly Dictionary<char, int> charPieces = new()
    {
        ['P'] = P,
        ['N'] = N,
        ['B'] = B,
        ['R'] = R,
        ['Q'] = Q,
        ['K'] = K,
        ['p'] = p,
        ['n'] = n,
        ['b'] = b,
        ['r'] = r,
        ['q'] = q,
        ['k'] = k
    };

    public static readonly char[] promotedPieces =
    [
        ' ', 'n', 'b', 'r', 'q', ' ',  // P, N, B, R, Q, K (white pieces)
        ' ', 'n', 'b', 'r', 'q', ' '   // p, n, b, r, q, k (black pieces)
    ];

    public static readonly string[] squareToCoordinates =
    {
        "a8","b8","c8","d8","e8","f8","g8","h8",
        "a7","b7","c7","d7","e7","f7","g7","h7",
        "a6","b6","c6","d6","e6","f6","g6","h6",
        "a5","b5","c5","d5","e5","f5","g5","h5",
        "a4","b4","c4","d4","e4","f4","g4","h4",
        "a3","b3","c3","d3","e3","f3","g3","h3",
        "a2","b2","c2","d2","e2","f2","g2","h2",
        "a1","b1","c1","d1","e1","f1","g1","h1"
    };

    public static ulong[] bitboards = new ulong[12];
    public static ulong[] occupancies = new ulong[3];
    public static int side;
    public static int enPassant = (int)noSquare;
    public static int castle;

    public static void ParseFEN(string fen)
    {
        Array.Fill(bitboards, 0UL);
        Array.Fill(occupancies, 0UL);

        side = 0;
        enPassant = (int)noSquare;
        castle = 0;

        int rank = 0, file = 0;

        for (int i = 0; i < fen.Length && rank < 8; i++)
        {
            char c = fen[i];

            if (char.IsLetter(c))
            {
                int piece = charPieces[c];
                BitboardOperations.SetBit(ref bitboards[piece], rank * 8 + file);
                file++;
            }
            else if (char.IsDigit(c))
            {
                file += c - '0';
            }
            else if (c == '/')
            {
                rank++;
                file = 0;
            }
            else if (c == ' ')
            {
                i++;
                side = (fen[i] == 'w') ? (int)white : (int)black;
                i += 2;

                while (fen[i] != ' ')
                {
                    if (fen[i] == 'K') castle |= (int)wk;
                    else if (fen[i] == 'Q') castle |= (int)wq;
                    else if (fen[i] == 'k') castle |= (int)bk;
                    else if (fen[i] == 'q') castle |= (int)bq;
                    i++;
                }

                i++;
                if (fen[i] != '-')
                {
                    int epFile = fen[i] - 'a';
                    int epRank = 8 - (fen[i + 1] - '0');
                    enPassant = epRank * 8 + epFile;
                }
                else
                    enPassant = (int)noSquare;

                break;
            }
        }

        UpdateOccupancies();
    }

    public static void UpdateOccupancies()
    {
        for (int piece = P; piece <= K; piece++)
            occupancies[(int)white] |= bitboards[piece];

        for (int piece = p; piece <= k; piece++)
            occupancies[(int)black] |= bitboards[piece];

        occupancies[(int)both] = occupancies[(int)white] | occupancies[(int)black];
    }

    public static BoardState CopyBoard()
    {
        return new BoardState
        {
            bitboards = (ulong[])bitboards.Clone(),
            occupancies = (ulong[])occupancies.Clone(),
            side = side,
            enPassant = enPassant,
            castle = castle
        };
    }

    public static void TakeBack(BoardState state)
    {
        Array.Copy(state.bitboards, bitboards, 12);
        Array.Copy(state.occupancies, occupancies, 3);
        side = state.side;
        enPassant = state.enPassant;
        castle = state.castle;
    }

    public static void PrintBoard()
    {
        Console.WriteLine();

        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                int square = rank * 8 + file;

                if (file == 0)
                    Console.Write($"  {8 - rank} ");

                int piece = -1;

                for (int bbPiece = P; bbPiece <= k; bbPiece++)
                    if (BitboardOperations.GetBit(bitboards[bbPiece], square))
                        piece = bbPiece;

                Console.Write($" {(piece == -1 ? '.' : asciiPieces[piece])}");
            }

            Console.WriteLine();
        }

        Console.WriteLine("\n     a b c d e f g h\n");
        Console.WriteLine($"     Side:     {(side == 0 ? "white" : "black")}");
        Console.WriteLine($"     Enpassant:   {(enPassant != (int)noSquare ? squareToCoordinates[enPassant] : "no")}");
        Console.WriteLine($"     Castling:  {((castle & (int)wk) != 0 ? 'K' : '-')}{((castle & (int)wq) != 0 ? 'Q' : '-')}{((castle & (int)bk) != 0 ? 'k' : '-')}{((castle & (int)bq) != 0 ? 'q' : '-')}\n");
    }

    public static void PrintBitboard(ulong bitboard)
    {
        Console.WriteLine();

        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                int square = rank * 8 + file;

                if (file == 0)
                    Console.Write($"  {8 - rank} ");

                Console.Write($" {(BitboardOperations.GetBit(bitboard, square) ? 1 : 0)}");
            }

            Console.WriteLine();
        }

        Console.WriteLine("\n     a b c d e f g h\n");
        Console.WriteLine($"     Bitboard: {bitboard}\n");
    }

    public static void PrintMoveList(ref MoveList moveList)
    {
        Console.WriteLine();

        for (int i = 0; i < moveList.count; i++)
        {
            int move = moveList.moves[i];
            Console.WriteLine($"{squareToCoordinates[MoveEncoding.GetMoveSource(move)]}{squareToCoordinates[MoveEncoding.GetMoveTarget(move)]}{(MoveEncoding.GetMovePromoted(move) != 0 ? promotedPieces[MoveEncoding.GetMovePromoted(move)] : ' ')}");
        }

        Console.WriteLine($"\n      Total number of moves: {moveList.count}\n");
    }
}
