using App.Interface.State;

namespace Game.Screen;

class HomeScreen
{
	public static void PrintHoemScreen()
	{
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

		var key = Console.ReadKey();

		var state = HomeScreenState.Choosegamemode;

		while (state is HomeScreenState.Choosegamemode)
		{
			state = key.Key switch
			{
				ConsoleKey.D1 => HomeScreenState.Choosegamemode,
				ConsoleKey.D2 => HomeScreenState.Setmenu,
				ConsoleKey.D3 => HomeScreenState.Exitgame,
				_ => HomeScreenState.Choosegamemode,
			};

			Action action = state switch
			{
				HomeScreenState.Choosegamemode => GameStart.ChooseGameMode,
				HomeScreenState.Setmenu => GameSetMenu.SetMenu,
				HomeScreenState.Exitgame => GameExitGame.ExitGame,
				_ => () => { }
			};
			action();
		}
	}
}
