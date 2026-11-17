using App.GameCore.Models;
namespace App.GameCore;

public sealed record SnakeState(//蛇只是玩家进入游戏后生成的一组数据
	IReadOnlyList<Point> Body,
	Direction Direction,
	bool IsAlive
)
{
	public Point Head => Body[0];
	public Point Tail => Body[^1];

	public static SnakeState Create(Point head, Direction dir, int length)
	{
		var body = new List<Point>(length);
		var one = dir switch
		{
			Direction.Up => (0, 1),
			Direction.Down => (0, -1),
			Direction.Left => (1, 0),
			Direction.Right => (-1, 0),
			_ => (0, 0)
		};
		for (int i = 0; i < length; i++)
			body.Add(new Point(head.X + one.Item1 * i, head.Y + one.Item2 * i));
		return new SnakeState(body, dir, true);
	}
}

//复杂行为写在外部 Extension (保持主文件整洁)
public static class SnakeStateExtension
{
	extension(SnakeState s)
	{
		// 向当前方向延伸（返回全新的 SnakeState）
		public SnakeState Extend(Point newHead)
		{
			// 这里必须生成一个全新列表，而不是修改旧的
			var newBody = new List<Point>(s.Body.Count + 1) { newHead };
			newBody.AddRange(s.Body); // 插入旧身体

			// 用 with 表达式创建新结构
			return s with { Body = newBody.AsReadOnly() };
		}
	}
}
