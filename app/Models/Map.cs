using App.Services;

namespace App.Models;

class Map
{
	public const int Width = 25;

	public const int Height = 30;

	readonly string[,] map = new string[Height, Width];
	readonly Random food = new();

	static bool IsBorder(int x, int y) => x == 0 || y == 0 || x == Width - 1 || y == Height - 1;

	void InitMap()
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				map[i, j] = IsBorder(j, i) ? "■" : " ";
			}
		}
	}

	public void GenerateFood()
	{
		int x, y;
		do
		{
			x = food.Next(1, Width - 1);
			y = food.Next(1, Height - 1);
		} while (map[y, x] != " ");//&& map[y, x] != "☻" && map[y, x] != "◊"
		map[y, x] = "✰";
	}

	public void DeleteFood()
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (map[i, j] == "✰")
				{
					map[i, j] = " ";
				}
			}
		}
	}

	public Map()
	{
		InitMap();
		GenerateFood();
	}

	public bool IsWall(int x, int y) => x < 0 || y < 0 || x >= Width || y >= Height || map[y, x] == "■";

	public bool IsFood(int x, int y) => x > 0 && y > 0 && x < Width && y < Height && map[y, x] == "✰";

	public void PrintMap(SnakeHead snake)
	{
		var body = snake.Snake.ToHashSet();
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (snake.Head.X == j && snake.Head.Y == i)
					Console.Write("☻");
				else if (body.Contains(new State.Point(j, i)))
					Console.Write("◊");
				else
					Console.Write(map[i, j]);
			}

			Console.WriteLine();
		}
	}
}
