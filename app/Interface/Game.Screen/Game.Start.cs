using App.Interface.State;
using App.Services;

namespace App.Game.Screen;

class GameStart : HomeScreen
{
	static void PringtChoose()
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
	}

	public static async Task<GameState.HomeScreenState> ChooseGameMode(GamePrepareArgs args)
	{
		var key = ReadKey();

		while (istate is not GameState.InterfaceState.ExitMode)
		{
			Console.Clear();
			PringtChoose();
			_ = key.Key switch
			{
				ConsoleKey.D1 => await SinglePlayerMode(args),
				ConsoleKey.D2 => await MultiPlayerMode(),
				ConsoleKey.D3 => await ReturnToMainMenu(),
				_ => istate
			};
		}

		return hstate;
	}

	public static async Task<GameState.InterfaceState> SinglePlayerMode(GamePrepareArgs args) => await FuncSystem.GameRunSystemAsync(args.Time, args.Snake, args.KeyBoardController, GamePrepareArgs.Button, GamePrepareArgs.Direction, args.CollisionController, GamePrepareArgs.Collision, args.PlayerScore, args.GameMap, args.Score)
			? (istate = GameState.InterfaceState.SinglePlayerMode)
			: istate = GameState.InterfaceState.ExitMode;

	public static async Task<GameState.InterfaceState> MultiPlayerMode()
	{
		Console.Clear();
		await Task.Delay(0);
		return istate = GameState.InterfaceState.MultiPlayerMode;
	}

	public static async Task<GameState.InterfaceState> ReturnToMainMenu()
	{
		Console.Clear();
		await Task.Delay(100);
		return istate = GameState.InterfaceState.ExitMode;
	}
}
