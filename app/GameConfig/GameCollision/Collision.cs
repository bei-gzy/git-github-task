using App.GameCore;

namespace App.GameCollision;

public class CollisionArgs(WorldState state)
{
	public WorldState WorldState { get; init; } = state;
}

public class Collision()
{
	public event EventHandler<CollisionArgs>? CollisionWhat;

	//触发事件
	protected virtual void TouchCollision(CollisionArgs ecollision) => CollisionWhat?.Invoke(this, ecollision);
}

public class CollisionController(Collision collision) : IDisposable
{
	void CollisionWhat(object? sender, CollisionArgs e)
	{
	}

	public void Dispose() => collision.CollisionWhat -= CollisionWhat;
}
