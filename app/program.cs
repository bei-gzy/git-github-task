//1.地图上的豆的随机性问题
//2.蛇的连续移动问题，如何保持身体沿着头的路径--身头分别封装
//3.蛇的固定移动速度
using App.Models;
using App.Services;

//program <- Snake <- Services

string? sname;
Console.WriteLine("Please set your snake's name: ");
while (true)
{
	sname = Console.ReadLine();
	if (string.IsNullOrEmpty(sname))
	{
		Console.WriteLine("That 's name is empty,please try again: ");
		continue;
	}
	else
	{
		break;
	}
}

var state = State.GamingState.GamingStateRunning;
var dir = State.Direction.Down;
State score = new();
var map = new Map();
//var collision = new Collision();
var snake = new SnakeHead()
{
	HId = 0,
	Name = sname,
};

while (state == State.GamingState.GamingStateRunning)
{
	Console.Clear();
	Console.WriteLine("——————————————Weclome TO Snake Eat Game——————————————");
	Console.WriteLine("法则:WASD控制方向,按Esc退出游戏");

	map.PrintMap(snake);
	if (Console.KeyAvailable)
	{
		var tkey = Console.ReadKey(intercept: true).Key;

		if (tkey == ConsoleKey.Escape)
		{
			state = State.GamingState.GamingGameover;
			break;
		}

		var nextdir = tkey switch
		{
			ConsoleKey.W when dir != State.Direction.Down => State.Direction.Up,
			ConsoleKey.S when dir != State.Direction.Up => State.Direction.Down,
			ConsoleKey.A when dir != State.Direction.Right => State.Direction.Left,
			ConsoleKey.D when dir != State.Direction.Left => State.Direction.Right,
			_ => dir
		};

		dir = nextdir;
	}

	snake.Move(dir);

	if (Collision.CheckWallCollision(snake, map) || Collision.CheckSnakeCollision(snake))
	{
		state = State.GamingState.GamingGameover;
		DefeatSystem.Defeat(score);
		break;
	}

	if (Collision.CheckFoodCollision(snake, map))
	{
		snake.Snake.Add(snake.Tail);
		score.SetScore(score.GetScore() + 1);
		map.GenerateFood();
	}

	snake.Snake.RemoveAt(0);

	Thread.Sleep(50);
}

// 只有普通语句 → 生成 void Main()
// 含 await → 生成 Task Main()（由你剪贴板里第一段话可知）
// 含 return → 生成 int Main()，即退出码（剪贴板第二段）
// 同时含 await 和 return → Task<int> Main()
