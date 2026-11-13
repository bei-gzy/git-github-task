namespace App.Interface;

class GameExitGame : HomeScreen
{
	public static Task<GameState.HomeScreenState> ExitGame(GameState gameRunningState)
	{
		DoExitGame(gameRunningState);

		return Task.FromResult(hstate);
	}

	protected static void DoExitGame(GameState gameRunningState)
	{
		Console.Clear();

		while (true)
		{
			Console.WriteLine("确定要退出游戏吗？ (是/否)");
			string? input = Console.ReadLine();

			if (input is "是")
			{
				Console.WriteLine("感谢游玩！");
				gameRunningState.GameRunningState = GameState.GameOnAndOffState.GameOff;
				hstate = GameState.HomeScreenState.Exitgame;
				return;
			}

			if (input is "否")
			{
				hstate = GameState.HomeScreenState.Choosegamemode;
				return;
			}

			Console.Clear();
			Console.WriteLine("无效输入，请输入是或否");
		}
	}
}
