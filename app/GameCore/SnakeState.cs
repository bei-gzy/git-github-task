using App.GameCore.Models;
namespace App.GameCore;

public readonly record struct SnakeState(//蛇只是玩家进入游戏后生成的一组数据
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
	public Direction Direction { get; init; } = Direction;
	public bool IsAlive { get; init; } = IsAlive;
}
