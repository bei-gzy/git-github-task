using App.World;

namespace App.Models;

//冲刺技能，冷却时间，E和0键，多人对打
public class Skills
{
	const double CooldownSeconds = 5.0;

	double lastDashAt = double.NegativeInfinity;//负无穷，保证冷却已经结束

	public void Reset() => lastDashAt = double.NegativeInfinity;

	public void MoveMode(GameWorld world, Player p)
	{
		if (!p.IsAlive) return;

		if (p.IsTouchButton && world.ElapsedTime.TotalSeconds - lastDashAt >= CooldownSeconds)
		{
			p.IsTouchButton = false;
			lastDashAt = world.ElapsedTime.TotalSeconds;
			for (int i = 0; i < 2; i++)
			{
				Move(p);
			}

			return;
		}

		p.IsTouchButton = false;
		Move(p);
	}
}
