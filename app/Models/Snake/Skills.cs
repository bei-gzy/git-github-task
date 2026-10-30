using App.Services;
using App.World;

namespace App.Models;

//冲刺技能，冷却时间，E和0键，多人对打
public class Skills
{
	const double CooldownSeconds = 5.0;

	double lastDashAt = double.NegativeInfinity;

	public void Reset() => lastDashAt = double.NegativeInfinity;

	public void MoveMode(GameWorld world, Player p)
	{
		if (!p.IsAlive) return;

		double now = world.ElapsedTime.TotalSeconds;
		bool dashReady = now - lastDashAt >= CooldownSeconds;

		if (p.IsTouchButton && dashReady)
		{
			p.IsTouchButton = false;
			lastDashAt = now;
			Move(p);
			Move(p);//冲刺这一帧连走两格
			return;
		}

		p.IsTouchButton = false;
		Move(p);
	}

	static void Move(Player p)
	{
		var head = p.MSnake.Head;
		var nextLocation = p.MSnake.Nextdir switch
		{
			State.Direction.Up => new State.Point(head.X, head.Y - 1),
			State.Direction.Down => new State.Point(head.X, head.Y + 1),
			State.Direction.Left => new State.Point(head.X - 1, head.Y),
			State.Direction.Right => new State.Point(head.X + 1, head.Y),
			_ => head//不会走到这一步
		};
		p.MSnake.Snake.Add(nextLocation);
	}
}
