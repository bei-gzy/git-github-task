using App.GameCore;
using App.GameCore.Models;

namespace App.InputAndOutput;

public enum KeyLayout { Wasd, Arrows }

public sealed class LocalInput(string id, KeyLayout layout, CancellationTokenSource cts) : IInputSource
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

			last = ToIntent(key, layout);
		}

		return last is null ? null : new PlayerInput(last, id);
	}
	static PlayerIntent? ToIntent(ConsoleKey key, KeyLayout layout) => (key, layout) switch
	{
		(ConsoleKey.W, KeyLayout.Wasd) or (ConsoleKey.UpArrow, KeyLayout.Arrows) => new PlayerIntent.Turn(Direction.Up),
		(ConsoleKey.S, KeyLayout.Wasd) or (ConsoleKey.DownArrow, KeyLayout.Arrows) => new PlayerIntent.Turn(Direction.Down),
		(ConsoleKey.A, KeyLayout.Wasd) or (ConsoleKey.LeftArrow, KeyLayout.Arrows) => new PlayerIntent.Turn(Direction.Left),
		(ConsoleKey.D, KeyLayout.Wasd) or (ConsoleKey.RightArrow, KeyLayout.Arrows) => new PlayerIntent.Turn(Direction.Right),
		(ConsoleKey.E, _) or (ConsoleKey.NumPad0, _) => new PlayerIntent.Dash(),
		_ => null
	};
}
