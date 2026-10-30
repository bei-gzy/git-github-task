using App.Game.Screen;
using App.World;

using System.Globalization;
using System.Text;

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
		//错开一行出生，不然两头会正对着撞在同一格
		p1.ResetForNewGame(10, world.Height / 3, State.Direction.Right);
		p2.ResetForNewGame(world.Width - 11, world.Height * 2 / 3, State.Direction.Left);

		//玩家1 复用同一套 WASD 控制器，玩家2 挂一套方向键——两者都走 Button 事件
		var p1Input = args.LocalInput;
		var p2Input = args.EnableGuestInput(p2, State.Direction.Left);
		p1Input.Begin(State.Direction.Right);

		//多人模式自己判生死，别让碰撞控制器提前把整局判死
		args.CollisionController.EndGameOnCollision = false;

		var tick = TimeSpan.FromMilliseconds(world.TickMilliseconds);
		using var timer = new PeriodicTimer(tick);

		Draw(p1, p2, world);

		while (!world.IsOver)
		{
			//双人：和单人用的是同一段按键分发
			args.PumpInput();

			if (world.QuitRequired) break;

			world.UpdateTime(tick);
			if (world.IsOver) break;

			world.UpdateFood(tick);

			p1.MSnake.Nextdir = p1Input.Direction;
			p2.MSnake.Nextdir = p2Input.Direction;
			p1.Skills.MoveMode(world, p1);
			p2.Skills.MoveMode(world, p2);

			EatFood(p1, world, args);
			EatFood(p2, world, args);

			if (IsDead(p1, p2, world))
			{
				p1.IsAlive = false;
				world.MarkGameOver();
			}

			if (IsDead(p2, p1, world))
			{
				p2.IsAlive = false;
				world.MarkGameOver();
			}

			Draw(p1, p2, world);

			if (world.IsOver) break;

			if (!await timer.WaitForNextTickAsync()) break;
		}

		p1.RecordSurvivalTime(world.ElapsedTime);
		p2.RecordSurvivalTime(world.ElapsedTime);

		if (!world.QuitRequired)
			EndSystem(p1, p2, world);

		return !world.QuitRequired;
	}

	static void Draw(Player p1, Player p2, GameWorld world)
	{
		var body1 = p1.MSnake.Snake.ToHashSet();
		var body2 = p2.MSnake.Snake.ToHashSet();
		var pW = new StringBuilder();

		_ = pW.AppendLine("———————————————————————————本机双人对战—————————————————————————");
		_ = pW.AppendLine("玩家1: W A S D 加速: E   玩家2: ↑ ↓ ← → 加速: 小键盘0 / 0    退出： Esc");

		for (int y = 0; y < world.Height; y++)
		{
			for (int x = 0; x < world.Width; x++)
			{
				var pt = new State.Point(x, y);

				char cell =
					world.IsWall(x, y) ? '█'
					: p1.IsAlive && p1.MSnake.Head == pt ? '☻'
					: p2.IsAlive && p2.MSnake.Head == pt ? '☺'
					: p1.IsAlive && body1.Contains(pt) ? '◊'
					: p2.IsAlive && body2.Contains(pt) ? '○'
					: world.FoodAt(x, y) is not null ? '✰'
					: ' ';

				_ = pW.Append(cell);
			}

			_ = pW.AppendLine();
		}

		_ = pW.AppendLine(CultureInfo.InvariantCulture, $"玩家1: {p1.Name} 分数: {p1.MScore.MyScore} 长度: {p1.MSnake.Snake.Count} {(p1.IsAlive ? "存活" : "死亡")}");
		_ = pW.AppendLine(CultureInfo.InvariantCulture, $"玩家2: {p2.Name} 分数: {p2.MScore.MyScore} 长度: {p2.MSnake.Snake.Count} {(p2.IsAlive ? "存活" : "死亡")}");
		_ = pW.Append(CultureInfo.InvariantCulture, $"剩余时间: {Math.Max(0, world.GameDurationSeconds - world.ElapsedTime.TotalSeconds):F0} 秒");

		Console.Clear();
		Console.WriteLine(pW.ToString());
	}

	//撞墙、撞自己、撞对方
	static bool IsDead(Player me, Player other, GameWorld world)
		=> !me.IsAlive
			|| world.IsWall(me.MSnake.Head.X, me.MSnake.Head.Y)
			|| me.MSnake.Snake.Take(me.MSnake.Snake.Count - 1).Contains(me.MSnake.Head)
			|| (other.IsAlive && other.MSnake.Snake.Contains(me.MSnake.Head));

	static void EatFood(Player p, GameWorld world, GamePrepareArgs args)
	{
		if (!p.IsAlive) return;

		var head = p.MSnake.Head;

		if (world.FoodAt(head.X, head.Y) is { } food)
		{
			world.RemoveFood(head.X, head.Y);
			args.Score.CallAddScore(p.MScore, food.Type);
			world.GenerateFood();
		}

		else
		{
			p.MSnake.Snake.RemoveAt(0);
		}
	}

	static void EndSystem(Player p1, Player p2, GameWorld world)
	{
		var winner = MultiRules.Winner(p1, p2);

		Console.Clear();
		Console.WriteLine("—————————————————————对局结束—————————————————————————————");

		bool timeUp = world.ElapsedTime.TotalSeconds >= world.GameDurationSeconds;
		Console.WriteLine(timeUp ? "时间到！" : winner is null ? "平局" : "🏆 " + winner.Name + " 获胜");

		Console.WriteLine($"{p1.Name}: {p1.MScore.MyScore} 分，长度 {p1.MSnake.Snake.Count}，{(p1.IsAlive ? "存活" : "死亡")}");
		Console.WriteLine($"{p2.Name}: {p2.MScore.MyScore} 分，长度 {p2.MSnake.Snake.Count}，{(p2.IsAlive ? "存活" : "死亡")}");
		Console.WriteLine($"用时 {world.ElapsedTime.TotalSeconds:F1} 秒");
		Console.WriteLine();
		Console.WriteLine("按任意键返回模式菜单…");

		_ = Console.ReadKey(intercept: true);
	}
}
