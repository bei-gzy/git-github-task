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
			"3. 返回主菜单"
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

			Func<GamePrepareArgs, Task>? runRound = WaitKey().Key switch
			{
				ConsoleKey.D1 => async gameArgs => { _ = await FuncSystem.GameRunSystemAsync(gameArgs); }
				,
				ConsoleKey.D2 => async gameArgs => { _ = await LocalDoubleSystem.GameRunSystemAsync(gameArgs); }
				,
				ConsoleKey.D3 => null,
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
					await run(args);
				}
				finally
				{
					await args.EndRound();
				}
			}
		}
	}
}
