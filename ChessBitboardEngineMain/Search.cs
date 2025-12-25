using System.Drawing;
using static Board;
using static MoveEncoding;
using static MoveGenerator;

public static class Search
{
    // half move counter
    private static int ply;

    // best move
    private static int best_move;

    // nodes counter
    private static long nodes;

    // negamax alpha beta search
    private static int Negamax(int alpha, int beta, int depth)
    {
        // recursion escape condition
        if (depth == 0)
            // return evaluation
            return Evaluation.Evaluate();

        // increment nodes count
        nodes++;

        // best move so far
        int best_sofar = 0;

        // old value of alpha
        int old_alpha = alpha;

        // create move list instance
        MoveList moveList = new MoveList();

        // generate moves
        GenerateMoves(ref moveList);

        // loop over moves within a movelist
        for (int count = 0; count < moveList.count; count++)
        {
            // preserve board state
            BoardState state = CopyBoard();

            // make sure to make only legal moves
            if (MakeMove(moveList.moves[count], (int)MoveFlag.allMoves) == 0)
            {
                // skip to next move
                continue;
            }

            // increment ply
            ply++;

            // score current move
            int score = -Negamax(-beta, -alpha, depth - 1);

            // decrement ply
            ply--;

            // take move back
            Board.TakeBack(state);

            // fail-hard beta cutoff
            if (score >= beta)
            {
                // node (move) fails high
                return beta;
            }

            // found a better move
            if (score > alpha)
            {
                // PV node (move)
                alpha = score;

                // if root move
                if (ply == 0)
                    // associate best move with the best score
                    best_sofar = moveList.moves[count];
            }
        }

        // found better move
        if (old_alpha != alpha)
            // init best move
            best_move = best_sofar;

        // node (move) fails low
        return alpha;
    }

    // Main search routine using negamax with alpha-beta pruning
    public static void SearchPosition(int depth)
    {
        // reset nodes counter
        nodes = 0;

        // find best move within a given position
        int score = Negamax(-50000, 50000, depth);

        // best move placeholder
        Console.WriteLine($"bestmove {GetMove(best_move)}");
    }
}