using App.World;

namespace App.GameTime;

//游戏时长预设：设置菜单和以后可能的联机房间共用同一份
static class SetTime
{
	public static readonly (ConsoleKey Key, char Digit, double Seconds, string Label)[] Options =
	[
		(ConsoleKey.D1, '1', 60.0, "一分钟"),
		(ConsoleKey.D2, '2', 180.0, "三分钟"),
		(ConsoleKey.D3, '3', 300.0, "五分钟")
	];

	public static void PrintOptions()
	{
		Console.WriteLine("请设置游戏时间");

		foreach (var (_, digit, _, label) in Options)
			Console.WriteLine($"{digit}、{label}");
	}

	/// <summary>按键命中预设就写进世界，返回是否改了</summary>
	public static bool Apply(GameWorld world, ConsoleKey key)
	{
		foreach (var (optionKey, _, seconds, _) in Options)
		{
			if (optionKey != key) continue;

			world.GameDurationSeconds = seconds;
			return true;
		}

		return false;
	}
}