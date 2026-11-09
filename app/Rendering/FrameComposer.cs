using App.World;
using App.Services;

using System.Globalization;
using System.Text;

namespace App.Rendering;

//快照->字符串   纯函数
public static class FrameComposer
{
	static readonly (char Head, char Body)[] Styles =
	[
		('☻', '◊'), ('☺', '○'), ('♥', '♦'), ('★', '☆')
	];

	static void PrintMap(WorldSnapShot ws, StringBuilder sb)
	{
		if (ws.Width * ws.Height is < 100 or > 5000
			|| ws.Players.Count > 8
			|| ws.Foods.Count > 50) { return; }

		var bodies = new HashSet<State.Point>[ws.Players.Count];//把身体的各点封装成去重哈希集合
		for (int i = 0; i < ws.Players.Count; i++)
			bodies[i] = [.. ws.Players[i].Segments];//new HashSet<State.Point>(~~)

		char c;
		for (int y = 0; y < ws.Height; y++)
		{
			for (int x = 0; x < ws.Width; x++)
			{
				c = (y == 0 || x == 0 || x == ws.Width - 1 || y == ws.Height - 1) ? '█' : ' ';

				var pt = new State.Point(x, y);
				for (int i = 0; i < ws.Players.Count; i++)
				{
					var p = ws.Players[i];
					if (!p.IsAlive) continue;

					var (head, body) = Styles[i % Styles.Length];
					if (p.Segments.Count > 0)
					{
						if (p.Segments[^1] == pt) { c = head; break; }
						else if (bodies[i].Contains(pt)) { c = body; break; }
					}
				}

				if (c == ' ')
				{
					foreach (var f in ws.Foods)
					{
						if (f.Position == pt) { c = '✰'; break; }
					}
				}

				_ = sb.Append(c);
			}

			_ = sb.AppendLine();
		}
	}

	static void PrintHud(WorldSnapShot ws, StringBuilder sb)
	{
		for (int i = 0; i < ws.Players.Count; i++)
		{
			var p = ws.Players[i];
			char head = Styles[i % Styles.Length].Head;
			string name = string.IsNullOrWhiteSpace(p.Name) ? $"玩家{i + 1}" : p.Name;
			_ = sb.AppendLine(CultureInfo.InvariantCulture, $"{head} {name}: {p.Score} 分 长度 {p.Segments.Count} {(p.IsAlive ? "存活" : "死亡")}");
		}

		_ = sb.AppendLine(CultureInfo.InvariantCulture, $"剩余 {ws.RemainingSeconds:F0} 秒 食物 {ws.Foods.Count} 个");
	}

	public static string Draw(WorldSnapShot ws, string title, string? hint = null)
	{
		var sb = new StringBuilder(ws.Width * ws.Height + ws.Height);

		_ = sb.AppendLine(title);
		if (!string.IsNullOrEmpty(hint))
			_ = sb.AppendLine(hint);
		_ = sb.AppendLine();

		PrintMap(ws, sb);
		PrintHud(ws, sb);
		return sb.ToString();
	}
}
