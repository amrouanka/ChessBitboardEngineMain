
public static class MoveEncoding
{
    public static int EncodeMove(int source, int target, int piece, int promoted, int capture, int @double, int enpassant, int castling)
    {
        return source | (target << 6) | (piece << 12) | (promoted << 16) | (capture << 20) | (@double << 21) | (enpassant << 22) | (castling << 23);
    }

    public static int GetMoveSource(int move) => move & 0x3F;
    public static int GetMoveTarget(int move) => (move & 0xFC0) >> 6;
    public static int GetMovePiece(int move) => (move & 0xF000) >> 12;
    public static int GetMovePromoted(int move) => (move & 0xF0000) >> 16;
    public static int GetMoveCapture(int move) => move & 0x100000;
    public static int GetMoveDouble(int move) => move & 0x200000;
    public static int GetMoveEnpassant(int move) => move & 0x400000;
    public static int GetMoveCastling(int move) => move & 0x800000;

    public static string GetMove(int move)
    {
        return $"{Board.squareToCoordinates[GetMoveSource(move)]}{Board.squareToCoordinates[GetMoveTarget(move)]}{Board.promotedPieces[GetMovePromoted(move)]}";
    }

    public static void PrintMove(int move)
    {
        Console.WriteLine($"{Board.squareToCoordinates[GetMoveSource(move)]}{Board.squareToCoordinates[GetMoveTarget(move)]}{Board.promotedPieces[GetMovePromoted(move)]}");
    }
}

public struct MoveList
{
    public int[] moves;
    public int count;

    public MoveList()
    {
        moves = new int[256];
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
