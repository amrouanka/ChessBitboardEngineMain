using System;
using static Program;
using static Color;

public static class PieceAttacks
{
    /*
         not A file

    8  0 1 1 1 1 1 1 1
    7  0 1 1 1 1 1 1 1
    6  0 1 1 1 1 1 1 1
    5  0 1 1 1 1 1 1 1
    4  0 1 1 1 1 1 1 1
    3  0 1 1 1 1 1 1 1
    2  0 1 1 1 1 1 1 1
    1  0 1 1 1 1 1 1 1

            not H file
        
    8  1 1 1 1 1 1 1 0
    7  1 1 1 1 1 1 1 0
    6  1 1 1 1 1 1 1 0
    5  1 1 1 1 1 1 1 0
    4  1 1 1 1 1 1 1 0
    3  1 1 1 1 1 1 1 0
    2  1 1 1 1 1 1 1 0
    1  1 1 1 1 1 1 1 0

        not HG file

    8  1 1 1 1 1 1 0 0
    7  1 1 1 1 1 1 0 0
    6  1 1 1 1 1 1 0 0
    5  1 1 1 1 1 1 0 0
    4  1 1 1 1 1 1 0 0
    3  1 1 1 1 1 1 0 0
    2  1 1 1 1 1 1 0 0
    1  1 1 1 1 1 1 0 0

        not AB file

    8  0 0 1 1 1 1 1 1
    7  0 0 1 1 1 1 1 1
    6  0 0 1 1 1 1 1 1
    5  0 0 1 1 1 1 1 1
    4  0 0 1 1 1 1 1 1
    3  0 0 1 1 1 1 1 1
    2  0 0 1 1 1 1 1 1
    1  0 0 1 1 1 1 1 1
    */
    public const ulong NotAFile = 18374403900871474942UL;
    public const ulong NotHFile = 9187201950435737471UL;
    public const ulong NotHGFile = 4557430888798830399UL;
    public const ulong NotABFile = 18229723555195321596UL;

	public static ulong[,] pawnAttacks = new ulong[2, 64];
    public static ulong[] knightAttacks = new ulong[64];

    public static ulong MaskPawnAttacks(int side, int square)
    {
        ulong attacks = 0UL;
        ulong bitboard = 0UL;

        SetBit(ref bitboard, square);

        if (side == 0) // white
        {
            if ((bitboard >> 7 & NotAFile) != 0) attacks |= bitboard >> 7;
            if ((bitboard >> 9 & NotHFile) != 0) attacks |= bitboard >> 9;
        }
        else // black
        {
            if ((bitboard << 7 & NotHFile) != 0) attacks |= bitboard << 7;
            if ((bitboard << 9 & NotAFile) != 0) attacks |= bitboard << 9;
        }

        return attacks;
    }

    public static ulong MaskKnightAttacks(int side, int square)
    {
        return 0ul;
    }

    public static void InitLeapersAttacks()
    {
        for (int square = 0; square < 64; square++)
        {
            pawnAttacks[(int)white, square] = MaskPawnAttacks((int)white, square);
            pawnAttacks[(int)black, square] = MaskPawnAttacks((int)black, square);
        }
    }
}