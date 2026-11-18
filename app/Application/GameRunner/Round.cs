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
	ulong TickMilliseconds,
	MqttBroadcast? Broadcast = null
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
			LocalInput.Clear();
			var intents = rct.Inputs
				.SelectMany(p => p.Capture()!)// 合并所有输入源捕获到的操作意图，统一交给后续状态更新逻辑
				.Where(i => i is not null)
				.Select(i => i!)                // 告诉编译器：经过过滤后，这里不会是 null
				.ToList();
			state = StepLoop.NewState(state, intents, random, rct.TickMilliseconds / 1000.0);

			rct.Renderer.Draw(state, rct.Title, rct.TickMilliseconds, rct.Hint);
			if (rct.Broadcast is not null)
				await rct.Broadcast.Publish(state, ct);

			if (!await timer.WaitForNextTickAsync(ct)) break;
		}

		if (rct.Broadcast is not null)
			await rct.Broadcast.Publish(state, ct);   // 终局帧
		rct.ResultView.ShowResult(RoundRules.Results(state, rct.TickMilliseconds));
	}
}
