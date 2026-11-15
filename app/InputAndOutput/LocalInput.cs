using App.GameCore;
using App.GameCore.Models;

namespace App.InputAndOutput;

public sealed class LocalInput(string id, CancellationTokenSource cts) : IInputSource
{
	public PlayerInput? Capture()
	{
		PlayerIntent? last = null;
		while (Console.KeyAvailable)
		{
			var key = Console.ReadKey(intercept: true).Key;

			if (key is ConsoleKey.Escape)
			{
				cts.Cancel();
				return null;//仍然返回空是因为和ToIntent做统一
			}

			last = ToIntent(key);
		}

		return last is null ? null : new PlayerInput(last, id);
	}
	static PlayerIntent? ToIntent(ConsoleKey key) => key switch
	{
		ConsoleKey.W or ConsoleKey.UpArrow => new PlayerIntent.Turn(Direction.Up),
		ConsoleKey.S or ConsoleKey.DownArrow => new PlayerIntent.Turn(Direction.Down),
		ConsoleKey.A or ConsoleKey.LeftArrow => new PlayerIntent.Turn(Direction.Left),
		ConsoleKey.D or ConsoleKey.RightArrow => new PlayerIntent.Turn(Direction.Right),
		ConsoleKey.E or ConsoleKey.NumPad0 => new PlayerIntent.Dash(),
		_ => null
	};
}
