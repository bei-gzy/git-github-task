using App.Config;
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

			switch (WaitKey().Key)
			{
				case ConsoleKey.D1:
					args.BeginRound();
					_ = await FuncSystem.GameRunSystemAsync(args);
					args.EndRound();
					break;

				case ConsoleKey.D2:
					args.BeginRound();
					_ = await LocalDoubleSystem.GameRunSystemAsync(args);
					args.EndRound();
					break;

				case ConsoleKey.D3:
					return hstate = GameState.HomeScreenState.Choosegamemode;

				default:
					break;
			}
		}
	}
}
