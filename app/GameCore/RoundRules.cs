namespace App.GameCore;

public readonly record struct PlayerResult(
	string Id,
	bool IsAlive,
	int Score,
	int Length,
	double SurviavalTime
);

public readonly record struct RoundResults(
	bool OutTime,
	IReadOnlyList<PlayerResult> PlayersResult,
	PlayerResult? Winner
);

public static class RoundRules
{
	public static bool IsGameOver(WorldState state) => state.IsOver || state.Players.Count(p => p.Snake.IsAlive) <= 1;

	public static RoundResults Results(WorldState s)
	{
		bool timeOut = s.Tick >= s.MaxTick;
		var results = s.Players
			.Select(p => new PlayerResult(p.Id, p.Snake.IsAlive, p.Score, p.Snake.Length, p.SurvivalTime))
			.ToList();
		return new RoundResults(timeOut, results, Winner(results));
	}

	public static PlayerResult? Winner(IReadOnlyList<PlayerResult> playersResult)
	{
		if (playersResult.Count is 0) return null;
		var msg = playersResult//Linq倒叙排序
			.OrderByDescending(p => p.IsAlive)
			.ThenByDescending(p => p.SurviavalTime)
			.ThenByDescending(p => p.Score)
			.ThenByDescending(p => p.Length)
			.ToList();
		var best = msg[0];
		return playersResult.Count > 1 && Same(playersResult) ? null : best;
	}

	//所有元素除了id进行比较
	static bool Same(IReadOnlyList<PlayerResult> playersResult) => playersResult.Count <= 1 ||
		playersResult.Skip(1).All(player =>
			player.IsAlive == playersResult[0].IsAlive &&
			player.Score == playersResult[0].Score &&
			player.Length == playersResult[0].Length &&
			player.SurviavalTime == playersResult[0].SurviavalTime);
}

// 		var p = players[0];
// 		return new RoundResult(world.IsOver || !p.IsAlive, [
// 			p.IsAlive ? "时间到！" : "Gaming Over!",
// 			"得分: " + p.MScore.MyScore,
// 			"长度: " + p.MSnake.Bodies.Count,
// 			$"存活: {world.ElapsedTime.TotalSeconds:F1} 秒",

// 		if (players.Count < 2)
// 		{
// 			return new RoundResult(true, [
// 				"————— 对局结束 —————",
// 				"等待玩家2加入…",
// 				$"用时 {world.ElapsedTime.TotalSeconds:F1} 秒"
// 			]);
// 		}

// 		var p1 = players[0];
// 		var p2 = players[1];
// 		var winner = Winner(p1, p2);
// 		bool timeUp = world.ElapsedTime.TotalSeconds >= world.GameDurationSeconds;
// 		return new RoundResult(world.IsOver || players.Any(p => !p.IsAlive), [
// 			"————— 对局结束 —————",
// 			timeUp ? "时间到" : winner is null ? "平局" : $"{winner.Name} 获胜",
// 			$"{p1.Name}: {p1.MScore.MyScore} 分 长度 {p1.MSnake.Bodies.Count} {(p1.IsAlive ? "存活" : "死亡")}",
// 			$"{p2.Name}: {p2.MScore.MyScore} 分 长度 {p2.MSnake.Bodies.Count} {(p2.IsAlive ? "存活" : "死亡")}",
// 			$"用时 {world.ElapsedTime.TotalSeconds:F1} 秒"

