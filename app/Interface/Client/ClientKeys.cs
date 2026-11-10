using App.Services;

namespace App.Interface;

public static class ClientReadKey
{
	public static PlayerIntent? Poll()
	{
		PlayerIntent? last = null;
		while (Console.KeyAvailable)
		{
			var key = Console.ReadKey(intercept: true).Key;
			if (key is ConsoleKey.Escape) return null;

			var (dir, dash) = key switch
			{
				ConsoleKey.W or ConsoleKey.UpArrow => (State.Direction.Up, false),
				ConsoleKey.S or ConsoleKey.DownArrow => (State.Direction.Down, false),
				ConsoleKey.A or ConsoleKey.LeftArrow => (State.Direction.Left, false),
				ConsoleKey.D or ConsoleKey.RightArrow => (State.Direction.Right, false),
				ConsoleKey.E or ConsoleKey.NumPad0 => ((State.Direction?)null, true),
				_ => ((State.Direction?)null, false)
			};

			if (!dash && dir is null) continue;
			last = new PlayerIntent(dir, dash);
		}

		return last;
	}
}
