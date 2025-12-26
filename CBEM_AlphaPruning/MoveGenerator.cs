using static Board;
using static CastlingRights;
using static MoveEncoding;
using static PieceAttacks;
using static Side;
using static Square;

public static class MoveGenerator
{
    private static readonly int[] castlingRights =
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
                        sourceSquare = BitboardOperations.GetLs1bIndex(bitboard);
                        targetSquare = sourceSquare - 8;

                        if (!(targetSquare < (int)a8) && !BitboardOperations.GetBit(occupancies[(int)both], targetSquare))
                        {
                            // pawn promotion
                            if (sourceSquare >= (int)a7 && sourceSquare <= (int)h7)
                            {
                                AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, Q, 0, 0, 0, 0));
                                AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, R, 0, 0, 0, 0));
                                AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, B, 0, 0, 0, 0));
                                AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, N, 0, 0, 0, 0));
                            }
                            else
                            {
                                // normal pawn move
                                AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, 0, 0, 0, 0, 0));

                                // double push pawn move
                                if (sourceSquare >= (int)a2 && sourceSquare <= (int)h2 && !BitboardOperations.GetBit(occupancies[(int)both], targetSquare - 8))
                                    AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare - 8, piece, 0, 0, 1, 0, 0));
                            }
                        }

                        attacks = pawnAttacks[side, sourceSquare] & occupancies[(int)black];

                        while (attacks != 0)
                        {
                            targetSquare = BitboardOperations.GetLs1bIndex(attacks);

                            // pawn promotion capture
                            if (sourceSquare >= (int)a7 && sourceSquare <= (int)h7)
                            {
                                AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, Q, 1, 0, 0, 0));
                                AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, R, 1, 0, 0, 0));
                                AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, B, 1, 0, 0, 0));
                                AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, N, 1, 0, 0, 0));
                            }
                            else    // normal capture
                                AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, 0, 1, 0, 0, 0));

                            BitboardOperations.PopBit(ref attacks, targetSquare);
                        }

                        if (enPassant != (int)noSquare)
                        {
                            ulong epAttacks = pawnAttacks[side, sourceSquare] & (1UL << enPassant);

                            if (epAttacks != 0)
                                AddMove(ref moveList, EncodeMove(sourceSquare, BitboardOperations.GetLs1bIndex(epAttacks), piece, 0, 1, 0, 1, 0));
                        }

                        BitboardOperations.PopBit(ref bitboard, sourceSquare);
                    }
                }

                // White castling
                if (piece == K)
                {
                    if ((castle & (int)wk) != 0 && !BitboardOperations.GetBit(occupancies[(int)both], (int)f1) && !BitboardOperations.GetBit(occupancies[(int)both], (int)g1) && !IsSquareAttacked((int)e1, (int)black) && !IsSquareAttacked((int)f1, (int)black))
                        AddMove(ref moveList, EncodeMove((int)e1, (int)g1, piece, 0, 0, 0, 0, 1));

                    if ((castle & (int)wq) != 0 && !BitboardOperations.GetBit(occupancies[(int)both], (int)d1) && !BitboardOperations.GetBit(occupancies[(int)both], (int)c1) && !BitboardOperations.GetBit(occupancies[(int)both], (int)b1) && !IsSquareAttacked((int)e1, (int)black) && !IsSquareAttacked((int)d1, (int)black))
                        AddMove(ref moveList, EncodeMove((int)e1, (int)c1, piece, 0, 0, 0, 0, 1));
                }
            }
            else
            {
                if (piece == p)
                {
                    while (bitboard != 0)
                    {
                        sourceSquare = BitboardOperations.GetLs1bIndex(bitboard);
                        targetSquare = sourceSquare + 8;

                        if (!(targetSquare > (int)h1) && !BitboardOperations.GetBit(occupancies[(int)both], targetSquare))
                        {
                            // pawn promotion
                            if (sourceSquare >= (int)a2 && sourceSquare <= (int)h2)
                            {
                                AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, q, 0, 0, 0, 0));
                                AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, r, 0, 0, 0, 0));
                                AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, b, 0, 0, 0, 0));
                                AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, n, 0, 0, 0, 0));
                            }
                            else
                            {
                                // normal pawn move
                                AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, 0, 0, 0, 0, 0));

                                // double pawn move
                                if (sourceSquare >= (int)a7 && sourceSquare <= (int)h7 && !BitboardOperations.GetBit(occupancies[(int)both], targetSquare + 8))
                                    AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare + 8, piece, 0, 0, 1, 0, 0));
                            }
                        }

                        attacks = pawnAttacks[side, sourceSquare] & occupancies[(int)white];
                        while (attacks != 0)
                        {
                            targetSquare = BitboardOperations.GetLs1bIndex(attacks);

                            if (sourceSquare >= (int)a2 && sourceSquare <= (int)h2)
                            {
                                AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, q, 1, 0, 0, 0));
                                AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, r, 1, 0, 0, 0));
                                AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, b, 1, 0, 0, 0));
                                AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, n, 1, 0, 0, 0));
                            }
                            else
                                AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, 0, 1, 0, 0, 0));

                            BitboardOperations.PopBit(ref attacks, targetSquare);
                        }

                        if (enPassant != (int)noSquare)
                        {
                            ulong epAttacks = pawnAttacks[side, sourceSquare] & (1UL << enPassant);

                            if (epAttacks != 0)
                                AddMove(ref moveList, EncodeMove(sourceSquare, BitboardOperations.GetLs1bIndex(epAttacks), piece, 0, 1, 0, 1, 0));
                        }

                        BitboardOperations.PopBit(ref bitboard, sourceSquare);
                    }
                }

                if (piece == k)
                {
                    if ((castle & (int)bk) != 0 && !BitboardOperations.GetBit(occupancies[(int)both], (int)f8) && !BitboardOperations.GetBit(occupancies[(int)both], (int)g8) && !IsSquareAttacked((int)e8, (int)white) && !IsSquareAttacked((int)f8, (int)white))
                        AddMove(ref moveList, EncodeMove((int)e8, (int)g8, piece, 0, 0, 0, 0, 1));

                    if ((castle & (int)bq) != 0 && !BitboardOperations.GetBit(occupancies[(int)both], (int)d8) && !BitboardOperations.GetBit(occupancies[(int)both], (int)c8) && !BitboardOperations.GetBit(occupancies[(int)both], (int)b8) && !IsSquareAttacked((int)e8, (int)white) && !IsSquareAttacked((int)d8, (int)white))
                        AddMove(ref moveList, EncodeMove((int)e8, (int)c8, piece, 0, 0, 0, 0, 1));
                }
            }

            if ((side == (int)white) ? piece == N : piece == n)
            {
                while (bitboard != 0)
                {
                    sourceSquare = BitboardOperations.GetLs1bIndex(bitboard);
                    attacks = knightAttacks[sourceSquare] & ((side == (int)white) ? ~occupancies[(int)white] : ~occupancies[(int)black]);

                    while (attacks != 0)
                    {
                        targetSquare = BitboardOperations.GetLs1bIndex(attacks);

                        // quite move
                        if (!BitboardOperations.GetBit((side == (int)white) ? occupancies[(int)black] : occupancies[(int)white], targetSquare))
                            AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, 0, 0, 0, 0, 0));

                        // capture move
                        else
                            AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, 0, 1, 0, 0, 0));

                        BitboardOperations.PopBit(ref attacks, targetSquare);
                    }

                    BitboardOperations.PopBit(ref bitboard, sourceSquare);
                }
            }

            if ((side == (int)white) ? piece == B : piece == b)
            {
                while (bitboard != 0)
                {
                    sourceSquare = BitboardOperations.GetLs1bIndex(bitboard);
                    attacks = GetBishopAttacks(sourceSquare, occupancies[(int)both]) & ((side == (int)white) ? ~occupancies[(int)white] : ~occupancies[(int)black]);

                    while (attacks != 0)
                    {
                        targetSquare = BitboardOperations.GetLs1bIndex(attacks);

                        if (!BitboardOperations.GetBit((side == (int)white) ? occupancies[(int)black] : occupancies[(int)white], targetSquare))
                            AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, 0, 0, 0, 0, 0));
                        else
                            AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, 0, 1, 0, 0, 0));

                        BitboardOperations.PopBit(ref attacks, targetSquare);
                    }

                    BitboardOperations.PopBit(ref bitboard, sourceSquare);
                }
            }

            if ((side == (int)white) ? piece == R : piece == r)
            {
                while (bitboard != 0)
                {
                    sourceSquare = BitboardOperations.GetLs1bIndex(bitboard);
                    attacks = GetRookAttacks(sourceSquare, occupancies[(int)both]) & ((side == (int)white) ? ~occupancies[(int)white] : ~occupancies[(int)black]);

                    while (attacks != 0)
                    {
                        targetSquare = BitboardOperations.GetLs1bIndex(attacks);

                        if (!BitboardOperations.GetBit((side == (int)white) ? occupancies[(int)black] : occupancies[(int)white], targetSquare))
                            AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, 0, 0, 0, 0, 0));
                        else
                            AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, 0, 1, 0, 0, 0));

                        BitboardOperations.PopBit(ref attacks, targetSquare);
                    }

                    BitboardOperations.PopBit(ref bitboard, sourceSquare);
                }
            }

            if ((side == (int)white) ? piece == Q : piece == q)
            {
                while (bitboard != 0)
                {
                    sourceSquare = BitboardOperations.GetLs1bIndex(bitboard);
                    attacks = GetQueenAttacks(sourceSquare, occupancies[(int)both]) & ((side == (int)white) ? ~occupancies[(int)white] : ~occupancies[(int)black]);

                    while (attacks != 0)
                    {
                        targetSquare = BitboardOperations.GetLs1bIndex(attacks);

                        if (!BitboardOperations.GetBit((side == (int)white) ? occupancies[(int)black] : occupancies[(int)white], targetSquare))
                            AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, 0, 0, 0, 0, 0));
                        else
                            AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, 0, 1, 0, 0, 0));

                        BitboardOperations.PopBit(ref attacks, targetSquare);
                    }

                    BitboardOperations.PopBit(ref bitboard, sourceSquare);
                }
            }

            if ((side == (int)white) ? piece == K : piece == k)
            {
                while (bitboard != 0)
                {
                    sourceSquare = BitboardOperations.GetLs1bIndex(bitboard);
                    attacks = kingAttacks[sourceSquare] & ((side == (int)white) ? ~occupancies[(int)white] : ~occupancies[(int)black]);

                    while (attacks != 0)
                    {
                        targetSquare = BitboardOperations.GetLs1bIndex(attacks);

                        if (!BitboardOperations.GetBit((side == (int)white) ? occupancies[(int)black] : occupancies[(int)white], targetSquare))
                            AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, 0, 0, 0, 0, 0));
                        else
                            AddMove(ref moveList, EncodeMove(sourceSquare, targetSquare, piece, 0, 1, 0, 0, 0));

                        BitboardOperations.PopBit(ref attacks, targetSquare);
                    }

                    BitboardOperations.PopBit(ref bitboard, sourceSquare);
                }
            }
        }
    }

    public static void AddMove(ref MoveList moveList, int move)
    {
        moveList.moves[moveList.count] = move;
        moveList.count++;
    }

    public static int MakeMove(int move, int moveFlag)
    {
        if (moveFlag == (int)MoveFlag.allMoves)
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

            BitboardOperations.PopBit(ref bitboards[piece], sourceSquare);
            BitboardOperations.SetBit(ref bitboards[piece], targetSquare);

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
                    if (BitboardOperations.GetBit(bitboards[bbPiece], targetSquare))
                    {
                        BitboardOperations.PopBit(ref bitboards[bbPiece], targetSquare);
                        break;
                    }
                }
            }

            if (promotedPiece != 0)
            {
                BitboardOperations.PopBit(ref bitboards[(side == (int)white) ? P : p], targetSquare);

                BitboardOperations.SetBit(ref bitboards[promotedPiece], targetSquare);
            }

            if (enpass != 0)
            {
                if (side == (int)white)
                    BitboardOperations.PopBit(ref bitboards[p], targetSquare + 8);
                else
                    BitboardOperations.PopBit(ref bitboards[P], targetSquare - 8);
            }

            enPassant = (int)noSquare;

            if (doublePush != 0)
            {
                enPassant = (side == (int)white)
                    ? targetSquare + 8
                    : targetSquare - 8;
            }

            if (castling != 0)
            {
                switch ((Square)targetSquare)
                {
                    case g1:
                        BitboardOperations.PopBit(ref bitboards[R], (int)h1);
                        BitboardOperations.SetBit(ref bitboards[R], (int)f1);
                        break;
                    case c1:
                        BitboardOperations.PopBit(ref bitboards[R], (int)a1);
                        BitboardOperations.SetBit(ref bitboards[R], (int)d1);
                        break;
                    case g8:
                        BitboardOperations.PopBit(ref bitboards[r], (int)h8);
                        BitboardOperations.SetBit(ref bitboards[r], (int)f8);
                        break;
                    case c8:
                        BitboardOperations.PopBit(ref bitboards[r], (int)a8);
                        BitboardOperations.SetBit(ref bitboards[r], (int)d8);
                        break;
                }
            }

            castle &= castlingRights[sourceSquare];
            castle &= castlingRights[targetSquare];

            Array.Fill(occupancies, 0UL);
            UpdateOccupancies();

            side ^= 1;

            int kingSquare = (side == (int)white)
                ? BitboardOperations.GetLs1bIndex(bitboards[k])
                : BitboardOperations.GetLs1bIndex(bitboards[K]);

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
                MakeMove(move, (int)MoveFlag.allMoves);

            return 0;
        }
    }
}
