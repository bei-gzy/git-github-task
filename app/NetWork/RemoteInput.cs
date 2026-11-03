//只发意图

using App.Protocol;
using App.KeyBoard;

namespace App.NetWork;//局域网的串通

public static class RemoteInput
{
	public static void ReadRemoteInput(Button button, PlayerInput input)
	{
		if (input is null) return;
		foreach (var key in ToKey(input))
		{
			button.TouchClick(new ConsoleKeyInfo('\0', key, false, false, false));
		}
	}

	static IEnumerable<ConsoleKey> ToKey(PlayerInput input)
	{
		ConsoleKey? tkey = input.Direction switch
		{
			Direction.Left => ConsoleKey.LeftArrow,
			Direction.Right => ConsoleKey.RightArrow,
			Direction.Up => ConsoleKey.UpArrow,
			Direction.Down => ConsoleKey.DownArrow,
			_ => null
		};
		if (tkey is not null) yield return tkey.Value;

		if (input.UseDash) yield return ConsoleKey.NumPad0;
	}
}
