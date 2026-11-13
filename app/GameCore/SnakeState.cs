namespace App.GameCore;

public record struct SnakeState(//蛇只是玩家进入游戏后生成的一组数据
	List<Point> Body,
	Direction Direction,
	int Length,
	bool IsAlive
)
{
	public List<Point> Body { get; init; } = Body;
	public readonly Point Head => Body[0];
	public readonly Point Tail => Body[^1];
	public int Length { get; init; } = Length;
	public Direction Direction1 { get; init; } = Direction;
	public bool IsAlive { get; init; } = IsAlive;
}

//初始化的蛇
// public Snake(int x, int y, Direction dir, int length)
// {
// 	Bodies.Clear();
// 	Nextdir = dir;

// 	//当前方向的单位向量
// 	(int dx, int dy) = dir switch
// 	{
// 		Direction.Left => (-1, 0),
// 		Direction.Right => (1, 0),
// 		Direction.Up => (0, -1),
// 		Direction.Down => (0, 1),
// 		_ => (0, 0)
// 	};

// 	//从尾巴往头铺：Head = Snake[^1] 就是这个点，身体都摆在它身后，
// 	//这样第一次移动只会往前走，不会一出生就踩到自己
// 	for (int i = length - 1; i >= 0; i--)
// 	{
// 		Bodies.Add(new Point(x - i * dx, y - i * dy));
// 	}
// }
