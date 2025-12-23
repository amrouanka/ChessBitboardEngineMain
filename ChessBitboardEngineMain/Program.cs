using static Side;
using static Square;
using static CastlingRights;
using static PieceAttacks;
using System.Numerics;

class Program
{
    static void Main()
    {
        InitAll();

        RunSelfTests();
    }
    static void RunSelfTests()
{
    Console.WriteLine("=== RUNNING ENGINE SELF TESTS ===\n");

    // ---------- FEN TEST ----------
    ParseFEN(TrickyPosition);

    int whiteCount = CountBits(occupancies[(int)Side.white]);
    int blackCount = CountBits(occupancies[(int)Side.black]);
    int bothCount  = CountBits(occupancies[(int)Side.both]);

    if (bothCount != whiteCount + blackCount)
        Console.WriteLine("❌ ERROR: Occupancy mismatch");
    else
        Console.WriteLine("✔ Occupancy OK");

    // ---------- MOVE GENERATION TEST ----------
    MoveList moveList = new MoveList(256);
    GenerateMoves(ref moveList);

    if (moveList.count == 0)
        Console.WriteLine("❌ ERROR: No moves generated");
    else
        Console.WriteLine($"✔ Generated {moveList.count} moves");

    // ---------- MOVE VALIDATION ----------
    HashSet<int> seenMoves = new HashSet<int>();

    for (int i = 0; i < moveList.count; i++)
    {
        int move = moveList.moves[i];

        int src = GetMoveSource(move);
        int trg = GetMoveTarget(move);
        int piece = GetMovePiece(move);

        // source / target range
        if (src < 0 || src > 63 || trg < 0 || trg > 63)
            Console.WriteLine($"❌ Invalid square index in move {i}");

        // piece range
        if (piece < P || piece > k)
            Console.WriteLine($"❌ Invalid piece in move {i}");

        // illegal flag combos
        if (GetMoveDouble(move) != 0 && GetMoveCapture(move) != 0)
            Console.WriteLine($"❌ Double push + capture at move {i}");

        // duplicate moves
        if (!seenMoves.Add(move))
            Console.WriteLine($"❌ Duplicate move detected at index {i}");
    }

    Console.WriteLine("✔ Move validation complete");

    // ---------- ENPASSANT SANITY ----------
    if (enPassant != (int)Square.noSquare)
    {
        if (enPassant < 0 || enPassant > 63)
            Console.WriteLine("❌ Invalid enpassant square");
        else
            Console.WriteLine("✔ Enpassant square OK");
    }

    Console.WriteLine("\n=== SELF TESTS COMPLETE ===\n");
}

    public const string EmptyBoard = "8/8/8/8/8/8/8/8 w - - ";
    public const string StartPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    public const string TrickyPosition = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1";
    public const string KillerPosition = "rnbqkb1r/pp1p1pPp/8/2p1pP2/1P1P4/3P3P/P1P1P3/RNBQKBNR w KQkq e6 0 1";
    public const string CmkPosition = "r2q1rk1/ppp2ppp/2n1bn2/2b1p3/3pP3/3P1NPP/PPP1NPB1/R1BQ1RK1 b - - 0 1";

/*
          binary move bits                               hexadecimal constants
    
    0000 0000 0000 0000 0011 1111    source square       0x3f
    0000 0000 0000 1111 1100 0000    target square       0xfc0
    0000 0000 1111 0000 0000 0000    piece               0xf000
    0000 1111 0000 0000 0000 0000    promoted piece      0xf0000
    0001 0000 0000 0000 0000 0000    capture flag        0x100000
    0010 0000 0000 0000 0000 0000    double push flag    0x200000
    0100 0000 0000 0000 0000 0000    enpassant flag      0x400000
    1000 0000 0000 0000 0000 0000    castling flag       0x800000
*/
    static int EncodeMove(
        int source,
        int target,
        int piece,
        int promoted,
        int capture,
        int @double,
        int enpassant,
        int castling)
    {
        return
            source |
            (target << 6) |
            (piece << 12) |
            (promoted << 16) |
            (capture << 20) |
            (@double << 21) |
            (enpassant << 22) |
            (castling << 23);
    }

    static int GetMoveSource(int move)     => move & 0x3F;

    static int GetMoveTarget(int move)     => (move & 0xFC0) >> 6;

    static int GetMovePiece(int move)      => (move & 0xF000) >> 12;

    static int GetMovePromoted(int move)   => (move & 0xF0000) >> 16;

    static int GetMoveCapture(int move)    => move & 0x100000;

    static int GetMoveDouble(int move)     => move & 0x200000;

    static int GetMoveEnpassant(int move)  => move & 0x400000;

    static int GetMoveCastling(int move)   => move & 0x800000;

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

     // generate all moves
    public static void GenerateMoves(ref MoveList moveList)
    {
        int sourceSquare, targetSquare;
        ulong bitboard, attacks;

        for (int piece = P; piece <= k; piece++)
        {
            bitboard = bitboards[piece];

            // ================= WHITE =================
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

                                if (sourceSquare >= (int)a2 && sourceSquare <= (int)h2 &&
                                    !GetBit(occupancies[(int)both], targetSquare - 8))
                                {
                                    AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare - 8, piece, 0, 0, 1, 0, 0));
                                }
                            }
                        }

                        attacks = pawnAttacks[side, sourceSquare] & occupancies[(int)black];

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
                            {
                                AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, 0, 1, 0, 0, 0));
                            }

                            PopBit(ref attacks, targetSquare);
                        }

                        if (enPassant != (int)noSquare)
                        {
                            ulong epAttacks =
                                pawnAttacks[side, sourceSquare] & (1UL << enPassant);

                            if (epAttacks != 0)
                            {
                                int epTarget = GetLs1bIndex(epAttacks);
                                AddMove(ref moveList, EncodeMove(sourceSquare, epTarget, piece, 0, 1, 0, 1, 0));
                            }
                        }

                        PopBit(ref bitboard, sourceSquare);
                    }
                }

                if (piece == K)
                {
                    if ((castle & (int)wk) != 0 &&
                        !GetBit(occupancies[(int)both], (int)f1) &&
                        !GetBit(occupancies[(int)both], (int)g1) &&
                        (IsSquareAttacked((int)e1, (int)black) != 0) &&
                        (IsSquareAttacked((int)f1, (int)black) != 0))
                    {
                        AddMove(ref moveList, EncodeMove((int)e1, (int)g1, piece, 0, 0, 0, 0, 1));
                    }

                    if ((castle & (int)wq) != 0 &&
                        !GetBit(occupancies[(int)both], (int)d1) &&
                        !GetBit(occupancies[(int)both], (int)c1) &&
                        !GetBit(occupancies[(int)both], (int)b1) &&
                        (IsSquareAttacked((int)e1, (int)black) != 0) &&
                        (IsSquareAttacked((int)d1, (int)black) != 0))
                    {
                        AddMove(ref moveList, EncodeMove((int)e1, (int)c1, piece, 0, 0, 0, 0, 1));
                    }
                }
            }

            // ================= BLACK =================
            else
            {
                if (piece == p)
                {
                    while (bitboard != 0)
                    {
                        sourceSquare = GetLs1bIndex(bitboard);
                        targetSquare = sourceSquare + 8;

                        if (!(targetSquare > (int)h1) &&
                            !GetBit(occupancies[(int)both], targetSquare))
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

                                if (sourceSquare >= (int)a7 && sourceSquare <= (int)h7 &&
                                    !GetBit(occupancies[(int)both], targetSquare + 8))
                                {
                                    AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare + 8, piece, 0, 0, 1, 0, 0));
                                }
                            }
                        }

                        attacks = pawnAttacks[side, sourceSquare] & occupancies[(int)white];

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
                            {
                                AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, 0, 1, 0, 0, 0));
                            }

                            PopBit(ref attacks, targetSquare);
                        }

                        if (enPassant != (int)noSquare)
                        {
                            ulong epAttacks =
                                pawnAttacks[side, sourceSquare] & (1UL << enPassant);

                            if (epAttacks != 0)
                            {
                                int epTarget = GetLs1bIndex(epAttacks);
                                AddMove(ref moveList, EncodeMove(sourceSquare, epTarget, piece, 0, 1, 0, 1, 0));
                            }
                        }

                        PopBit(ref bitboard, sourceSquare);
                    }
                }

                if (piece == k)
                {
                    if ((castle & (int)bk) != 0 &&
                        !GetBit(occupancies[(int)both], (int)f8) &&
                        !GetBit(occupancies[(int)both], (int)g8) &&
                        (IsSquareAttacked((int)e8, (int)white) != 0) &&
                        (IsSquareAttacked((int)f8, (int)white) != 0))
                    {
                        AddMove(ref moveList, EncodeMove((int)e8, (int)g8, piece, 0, 0, 0, 0, 1));
                    }

                    if ((castle & (int)bq) != 0 &&
                        !GetBit(occupancies[(int)both], (int)d8) &&
                        !GetBit(occupancies[(int)both], (int)c8) &&
                        !GetBit(occupancies[(int)both], (int)b8) &&
                        (IsSquareAttacked((int)e8, (int)white) != 0) &&
                        (IsSquareAttacked((int)d8, (int)white) != 0))
                    {
                        AddMove(ref moveList, EncodeMove((int)e8, (int)c8, piece, 0, 0, 0, 0, 1));
                    }
                }
            }

            // generate knight moves
            if ((side == (int)white) ? piece == N : piece == n)
            {
                while (bitboard != 0)
                {
                    sourceSquare = GetLs1bIndex(bitboard);

                    attacks = knightAttacks[sourceSquare] &
                            ((side == (int)white) ? ~occupancies[(int)white] : ~occupancies[(int)black]);

                    while (attacks != 0)
                    {
                        targetSquare = GetLs1bIndex(attacks);

                        if (!GetBit(
                            (side == (int)white) ? occupancies[(int)black] : occupancies[(int)white],
                            targetSquare))
                            AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, 0, 0, 0, 0, 0));
                        else
                            AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, 0, 1, 0, 0, 0));

                        PopBit(ref attacks, targetSquare);
                    }

                    PopBit(ref bitboard, sourceSquare);
                }
            }

            // generate bishop moves
            if ((side == (int)white) ? piece == B : piece == b)
            {
                while (bitboard != 0)
                {
                    sourceSquare = GetLs1bIndex(bitboard);

                    attacks = GetBishopAttacks(sourceSquare, occupancies[(int)both]) &
                            ((side == (int)white) ? ~occupancies[(int)white] : ~occupancies[(int)black]);

                    while (attacks != 0)
                    {
                        targetSquare = GetLs1bIndex(attacks);

                        if (!GetBit(
                            (side == (int)white) ? occupancies[(int)black] : occupancies[(int)white],
                            targetSquare))
                            AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, 0, 0, 0, 0, 0));
                        else
                            AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, 0, 1, 0, 0, 0));

                        PopBit(ref attacks, targetSquare);
                    }

                    PopBit(ref bitboard, sourceSquare);
                }
            }

            // generate rook moves
            if ((side == (int)white) ? piece == R : piece == r)
            {
                while (bitboard != 0)
                {
                    sourceSquare = GetLs1bIndex(bitboard);

                    attacks = GetRookAttacks(sourceSquare, occupancies[(int)both]) &
                            ((side == (int)white) ? ~occupancies[(int)white] : ~occupancies[(int)black]);

                    while (attacks != 0)
                    {
                        targetSquare = GetLs1bIndex(attacks);

                        if (!GetBit(
                            (side == (int)white) ? occupancies[(int)black] : occupancies[(int)white],
                            targetSquare))
                            AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, 0, 0, 0, 0, 0));
                        else
                            AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, 0, 1, 0, 0, 0));

                        PopBit(ref attacks, targetSquare);
                    }

                    PopBit(ref bitboard, sourceSquare);
                }
            }

            // generate queen moves
            if ((side == (int)white) ? piece == Q : piece == q)
            {
                while (bitboard != 0)
                {
                    sourceSquare = GetLs1bIndex(bitboard);

                    attacks = GetQueenAttacks(sourceSquare, occupancies[(int)both]) &
                            ((side == (int)white) ? ~occupancies[(int)white] : ~occupancies[(int)black]);

                    while (attacks != 0)
                    {
                        targetSquare = GetLs1bIndex(attacks);

                        if (!GetBit(
                            (side == (int)white) ? occupancies[(int)black] : occupancies[(int)white],
                            targetSquare))
                            AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, 0, 0, 0, 0, 0));
                        else
                            AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, 0, 1, 0, 0, 0));

                        PopBit(ref attacks, targetSquare);
                    }

                    PopBit(ref bitboard, sourceSquare);
                }
            }

            // generate king moves
            if ((side == (int)white) ? piece == K : piece == k)
            {
                while (bitboard != 0)
                {
                    sourceSquare = GetLs1bIndex(bitboard);

                    attacks = kingAttacks[sourceSquare] &
                            ((side == (int)white) ? ~occupancies[(int)white] : ~occupancies[(int)black]);

                    while (attacks != 0)
                    {
                        targetSquare = GetLs1bIndex(attacks);

                        if (!GetBit(
                            (side == (int)white) ? occupancies[(int)black] : occupancies[(int)white],
                            targetSquare))
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
        Console.WriteLine(
            $"{squareToCoordinates[GetMoveSource(move)]}" +
            $"{squareToCoordinates[GetMoveTarget(move)]}" +
            $"{promotedPieces[GetMovePromoted(move)]}"
        );
    }

    // ASCII pieces
    public static readonly char[] asciiPieces = "PNBRQKpnbrqk".ToCharArray();

    // Encoded piece constants
    public const int P = 0, N = 1, B = 2, R = 3, Q = 4, K = 5;
    public const int p = 6, n = 7, b = 8, r = 9, q = 10, k = 11;

    // map ASCII characters to piece constants
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

    // piece bitboards
    public static ulong[] bitboards = new ulong[12];

    // occupancy bitboards ((int)Color.white, (int)Color.black, both)
    public static ulong[] occupancies = new ulong[3];

    // side to move (-1 = none, 0 = (int)Color.white, 1 = (int)Color.black)
    public static int side = -1;

    // enpassant square
    public static int enPassant = (int) noSquare;

    // castling rights
    public static int castle;

    public static void ParseFEN(string fen)
    {
        // reset board position (bitboards)
        Array.Fill(bitboards, 0UL);

        // reset occupancies (bitboards)
        Array.Fill(occupancies, 0UL);

        // reset game state variables
        side = 0;
        enPassant = (int)Square.noSquare;
        castle = 0;

        int rank = 0, file = 0;
        for (int i = 0; i < fen.Length && rank < 8; i++)
        {
            char c = fen[i];

            // match ascii pieces
            if (char.IsLetter(c))
            {
                int piece = charPieces[c];
                SetBit(ref bitboards[piece], rank * 8 + file);
                file++;
            }
            // match empty squares
            else if (char.IsDigit(c))
            {
                file += c - '0';
            }
            // rank separator
            else if (c == '/')
            {
                rank++;
                file = 0;
            }
            // end of piece placement
            else if (c == ' ')
            {
                i++;
                // side to move
                side = (fen[i] == 'w') ? (int)Side.white : (int)Side.black;
                i += 2; // skip space after side

                // castling rights
                while (fen[i] != ' ')
                {
                    switch (fen[i])
                    {
                        case 'K': castle |= (int)CastlingRights.wk; break;
                        case 'Q': castle |= (int)CastlingRights.wq; break;
                        case 'k': castle |= (int)CastlingRights.bk; break;
                        case 'q': castle |= (int)CastlingRights.bq; break;
                    }
                    i++;
                }

                i++; // en passant
                if (fen[i] != '-')
                {
                    int epFile = fen[i] - 'a';
                    int epRank = 8 - (fen[i + 1] - '0');
                    enPassant = epRank * 8 + epFile;
                }
                else
                    enPassant = (int)Square.noSquare;

                break; // done parsing
            }
        }

        // populate occupancies
        for (int piece = P; piece <= K; piece++)
            occupancies[(int)Side.white] |= bitboards[piece];

        for (int piece = p; piece <= k; piece++)
            occupancies[(int)Side.black] |= bitboards[piece];

        occupancies[(int)Side.both] = occupancies[(int)Side.white] | occupancies[(int)Side.black];
    }

    public static void PrintMoveList(ref MoveList moveList)
    {
        Console.WriteLine();
        Console.WriteLine("    move    piece   capture   double    enpass    castling\n");

        for (int i = 0; i < moveList.count; i++)
        {
            int move = moveList.moves[i];

            Console.WriteLine(
            $"      {squareToCoordinates[GetMoveSource(move)]}" +
            $"{squareToCoordinates[GetMoveTarget(move)]}" +
            $"{(GetMovePromoted(move) != 0 ? promotedPieces[GetMovePromoted(move)] : ' ')}   " +
            $"{asciiPieces[GetMovePiece(move)]}         " +
            $"{(GetMoveCapture(move) != 0 ? 1 : 0)}         " +
            $"{(GetMoveDouble(move) != 0 ? 1 : 0)}         " +
            $"{(GetMoveEnpassant(move) != 0 ? 1 : 0)}         " +
            $"{(GetMoveCastling(move) != 0 ? 1 : 0)}"
            );
        }

        Console.WriteLine($"\n      Total number of moves: {moveList.count}\n");
    }
/*

*/
    static void PrintBitboard(ulong bitboard)
    {
        Console.WriteLine();

        // loop over board ranks
        for (int rank = 0; rank < 8; rank++)
        {
            // loop over board files
            for (int file = 0; file < 8; file++)
            {
                // convert file & rank into square index
                int square = rank * 8 + file;

                // print ranks
                if (file == 0)
                    Console.Write($"  {8 - rank} ");

                // print bit state (1 or 0)
                Console.Write($" {(GetBit(bitboard, square) ? 1 : 0)}");
            }

            // new line every rank
            Console.WriteLine();
        }

        // print board files
        Console.WriteLine("\n     a b c d e f g h\n");

        // print bitboard as unsigned decimal number
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

                for (int bbPiece = (int)Piece.P; bbPiece <= (int)Piece.k; bbPiece++)
                {
                    if (GetBit(bitboards[bbPiece], square))
                        piece = bbPiece;
                }

                Console.Write($" {(piece == -1 ? '.' : asciiPieces[piece])}");
            }

            Console.WriteLine();
        }

        Console.WriteLine("\n     a b c d e f g h\n");

        Console.WriteLine($"     Side:     {(side == 0 ? "white" : "black")}");

        Console.WriteLine($"     Enpassant:   {(enPassant != (int)noSquare ? squareToCoordinates[enPassant] : "no")}");

        Console.WriteLine($"     Castling:  {( (castle & (int)CastlingRights.wk) != 0 ? 'K' : '-')}" +
                        $"{( (castle & (int)CastlingRights.wq) != 0 ? 'Q' : '-')}" +
                        $"{( (castle & (int)CastlingRights.bk) != 0 ? 'k' : '-')}" +
                        $"{( (castle & (int)CastlingRights.bq) != 0 ? 'q' : '-')}\n");
    }

    public static void SetBit(ref ulong bitboard, int square)
    {
        bitboard |= 1UL << square;
    }

    public static bool GetBit(ulong bitboard, int square)
    {
        return (bitboard & (1UL << square)) != 0;
    }

    public static void PopBit(ref ulong bitboard, int square)
    {
        bitboard &= ~(1UL << square);
    }

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

/*

    // pseudo random number state
    public static uint randomState = 1804289383u;

    // generate 32-bit pseudo random numbers
    public static uint GetRandomU32()
    {
        uint number = randomState;

        number ^= number << 13;
        number ^= number >> 17;
        number ^= number << 5;

        randomState = number;
        return number;
    }

    // generate 64-bit pseudo random numbers
    public static ulong GetRandomU64()
    {
        ulong n1 = GetRandomU32() & 0xFFFFUL;
        ulong n2 = GetRandomU32() & 0xFFFFUL;
        ulong n3 = GetRandomU32() & 0xFFFFUL;
        ulong n4 = GetRandomU32() & 0xFFFFUL;

        return n1
            | (n2 << 16)
            | (n3 << 32)
            | (n4 << 48);
    }

    // generate magic number candidate
    public static ulong GenerateMagicNumber()
    {
        return GetRandomU64()
            & GetRandomU64()
            & GetRandomU64();
    }
    public static ulong FindMagicNumber(int square, int relevantBits, Piece piece)
    {
        // init occupancies
        ulong[] occupancies = new ulong[4096];

        // init attack tables
        ulong[] attacks = new ulong[4096];

        // init used attacks
        ulong[] usedAttacks = new ulong[4096];

        // init attack mask for current piece
        ulong attackMask = piece == Piece.bishop
            ? MaskBishopAttacks(square)
            : MaskRookAttacks(square);

        // init occupancy indices
        int occupancyIndices = 1 << relevantBits;

        // loop over occupancy indices
        for (int index = 0; index < occupancyIndices; index++)
        {
            occupancies[index] = SetOccupancy(index, relevantBits, attackMask);

            attacks[index] = piece == Piece.bishop
                ? BishopAttacksOnTheFly(square, occupancies[index])
                : RookAttacksOnTheFly(square, occupancies[index]);
        }

        // test magic numbers
        for (int randomCount = 0; randomCount < 100_000_000; randomCount++)
        {
            ulong magicNumber = GenerateMagicNumber();

            // skip weak magic numbers
            if (CountBits((attackMask * magicNumber) & 0xFF00000000000000UL) < 6)
                continue;

            // reset used attacks
            Array.Clear(usedAttacks, 0, usedAttacks.Length);

            bool fail = false;

            // test magic indexing
            for (int index = 0; index < occupancyIndices && !fail; index++)
            {
                int magicIndex = (int)((occupancies[index] * magicNumber) >> (64 - relevantBits));

                if (usedAttacks[magicIndex] == 0UL)
                {
                    usedAttacks[magicIndex] = attacks[index];
                }
                else if (usedAttacks[magicIndex] != attacks[index])
                {
                    fail = true;
                }
            }

            // magic number works
            if (!fail)
                return magicNumber;
        }

        Console.WriteLine("Magic number fails!");
        return 0UL;
    }
    public static void InitMagicNumbers()
    {
        for (int square = 0; square < 64; square++)
            rookMagicNumbers[square] =
                FindMagicNumber(square, rookRelevantBits[square], Piece.rook);

        for (int square = 0; square < 64; square++)
            bishopMagicNumbers[square] =
                FindMagicNumber(square, bishopRelevantBits[square], Piece.bishop);
    }
*/