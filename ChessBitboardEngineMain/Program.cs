using static Side;
using static Square;
using static CastlingRights;
using static PieceAttacks;
using static MoveFlag;
using System.Numerics;

class Program
{
    static void Main()
    {
        InitAll();
        ParseFEN(StartPosition);
        PrintBoard();
        int start = GetTimeMs();
        PerftTest(3);
    }

    static long nodes;

    static void PerftDriver(int depth)
    {
        if (depth == 0)
        {
            nodes++;
            return;
        }

        MoveList moveList = new MoveList(256);
        GenerateMoves(ref moveList);

        for (int moveCount = 0; moveCount < moveList.count; moveCount++)
        {
            var boardState = CopyBoard();

            if (MakeMove(moveList.moves[moveCount], (int)allMoves) == 0)
                continue;

            PerftDriver(depth - 1);
            TakeBack(boardState);
        }
    }

    public static void PerftTest(int depth)
    {
        Console.WriteLine("\n     Performance test\n");

        MoveList moveList = new MoveList(256);
        GenerateMoves(ref moveList);

        var watch = System.Diagnostics.Stopwatch.StartNew();
        nodes = 0;

        for (int count = 0; count < moveList.count; count++)
        {
            int move = moveList.moves[count];
            BoardState state = CopyBoard();

            if (MakeMove(move, (int)MoveFlag.allMoves) == 0)
                continue;

            long cumulativeNodes = nodes;
            PerftDriver(depth - 1);
            long branchNodes = nodes - cumulativeNodes;

            TakeBack(state);

            string source = Enum.GetName(typeof(Square), GetMoveSource(move)) ?? "?";
            string target = Enum.GetName(typeof(Square), GetMoveTarget(move)) ?? "?";
            int promoted = GetMovePromoted(move);
            char promotedChar = promoted != 0 ? GetPromotedChar(promoted) : ' ';

            Console.WriteLine($"     move: {source}{target}{promotedChar}  nodes: {branchNodes}");
        }

        watch.Stop();

        Console.WriteLine($"\n    Depth: {depth}");
        Console.WriteLine($"    Nodes: {nodes}");
        Console.WriteLine($"     Time: {watch.ElapsedMilliseconds}ms\n");
    }

    private static char GetPromotedChar(int piece)
    {
        string pieces = "PNBRQKpnbrqk";
        return pieces[piece];
    }

    public static int GetTimeMs() => Environment.TickCount;

    public const string EmptyBoard = "8/8/8/8/8/8/8/8 w - - ";
    public const string StartPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    public const string TrickyPosition = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1";
    public const string KillerPosition = "rnbqkb1r/pp1p1pPp/8/2p1pP2/1P1P4/3P3P/P1P1P3/RNBQKBNR w KQkq e6 0 1";
    public const string CmkPosition = "r2q1rk1/ppp2ppp/2n1bn2/2b1p3/3pP3/3P1NPP/PPP1NPB1/R1BQ1RK1 b - - 0 1";

    static readonly int[] castlingRights =
    [
        7, 15, 15, 15, 3, 15, 15, 11,
        15, 15, 15, 15, 15, 15, 15, 15,
        15, 15, 15, 15, 15, 15, 15, 15,
        15, 15, 15, 15, 15, 15, 15, 15,
        15, 15, 15, 15, 15, 15, 15, 15,
        15, 15, 15, 15, 15, 15, 15, 15,
        15, 15, 15, 15, 15, 15, 15, 15,
        13, 15, 15, 15, 12, 15, 15, 14
    ];

    static int MakeMove(int move, int moveFlag)
    {
        if (moveFlag == (int)allMoves)
        {
            BoardState state = CopyBoard();

            int sourceSquare = GetMoveSource(move);
            int targetSquare = GetMoveTarget(move);
            int piece = GetMovePiece(move);
            int promotedPiece = GetMovePromoted(move);
            int capture = GetMoveCapture(move);
            int doublePush = GetMoveDouble(move);
            int enpass = GetMoveEnpassant(move);
            int castling = GetMoveCastling(move);

            PopBit(ref bitboards[piece], sourceSquare);
            SetBit(ref bitboards[piece], targetSquare);

            if (capture != 0)
            {
                int startPiece, endPiece;

                if (side == (int)white)
                {
                    startPiece = p;
                    endPiece = k;
                }
                else
                {
                    startPiece = P;
                    endPiece = K;
                }

                for (int bbPiece = startPiece; bbPiece <= endPiece; bbPiece++)
                {
                    if (GetBit(bitboards[bbPiece], targetSquare))
                    {
                        PopBit(ref bitboards[bbPiece], targetSquare);
                        break;
                    }
                }
            }

            if (promotedPiece != 0)
            {
                PopBit(ref bitboards[(side == (int)white) ? P : p], targetSquare);
                SetBit(ref bitboards[promotedPiece], targetSquare);
            }

            if (enpass != 0)
            {
                if (side == (int)white)
                    PopBit(ref bitboards[p], targetSquare - 8); 
                else
                    PopBit(ref bitboards[P], targetSquare + 8); 
            }

            enPassant = (int)noSquare;

            if (doublePush != 0)
            {
                enPassant = (side == (int)white)
                    ? targetSquare - 8  
                    : targetSquare + 8;
            }

            if (castling != 0)
            {
                switch ((Square)targetSquare)
                {
                    case g1:
                        PopBit(ref bitboards[R], (int)h1);
                        SetBit(ref bitboards[R], (int)f1);
                        break;
                    case c1:
                        PopBit(ref bitboards[R], (int)a1);
                        SetBit(ref bitboards[R], (int)d1);
                        break;
                    case g8:
                        PopBit(ref bitboards[r], (int)h8);
                        SetBit(ref bitboards[r], (int)f8);
                        break;
                    case c8:
                        PopBit(ref bitboards[r], (int)a8);
                        SetBit(ref bitboards[r], (int)d8);
                        break;
                }
            }

            castle &= castlingRights[sourceSquare];
            castle &= castlingRights[targetSquare];

            Array.Clear(occupancies, 0, occupancies.Length);

            for (int bbPiece = P; bbPiece <= K; bbPiece++)
                occupancies[(int)white] |= bitboards[bbPiece];

            for (int bbPiece = p; bbPiece <= k; bbPiece++)
                occupancies[(int)black] |= bitboards[bbPiece];

            occupancies[(int)both] = occupancies[(int)white] | occupancies[(int)black];

            side ^= 1;

            int kingSquare = (side == (int)white)
                ? GetLs1bIndex(bitboards[k])
                : GetLs1bIndex(bitboards[K]);

            if (IsSquareAttacked(kingSquare, side))
            {
                TakeBack(state);
                return 0;
            }

            return 1;
        }
        else
        {
            if (GetMoveCapture(move) != 0)
                return MakeMove(move, (int)allMoves);

            return 0;
        }
    }

    static int EncodeMove(int source, int target, int piece, int promoted, int capture, int @double, int enpassant, int castling)
    {
        return source | (target << 6) | (piece << 12) | (promoted << 16) | (capture << 20) | (@double << 21) | (enpassant << 22) | (castling << 23);
    }

    static int GetMoveSource(int move) => move & 0x3F;
    static int GetMoveTarget(int move) => (move & 0xFC0) >> 6;
    static int GetMovePiece(int move) => (move & 0xF000) >> 12;
    static int GetMovePromoted(int move) => (move & 0xF0000) >> 16;
    static int GetMoveCapture(int move) => move & 0x100000;
    static int GetMoveDouble(int move) => move & 0x200000;
    static int GetMoveEnpassant(int move) => move & 0x400000;
    static int GetMoveCastling(int move) => move & 0x800000;



    public struct MoveList
    {
        public int[] moves;
        public int count;

        public MoveList(int capacity)
        {
            moves = new int[capacity];
            count = 0;
        }
    }

    public struct BoardState
    {
        public ulong[] bitboards;
        public ulong[] occupancies;
        public int side;
        public int enPassant;
        public int castle;
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

    public static void GenerateMoves(ref MoveList moveList)
    {
        moveList.count = 0;

        int sourceSquare, targetSquare;
        ulong bitboard, attacks;

        for (int piece = P; piece <= k; piece++)
        {
            bitboard = bitboards[piece];

            if (side == (int)white)
            {
                if (piece == P)
                {
                    while (bitboard != 0)
                    {
                        sourceSquare = GetLs1bIndex(bitboard);
                        targetSquare = sourceSquare - 8;

                        if (!(targetSquare < (int)a8) && !GetBit(occupancies[(int)both], targetSquare))
                        {
                            if (sourceSquare >= (int)a7 && sourceSquare <= (int)h7)
                            {
                                AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, Q, 0, 0, 0, 0));
                                AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, R, 0, 0, 0, 0));
                                AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, B, 0, 0, 0, 0));
                                AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, N, 0, 0, 0, 0));
                            }
                            else
                            {
                                AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, 0, 0, 0, 0, 0));
                                if (sourceSquare >= (int)a2 && sourceSquare <= (int)h2 && !GetBit(occupancies[(int)both], targetSquare - 8))
                                    AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare - 8, piece, 0, 0, 1, 0, 0));
                            }
                        }

                        attacks = pawnAttacks[(int)white, sourceSquare] & occupancies[(int)black];
                        while (attacks != 0)
                        {
                            targetSquare = GetLs1bIndex(attacks);
                            if (sourceSquare >= (int)a7 && sourceSquare <= (int)h7)
                            {
                                AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, Q, 1, 0, 0, 0));
                                AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, R, 1, 0, 0, 0));
                                AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, B, 1, 0, 0, 0));
                                AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, N, 1, 0, 0, 0));
                            }
                            else
                                AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, 0, 1, 0, 0, 0));

                            PopBit(ref attacks, targetSquare);
                        }

                        if (enPassant != (int)noSquare)
                        {
                            ulong epAttacks = pawnAttacks[(int)white, sourceSquare] & (1UL << enPassant);
                            if (epAttacks != 0)
                                AddMove(ref moveList, EncodeMove(sourceSquare, GetLs1bIndex(epAttacks), piece, 0, 1, 0, 1, 0));
                        }

                        PopBit(ref bitboard, sourceSquare);
                    }
                }

                if (piece == K)
                {
                    if ((castle & (int)wk) != 0 && !GetBit(occupancies[(int)both], (int)f1) && !GetBit(occupancies[(int)both], (int)g1) && !IsSquareAttacked((int)e1, (int)black) && !IsSquareAttacked((int)f1, (int)black))
                        AddMove(ref moveList, EncodeMove((int)e1, (int)g1, piece, 0, 0, 0, 0, 1));

                    if ((castle & (int)wq) != 0 && !GetBit(occupancies[(int)both], (int)d1) && !GetBit(occupancies[(int)both], (int)c1) && !GetBit(occupancies[(int)both], (int)b1) && !IsSquareAttacked((int)e1, (int)black) && !IsSquareAttacked((int)d1, (int)black))
                        AddMove(ref moveList, EncodeMove((int)e1, (int)c1, piece, 0, 0, 0, 0, 1));
                }
            }
            else
            {
                if (piece == p)
                {
                    while (bitboard != 0)
                    {
                        sourceSquare = GetLs1bIndex(bitboard);
                        targetSquare = sourceSquare + 8;

                        if (!(targetSquare > (int)h1) && !GetBit(occupancies[(int)both], targetSquare))
                        {
                            if (sourceSquare >= (int)a2 && sourceSquare <= (int)h2)
                            {
                                AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, q, 0, 0, 0, 0));
                                AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, r, 0, 0, 0, 0));
                                AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, b, 0, 0, 0, 0));
                                AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, n, 0, 0, 0, 0));
                            }
                            else
                            {
                                AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, 0, 0, 0, 0, 0));
                                if (sourceSquare >= (int)a7 && sourceSquare <= (int)h7 && !GetBit(occupancies[(int)both], targetSquare + 8))
                                    AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare + 8, piece, 0, 0, 1, 0, 0));
                            }
                        }

                        attacks = pawnAttacks[(int)black, sourceSquare] & occupancies[(int)white];
                        while (attacks != 0)
                        {
                            targetSquare = GetLs1bIndex(attacks);
                            if (sourceSquare >= (int)a2 && sourceSquare <= (int)h2)
                            {
                                AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, q, 1, 0, 0, 0));
                                AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, r, 1, 0, 0, 0));
                                AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, b, 1, 0, 0, 0));
                                AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, n, 1, 0, 0, 0));
                            }
                            else
                                AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, 0, 1, 0, 0, 0));

                            PopBit(ref attacks, targetSquare);
                        }

                        if (enPassant != (int)noSquare)
                        {
                            ulong epAttacks = pawnAttacks[(int)black, sourceSquare] & (1UL << enPassant);
                            if (epAttacks != 0)
                                AddMove(ref moveList, EncodeMove(sourceSquare, GetLs1bIndex(epAttacks), piece, 0, 1, 0, 1, 0));
                        }

                        PopBit(ref bitboard, sourceSquare);
                    }
                }

                if (piece == k)
                {
                    if ((castle & (int)bk) != 0 && !GetBit(occupancies[(int)both], (int)f8) && !GetBit(occupancies[(int)both], (int)g8) && !IsSquareAttacked((int)e8, (int)white) && !IsSquareAttacked((int)f8, (int)white))
                        AddMove(ref moveList, EncodeMove((int)e8, (int)g8, piece, 0, 0, 0, 0, 1));

                    if ((castle & (int)bq) != 0 && !GetBit(occupancies[(int)both], (int)d8) && !GetBit(occupancies[(int)both], (int)c8) && !GetBit(occupancies[(int)both], (int)b8) && !IsSquareAttacked((int)e8, (int)white) && !IsSquareAttacked((int)d8, (int)white))
                        AddMove(ref moveList, EncodeMove((int)e8, (int)c8, piece, 0, 0, 0, 0, 1));
                }
            }

            if ((side == (int)white) ? piece == N : piece == n)
            {
                while (bitboard != 0)
                {
                    sourceSquare = GetLs1bIndex(bitboard);
                    attacks = knightAttacks[sourceSquare] & ((side == (int)white) ? ~occupancies[(int)white] : ~occupancies[(int)black]);

                    while (attacks != 0)
                    {
                        targetSquare = GetLs1bIndex(attacks);

                        if (!GetBit((side == (int)white) ? occupancies[(int)black] : occupancies[(int)white], targetSquare))
                            AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, 0, 0, 0, 0, 0));
                        else
                            AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, 0, 1, 0, 0, 0));

                        PopBit(ref attacks, targetSquare);
                    }

                    PopBit(ref bitboard, sourceSquare);
                }
            }

            if ((side == (int)white) ? piece == B : piece == b)
            {
                while (bitboard != 0)
                {
                    sourceSquare = GetLs1bIndex(bitboard);
                    attacks = GetBishopAttacks(sourceSquare, occupancies[(int)both]) & ((side == (int)white) ? ~occupancies[(int)white] : ~occupancies[(int)black]);

                    while (attacks != 0)
                    {
                        targetSquare = GetLs1bIndex(attacks);

                        if (!GetBit((side == (int)white) ? occupancies[(int)black] : occupancies[(int)white], targetSquare))
                            AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, 0, 0, 0, 0, 0));
                        else
                            AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, 0, 1, 0, 0, 0));

                        PopBit(ref attacks, targetSquare);
                    }

                    PopBit(ref bitboard, sourceSquare);
                }
            }

            if ((side == (int)white) ? piece == R : piece == r)
            {
                while (bitboard != 0)
                {
                    sourceSquare = GetLs1bIndex(bitboard);
                    attacks = GetRookAttacks(sourceSquare, occupancies[(int)both]) & ((side == (int)white) ? ~occupancies[(int)white] : ~occupancies[(int)black]);

                    while (attacks != 0)
                    {
                        targetSquare = GetLs1bIndex(attacks);

                        if (!GetBit((side == (int)white) ? occupancies[(int)black] : occupancies[(int)white], targetSquare))
                            AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, 0, 0, 0, 0, 0));
                        else
                            AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, 0, 1, 0, 0, 0));

                        PopBit(ref attacks, targetSquare);
                    }

                    PopBit(ref bitboard, sourceSquare);
                }
            }

            if ((side == (int)white) ? piece == Q : piece == q)
            {
                while (bitboard != 0)
                {
                    sourceSquare = GetLs1bIndex(bitboard);
                    attacks = GetQueenAttacks(sourceSquare, occupancies[(int)both]) & ((side == (int)white) ? ~occupancies[(int)white] : ~occupancies[(int)black]);

                    while (attacks != 0)
                    {
                        targetSquare = GetLs1bIndex(attacks);

                        if (!GetBit((side == (int)white) ? occupancies[(int)black] : occupancies[(int)white], targetSquare))
                            AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, 0, 0, 0, 0, 0));
                        else
                            AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, 0, 1, 0, 0, 0));

                        PopBit(ref attacks, targetSquare);
                    }

                    PopBit(ref bitboard, sourceSquare);
                }
            }

            if ((side == (int)white) ? piece == K : piece == k)
            {
                while (bitboard != 0)
                {
                    sourceSquare = GetLs1bIndex(bitboard);
                    attacks = kingAttacks[sourceSquare] & ((side == (int)white) ? ~occupancies[(int)white] : ~occupancies[(int)black]);

                    while (attacks != 0)
                    {
                        targetSquare = GetLs1bIndex(attacks);

                        if (!GetBit((side == (int)white) ? occupancies[(int)black] : occupancies[(int)white], targetSquare))
                            AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, 0, 0, 0, 0, 0));
                        else
                            AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, 0, 1, 0, 0, 0));

                        PopBit(ref attacks, targetSquare);
                    }

                    PopBit(ref bitboard, sourceSquare);
                }
            }
        }
    }

    public static void AddMove(ref MoveList moveList, int move)
    {
        moveList.moves[moveList.count] = move;
        moveList.count++;
    }

    static void PrintMove(int move)
    {
        Console.WriteLine($"{squareToCoordinates[GetMoveSource(move)]}{squareToCoordinates[GetMoveTarget(move)]}{promotedPieces[GetMovePromoted(move)]}");
    }

    public static readonly char[] asciiPieces = "PNBRQKpnbrqk".ToCharArray();

    public const int P = 0, N = 1, B = 2, R = 3, Q = 4, K = 5;
    public const int p = 6, n = 7, b = 8, r = 9, q = 10, k = 11;

    public static readonly Dictionary<char, int> charPieces = new()
    {
        ['P'] = P, ['N'] = N, ['B'] = B, ['R'] = R, ['Q'] = Q, ['K'] = K,
        ['p'] = p, ['n'] = n, ['b'] = b, ['r'] = r, ['q'] = q, ['k'] = k
    };

    static readonly char[] promotedPieces = { ' ', 'n', 'b', 'r', 'q' };

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
                SetBit(ref bitboards[piece], rank * 8 + file);
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

        for (int piece = P; piece <= K; piece++)
            occupancies[(int)white] |= bitboards[piece];

        for (int piece = p; piece <= k; piece++)
            occupancies[(int)black] |= bitboards[piece];

        occupancies[(int)both] = occupancies[(int)white] | occupancies[(int)black];
    }

    public static void PrintMoveList(ref MoveList moveList)
    {
        Console.WriteLine();

        for (int i = 0; i < moveList.count; i++)
        {
            int move = moveList.moves[i];
            Console.WriteLine($"{squareToCoordinates[GetMoveSource(move)]}{squareToCoordinates[GetMoveTarget(move)]}{(GetMovePromoted(move) != 0 ? promotedPieces[GetMovePromoted(move)] : ' ')}");
        }

        Console.WriteLine($"\n      Total number of moves: {moveList.count}\n");
    }

    static void PrintBitboard(ulong bitboard)
    {
        Console.WriteLine();

        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                int square = rank * 8 + file;

                if (file == 0)
                    Console.Write($"  {8 - rank} ");

                Console.Write($" {(GetBit(bitboard, square) ? 1 : 0)}");
            }

            Console.WriteLine();
        }

        Console.WriteLine("\n     a b c d e f g h\n");
        Console.WriteLine($"     Bitboard: {bitboard}\n");
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
                    if (GetBit(bitboards[bbPiece], square))
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

    public static void SetBit(ref ulong bitboard, int square) => bitboard |= 1UL << square;
    public static bool GetBit(ulong bitboard, int square) => (bitboard & (1UL << square)) != 0;
    public static void PopBit(ref ulong bitboard, int square) => bitboard &= ~(1UL << square);

    public static int CountBits(ulong bitboard)
    {
        int count = 0;
        while (bitboard != 0)
        {
            count++;
            bitboard &= bitboard - 1;
        }
        return count;
    }

    public static int GetLs1bIndex(ulong bitboard)
    {
        return BitOperations.TrailingZeroCount(bitboard);
    }
}
