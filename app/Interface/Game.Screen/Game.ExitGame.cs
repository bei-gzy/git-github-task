using App.Interface.State;
namespace App.Game.Screen;

class GameExitGame : HomeScreen
{
	public static async Task<GameState.HomeScreenState> ExitGame(GameState gameRunningState)
	{
		await DoExitGame(gameRunningState);

		return hstate = GameState.HomeScreenState.Exitgame;
	}

	protected static Task DoExitGame(GameState gameRunningState)
	{
		while (true)
		{
			Console.WriteLine("确定要退出游戏吗？ (是/否)");
			string? input = Console.ReadLine();
			if (input is "是")
			{
				Console.WriteLine("感谢游玩！");
				Console.Clear();
				gameRunningState.GameRunningState = GameState.GameOnAndOffState.GameOff;
				break;
				// 这里可以添加退出游戏的逻辑，例如关闭窗口或退出应用程序
			}
			else if (input is "否")
			{
				hstate = GameState.HomeScreenState.Choosegamemode;
				break;
			}
			else
			{
				Console.WriteLine("无效输入，请输入是或否");
			}
		}

		return Task.CompletedTask;
	}
}
