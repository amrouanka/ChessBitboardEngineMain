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
    public static ulong[] kingAttacks = new ulong[64];

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

    static ulong MaskKingAttacks(int square)
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

    static ulong MaskBishopAttacks(int square)
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

    static ulong MaskRookAttacks(int square)
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

    static ulong BishopAttacksOnTheFly(int square, ulong block)
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

    static ulong RookAttacksOnTheFly(int square, ulong block)
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
}