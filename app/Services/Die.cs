namespace App.Services;

class DefeatSystem
{
	public static void Defeat(State state)
	{
		Console.Clear();
		Console.ResetColor();
		int score = state.GetScore();

		string[] defeatMessage =
		[
			"Gaming Over!",
			"得分: " + score,
			"游戏结束。"
		];

		foreach (string line in defeatMessage)
		{
			Console.WriteLine(line);
		}
	}
	//是否复活--待开发
}
