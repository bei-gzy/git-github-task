using App.GameCore.Models;

namespace App.GameCore;

//决策
public static class CollisionRules
{
	public static bool HitWall(WorldState state, Point head) => state.IsWall(head.X, head.Y);

	public static bool HitBody(WorldState oldstate, Point newhead, string playerId) => oldstate.Players.Any(
		p => p.Snake.IsAlive && (
			p.Id == playerId
				? p.Snake.Body.Take(p.Snake.Body.Count - 1).Contains(newhead)
				: p.Snake.Body.Contains(newhead)));//身体是动的，得由新碰旧判断
}
