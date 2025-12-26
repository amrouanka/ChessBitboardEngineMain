using static Board;
using static Square;

public static class Evaluation
{
    public static readonly int[] materialScore =
    [
        100,
        320,
        340,
        500,
        1000,
        20000,
        -100,
        -320,
        -340,
        -500,
        -1000,
        -20000,
    ];

    // Center bonus table
    private static readonly int[] centerBonus =
    [
        0,  0,  0,  0,  0,  0,  0, 0,
        0, 10, 10, 10, 10, 10, 10, 0,
        0, 10, 20, 20, 20, 20, 10, 0,
        0, 10, 20, 30, 30, 20, 10, 0,
        0, 10, 20, 30, 30, 20, 10, 0,
        0, 10, 20, 20, 20, 20, 10, 0,
        0, 10, 10, 10, 10, 10, 10, 0,
        0,  0,  0,  0,  0,  0,  0, 0
    ];

    // Pawn positional score
    private static readonly int[] pawnScore =
    [
         0,   0,   0,   0,   0,   0,   0,   0,
        40,  40,  40,  40,  40,  40,  40,  40,
        20,  20,  20,  30,  30,  30,  20,  20,
        10,  10,  10,  20,  20,  10,  10,  10,
         5,   5,  10,  20,  20,   5,   5,   5,
         0,   0,   0,   5,   5,   0,   0,   0,
         0,   0,   0, -10, -10,   0,   0,   0,
         0,   0,   0,   0,   0,   0,   0,   0
    ];

    // Rook positional score
    private static readonly int[] rookScore =
    [
        25,  25,  25,  25,  25,  25,  25,  25,
        30,  30,  30,  30,  30,  30,  30,  30,
         0,   0,  10,  10,  10,  10,   0,   0,
         0,   0,  10,  10,  10,  10,   0,   0,
         0,   0,  10,  10,  10,  10,   0,   0,
         0,   0,  10,  10,  10,  10,   0,   0,
         0,   0,  10,  10,  10,  10,   0,   0,
         0,   0,   0,  10,  10,   0,   0,   0
    ];

    // King positional score
    private static readonly int[] kingScore =
    [
         0,   0,   0,   0,   0,   0,   0,   0,
         0,   0,   5,   5,   5,   5,   0,   0,
         0,   5,   5,  10,  10,   5,   5,   0,
         0,   5,  10,  20,  20,  10,   5,   0,
         0,   5,  10,  20,  20,  10,   5,   0,
         0,   0,   5,  10,  10,   5,   0,   0,
         0,   5,   5,  -5,  -5,   0,   5,   0,
         0,   0,   5,   0, -15,  -5,  10,   0
    ];

    // Mirror score table for black pieces (maps black squares to white perspective)
    private static readonly int[] mirrorScore =
    [
        (int)a1, (int)b1, (int)c1, (int)d1, (int)e1, (int)f1, (int)g1, (int)h1,
        (int)a2, (int)b2, (int)c2, (int)d2, (int)e2, (int)f2, (int)g2, (int)h2,
        (int)a3, (int)b3, (int)c3, (int)d3, (int)e3, (int)f3, (int)g3, (int)h3,
        (int)a4, (int)b4, (int)c4, (int)d4, (int)e4, (int)f4, (int)g4, (int)h4,
        (int)a5, (int)b5, (int)c5, (int)d5, (int)e5, (int)f5, (int)g5, (int)h5,
        (int)a6, (int)b6, (int)c6, (int)d6, (int)e6, (int)f6, (int)g6, (int)h6,
        (int)a7, (int)b7, (int)c7, (int)d7, (int)e7, (int)f7, (int)g7, (int)h7,
        (int)a8, (int)b8, (int)c8, (int)d8, (int)e8, (int)f8, (int)g8, (int)h8
    ];

    // Board evaluation routine with positional scoring
    public static int Evaluate()
    {
        int score = 0;
        int piece, square;

        for (int pieceBB = P; pieceBB <= k; pieceBB++)
        {
            ulong bitboard = bitboards[pieceBB];

            while (bitboard != 0)
            {
                piece = pieceBB;
                square = BitboardOperations.GetLs1bIndex(bitboard);

                score += materialScore[piece];

                // Add positional scores
                switch (piece)
                {
                    // evaluate white pieces
                    case (int)Piece.P: score += pawnScore[square]; break;
                    case (int)Piece.N: score += centerBonus[square]; break;
                    case (int)Piece.B: score += centerBonus[square]; break;
                    case (int)Piece.R: score += rookScore[square]; break;
                    case (int)Piece.K: score += kingScore[square]; break;

                    // evaluate black pieces
                    case (int)Piece.p: score -= pawnScore[mirrorScore[square]]; break;
                    case (int)Piece.n: score -= centerBonus[square]; break;
                    case (int)Piece.b: score -= centerBonus[square]; break;
                    case (int)Piece.r: score -= rookScore[mirrorScore[square]]; break;
                    case (int)Piece.k: score -= kingScore[mirrorScore[square]]; break;
                }

                BitboardOperations.PopBit(ref bitboard, square);
            }
        }

        return (side == (int)Side.white) ? score : -score;
    }
}
