using App.Models;
using App.GameScore;
using App.World;

namespace App.GameCollision;

class CollisionArgs(SnakeHead snake, PlayerScore playerscore, GameWorld world, Score score)
{
	public SnakeHead Snake { get; } = snake;
	public PlayerScore PlayerScore { get; } = playerscore;
	public Score Score { get; } = score;
	public GameWorld World { get; } = world;
}

class Collision()
{
	public event EventHandler<CollisionArgs>? CollisionWhat;

	public void TouchCollision(SnakeHead snake, PlayerScore playerscore, GameWorld world, Score score)
	{
		var args = new CollisionArgs(snake, playerscore, world, score);

		OnCollisionWhat(args);
	}

	//触发事件
	protected virtual void OnCollisionWhat(CollisionArgs ecollision) => CollisionWhat?.Invoke(this, ecollision);
}

class CollisionController(Collision collision)
{
	/// <summary>撞墙/撞自己是否直接判整局结束。多人模式由 LocalDoubleSystem 自己判生死，这里关掉</summary>
	public bool EndGameOnCollision { get; set; } = true;

	public void Subscribe()
	{
		collision.CollisionWhat -= OnCollision;//先取消再订阅，important
		collision.CollisionWhat += OnCollision;
	}

	public void Unsubscribe() => collision.CollisionWhat -= OnCollision;

	void OnCollision(object? sender, CollisionArgs e)
	{
		if (e.World.IsWall(e.Snake.Head.X, e.Snake.Head.Y) || e.Snake.Snake.Take(e.Snake.Snake.Count - 1).Contains(e.Snake.Head))
		{
			if (EndGameOnCollision)
				e.World.MarkGameOver();
			else
				return;
		}

		if (e.World.FoodAt(e.Snake.Head.X, e.Snake.Head.Y) is { } food)
		{
			e.World.RemoveFood(e.Snake.Head.X, e.Snake.Head.Y);
			e.Score.CallAddScore(e.PlayerScore, food.Type);
			e.World.GenerateFood();
		}

		else
		{
			e.Snake.Snake.RemoveAt(0);
		}
	}
}
