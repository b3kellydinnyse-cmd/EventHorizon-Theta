namespace Security
{
    public static class DebugCommands
    {
        public static int GetHashCode(string data)
        {
            return System.Math.Abs(data.GetHashCode()) % 100000;
        }

        // Original code:
        // public static int Decode(string command, int hash)
        // {
        // 	return int.TryParse(command, out var result) ? result : -1;
        // }

        // Returns a string to preserve leading zeroes.
        // Now "1", "01", and "00000001" are treated as completely different codes.
        public static string Decode(string command, int hash)
        {
            // Check if it's a number, but return the exact string (with all leading zeroes)
            return int.TryParse(command, out _) ? command : string.Empty;
        }

        // Original code:
        // public static string Encode(int id, int hash)
        // {
        // 	return id.ToString();
        // }
        public static string Encode(string id, int hash)
        {
            return id;
        }
    }
}