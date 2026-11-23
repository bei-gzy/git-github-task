using App.GameCore;
using App.GameCore.Models;

namespace App.InputAndOutput;

public enum KeyLayout { Wasd, Arrows }

public sealed class LocalInput(string id, KeyLayout layout, CancellationTokenSource cts) : IInputSource
{
	// 每 tick 由 Round / ClientRunner 调 Drain() 统一读一次，多个 LocalInput 共享这批按键。
	// 否则先执行的 Capture 会把 Console 缓冲读光，后一个玩家永远收不到键。
	static readonly List<ConsoleKey> SharedKeys = [];

	public static void Clear()
	{
		SharedKeys.Clear();
		while (Console.KeyAvailable)
			SharedKeys.Add(Console.ReadKey(intercept: true).Key);
	}

	public IReadOnlyList<PlayerInput> Capture()
	{
		foreach (var key in SharedKeys)
		{
			if (key is ConsoleKey.Escape)
			{
				cts.Cancel();
				return [];//仍然返回空是因为和ToIntent做统一
			}
		}

		var intent = Route(SharedKeys, layout);
		return intent is null ? [] : [new PlayerInput(intent, id)];
	}

	// 把同一批按键按 layout 路由成意图（纯函数，便于单测：双人两人各取各的键）
	public static PlayerIntent? Route(IReadOnlyList<ConsoleKey> keys, KeyLayout layout)
	{
		PlayerIntent? last = null;
		foreach (var key in keys)
		{
			var intent = ToIntent(key, layout);
			if (intent is not null) last = intent;
		}

		return last;
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
