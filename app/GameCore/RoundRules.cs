namespace App.GameCore;

public readonly record struct PlayerResult(
	string Id,
	bool IsAlive,
	int Score,
	int Length,
	double SurvivalTime
);

public readonly record struct RoundResults(
	bool IsOver,
	IReadOnlyList<PlayerResult> PlayersResult,
	PlayerResult? Winner,
	double UseTime
);

public static class RoundRules
{
	public static bool IsGameOver(WorldState state)
	{
		if (state.OverTime) return true;
		int alive = state.Players.Count(p => p.Snake.IsAlive);
		return state.Players.Count == 1 ? alive == 0 : alive <= 1;
	}

	public static RoundResults Results(WorldState s, ulong tickMs)
	{
		var results = s.Players
			.Select(p => new PlayerResult(p.Id, p.Snake.IsAlive, p.Score, p.Snake.Body.Count, p.SurvivalTime))
			.ToList();
		return new RoundResults(s.OverTime, results, Winner(results), s.Tick * tickMs / 1000.0);
	}

	public static PlayerResult? Winner(IReadOnlyList<PlayerResult> playersResult)
	{
		if (playersResult.Count is 0) return null;
		var msg = playersResult//Linq倒叙排序
			.OrderByDescending(p => p.IsAlive)
			.ThenByDescending(p => p.SurvivalTime)
			.ThenByDescending(p => p.Score)
			.ThenByDescending(p => p.Length)
			.ToList();
		var best = msg[0];
		return playersResult.Count > 1 && Same(msg) ? null : best;
	}

	//所有元素除了id进行比较
	static bool Same(List<PlayerResult> playersResult) => playersResult.Count <= 1 ||
		playersResult.Skip(1).All(player =>
			player.IsAlive == playersResult[0].IsAlive &&
			player.Score == playersResult[0].Score &&
			player.Length == playersResult[0].Length &&
			player.SurvivalTime == playersResult[0].SurvivalTime);
}
