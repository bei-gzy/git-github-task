using App.Models;
using App.Services;
using App.GameScore;

namespace App.GameCollision;

public class CollisionArgs(Collision ecollision, SnakeHead snake, PlayerScore playerscore, GameMap map, Score score)
{
	public Collision Mcollision { get; } = ecollision;
	public SnakeHead Snake { get; } = snake;
	public PlayerScore PlayerScore { get; } = playerscore;
	public int GetScore { get; set; }
	public GameMap Map { get; } = map;
	public Score Score { get; } = score;
}

public class Collision()
{
	public event EventHandler<CollisionArgs>? CollisionWhat;

	public void TouchCollision(Collision ecollision, SnakeHead snake, PlayerScore playerscore, GameMap map, Score score)
	{
		var args = new CollisionArgs(ecollision, snake, playerscore, map, score);

		OnCollisionWhat(args);
	}

	//触发事件
	protected virtual void OnCollisionWhat(CollisionArgs ecollision) => CollisionWhat?.Invoke(this, ecollision);
}

public class CollisionController
{
	readonly Collision? collision;

	public CollisionController(Collision collision)
	{
		this.collision = collision;

		this.collision.CollisionWhat += OnCollision;
	}

	void OnCollision(object? sender, CollisionArgs e)
	{
		if (GameMap.IsWall(e.Snake.Head.X, e.Snake.Head.Y) || e.Snake.Snake.Take(e.Snake.Snake.Count - 1).Contains(e.Snake.Head))
		{
			State.IsGameOver = true;
			DefeatSystem.Defeat(e.PlayerScore);
		}

		if (GameMap.IsFood(e.Snake.Head.X, e.Snake.Head.Y))
		{
			e.GetScore = GameMap.FoodType(e.Snake.Head.X, e.Snake.Head.Y);
			GameMap.DeleteFood();
			e.Snake.Snake.Insert(0, e.Snake.Tail);
			e.Score.CallAddScore(e.PlayerScore, e.GetScore);
			e.Map.GenerateFood();
		}

		else
		{
			e.Snake.Snake.RemoveAt(0);
		}
	}

	public void CancelCollision() => collision?.CollisionWhat -= OnCollision;
}

// public class CollisionSystem
// {
// 	EventHandler<CollisionArgs>? mCollisionWhat;

// 	public void DoCollision(Collision ecollision, SnakeHead snake, PlayerScore playerscore, int getScore, GameMap map, Score score)
// 	{
// 		mCollisionWhat = (sender, args) =>
// 		{
// 			if (GameMap.IsWall(snake.Head.X, snake.Head.Y) || snake.Snake.Take(snake.Snake.Count - 1).Contains(snake.Head))
// 			{
// 				State.IsGameOver = true;
// 				DefeatSystem.Defeat(playerscore);
// 			}

// 			if (GameMap.IsFood(snake.Head.X, snake.Head.Y))
// 			{
// 				getScore = GameMap.FoodType(snake.Head.X, snake.Head.Y);
// 				GameMap.DeleteFood();
// 				snake.Snake.Insert(0, snake.Tail);
// 				score.CallAddScore(getScore);
// 				map.GenerateFood();
// 			}

// 			else
// 			{
// 				snake.Snake.RemoveAt(0);
// 			}
// 		};

// 		ecollision.CollisionWhat += mCollisionWhat;
// 	}

// 	public void CancleDoCollision(Collision ecollision)
// 	{
// 		if (mCollisionWhat != null)
// 		{
// 			ecollision.CollisionWhat -= mCollisionWhat;
// 			mCollisionWhat = null;
// 		}
// 	}
// }
