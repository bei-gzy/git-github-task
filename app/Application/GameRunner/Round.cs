using App.GameCore;
using App.InputAndOutput;

namespace App.Application.GameRunner;

public sealed record RoundContext
(
	WorldState State,
	string? Hint,
	string Title,
	IReadOnlyList<IInputSource> Inputs,
	IRenderer Renderer,
	IRoundResultView ResultView,
	ulong TickMilliseconds
);

public static class Round
{
	//单游戏逻辑--组装返回游戏结果--丢给渲染层
	public static async Task RunAsync(RoundContext rct, Random random, CancellationToken ct)
	{
		var state = rct.State;
		using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(rct.TickMilliseconds));
		while (!RoundRules.IsGameOver(state) && !ct.IsCancellationRequested)
		{
			var intents = rct.Inputs
				.Select(p => p.Capture())
				.Where(i => i is not null)
				.Select(i => i!)                // 告诉编译器：经过过滤后，这里不会是 null
				.ToList();
			state = StepLoop.NewState(state, intents, random);

			rct.Renderer.Draw(state, rct.Title, rct.TickMilliseconds, rct.Hint);

			if (!await timer.WaitForNextTickAsync(ct)) break;
		}

		rct.ResultView.ShowResult(RoundRules.Results(state));
	}
}
