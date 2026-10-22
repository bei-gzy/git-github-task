using App.Interface.State;

namespace App.Game.Screen;

class HomeScreen
{
	protected static GameState.HomeScreenState hstate = GameState.HomeScreenState.Choosegamemode;

	protected static GameState.InterfaceState istate = GameState.InterfaceState.ChoosingMode;

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

	public static ConsoleKeyInfo ReadKey() => Console.KeyAvailable ? Console.ReadKey() : default;

	public static async Task ScreenFuncSystem(GameState gameRunningState)
	{

		while (hstate is GameState.HomeScreenState.Choosegamemode)
		{
			Console.Clear();
			PrintHomeScreen();
			var tkey = ReadKey().Key;
			_ = tkey switch
			{
				ConsoleKey.D1 => await GameStart.ChooseGameMode(),
				ConsoleKey.D2 => await GameSetMenu.SetMenu(),
				ConsoleKey.D3 => await GameExitGame.ExitGame(gameRunningState),
				_ => hstate
			};
		}
	}
}

