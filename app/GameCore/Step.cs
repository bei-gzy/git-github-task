//唯一的移动函数
using App.GameCore.Food;
using App.GameCore.Models;

namespace App.GameCore;

public static class StepLoop//纯函数
{
	public static WorldState Loop(WorldState oldw, IReadOnlyList<PlayerIntent> intents, Random random, ulong maxTick)
	{
		var players = new List<PlayerState>(oldw.Players.Count);
		var foods = new List<FoodItem>(oldw.Foods.Count);

		for (int i = 0; i < oldw.Players.Count; i++)//底层不需要知道谁是谁的意图，对齐排序交给Round
		{
			var p = oldw.Players[i];
			if (!p.Snake.IsAlive) continue;
			var intent = intents[i];
			var snake = p.Snake;

			//移动
			var (dir, dash) = intent switch
			{
				PlayerIntent.Turn(var d) => (d, false),
				PlayerIntent.Dash => (snake.Direction, true),
				_ => (snake.Direction, false)
			};
			var fandir = snake.Direction switch
			{
				Direction.Up => Direction.Down,
				Direction.Down => Direction.Up,
				Direction.Left => Direction.Right,
				Direction.Right => Direction.Left,
				_ => snake.Direction
			};
			if (dir == fandir) dir = snake.Direction;

			var newhead = Move(snake, dir, dash);

			//碰撞
			if (CollisionRules.HitWall(oldw, newhead) || CollisionRules.HitBody(oldw, newhead, p.Id))
			{
				p = p with { Snake = snake with { IsAlive = false } };
			}
			else
			{
				snake.Body.Insert(0, newhead);
				//食物 + 加分
				foreach (var f in oldw.Foods)
				{
					foods.Add(f);
					if (newhead == f.Position)
					{
						p = p with { Score = p.Score + f.Type };
						foods.Add(FoodRules.RemoveEatenFood(newhead, oldw));
					}
					else
					{
						snake.Body.RemoveAt(snake.Body.Count - 1);
					}
				}
			}

			players.Add(p);
		}

		int foodnum = oldw.FoodNum;
		if (FoodRules.GenerateFood(random, oldw) is { } generatedFood)
			foods.Add(generatedFood); foodnum++;

		return oldw with { Tick = oldw.Tick + 1, Players = players, Foods = foods, FoodNum = foodnum };
	}

	static Point Move(SnakeState snake, Direction dir, bool dash)
	{
		var head = snake.Head;
		return dash
			? dir switch
			{
				Direction.Up => new Point(head.X, head.Y - 2),
				Direction.Down => new Point(head.X, head.Y + 2),
				Direction.Left => new Point(head.X - 2, head.Y),
				Direction.Right => new Point(head.X + 2, head.Y),
				_ => head//不会走到这一步
			}
			: dir switch
			{
				Direction.Up => new Point(head.X, head.Y - 1),
				Direction.Down => new Point(head.X, head.Y + 1),
				Direction.Left => new Point(head.X - 1, head.Y),
				Direction.Right => new Point(head.X + 1, head.Y),
				_ => head//不会走到这一步
			};
	}
}
