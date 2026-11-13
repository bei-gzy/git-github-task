//唯一的移动函数
namespace App.GameCore;

public static class StepLoop//纯函数
{
	public static WorldState Loop(WorldState oldw, PlayerIntent intent, ulong maxTick, int foodNum)
	{
		//移动
		Move(oldw, intent);

		//碰撞

		//加分

		//食物

		return new WorldState();
	}

	static void Move(WorldState w, PlayerIntent pi)
	{
		foreach (var p in w.Players)
		{
			var head = p.Snake.Head;
			var nextLocation = pi switch
			{
				Direction dir => dir.Dir switch
				{
					DirectionType.Up => new Point(head.X, head.Y - 1),
					DirectionType.Down => new Point(head.X, head.Y + 1),
					DirectionType.Left => new Point(head.X - 1, head.Y),
					DirectionType.Right => new Point(head.X + 1, head.Y),
					_ => head//不会走到这一步
				},
				Skills => head,
				_ => head
			};
		}
	}
}

// if (hitWall || hitSelf || hitother)
// 	{
// 		e.Player.IsAlive = false;
// 		e.World.MarkGameOver();
// 		return;
// 	}

// 	if (e.World.FoodAt(e.Player.MSnake.Head.X, e.Player.MSnake.Head.Y) is { } food)
// 	{
// 		e.World.RemoveFood(e.Player.MSnake.Head.X, e.Player.MSnake.Head.Y);
// 		e.Player.MScore.AddScore(food);
// 		e.World.GenerateFood();
// 	}

// 	else
// 	{
// 		e.Player.MSnake.Bodies.RemoveAt(0);
// 	}
