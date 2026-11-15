using App.GameCore;
using App.InputAndOutput;

namespace App.Application.RoundRunner;

public sealed record RoundContext
(
	WorldState State,
	string? Hint,
	string Title,
	IReadOnlyList<IInputSource> Inputs,
	ConsoleRenderer Renderer,
	ConsoleResultView ResultView,
	double TickMilliseconds
);

public static class Round
{
	//单游戏逻辑--组装返回游戏结果--丢给渲染层
	public static async Task RunAsync(RoundContext rct, Random random, CancellationToken ct)
	{
		var state = rct.State;
		using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(rct.TickMilliseconds));
		while (!RoundRules.IsGameOver(rct.State) && !ct.IsCancellationRequested)
		{
			var intents = rct.Inputs
				.Select(p => p.Capture())
				.Where(i => i is not null)
				.Select(i => i!)                // 告诉编译器：经过过滤后，这里不会是 null
				.ToList();
			state = StepLoop.NewState(rct.State, intents, random);

			rct.Renderer.Draw(state, rct.Title, rct.Hint);

			if (!await timer.WaitForNextTickAsync(ct)) break;
		}

		var results = RoundRules.Results(state);
		if (!results.IsOver)
			return;
		rct.ResultView.ShowResult(results);
	}
}
