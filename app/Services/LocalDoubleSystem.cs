using App.Game.Screen;
using App.World;
using App.NetWork;

namespace App.Services;

class LocalDoubleSystem
{
	public static async Task<bool> GameRunSystemAsync(GamePrepareArgs args)
	{
		var world = args.GameWorld;
		var p1 = args.Player;

		var p2 = world.FindPlayer("guest") ?? new Player("guest");
		if (world.FindPlayer(p2.Id) is null)
			p2 = world.AddPlayer(p2);

		var room = new RoomPlayerLists();
		if (!room.AddPlayer(p1) || !room.AddPlayer(p2))
		{
			Console.WriteLine("房间已满，无法开始对局");
			_ = Console.ReadKey(intercept: true);
			return false;
		}

		p1.SetName("玩家1");
		p2.SetName("玩家2");
		p1.ResetForNewGame(10, world.Height / 3, State.Direction.Down);
		p2.ResetForNewGame(world.Width - 11, world.Height * 2 / 3, State.Direction.Up);

		var p1Input = args.LocalInput;
		var p2Input = args.EnableGuestInput(p2, State.Direction.Up);

		var tick = TimeSpan.FromMilliseconds(world.TickMilliseconds);
		using var timer = new PeriodicTimer(tick);

		while (!world.IsOver)
		{
			while (Console.KeyAvailable)
				args.Button.TouchClick(Console.ReadKey(intercept: true));
			NetHost.PumpInput(args.Button);
			lock (world.Lock)//上锁
			{
				if (world.QuitRequired) break;

				world.UpdateTime(tick);
				if (world.IsOver) break;

				world.UpdateFood(tick);

				p1.MSnake.Nextdir = p1Input.Direction;
				p2.MSnake.Nextdir = p2Input.Direction;
				p1.Skills.MoveMode(world, p1);
				p2.Skills.MoveMode(world, p2);

				foreach (var p in world.Players)
				{
					if (p.IsAlive)
						args.Collision.TouchCollision(p, world);
				}
			}

			Draw(world);

			if (world.IsOver) break;

			if (!await timer.WaitForNextTickAsync()) break;
		}

		p1.RecordSurvivalTime(world.ElapsedTime);
		p2.RecordSurvivalTime(world.ElapsedTime);

		if (!world.QuitRequired)
			DefeatSystem.DoubleEndSystem(p1, p2, world);

		return !world.QuitRequired;
	}

	static void Draw(GameWorld world) => MapPrinter.Draw(world.Snapshot(),
		"————— 本机双人对战 —————",
		"玩家1: W A S D / E 加速   玩家2: ↑ ↓ ← → / 0 加速   Esc 退出"
		);
}
