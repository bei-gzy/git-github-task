using App.GameTime;
using App.GameStates;

namespace App.Game.Screen;

class GameSetMenu : HomeScreen
{
	public static Task<GameState.HomeScreenState> SetMenu(GamePrepareArgs args)
	{
		while (true)
		{
			PrintOptions(args);
			Console.WriteLine();
			Console.Write("请选择：");

			Action? action = WaitKey().Key switch
			{
				ConsoleKey.D1 => () => ChangeMapSize(args),
				ConsoleKey.D2 => () => ChangeSpeed(args),
				ConsoleKey.D3 => () => ChangeDuration(args),
				ConsoleKey.D4 => () => Rename(args),
				_ => null
			};

			if (action is not null)
				action();
			break;
		}

		return Task.FromResult(GameState.HomeScreenState.Choosegamemode);
	}

	static void PrintOptions(GamePrepareArgs args)
	{
		Console.Clear();
		var world = args.GameWorld;

		string[] choice =
		[
			"—————设置菜单—————",
			$"1、设置地图大小(当前 {world.Width}, {world.Height})",
			$"2、设置初始速度(当前 {world.TickMilliseconds} 毫秒/格）",
			$"3、设置游戏时间(当前 {world.GameDurationSeconds} 秒）",
			$"4、修改名字(当前 {args.Player.Name})",
			"5、退出设置菜单"
		];

		foreach (string e in choice)
		{
			Console.WriteLine(e);
		}
	}

	static void ChangeMapSize(GamePrepareArgs args)
	{
		Console.Clear();
		Console.WriteLine("1、小地图 20×25 2、中地图 25×30 3、大地图 35×40");
		Console.Write("请选择：");

		var (width, height) = WaitKey().Key switch
		{
			ConsoleKey.D1 => (20, 25),
			ConsoleKey.D2 => (25, 30),
			ConsoleKey.D3 => (35, 40),
			_ => (args.GameWorld.Width, args.GameWorld.Height)
		};

		args.GameWorld.Resize(width, height);
		Done();
	}

	static void ChangeSpeed(GamePrepareArgs args)
	{
		Console.Clear();
		Console.WriteLine("1、慢 300ms 2、正常 200ms 3、快 150ms 4、极快 100ms");
		Console.Write("请选择：");

		int speed = WaitKey().Key switch
		{
			ConsoleKey.D1 => 300,
			ConsoleKey.D2 => 200,
			ConsoleKey.D3 => 150,
			ConsoleKey.D4 => 100,
			_ => args.GameWorld.TickMilliseconds
		};

		args.GameWorld.TickMilliseconds = speed;
		Done();
	}

	static void ChangeDuration(GamePrepareArgs args)
	{
		Console.Clear();
		SetTime.PrintOptions();
		Console.Write("请选择：");

		if (!SetTime.Apply(args.GameWorld, WaitKey().Key))
			Console.WriteLine("未做修改");

		Done();
	}

	static void Rename(GamePrepareArgs args)
	{
		Console.Clear();
		Console.Write("请输入新名字：");
		string? name = Console.ReadLine();

		if (!string.IsNullOrWhiteSpace(name))
			args.Player.SetName(name.Trim());

		Done();
	}

	static void Done()
	{
		Console.WriteLine("已保存，按任意键返回设置菜单…");
		_ = WaitKey();
	}
}
