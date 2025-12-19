using System;
using static Square;
using static PieceAttacks;

class Program
{
    static void Main()
    {
        InitLeapersAttacks();
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
}
