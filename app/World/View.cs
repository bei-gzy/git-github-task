using App.Models;
using App.Services;

namespace App.World;

public static class MapPrinter
{
	public static void PrintMap(GameWorld world, SnakeHead snake)
	{
		var body = snake.Snake.ToHashSet();
		for (int i = 0; i < world.Height; i++)
		{
			for (int j = 0; j < world.Width; j++)
			{
				if (world.IsWall(j, i))
					Console.Write("■");
				else if (snake.Head.X == j && snake.Head.Y == i)
					Console.Write("☻");
				else if (body.Contains(new State.Point(j, i)))
					Console.Write("◊");
				else if (world.FoodAt(j, i) is not null)
					Console.Write("✰");
				else
					Console.Write(" ");
			}

			Console.WriteLine();
		}
	}
}
