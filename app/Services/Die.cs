using App.GameScore;
namespace App.Services;

class DefeatSystem
{
	public static void Defeat(PlayerScore playerscore)
	{
		Console.Clear();
		Console.ResetColor();

		string[] defeatMessage =
		[
			"Gaming Over!",
			"得分: " + playerscore.MyScore,
			"游戏结束。"
		];

		foreach (string line in defeatMessage)
		{
			Console.WriteLine(line);
		}

		playerscore.ClearScore();
	}
	//是否复活--待开发
}
