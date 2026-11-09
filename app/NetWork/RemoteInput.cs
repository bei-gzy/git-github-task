//只发意图
//远端和本地的按键为什么要不一致，网络层返回的Direction应该由player管理

using App.Protocol;
using App.KeyBoard;

namespace App.NetWork;//局域网的串通

public static class RemoteInput
{
	static readonly Dictionary<string, ulong> LastTick = [];

	public static void ReadRemoteInput(Button button, string roomId, PlayerInput input)
	{
		if (input.RoomId != roomId) return;

		//去重：只应用“更新的”包；第一次收到时字典里没有这个玩家
		if (LastTick.TryGetValue(input.PlayerId, out ulong last) && input.ClientTick <= last)
			return;
		LastTick[input.PlayerId] = input.ClientTick;

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
