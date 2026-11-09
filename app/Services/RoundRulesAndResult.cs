using App.World;

namespace App.Services;

public interface IRoundRules
{
	bool IsGameOver(GameWorld world, IReadOnlyList<Player> players);
	RoundResult Result(GameWorld world, IReadOnlyList<Player> players);
}

sealed class SingleRules : IRoundRules
{
	public bool IsGameOver(GameWorld world, IReadOnlyList<Player> players) => world.IsOver || !players[0].IsAlive;
	public RoundResult Result(GameWorld world, IReadOnlyList<Player> players)
	{
		var p = players[0];
		return new RoundResult(world.IsOver || !p.IsAlive, [
			p.IsAlive ? "时间到！" : "Gaming Over!",
			"得分: " + p.MScore.MyScore,
			"长度: " + p.MSnake.Snake.Count,
			$"存活: {world.ElapsedTime.TotalSeconds:F1} 秒",
		]);
	}
}

sealed class DoubleRules : IRoundRules
{
	public bool IsGameOver(GameWorld world, IReadOnlyList<Player> players) => world.IsOver || players.Any(x => !x.IsAlive);
	public RoundResult Result(GameWorld world, IReadOnlyList<Player> players)
	{
		var p1 = players[0];
		var p2 = players[1];
		var winner = Winner(p1, p2);
		bool timeUp = world.ElapsedTime.TotalSeconds >= world.GameDurationSeconds;
		return new RoundResult(world.IsOver || players.Any(p => !p.IsAlive), [
			"————— 对局结束 —————",
			timeUp ? "时间到" : winner is null ? "平局" : $"{winner.Name} 获胜",
			$"{p1.Name}: {p1.MScore.MyScore} 分 长度 {p1.MSnake.Snake.Count} {(p1.IsAlive ? "存活" : "死亡")}",
			$"{p2.Name}: {p2.MScore.MyScore} 分 长度 {p2.MSnake.Snake.Count} {(p2.IsAlive ? "存活" : "死亡")}",
			$"用时 {world.ElapsedTime.TotalSeconds:F1} 秒"
		]);
	}

	public static Player? Winner(Player p1, Player p2) => p1.IsAlive != p2.IsAlive
			? p1.IsAlive ? p1 : p2
			: p1.MSnake.Snake.Count != p2.MSnake.Snake.Count
			? p1.MSnake.Snake.Count > p2.MSnake.Snake.Count ? p1 : p2
			: p1.MScore.MyScore != p2.MScore.MyScore ? p1.MScore.MyScore > p2.MScore.MyScore ? p1 : p2 : null;
}

