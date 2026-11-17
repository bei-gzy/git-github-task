using App.GameCore.Models;

namespace App.GameCore;

//决策
public static class CollisionRules
{
	public static HitResult HitTest(WorldState state, Point newhead, string playerId)
	{
		if (state.IsWall(newhead.X, newhead.Y)) return HitResult.Wall;
		foreach (var p in state.Players)
		{
			bool itSelf = p.Id == playerId;
			var body = itSelf ? p.Snake.Body.Take(p.Snake.Body.Count - 1) : p.Snake.Body;
			if (body.Contains(newhead)) return itSelf ? HitResult.Self : HitResult.Other;
		}

		return HitResult.None;
	}
}

// public static bool HitWall(WorldState state, Point head) => state.IsWall(head.X, head.Y);

// public static bool HitBody(WorldState oldstate, Point newhead, string playerId) => oldstate.Players.Any(
// 	p => p.Snake.IsAlive && (
// 		p.Id == playerId
// 			? p.Snake.Body.Take(p.Snake.Body.Count - 1).Contains(newhead)
// 			: p.Snake.Body.Contains(newhead)));//身体是动的，得由新碰旧判断
