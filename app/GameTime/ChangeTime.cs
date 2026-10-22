using App.Game.Screen;

namespace App.GameTime;

public class SetTime
{
	internal static (ConsoleKeyInfo, double) GameTimeOptions;
	public static void ChangeTime(GamePrepareArgs args)
	{
		PrintOptions();
		if (ReadKey().Key is ConsoleKey.D1 && ReadKey().Key is ConsoleKey.D2)
		{
			GameTimeOptions.Item1 = ReadKey();
			GameTimeOptions.Item2 = GameTimeOptions.Item1.Key switch
			{
				ConsoleKey.D1 => 60.0,
				ConsoleKey.D2 => 180.0,
				ConsoleKey.D3 => 300.0,
				_ => 60.0
			};

			args.Time.SetTime(GameTimeOptions.Item2);
		}

		Console.WriteLine("设置失败！");
		return;
	}

	public static ConsoleKeyInfo ReadKey() => Console.KeyAvailable ? Console.ReadKey() : default;

	static void PrintOptions()
	{
		string[] choice =
		[
			"请设置游戏时间",
			"1、一分钟",
			"2、三分钟",
			"3、五分钟"
		];

		foreach (string e in choice)
		{
			Console.WriteLine(e);
		}
	}
}
