using App.Game.Screen;
using App.KeyBoard;
using App.NetWork;
using App.World;

namespace App.Services;

public sealed record RoundContext
(
	string? Hint,
	string Title,
	IReadOnlyList<KeyBoardController> Inputs,
	IRoundRules Rules
);

public interface IRoundRules
{
	bool IsGameOver(GameWorld world, IReadOnlyList<Player> players);
	void Result(GameWorld world, IReadOnlyList<Player> players);
}

sealed class SingleRules : IRoundRules
{
	public bool IsGameOver(GameWorld world, IReadOnlyList<Player> players) => world.IsOver || !players[0].IsAlive;
	public void Result(GameWorld world, IReadOnlyList<Player> players) => DefeatSystem.SingleEndSystem(players[0], world);
}

sealed class DoubleRules : IRoundRules
{
	public bool IsGameOver(GameWorld world, IReadOnlyList<Player> players) => world.IsOver || players.Any(x => !x.IsAlive);
	public void Result(GameWorld world, IReadOnlyList<Player> players)
	{
		var p1 = players[0];
		var p2 = players[1];

		DefeatSystem.DoubleEndSystem(p1, p2, world);
	}
}

public static class Round
{
	//单游戏逻辑
	public static async Task RunAsync(GamePrepareArgs args, RoundContext rct)
	{
		var world = args.GameWorld;
		var players = world.Players;
		var tick = TimeSpan.FromMilliseconds(world.TickMilliseconds);
		using var timer = new PeriodicTimer(tick);

		while (!world.IsOver)
		{
			while (Console.KeyAvailable)
				args.Button.TouchClick(Console.ReadKey(intercept: true));
			NetHost.PumpInput(args.Button);
			if (world.QuitRequired) break;
			lock (world.Lock)
			{
				world.UpdateTime(tick);
				if (world.IsOver) break;
				for (int i = 0; i < players.Count; i++)
				{
					var p = players[i];
					p.MSnake.Nextdir = rct.Inputs[i].Direction;
					p.Skills.MoveMode(world, p);
				}

				world.UpdateFood(tick);
				foreach (var p in players)
				{
					if (p.IsAlive)
						args.Collision.TouchCollision(p, world);
				}
			}

			var snap = world.Snapshot();
			MapPrinter.Draw(snap, rct.Title ?? string.Empty);

			if (!await timer.WaitForNextTickAsync()) break;
		}
	}
}
