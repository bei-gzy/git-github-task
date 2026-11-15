//唯一的移动函数
using App.GameCore.Food;
using App.GameCore.Models;

namespace App.GameCore;

public static class StepLoop//纯函数
{
	readonly record struct MoveResult(
		string Id, Point NewHead, Direction NextDir, bool IsDash
	);

	public static WorldState NewState(WorldState oldw, IReadOnlyList<PlayerInput> inputs, Random random)
	{
		var moveResults = oldw.Players
			.Where(p => p.Snake.IsAlive) // 筛选
			.Select(p => Result(p, FindIntent(inputs, p.Id))) // 投影
			.ToList();

		//碰撞判定
		var dead = CollisionResult(oldw, moveResults);
		var eaten = new HashSet<Point>();

		var players = oldw.Players.Select(p =>
		{
			var body = new List<Point>(p.Snake.Body);
			var nextDir = moveResults.First(m => m.Id == p.Id).NextDir;
			var newhead = moveResults.First(m => m.Id == p.Id).NewHead;
			int score = p.Score;
			if (moveResults.First(m => m.Id == p.Id).IsDash)
			{
				for (int i = 0; i < 2;)
				{
					//碰撞
					if (!p.Snake.IsAlive || dead.Contains(p.Id))
						return p with { Snake = p.Snake with { IsAlive = false } };
					//食物 + 加分
					body.Insert(0, newhead);

					foreach (var f in oldw.Foods)
					{
						if (p.Snake.Head == f.Position)
						{
							score += f.Type;
							_ = eaten.Add(FoodRules.RemoveEatenFood(newhead, oldw).Position);
						}
						else
						{
							body.RemoveAt(p.Snake.Body.Count - 1);
						}
					}

					return p with { Snake = p.Snake with { Body = body, Direction = nextDir }, Score = score };
				}
			}
			//碰撞
			if (!p.Snake.IsAlive || dead.Contains(p.Id))
				return p with { Snake = p.Snake with { IsAlive = false } };
			//食物 + 加分
			body.Insert(0, newhead);
			foreach (var f in oldw.Foods)
			{
				if (p.Snake.Head == f.Position)
				{
					score += f.Type;
					_ = eaten.Add(FoodRules.RemoveEatenFood(newhead, oldw).Position);
				}
				else
				{
					body.RemoveAt(p.Snake.Body.Count - 1);
				}
			}

			return p with { Snake = p.Snake with { Body = body, Direction = nextDir }, Score = score };
		}).ToList();

		var foods = oldw.Foods.Where(f => !eaten.Contains(f.Position)).ToList();

		int foodnum = oldw.FoodNum;
		if (FoodRules.GenerateFood(random, oldw) is { } generatedFood)
			foods.Add(generatedFood); foodnum++;

		return oldw with { Tick = oldw.Tick + 1, Players = players, Foods = foods, FoodNum = foodnum };
	}

	static PlayerIntent? FindIntent(IReadOnlyList<PlayerInput> inputs, string id) => inputs.FirstOrDefault(i => i.Id == id)?.Intent;

	static MoveResult Result(PlayerState ps, PlayerIntent? intent)
	{
		//移动
		var (dir, dash) = intent switch
		{
			PlayerIntent.Turn(var d) => (d, false),
			PlayerIntent.Dash => (ps.Snake.Direction, true),
			_ => (ps.Snake.Direction, false)
		};
		var fandir = ps.Snake.Direction switch
		{
			Direction.Up => Direction.Down,
			Direction.Down => Direction.Up,
			Direction.Left => Direction.Right,
			Direction.Right => Direction.Left,
			_ => ps.Snake.Direction
		};
		if (dir == fandir) dir = ps.Snake.Direction;

		var newhead = Move(ps.Snake, dir);
		return new MoveResult(ps.Id, newhead, dir, dash);
	}

	static Point Move(SnakeState snake, Direction dir)
	{
		var head = snake.Head;
		return dir switch
		{
			Direction.Up => new Point(head.X, head.Y - 1),
			Direction.Down => new Point(head.X, head.Y + 1),
			Direction.Left => new Point(head.X - 1, head.Y),
			Direction.Right => new Point(head.X + 1, head.Y),
			_ => head//不会走到这一步
		};
	}

	//碰撞判死只用Id记录
	static HashSet<string> CollisionResult(WorldState w, IReadOnlyList<MoveResult> moveResults)
	{
		var dead = new HashSet<string>();
		//头碰头一起死，GroupBy
		foreach (var same in moveResults.GroupBy(m => m.NewHead))
		{
			if (same.Count() > 1)
			{
				foreach (var died in same)
					_ = dead.Add(died.Id);
			}
		}

		foreach (var mr in moveResults)
		{
			if (CollisionRules.HitTest(w, mr.NewHead, mr.Id) is not HitResult.None)
				_ = dead.Add(mr.Id);
		}

		return dead;
	}

	// static PlayerState NewPlayerState(WorldState oldw, PlayerState p, HashSet<string> dead, HashSet<Point> eaten, Direction nextDir, int score, Point newhead, List<Point> body)
	// {
	// 	//碰撞
	// 	if (!p.Snake.IsAlive || dead.Contains(p.Id))
	// 		return p with { Snake = p.Snake with { IsAlive = false } };
	// 	//食物 + 加分
	// 	body.Insert(0, newhead);
	// 	foreach (var f in oldw.Foods)
	// 	{
	// 		if (p.Snake.Head == f.Position)
	// 		{
	// 			score += f.Type;
	// 			_ = eaten.Add(FoodRules.RemoveEatenFood(newhead, oldw).Position);
	// 		}
	// 		else
	// 		{
	// 			body.RemoveAt(p.Snake.Body.Count - 1);
	// 		}
	// 	}

	// 	return p with { Snake = p.Snake with { Body = body, Direction = nextDir }, Score = score };
	// }
}
