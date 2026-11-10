using App.Interface;

namespace App.Services;

public sealed record RoundContext
(
	string? Hint,
	string Title,
	IReadOnlyList<IInputSource> Inputs,
	IRoundRules Rules,
	IRenderer Renderer
);

public static class Round
{
	//单游戏逻辑
	public static async Task RunAsync(GamePrepareArgs args, RoundContext rct)
	{
		var world = args.GameWorld;
		var players = world.Players;
		var tick = TimeSpan.FromMilliseconds(world.TickMilliseconds);
		using var timer = new PeriodicTimer(tick);

		while (!world.IsOver && !rct.Rules.IsGameOver(world, players))
		{
			while (Console.KeyAvailable)
				args.Button.TouchClick(Console.ReadKey(intercept: true));
			args.PumpInput();
			if (world.QuitRequired) break;
			lock (world.Lock)
			{
				world.UpdateTime(tick);
				if (world.IsOver) break;

				var intents = new PlayerIntent[players.Count];
				for (int i = 0; i < players.Count; i++)
					intents[i] = rct.Inputs[i].Poll();

				for (int i = 0; i < players.Count && i < rct.Inputs.Count; i++)
				{
					var p = players[i];
					p.MSnake.Nextdir = intents[i].Direction;
					p.IsTouchButton = intents[i].IsDash;
					p.Skills.MoveMode(world, p);
				}

				world.UpdateFood(tick);
				foreach (var p in players)
				{
					if (p.IsAlive)
						args.Collision.TouchCollision(p, world);
				}
			}

			rct.Renderer.Draw(world.Snapshot(), rct.Title, rct.Hint);
			if (!await timer.WaitForNextTickAsync()) break;
		}

		if (!world.QuitRequired)
		{
			var result = rct.Rules.Result(world, players);
			args.ResultView.ShowResult(result);
			args.ResultView.WaitForNext();
		}
	}
}
