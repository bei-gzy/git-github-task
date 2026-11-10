using App.Config;
using App.NetWork;
using App.Services;

namespace App.Interface;

class HomeScreen
{
	protected static GameState.HomeScreenState hstate = GameState.HomeScreenState.Choosegamemode;

	static void PrintHomeScreen()
	{
		Console.Clear();

		string[] _ =
		[
			"Snake Game!欢迎游玩贪吃蛇小游戏",
			"1. 开始游戏",
			"2. 设置菜单",
			"3. 退出游戏"
		];

		foreach (string line in _)
		{
			Console.WriteLine(line);
		}
	}

	public static ConsoleKeyInfo WaitKey() => Console.ReadKey(intercept: true);

	public static async Task ScreenFuncSystem(GameState gameRunningState, GameSettings gameSettings, NetSession? session, IRenderer renderer, IRoundResultView resultView)
	{
		var args = new GamePrepareArgs(gameSettings, session, renderer, resultView);
		hstate = GameState.HomeScreenState.Choosegamemode;

		while (hstate is GameState.HomeScreenState.Choosegamemode)
		{
			PrintHomeScreen();

			var tkey = WaitKey().Key;
			hstate = tkey switch
			{
				ConsoleKey.D1 => await GameStart.ChooseGameMode(args),
				ConsoleKey.D2 => await GameSetMenu.SetMenu(args),
				ConsoleKey.D3 => await GameExitGame.ExitGame(gameRunningState),
				_ => hstate
			};
		}
	}
}
