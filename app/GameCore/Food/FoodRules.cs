using App.GameCore.Models;

namespace App.GameCore.Food;

public static class FoodRules//纯生成和销毁
{
	public static FoodItem? GenerateFood(Random random, WorldState state)
	{
		if (state.Foods.Count < state.FoodNum)
		{
			for (int guard = 0; guard < state.Width * state.Height; guard++)//总次数
			{
				int x = random.Next(1, Math.Max(2, state.Width - 1));
				int y = random.Next(1, Math.Max(2, state.Height - 1));
				if (state.IsWall(x, y) || state.FoodAt(new Point(x, y)))
					continue;
				return new FoodItem(new Point(x, y), 1);
			}
		}

		return null;
	}

	public static FoodItem? RemoveEatenFood(Point head, WorldState w)
	{
		var f = w.Foods.FirstOrDefault(f => f.Position == head);
		return f.Position == head ? f : null;
	}
}
