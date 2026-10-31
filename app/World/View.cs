using App.Services;

using System.Text;

namespace App.World;

public static class MapPrinter
{
	// 每个玩家一套符号，超出 4 个循环使用
	static readonly (char Head, char Body)[] Styles =
	[
		('☻', '◊'), ('☺', '○'), ('♥', '♦'), ('★', '☆')
	];

	public static void PrintMap(GameWorld world, IReadOnlyList<Player> players)
	{
		var bodies = new HashSet<State.Point>[players.Count];
		for (int i = 0; i < players.Count; i++)
			bodies[i] = [.. players[i].MSnake.Snake];

		var sb = new StringBuilder(world.Width * world.Height + world.Height);
		for (int y = 0; y < world.Height; y++)
		{
			for (int x = 0; x < world.Width; x++)
			{
				if (world.IsWall(x, y)) { _ = sb.Append('█'); continue; }

				var pt = new State.Point(x, y);
				char c = ' ';
				for (int i = 0; i < players.Count; i++)
				{
					var p = players[i];
					if (!p.IsAlive) continue;
					var (head, body) = Styles[i % Styles.Length];
					if (p.MSnake.Head == pt) { c = head; break; }

					if (bodies[i].Contains(pt)) { c = body; break; }
				}

				if (c == ' ' && world.FoodAt(x, y) is not null) c = '✰';
				_ = sb.Append(c);
			}

			_ = sb.AppendLine();
		}

		Console.Write(sb.ToString());
	}

	public static void PrintHud(GameWorld world, IReadOnlyList<Player> players)
	{
		for (int i = 0; i < players.Count; i++)
		{
			var p = players[i];
			char head = Styles[i % Styles.Length].Head;
			string name = string.IsNullOrWhiteSpace(p.Name) ? $"玩家{i + 1}" : p.Name;
			Console.WriteLine($"{head} {name}: {p.MScore.MyScore} 分　长度 {p.MSnake.Snake.Count}　{(p.IsAlive ? "存活" : "死亡")}");
		}

		double left = Math.Max(0, world.GameDurationSeconds - world.ElapsedTime.TotalSeconds);
		Console.WriteLine($"剩余 {left:F0} 秒　食物 {world.FoodNum} 个");
	}
}
