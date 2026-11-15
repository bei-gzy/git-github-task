namespace App.GameCore;

public readonly record struct PlayerResult(
	string Id,
	bool IsAlive,
	int Score,
	int Length,
	double SurviavalTime
);

public readonly record struct RoundResults(
	bool IsOver,
	IReadOnlyList<PlayerResult> PlayersResult,
	PlayerResult? Winner,
	double UseTime
);

public static class RoundRules
{
	public static bool IsGameOver(WorldState state) => state.OverTime || state.Players.Count(p => p.Snake.IsAlive) <= 1;

	public static RoundResults Results(WorldState s)
	{
		var results = s.Players
			.Select(p => new PlayerResult(p.Id, p.Snake.IsAlive, p.Score, p.Snake.Length, p.SurvivalTime))
			.ToList();
		return new RoundResults(s.OverTime, results, Winner(results), s.RemainingSeconds);
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
