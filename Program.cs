
using static MoveFlag;
using static MoveGenerator;

class Program
{
    static void Main()
    {
        PieceAttacks.InitAll();

        // Debug mode variable
        bool debug = false;

        if (debug)
        {
            // Parse FEN and print board
            Board.ParseFEN(TrickyPosition);
            Board.PrintBoard();

            // Placeholder for evaluation when you implement it
            Console.WriteLine($"score: {Evaluation.Evaluate()}");

            Search.SearchPosition(3);
        }
        else
        {
            // Connect to the GUI
            Uci.UciLoop();
        }
    }

    static long nodes;
    public static int GetTimeMs() => Environment.TickCount;

    public const string EmptyBoard = "8/8/8/8/8/8/8/8 w - - ";
    public const string StartPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    public const string TrickyPosition = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1";
    public const string PinPosition = "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1";
    public const string Position5 = "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8";

    static void PerftDriver(int depth)
    {
        if (depth == 0)
        {
            nodes++;
            return;
        }

        MoveList moveList = new MoveList();
        GenerateMoves(ref moveList);

        for (int i = 0; i < moveList.count; i++)
        {
            BoardState state = Board.CopyBoard();

            if (MakeMove(moveList.moves[i], (int)allMoves) == 0)
            {
                continue;
            }

            PerftDriver(depth - 1);

            Board.TakeBack(state);
        }
    }

    static void PerftTest(int depth)
    {
        Console.WriteLine("\n     Performance test\n");

        MoveList moveList = new MoveList();
        GenerateMoves(ref moveList);

        int start = GetTimeMs();

        for (int i = 0; i < moveList.count; i++)
        {
            BoardState state = Board.CopyBoard();

            if (MakeMove(moveList.moves[i], (int)allMoves) == 0)
                continue;

            long cumulative = nodes;

            PerftDriver(depth - 1);

            long oldNodes = nodes - cumulative;

            Board.TakeBack(state);

            Console.WriteLine($"     move: {MoveEncoding.GetMove(moveList.moves[i])}  nodes: {oldNodes}");
        }

        Console.WriteLine($"\n    Depth: {depth}");
        Console.WriteLine($"    Nodes: {nodes}");
        Console.WriteLine($"     Time: {GetTimeMs() - start}ms\n");
    }
}
