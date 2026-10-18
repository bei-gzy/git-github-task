using App.Services;
namespace App.Models;

public class GameMap
{
	protected const int MapWidth = 25;
	protected const int MapHeight = 30;
	protected static (int Width, int Height) MapSize => (MapWidth, MapHeight);
	static readonly string[,] Map = new string[MapSize.Height, MapSize.Width];

	readonly Random food = new();

	static bool IsBorder(int x, int y) => x == 0 || y == 0 || x == MapSize.Width - 1 || y == MapSize.Height - 1;

	static void InitMap()
	{
		for (int i = 0; i < MapSize.Height; i++)
		{
			for (int j = 0; j < MapSize.Width; j++)
			{
				Map[i, j] = IsBorder(j, i) ? "■" : " ";
			}
		}
	}

	public void GenerateFood()
	{
		int x, y;
		do
		{
			x = food.Next(1, MapSize.Width - 1);
			y = food.Next(1, MapSize.Height - 1);
		} while (Map[y, x] != " ");//&& map[y, x] != "☻" && map[y, x] != "◊"
		Map[y, x] = "✰";
	}

	public static void DeleteFood()
	{
		for (int i = 0; i < MapSize.Height; i++)
		{
			for (int j = 0; j < MapSize.Width; j++)
			{
				if (Map[i, j] == "✰")
				{
					Map[i, j] = " ";
				}
			}
		}
	}

	public GameMap()
	{
		InitMap();
		GenerateFood();
	}

	public static bool IsWall(int x, int y) => x < 0 || y < 0 || x >= MapSize.Width || y >= MapSize.Height || Map[y, x] == "■";

	public static bool IsFood(int x, int y) => x > 0 && y > 0 && x < MapSize.Width && y < MapSize.Height && (Map[y, x] == "✰" || Map[y, x] == "♠" || Map[y, x] == "♥" || Map[y, x] == "♦" || Map[y, x] == "♣");

	public static int FoodType(int x, int y) => Map[y, x] switch
	{
		"✰" => 1,
		"♠" => 2,
		"♥" => 3,
		"♦" => 4,
		"♣" => 5,
		_ => 0
	};

	public static void PrintMap(SnakeHead snake)
	{
		var body = snake.Snake.ToHashSet();
		for (int i = 0; i < MapSize.Height; i++)
		{
			for (int j = 0; j < MapSize.Width; j++)
			{
				if (snake.Head.X == j && snake.Head.Y == i)//先打蛇头
					Console.Write("☻");
				else if (body.Contains(new State.Point(j, i)))
					Console.Write("◊");
				else
					Console.Write(Map[i, j]);
			}

			Console.WriteLine();
		}
	}
}
