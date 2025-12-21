using System;
using static Square;
using static PieceAttacks;

class Program
{
    public const string EmptyBoard = "8/8/8/8/8/8/8/8 w - - ";
    public const string StartPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    public const string TrickyPosition = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1";
    public const string KillerPosition = "rnbqkb1r/pp1p1pPp/8/2p1pP2/1P1P4/3P3P/P1P1P3/RNBQKBNR w KQkq e6 0 1";
    public const string CmkPosition = "r2q1rk1/ppp2ppp/2n1bn2/2b1p3/3pP3/3P1NPP/PPP1NPB1/R1BQ1RK1 b - - 0 1";

    static void Main()
    {
        InitAll();

        ParseFEN(StartPosition);
        PrintBoard();

        PrintAttackedSquares((int)Side.black);
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

        Console.WriteLine($"     Enpassant:   {(enPassant != (int)noSquare ? SquareToCoordinates[enPassant] : "no")}");

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

    public static readonly string[] SquareToCoordinates =
    {
        "a8", "b8", "c8", "d8", "e8", "f8", "g8", "h8",
        "a7", "b7", "c7", "d7", "e7", "f7", "g7", "h7",
        "a6", "b6", "c6", "d6", "e6", "f6", "g6", "h6",
        "a5", "b5", "c5", "d5", "e5", "f5", "g5", "h5",
        "a4", "b4", "c4", "d4", "e4", "f4", "g4", "h4",
        "a3", "b3", "c3", "d3", "e3", "f3", "g3", "h3",
        "a2", "b2", "c2", "d2", "e2", "f2", "g2", "h2",
        "a1", "b1", "c1", "d1", "e1", "f1", "g1", "h1",
    };

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
        if (bitboard != 0)
            return CountBits((bitboard & (~bitboard + 1)) - 1);

        return -1;
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