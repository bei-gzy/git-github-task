using App.GameCore.Models;
using App.GameCore.Food;

namespace App.GameCore;

//机制
public sealed record WorldState(//传入数据，更新状态--合并快照和状态
	ulong Tick,
	ulong MaxTick,
	int Width,
	int Height,
	IReadOnlyList<PlayerState> Players,
	IReadOnlyList<FoodItem> Foods,
	int FoodNum,
	double RemainingSeconds
);

public static class WorldStateExtension
{
	extension(WorldState w)
	{
		public bool OverTime => w.Tick >= w.MaxTick;

		public bool IsWall(int x, int y) => x <= 0 || y <= 0 || x >= w.Width - 1 || y >= w.Height - 1;

		public bool FoodAt(Point p) => w.Foods.Any(f => f.Position == p);
	}
}
