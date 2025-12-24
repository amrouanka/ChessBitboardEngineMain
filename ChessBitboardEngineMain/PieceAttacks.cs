
using static Program;
using static Side;

public static class PieceAttacks
{
    public const ulong NotAFile = 18374403900871474942UL;
    public const ulong NotHFile = 9187201950435737471UL;
    public const ulong NotHGFile = 4557430888798830399UL;
    public const ulong NotABFile = 18229723555195321596UL;

    public static ulong[,] pawnAttacks = new ulong[2, 64];
    public static ulong[] knightAttacks = new ulong[64];
    public static ulong[] kingAttacks = new ulong[64];

    public static readonly int[] bishopRelevantBits =
    {
        6, 5, 5, 5, 5, 5, 5, 6, 
        5, 5, 5, 5, 5, 5, 5, 5, 
        5, 5, 7, 7, 7, 7, 5, 5, 
        5, 5, 7, 9, 9, 7, 5, 5, 
        5, 5, 7, 9, 9, 7, 5, 5, 
        5, 5, 7, 7, 7, 7, 5, 5, 
        5, 5, 5, 5, 5, 5, 5, 5, 
        6, 5, 5, 5, 5, 5, 5, 6
    };

    public static readonly int[] rookRelevantBits =
    {
        12, 11, 11, 11, 11, 11, 11, 12, 
        11, 10, 10, 10, 10, 10, 10, 11, 
        11, 10, 10, 10, 10, 10, 10, 11, 
        11, 10, 10, 10, 10, 10, 10, 11, 
        11, 10, 10, 10, 10, 10, 10, 11, 
        11, 10, 10, 10, 10, 10, 10, 11, 
        11, 10, 10, 10, 10, 10, 10, 11, 
        12, 11, 11, 11, 11, 11, 11, 12
    };

    public static readonly ulong[] rookMagicNumbers =
    {
        0x8A80104000800020UL,
        0x0140002000100040UL,
        0x02801880A0017001UL,
        0x0100081001000420UL,
        0x0200020010080420UL,
        0x03001C0002010008UL,
        0x8480008002000100UL,
        0x2080088004402900UL,
        0x0000800098204000UL,
        0x2024401000200040UL,
        0x0100802000801000UL,
        0x0120800800801000UL,
        0x0208808088000400UL,
        0x0002802200800400UL,
        0x2200800100020080UL,
        0x0801000060821100UL,
        0x0080044006422000UL,
        0x0100808020004000UL,
        0x12108A0010204200UL,
        0x0140848010000802UL,
        0x0481828014002800UL,
        0x8094004002004100UL,
        0x4010040010010802UL,
        0x0000020008806104UL,
        0x0100400080208000UL,
        0x2040002120081000UL,
        0x0021200680100081UL,
        0x0020100080080080UL,
        0x0002000A00200410UL,
        0x0000020080800400UL,
        0x0080088400100102UL,
        0x0080004600042881UL,
        0x4040008040800020UL,
        0x0440003000200801UL,
        0x0004200011004500UL,
        0x0188020010100100UL,
        0x0014800401802800UL,
        0x2080040080800200UL,
        0x0124080204001001UL,
        0x0200046502000484UL,
        0x0480400080088020UL,
        0x1000422010034000UL,
        0x0030200100110040UL,
        0x0000100021010009UL,
        0x2002080100110004UL,
        0x0202008004008002UL,
        0x0020020004010100UL,
        0x2048440040820001UL,
        0x0101002200408200UL,
        0x0040802000401080UL,
        0x4008142004410100UL,
        0x02060820C0120200UL,
        0x0001001004080100UL,
        0x020C020080040080UL,
        0x2935610830022400UL,
        0x0044440041009200UL,
        0x0280001040802101UL,
        0x2100190040002085UL,
        0x80C0084100102001UL,
        0x4024081001000421UL,
        0x00020030A0244872UL,
        0x0012001008414402UL,
        0x02006104900A0804UL,
        0x0001004081002402UL,
    };
    public static readonly ulong[] bishopMagicNumbers =
    {
        0x0040040844404084UL,
        0x002004208A004208UL,
        0x0010190041080202UL,
        0x0108060845042010UL,
        0x0581104180800210UL,
        0x2112080446200010UL,
        0x1080820820060210UL,
        0x03C0808410220200UL,
        0x0004050404440404UL,
        0x0000021001420088UL,
        0x24D0080801082102UL,
        0x0001020A0A020400UL,
        0x0000040308200402UL,
        0x0004011002100800UL,
        0x0401484104104005UL,
        0x0801010402020200UL,
        0x00400210C3880100UL,
        0x0404022024108200UL,
        0x0810018200204102UL,
        0x0004002801A02003UL,
        0x0085040820080400UL,
        0x810102C808880400UL,
        0x000E900410884800UL,
        0x8002020480840102UL,
        0x0220200865090201UL,
        0x2010100A02021202UL,
        0x0152048408022401UL,
        0x0020080002081110UL,
        0x4001001021004000UL,
        0x800040400A011002UL,
        0x00E4004081011002UL,
        0x001C004001012080UL,
        0x8004200962A00220UL,
        0x8422100208500202UL,
        0x2000402200300C08UL,
        0x8646020080080080UL,
        0x80020A0200100808UL,
        0x2010004880111000UL,
        0x623000A080011400UL,
        0x42008C0340209202UL,
        0x0209188240001000UL,
        0x400408A884001800UL,
        0x00110400A6080400UL,
        0x1840060A44020800UL,
        0x0090080104000041UL,
        0x0201011000808101UL,
        0x1A2208080504F080UL,
        0x0500861011240000UL,
        0x0180806108200800UL,
        0x4000020E01040044UL,
        0x300000261044000AUL,
        0x0802241102020002UL,
        0x0020906061210001UL,
        0x5A84841004010310UL,
        0x0004010801011C04UL,
        0x000A010109502200UL,
        0x0000004A02012000UL,
        0x500201010098B028UL,
        0x8040002811040900UL,
        0x0028000010020204UL,
        0x06000020202D0240UL,
        0x8918844842082200UL,
        0x4010011029020020UL,
        0x8040201008040200UL
    };
    
   
    public static ulong SetOccupancy(int index, int bitsInMask, ulong attackMask)
    {
        ulong occupancy = 0UL;

        for (int count = 0; count < bitsInMask; count++)
        {
            int square = GetLs1bIndex(attackMask);

            attackMask &= attackMask - 1;

            if ((index & (1 << count)) != 0)
                occupancy |= 1UL << square;
        }

        return occupancy;
    }

    public static ulong MaskPawnAttacks(int side, int square)
    {
        ulong attacks = 0UL;
        ulong bitboard = 1UL << square;

        if (side == 0) // White
        {
            // White moves UP the board (negative index change)
            if (((bitboard >> 7) & NotAFile) != 0) attacks |= bitboard >> 7;
            if (((bitboard >> 9) & NotHFile) != 0) attacks |= bitboard >> 9;
        }
        else // Black
        {
            // Black moves DOWN the board (positive index change)
            if (((bitboard << 7) & NotHFile) != 0) attacks |= bitboard << 7;
            if (((bitboard << 9) & NotAFile) != 0) attacks |= bitboard << 9;
        }
        return attacks;
    }

    public static ulong MaskKnightAttacks(int square)
    {
        ulong attacks = 0UL;
        ulong bitboard = 0UL;

        SetBit(ref bitboard, square);

        if ((bitboard >> 17 & NotHFile) != 0) attacks |= bitboard >> 17;
        if ((bitboard >> 15 & NotAFile) != 0) attacks |= bitboard >> 15;
        if ((bitboard >> 10 & NotHGFile) != 0) attacks |= bitboard >> 10;
        if ((bitboard >> 6  & NotABFile) != 0) attacks |= bitboard >> 6;
        if ((bitboard << 17 & NotAFile) != 0) attacks |= bitboard << 17;
        if ((bitboard << 15 & NotHFile) != 0) attacks |= bitboard << 15;
        if ((bitboard << 10 & NotABFile) != 0) attacks |= bitboard << 10;
        if ((bitboard << 6  & NotHGFile) != 0) attacks |= bitboard << 6;

        return attacks;

    }

    public static ulong MaskKingAttacks(int square)
    {
        ulong attacks = 0UL;
        ulong bitboard = 0UL;

        bitboard |= 1UL << square;

        if ((bitboard >> 8) != 0) attacks |= bitboard >> 8;
        if (((bitboard >> 9) & NotHFile) != 0) attacks |= bitboard >> 9;
        if (((bitboard >> 7) & NotAFile) != 0) attacks |= bitboard >> 7;
        if (((bitboard >> 1) & NotHFile) != 0) attacks |= bitboard >> 1;

        if ((bitboard << 8) != 0) attacks |= bitboard << 8;
        if (((bitboard << 9) & NotAFile) != 0) attacks |= bitboard << 9;
        if (((bitboard << 7) & NotHFile) != 0) attacks |= bitboard << 7;
        if (((bitboard << 1) & NotAFile) != 0) attacks |= bitboard << 1;

        return attacks;
    }

    public static ulong MaskBishopAttacks(int square)
    {
        ulong attacks = 0UL;

        int tr = square / 8;
        int tf = square % 8;

        for (int r = tr + 1, f = tf + 1; r <= 6 && f <= 6; r++, f++)
            attacks |= 1UL << (r * 8 + f);

        for (int r = tr - 1, f = tf + 1; r >= 1 && f <= 6; r--, f++)
            attacks |= 1UL << (r * 8 + f);

        for (int r = tr + 1, f = tf - 1; r <= 6 && f >= 1; r++, f--)
            attacks |= 1UL << (r * 8 + f);

        for (int r = tr - 1, f = tf - 1; r >= 1 && f >= 1; r--, f--)
            attacks |= 1UL << (r * 8 + f);

        return attacks;
    }

    public static ulong MaskRookAttacks(int square)
    {
        ulong attacks = 0UL;

        int tr = square / 8;
        int tf = square % 8;

        for (int r = tr + 1; r <= 6; r++)
            attacks |= 1UL << (r * 8 + tf);

        for (int r = tr - 1; r >= 1; r--)
            attacks |= 1UL << (r * 8 + tf);

        for (int f = tf + 1; f <= 6; f++)
            attacks |= 1UL << (tr * 8 + f);

        for (int f = tf - 1; f >= 1; f--)
            attacks |= 1UL << (tr * 8 + f);

        return attacks;
    }

    public static ulong BishopAttacksOnTheFly(int square, ulong block)
    {
        ulong attacks = 0UL;

        int tr = square / 8;
        int tf = square % 8;

        for (int r = tr + 1, f = tf + 1; r <= 7 && f <= 7; r++, f++)
        {
            ulong sq = 1UL << (r * 8 + f);
            attacks |= sq;
            if ((sq & block) != 0) break;
        }

        for (int r = tr - 1, f = tf + 1; r >= 0 && f <= 7; r--, f++)
        {
            ulong sq = 1UL << (r * 8 + f);
            attacks |= sq;
            if ((sq & block) != 0) break;
        }

        for (int r = tr + 1, f = tf - 1; r <= 7 && f >= 0; r++, f--)
        {
            ulong sq = 1UL << (r * 8 + f);
            attacks |= sq;
            if ((sq & block) != 0) break;
        }

        for (int r = tr - 1, f = tf - 1; r >= 0 && f >= 0; r--, f--)
        {
            ulong sq = 1UL << (r * 8 + f);
            attacks |= sq;
            if ((sq & block) != 0) break;
        }

        return attacks;
    }

    public static ulong RookAttacksOnTheFly(int square, ulong block)
    {
        ulong attacks = 0UL;

        int tr = square / 8;
        int tf = square % 8;

        for (int r = tr + 1; r <= 7; r++)
        {
            ulong sq = 1UL << (r * 8 + tf);
            attacks |= sq;
            if ((sq & block) != 0) break;
        }

        for (int r = tr - 1; r >= 0; r--)
        {
            ulong sq = 1UL << (r * 8 + tf);
            attacks |= sq;
            if ((sq & block) != 0) break;
        }

        for (int f = tf + 1; f <= 7; f++)
        {
            ulong sq = 1UL << (tr * 8 + f);
            attacks |= sq;
            if ((sq & block) != 0) break;
        }

        for (int f = tf - 1; f >= 0; f--)
        {
            ulong sq = 1UL << (tr * 8 + f);
            attacks |= sq;
            if ((sq & block) != 0) break;
        }

        return attacks;
    }

    public static void InitLeapersAttacks()
    {
        for (int square = 0; square < 64; square++)
        {
            pawnAttacks[(int)white, square] = MaskPawnAttacks((int)white, square);
            pawnAttacks[(int)black, square] = MaskPawnAttacks((int)black, square);

            knightAttacks[square] = MaskKnightAttacks(square);

            kingAttacks[square] = MaskKingAttacks(square);
        }
    }

    public static ulong[] bishopMasks = new ulong[64];

    public static ulong[] rookMasks = new ulong[64];

    public static ulong[,] bishopAttacks = new ulong[64, 512];

    public static ulong[,] rookAttacks = new ulong[64, 4096];

    public static void InitSliderAttacks(bool bishop)
    {
        for (int square = 0; square < 64; square++)
        {
            bishopMasks[square] = MaskBishopAttacks(square);
            rookMasks[square] = MaskRookAttacks(square);

            ulong attackMask = bishop ? bishopMasks[square] : rookMasks[square];
            int relevantBits = bishop ? bishopRelevantBits[square] : rookRelevantBits[square];
            int occupancyIndices = 1 << relevantBits;

            for (int index = 0; index < occupancyIndices; index++)
            {
                ulong occupancy = SetOccupancy(index, relevantBits, attackMask);
                int magicIndex;

                if (bishop)
                {
                    ulong x = bishopMagicNumbers[square];
                    int y = bishopRelevantBits[square];
                    magicIndex = (int)((occupancy * bishopMagicNumbers[square]) >> (64 - bishopRelevantBits[square]));
                    bishopAttacks[square, magicIndex] = BishopAttacksOnTheFly(square, occupancy);
                }
                else
                {
                    magicIndex = (int)((occupancy * rookMagicNumbers[square]) >> (64 - rookRelevantBits[square]));
                    rookAttacks[square, magicIndex] = RookAttacksOnTheFly(square, occupancy);
                }
            }
        }
    }

    public static ulong GetBishopAttacks(int square, ulong occupancy)
    {
        occupancy &= bishopMasks[square];
        occupancy *= bishopMagicNumbers[square];
        occupancy >>= 64 - bishopRelevantBits[square];
        return bishopAttacks[square, occupancy];
    }

    public static ulong GetRookAttacks(int square, ulong occupancy)
    {
        occupancy &= rookMasks[square];
        occupancy *= rookMagicNumbers[square];
        occupancy >>= 64 - rookRelevantBits[square];
        return rookAttacks[square, occupancy];
    }

    public static ulong GetQueenAttacks(int square, ulong occupancy)
    {
        ulong bishopOccupancy = occupancy & bishopMasks[square];
        bishopOccupancy *= bishopMagicNumbers[square];
        bishopOccupancy >>= 64 - bishopRelevantBits[square];
        ulong queenAttacks = bishopAttacks[square, bishopOccupancy];

        ulong rookOccupancy = occupancy & rookMasks[square];
        rookOccupancy *= rookMagicNumbers[square];
        rookOccupancy >>= 64 - rookRelevantBits[square];
        queenAttacks |= rookAttacks[square, rookOccupancy];

        return queenAttacks;
    }

    public static void InitAll()
    {
        InitLeapersAttacks();

        InitSliderAttacks(true);
        InitSliderAttacks(false);
    }

    public static bool IsSquareAttacked(int square, int side)
    {
        if (side == (int)white && (pawnAttacks[(int)black, square] & bitboards[P]) != 0)
            return true;

        if (side == (int)black && (pawnAttacks[(int)white, square] & bitboards[p]) != 0)
            return true;

        if ((knightAttacks[square] & (side == (int)white ? bitboards[N] : bitboards[n])) != 0)
            return true;

        if ((GetBishopAttacks(square, occupancies[(int)both]) &
            (side == (int)white ? bitboards[B] : bitboards[b])) != 0)
            return true;

        if ((GetRookAttacks(square, occupancies[(int)both]) &
            (side == (int)white ? bitboards[R] : bitboards[r])) != 0)
            return true;

        if ((GetQueenAttacks(square, occupancies[(int)both]) &
            (side == (int)white ? bitboards[Q] : bitboards[q])) != 0)
            return true;

        if ((kingAttacks[square] & (side == (int)white ? bitboards[K] : bitboards[k])) != 0)
            return true;

        return false;
    }

    public static void PrintAttackedSquares(int side)
    {
        Console.WriteLine();

        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                int square = rank * 8 + file;

                if (file == 0)
                    Console.Write($"  {8 - rank} ");

                Console.Write($" {IsSquareAttacked(square, side)}");
            }

            Console.WriteLine();
        }

        Console.WriteLine("\n     a b c d e f g h\n");
    }
}
