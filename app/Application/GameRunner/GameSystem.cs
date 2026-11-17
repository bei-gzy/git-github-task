using App.GameCore;
using App.GameCore.Models;
using App.InputAndOutput;

namespace App.Application.GameRunner;

public static class GameSystem
{
	//公共依赖
	public sealed record RunEnv(
		IRenderer Renderer,
		IRoundResultView ResultView,
		ulong TickMilliseconds,
		Random Random);

	//单人
	public static Task RunSingleAsync(
		WorldState state,
		IInputSource local,
		RunEnv env,
		CancellationToken ct)
		=> RunLoop(state, [local], "单人模式", "WASD 控制,E 加速,Esc 退出", env, ct);

	//双人
	public static Task RunDoubleAsync(
		WorldState state,
		IInputSource player1,
		IInputSource player2,
		RunEnv env,
		CancellationToken ct)
		=> RunLoop(state, [player1, player2], "本机双人对战", "玩家1: WASD/E   玩家2: ↑↓←→/0", env, ct);

	//网络多人
	public static Task RunNetAsync(
		WorldState state,
		IInputSource local,
		IReadOnlyList<IInputSource> remotes,
		RunEnv env,
		CancellationToken ct)
	{
		var inputs = new List<IInputSource>(remotes.Count + 1) { local };//为首
		inputs.AddRange(remotes);
		return RunLoop(state, inputs, "网络对战", "玩家1: WASD/E   Esc 退出", env, ct);
	}

	//───────── 公共部分：拼 RoundContext + 跑一局 ─────────
	static Task RunLoop(
		WorldState state,
		IReadOnlyList<IInputSource> inputs,
		string title,
		string? hint,
		RunEnv env,
		CancellationToken ct)
	{
		var context = new RoundContext(
			state, hint, title, inputs, env.Renderer, env.ResultView, env.TickMilliseconds);
		return Round.RunAsync(context, env.Random, ct);
	}

	//初始世界
	public static WorldState InitialWorldState(
		IReadOnlyList<string> playerIds,
		int maxTick,
		int width,
		int height,
		int foodNum)
	{
		var players = new List<PlayerState>(playerIds.Count);
		for (int i = 0; i < playerIds.Count; i++)
		{
			// 单人居中；多人沿横向平均分散，纵向错开
			int x = playerIds.Count == 1
				? width / 2
				: 10 + i * (width - 21) / (playerIds.Count - 1);
			int y = height * (i + 1) / (playerIds.Count + 1);
			var dir = i % 2 == 0 ? Direction.Down : Direction.Up;
			players.Add(PlayerState.Create(
				playerIds[i], $"玩家{i + 1}", new Point(x, y), dir, 3));
		}

		return new WorldState(
			0UL,//UL表示这是ulong
			(ulong)Math.Max(1, maxTick),
			width,
			height,
			players,
			[],
			foodNum
		);
	}
}
