using static Board;
using static MoveEncoding;
using static MoveGenerator;

public static class Uci
{
    // Parse a move string (e.g., "e2e4", "a7a8q") and return the encoded move, or 0 if illegal/not found.
    public static int ParseMove(string moveString)
    {
        if (string.IsNullOrEmpty(moveString) || moveString.Length < 4)
            return 0;

        // Generate all legal moves for the current position
        var moveList = new MoveList();
        GenerateMoves(ref moveList);

        // Parse source square
        int sourceSquare = (moveString[0] - 'a') + (8 - (moveString[1] - '0')) * 8;

        // Parse target square
        int targetSquare = (moveString[2] - 'a') + (8 - (moveString[3] - '0')) * 8;

        // Loop over the generated moves
        for (int i = 0; i < moveList.count; i++)
        {
            int move = moveList.moves[i];

            // Match source and target squares
            if (sourceSquare == GetMoveSource(move) && targetSquare == GetMoveTarget(move))
            {
                int promotedPiece = GetMovePromoted(move);

                // If there is a promotion, verify it matches the move string
                if (promotedPiece != 0)
                {
                    if (moveString.Length < 5)
                        continue; // promotion character missing

                    char promoChar = moveString[4];
                    switch (promoChar)
                    {
                        case 'q' when (promotedPiece == Q || promotedPiece == q):
                            return move;
                        case 'r' when (promotedPiece == R || promotedPiece == r):
                            return move;
                        case 'b' when (promotedPiece == B || promotedPiece == b):
                            return move;
                        case 'n' when (promotedPiece == N || promotedPiece == n):
                            return move;
                        default:
                            continue; // mismatched promotion character
                    }
                }

                // Non-promotion move matches
                return move;
            }
        }

        // No matching legal move found
        return 0;
    }

    // Parse UCI "position" command (e.g., "position startpos moves e2e4 e7e5")
    public static void ParsePosition(string command)
    {
        if (string.IsNullOrEmpty(command))
            return;

        // Trim leading whitespace and skip "position"
        var parts = command.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0 || parts[0] != "position")
            return;

        int idx = 1;

        // Handle "startpos"
        if (idx < parts.Length && parts[idx] == "startpos")
        {
            Board.ParseFEN(Program.StartPosition);
            idx++;
        }
        // Handle "fen ..."
        else if (idx < parts.Length && parts[idx] == "fen")
        {
            idx++;
            if (idx < parts.Length)
            {
                // Reconstruct FEN from remaining tokens until "moves" or end
                var fenTokens = new List<string>();
                while (idx < parts.Length && parts[idx] != "moves")
                {
                    fenTokens.Add(parts[idx]);
                    idx++;
                }
                if (fenTokens.Count > 0)
                {
                    var fen = string.Join(" ", fenTokens);
                    Board.ParseFEN(fen);
                }
                else
                {
                    // FEN missing; fall back to start position
                    Board.ParseFEN(Program.StartPosition);
                }
            }
            else
            {
                // FEN missing; fall back to start position
                Board.ParseFEN(Program.StartPosition);
            }
        }
        else
        {
            // Unknown or missing position specifier; default to start position
            Board.ParseFEN(Program.StartPosition);
        }

        // Parse "moves" section if present
        if (idx < parts.Length && parts[idx] == "moves")
        {
            idx++;
            while (idx < parts.Length)
            {
                string moveStr = parts[idx];
                int move = ParseMove(moveStr);
                if (move == 0)
                    break; // illegal or unparsable move
                MakeMove(move, (int)MoveFlag.allMoves);
                idx++;
            }
        }
    }

    // Parse UCI "go" command (e.g., "go depth 5")
    public static void ParseGo(string command)
    {
        if (string.IsNullOrEmpty(command))
            return;

        int depth = -1;
        var parts = command.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i] == "go")
                continue;

            if (parts[i] == "depth" && i + 1 < parts.Length)
            {
                if (int.TryParse(parts[i + 1], out int parsedDepth))
                    depth = parsedDepth;
                break;
            }
        }

        // Default depth if no "depth" argument found
        if (depth == -1)
            depth = 5;

        // Placeholder for actual search invocation
        Search.SearchPosition(depth);
    }

    // Main UCI loop
    public static void UciLoop()
    {
        // Print engine info
        Console.WriteLine("id name BBC");
        Console.WriteLine("id name Amrou");
        Console.WriteLine("uciok");

        while (true)
        {
            // Read a line from stdin
            string? input = Console.ReadLine();
            if (string.IsNullOrEmpty(input))
                continue;

            // Trim whitespace
            input = input.Trim();

            // Parse UCI commands
            if (input.StartsWith("isready"))
            {
                Console.WriteLine("readyok");
            }
            else if (input.StartsWith("position"))
            {
                ParsePosition(input);
            }
            else if (input.StartsWith("ucinewgame"))
            {
                ParsePosition("position startpos");
            }
            else if (input.StartsWith("go"))
            {
                ParseGo(input);
            }
            else if (input.StartsWith("quit"))
            {
                break;
            }
            else if (input.StartsWith("uci"))
            {
                Console.WriteLine("id name BBC");
                Console.WriteLine("id name Amrou");
                Console.WriteLine("uciok");
            }
        }
    }
}
