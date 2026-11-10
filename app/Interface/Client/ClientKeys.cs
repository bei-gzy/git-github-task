using App.Services;
using App.Protocol;

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
				ConsoleKey.W or ConsoleKey.UpArrow => (Direction.Up, false),
				ConsoleKey.S or ConsoleKey.DownArrow => (Direction.Down, false),
				ConsoleKey.A or ConsoleKey.LeftArrow => (Direction.Left, false),
				ConsoleKey.D or ConsoleKey.RightArrow => (Direction.Right, false),
				ConsoleKey.E or ConsoleKey.NumPad0 => (Direction.Unspecified, true),
				_ => (Direction.Unspecified, false)
			};

			if (!dash && dir is Direction.Unspecified) continue;
			last = new PlayerIntent(ToState(dir), dash);
		}

		return last;
	}

	static State.Direction ToState(Direction d) => d switch
	{
		Direction.Up => State.Direction.Up,
		Direction.Down => State.Direction.Down,
		Direction.Left => State.Direction.Left,
		Direction.Right => State.Direction.Right,
		_ => State.Direction.Up
	};
}
