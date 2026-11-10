using App.Services;

namespace App.Interface;

class GameStart : HomeScreen
{
	static void PrintChoose()
	{
		Console.Clear();
		string[] _ =
		[
			"请选择游戏模式：",
			"1. 单人模式",
			"2. 本机双人",
			"3. 网络多人",
			"4. 返回主菜单"
		];
		foreach (string line in _)
		{
			Console.WriteLine(line);
		}
	}

	public static async Task<GameState.HomeScreenState> ChooseGameMode(GamePrepareArgs args)
	{
		while (true)
		{
			PrintChoose();

			Func<Task>? runRound = WaitKey().Key switch//委托接受args，返回一个Task
			{
				ConsoleKey.D1 => async () =>
				{
					await Round.RunAsync(args, new RoundContext(
						"——————————————Welcome TO Snake Eat Game——————————————",
						"法则:WASD控制方向,E加速(冷却5秒),Esc退出游戏",
						[args.LocalInput],
						new SingleRules(),
						args.Renderer
						));
				}
				,
				ConsoleKey.D2 => async () => await LocalDoubleSystem.GameRunSystemAsync(args)
				,
				ConsoleKey.D3 => async () => await NetMuiltiSystem.RunAsync(args)
				,
				ConsoleKey.D4 => null,
				_ => null
			};

			if (runRound is null)
			{
				return hstate = GameState.HomeScreenState.Choosegamemode;
			}

			if (runRound is { } run)//表示当runRound非null时，将其赋值给run
			{
				args.BeginRound();
				try
				{
					await run();
				}
				finally
				{
					args.EndRound();
				}
			}
		}
	}
}
