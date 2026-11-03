using App.Protocol;
using App.Services;
using App.World;

namespace App.NetWork;

public static class ProtoCodec
{
	public static GameState ToProtoState(WorldSnapShot gss, string roomId)
	{
		var msg = new GameState
		{
			RoomId = roomId,
			ServerTick = gss.Tick,
			Width = gss.Width,
			Height = gss.Height,
			RemainingSeconds = gss.RemainingSeconds,
			IsGameOver = gss.IsOver
		};

		msg.Players.AddRange(gss.Players.Select(p =>
		{
			var player = new PlayerState
			{
				PlayerId = p.Id,
				Name = p.Name,
				Score = p.Score,
				IsAlive = p.IsAlive,
				Direction = ToProtoDirection(p.Direction)
			};

			player.SnakeSegments.AddRange(p.Segments.Select(s => new Point { X = s.X, Y = s.Y }));
			return player;
		}));

		msg.Foods.AddRange(gss.Foods.Select(f => new FoodState
		{
			Position = new Point { X = f.Position.X, Y = f.Position.Y },
			FoodType = f.Type
		}));

		return msg;
	}

	public static WorldSnapShot FromProtoState(GameState s) => new(
		s.ServerTick,
		s.Width,
		s.Height,
		[.. s.Players.Select(p => new PlayerSnapShot(
			p.PlayerId,
			p.Name,
			[.. p.SnakeSegments.Select(pt => new State.Point(pt.X, pt.Y))],
			p.Score,
			p.IsAlive,
			FromProtoDirection(p.Direction)))],
		[.. s.Foods.Select(f => new FoodSnapShot(
			new State.Point(f.Position.X, f.Position.Y),
			f.FoodType))],
		s.RemainingSeconds,
		s.IsGameOver);

	static Direction ToProtoDirection(State.Direction d) => d switch
	{
		State.Direction.Up => Direction.Up,
		State.Direction.Down => Direction.Down,
		State.Direction.Left => Direction.Left,
		State.Direction.Right => Direction.Right,
		_ => Direction.Unspecified
	};

	static State.Direction FromProtoDirection(Direction d) => d switch
	{
		Direction.Up => State.Direction.Up,
		Direction.Down => State.Direction.Down,
		Direction.Left => State.Direction.Left,
		Direction.Right => State.Direction.Right,
		_ => State.Direction.Down
	};
}
