using App.Interface.State;
namespace Game.Screen;

class GameStart
{
	public static void ChooseGameMode()
	{
		string[] _ =
		[
			"请选择游戏模式：",
			"1. 单人模式",
			"2. 多人模式",
			"3. 返回主菜单"
		];
		foreach (string line in _)
		{
			Console.WriteLine(line);
		}

		var key = Console.ReadKey();

		var state = InterfaceState.SinglePlayerMode;

		while (state == InterfaceState.SinglePlayerMode)
		{
			Action action = key.Key switch
			{
				ConsoleKey.D1 => SinglePlayerMode,
				ConsoleKey.D2 => MultiPlayerMode,
				ConsoleKey.D3 => ReturnToMainMenu,
				_ => () => { }
			};
			action();
		}
	}

	public static void SinglePlayerMode()
	{
	}

	public static void MultiPlayerMode()
	{
	}

	public static void ReturnToMainMenu()
	{
	}
}
