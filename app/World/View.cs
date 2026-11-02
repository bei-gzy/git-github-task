using App.Config;
using App.Protocol;
using App.Services;

using System.Text;

namespace App.World;

public static class MapPrinter
{
	static readonly (char Head, char Body)[] Styles =
	[
		('@', 'o'), ('O', 'x'), ('S', '+'), ('M', '=')
	];

	const char Wall = '#';
	const char Food = '*';

	public static void PrintMap(WorldSnapShot ws)
	{
		var bodies = new HashSet<State.Point>[ws.Players.Count];
		for (int i = 0; i < ws.Players.Count; i++)
			bodies[i] = [.. ws.Players[i].Segments];

		var sb = new StringBuilder(ws.Width * ws.Height + ws.Height);

		for (int y = 0; y < ws.Height; y++)
		{
			for (int x = 0; x < ws.Width; x++)
			{
				char c = (y == 0 || x == 0 || x == ws.Width - 1 || y == ws.Height - 1) ? Wall : ' ';

				var pt = new State.Point(x, y);

				for (int i = 0; i < ws.Players.Count; i++)
				{
					var p = ws.Players[i];
					if (!p.IsAlive) continue;

					var (head, body) = Styles[i % Styles.Length];

					if (p.Segments.Count > 0 && p.Segments[^1] == pt) { c = head; break; }
					if (bodies[i].Contains(pt)) { c = body; break; }
				}

				if (c == ' ')
				{
					foreach (var f in ws.Foods)
					{
						if (f.Position == pt) { c = Food; break; }
					}
				}

				sb.Append(c);
			}

			sb.AppendLine();
		}

		Console.Write(sb.ToString());
	}

	public static void PrintHud(WorldSnapShot ws)
	{
		for (int i = 0; i < ws.Players.Count; i++)
		{
			var p = ws.Players[i];
			char head = Styles[i % Styles.Length].Head;
			string name = string.IsNullOrWhiteSpace(p.Name) ? $"P{i + 1}" : p.Name;
			Console.WriteLine($"[{head}] {name} 分数 {p.Score} 长度 {p.Segments.Count} {(p.IsAlive ? "存活" : "死亡")}");
		}

		Console.WriteLine($"剩余 {ws.RemainingSeconds:F0} 秒 食物 {ws.Foods.Count} 个");
	}

	public static void Draw(GameState state)
	{
		var ws = ToSnapshot(state);

		Console.Clear();
		Console.WriteLine($"--- 房间 {state.RoomId}  tick {state.ServerTick} ---");
		PrintMap(ws);
		PrintHud(ws);

		if (state.IsGameOver)
			Console.WriteLine("[对局结束]");
	}

	static WorldSnapShot ToSnapshot(GameState s)
	{
		int w = s.Width;
		int h = s.Height;

		if (w <= 0 || h <= 0)
		{
			var settings = GameSettingLoader.Load();
			w = settings.MapConfig.GameMapWidth;
			h = settings.MapConfig.GameMapHeight;
		}

		return new WorldSnapShot(
			s.ServerTick,
			w,
			h,
			[.. s.Players.Select(p => new PlayerSnapShot(
				p.PlayerId,
				p.Name,
				[.. p.SnakeSegments.Select(pt => new State.Point(pt.X, pt.Y))],
				p.Score,
				p.IsAlive,
				FromProtoDirection(p.Direction)))],
			[.. s.Foods.Select(f => new FoodSnapShot(
				new State.Point(f.Position.X, f.Position.Y),
				f.FoodType))],
			s.RemainingSeconds,
			s.IsGameOver);
	}

	static State.Direction FromProtoDirection(Direction d) => d switch
	{
		Direction.Up => State.Direction.Up,
		Direction.Down => State.Direction.Down,
		Direction.Left => State.Direction.Left,
		Direction.Right => State.Direction.Right,
		_ => State.Direction.Down
	};
}
