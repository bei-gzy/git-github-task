using App.Game.Screen;
using App.World;

namespace App.Services;

class LocalDoubleSystem
{
	public static async Task<bool> GameRunSystemAsync(GamePrepareArgs args)
	{
		var world = args.GameWorld;
		var p1 = args.Player;
		var p2 = world.FindPlayer("guest") ?? new Player("guest");
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
		p1.ResetForNewGame(10, world.Height / 3, State.Direction.Right);
		p2.ResetForNewGame(world.Width - 11, world.Height * 2 / 3, State.Direction.Up);

		var p1Input = args.LocalInput;
		var p2Input = args.EnableGuestInput(p2, State.Direction.Up);

		var tick = TimeSpan.FromMilliseconds(world.TickMilliseconds);
		using var timer = new PeriodicTimer(tick);

		while (!world.IsOver)
		{
			while (Console.KeyAvailable)
				args.Button.TouchClick(Console.ReadKey(intercept: true));

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
					args.Collision.TouchCollision(p, world, args.Score);
			}

			Draw(world);

			if (world.IsOver) break;

			if (!await timer.WaitForNextTickAsync()) break;
		}

		p1.RecordSurvivalTime(world.ElapsedTime);
		p2.RecordSurvivalTime(world.ElapsedTime);

		if (!world.QuitRequired)
			EndSystem(p1, p2, world);

		return !world.QuitRequired;
	}

	static void Draw(GameWorld world)
	{
		Console.Clear();
		Console.WriteLine("———————————————————————————本机双人对战—————————————————————————");
		Console.WriteLine("玩家1: W A S D  加速: E   玩家2: ↑ ↓ ← →  加速: 0    退出： Esc");
		Console.WriteLine();
		MapPrinter.PrintMap(world.Snapshot());
		MapPrinter.PrintHud(world.Snapshot());
	}

	static void EndSystem(Player p1, Player p2, GameWorld world)
	{
		var winner = MultiRules.Winner(p1, p2);

		Console.Clear();
		Console.WriteLine("—————————————————————对局结束—————————————————————————————");

		bool timeUp = world.ElapsedTime.TotalSeconds >= world.GameDurationSeconds;
		Console.WriteLine(timeUp ? "时间到！" : winner is null ? "平局" : "🏆 " + winner.Name + " 获胜");
		Console.WriteLine($"{p1.Name}: {p1.MScore.MyScore} 分，长度 {p1.MSnake.Snake.Count},{(p1.IsAlive ? "存活" : "死亡")}");
		Console.WriteLine($"{p2.Name}: {p2.MScore.MyScore} 分，长度 {p2.MSnake.Snake.Count},{(p2.IsAlive ? "存活" : "死亡")}");
		Console.WriteLine($"用时 {world.ElapsedTime.TotalSeconds:F1} 秒");
		Console.WriteLine();
		Console.WriteLine("按任意键返回模式菜单…");
		_ = Console.ReadKey(intercept: true);
	}
}

// var body1 = p1.MSnake.Snake.ToHashSet();
// var body2 = p2.MSnake.Snake.ToHashSet();
// var pW = new StringBuilder();

// _ = pW.AppendLine("———————————————————————————本机双人对战—————————————————————————");
// _ = pW.AppendLine("玩家1: W A S D  加速: E   玩家2: 👆 👇 ← →  加速: 0    退出： Esc");

// for (int y = 0; y < world.Height; y++)
// {
// 	for (int x = 0; x < world.Width; x++)
// 	{
// 		var pt = new State.Point(x, y);

// 		_ = pW.Append(
// 			world.IsWall(x, y) ? '█'
// 			: p1.IsAlive && p1.MSnake.Head == pt ? '☻'
// 			: p2.IsAlive && p2.MSnake.Head == pt ? '☺'
// 			: p1.IsAlive && body1.Contains(pt) ? '◊'
// 			: p2.IsAlive && body2.Contains(pt) ? '○'
// 			: world.FoodAt(x, y) is not null ? '✰'
// 			: ' '
// 			);
// 	}

// 	_ = pW.AppendLine();
// }

// _ = pW.AppendLine(CultureInfo.InvariantCulture, $"玩家1: {p1.Name} 分数: {p1.MScore.MyScore} 长度: {p1.MSnake.Snake.Count} {(p1.IsAlive ? "存活" : "死亡")}");
// _ = pW.AppendLine(CultureInfo.InvariantCulture, $"玩家2: {p2.Name} 分数: {p2.MScore.MyScore} 长度: {p2.MSnake.Snake.Count} {(p2.IsAlive ? "存活" : "死亡")}");
// _ = pW.Append(CultureInfo.InvariantCulture, $"剩余时间: {Math.Max(0, world.GameDurationSeconds - world.ElapsedTime.TotalSeconds):F0} 秒");

// Console.Clear();
// Console.WriteLine(pW.ToString());
