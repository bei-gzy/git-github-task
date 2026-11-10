using App.Game.Screen;
using App.World;

namespace App.Services;

public sealed class NetMuiltiSystem
{
	public static async Task RunAsync(GamePrepareArgs args)
	{
		if (args.Session is null)
		{
			Console.WriteLine("并没有联机");
			_ = Console.ReadKey(intercept: true);
			return;
		}

		var world = args.GameWorld;
		var p1 = args.Player;
		var p2 = world.FindPlayer("guest") ?? new Player("guest");
		if (world.FindPlayer(p2.Id) is null)
			p2 = world.AddPlayer(p2);

		p1.ResetForNewGame(10, world.Height / 3, State.Direction.Down);
		p2.ResetForNewGame(world.Width - 11, world.Height * 2 / 3, State.Direction.Up);

		var p2Input = args.EnableGuestInput(State.Direction.Up);

		await Round.RunAsync(args, new RoundContext(
			null,
			"玩家1: W A S D / E 加速   玩家2: ↑ ↓ ← → / 0 加速   Esc 退出",
			[args.LocalInput, p2Input],
			new DoubleRules(),
			args.Renderer));
	}
}
