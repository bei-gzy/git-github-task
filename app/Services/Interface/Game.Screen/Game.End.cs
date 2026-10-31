using App.World;

namespace App.Game.Screen;

class MultiRules
{
	// internal Player p1 = PlayerRegisterInterface.PlayerLists[0];

	// internal Player p2 = PlayerRegisterInterface.PlayerLists[1];

	// public async Task Winner()
	// {
	// 	var a = await await Task.WhenAny(FromScores(), FromTimes(), FromLength());
	// 	Console.WriteLine("第一名是：" + a.Name);
	// 	Console.WriteLine("得分：" + a.MScore.MyScore);
	// 	Console.WriteLine("存活时长：" + a.MSnake.Snake.Count);
	// }
	// Task<Player> FromLength()
	// {
	// 	if (p1.MSnake.Snake.Count == p2.MSnake.Snake.Count)
	// 	{
	// 		p1.Rename(p1.Name + p2.Name);
	// 		return Task.FromResult(p1);
	// 	}

	// 	return (p1.MSnake.Snake.Count < p2.MSnake.Snake.Count) ? Task.FromResult(p1) : Task.FromResult(p2);
	// }

	// Task<Player> FromTimes() => (p1.SurvivalTime < p2.SurvivalTime) ? Task.FromResult(p1) : Task.FromResult(p2);

	// Task<Player> FromScores()
	// {
	// 	if (p1.MScore.MyScore == p2.MScore.MyScore)
	// 	{
	// 		p1.Rename(p1.Name + p2.Name);
	// 		return Task.FromResult(p1);
	// 	}

	// 	return (p1.MScore.MyScore < p2.MScore.MyScore) ? Task.FromResult(p2) : Task.FromResult(p1);
	// }

	public static Player? Winner(Player p1, Player p2) => p1.IsAlive != p2.IsAlive
			? p1.IsAlive ? p1 : p2
			: p1.MSnake.Snake.Count != p2.MSnake.Snake.Count
			? p1.MSnake.Snake.Count > p2.MSnake.Snake.Count ? p1 : p2
			: p1.MScore.MyScore != p2.MScore.MyScore ? p1.MScore.MyScore > p2.MScore.MyScore ? p1 : p2 : null;
}
