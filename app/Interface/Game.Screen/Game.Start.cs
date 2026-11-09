using App.GameStates;
using App.Services;

namespace App.Game.Screen;

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
				ConsoleKey.D2 => async () => _ = await LocalDoubleSystem.GameRunSystemAsync(args)
				,
				ConsoleKey.D3 => async () =>
				{
					if (args.Session is not null)
					{
						await Round.RunAsync(args, new RoundContext(
							"————— 本机双人对战 —————",
							"玩家1: W A S D / E 加速   玩家2: ↑ ↓ ← → / 0 加速   Esc 退出",
							args.GuestInput is null
								? [args.LocalInput]
								: [args.LocalInput, args.GuestInput],//空判断处理，或者自己补一个empty()
							new DoubleRules(),
							args.Renderer
							));
					}
					else
					{
						Console.WriteLine("并没有联机");
						return;
					}
				}
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
