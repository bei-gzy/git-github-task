using App.GameScore;
using App.World;

namespace App.GameCollision;

class CollisionArgs(Player player, GameWorld world, Score score)
{
	public Player Player { get; } = player;
	public Score Score { get; } = score;
	public GameWorld World { get; } = world;
}

class Collision()
{
	public event EventHandler<CollisionArgs>? CollisionWhat;

	public void TouchCollision(Player player, GameWorld world, Score score)
	{
		var args = new CollisionArgs(player, world, score);

		OnCollisionWhat(args);
	}

	//触发事件
	protected virtual void OnCollisionWhat(CollisionArgs ecollision) => CollisionWhat?.Invoke(this, ecollision);
}

class CollisionController(Collision collision)
{
	public void Subscribe()
	{
		collision.CollisionWhat -= OnCollision;//先取消再订阅，important
		collision.CollisionWhat += OnCollision;
	}

	public void Unsubscribe() => collision.CollisionWhat -= OnCollision;

	void OnCollision(object? sender, CollisionArgs e)
	{
		bool hitWall = e.World.IsWall(e.Player.MSnake.Head.X, e.Player.MSnake.Head.Y);
		bool hitSelf = e.Player.MSnake.Snake.Take(e.Player.MSnake.Snake.Count - 1).Contains(e.Player.MSnake.Head);
		bool hitother = e.World.Players.Any(p => p != e.Player && p.IsAlive && p.MSnake.Snake.Contains(e.Player.MSnake.Head));

		if (hitWall || hitSelf || hitother)
		{
			e.Player.IsAlive = false;
			e.World.MarkGameOver();
			return;
		}

		if (e.World.FoodAt(e.Player.MSnake.Head.X, e.Player.MSnake.Head.Y) is { } food)
		{
			e.World.RemoveFood(e.Player.MSnake.Head.X, e.Player.MSnake.Head.Y);
			e.Score.CallAddScore(e.Player.MScore, food.Type);
			e.World.GenerateFood();
		}

		else
		{
			e.Player.MSnake.Snake.RemoveAt(0);
		}
	}
}