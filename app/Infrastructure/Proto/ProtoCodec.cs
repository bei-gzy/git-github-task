using App.GameCore.Food;
using App.GameCore;
using App.Protocol;

using DomainPoint = App.GameCore.Models.Point;
using DomainDirection = App.GameCore.Models.Direction;
using DomainSnake = App.GameCore.SnakeState;
using DomainPlayer = App.GameCore.PlayerState;

namespace App.Infrastructure.Proto;

public static class ProtoCodec
{
	//输入（来自客户端）
	public static Protocol.PlayerInput ToProtoInput(NetOptions no, bool isDash, DomainDirection dir, ulong clientTick) => new()
	{
		RoomId = no.Room,
		PlayerId = no.Name,
		ClientTick = clientTick,
		UseDash = isDash,
		Direction = ToProtoDirection(dir)
	};

	//服务器发
	public static GameState ToProtoState(WorldState w, string roomId)
	{
		var msg = new GameState
		{
			RoomId = roomId,
			ServerTick = w.Tick,
			MaxTick = w.MaxTick,
			Width = w.Width,
			Height = w.Height,
			FoodNum = w.FoodNum,
			IsOverTime = w.OverTime
		};

		msg.Players.AddRange(w.Players.Select(p =>
		{
			var player = new Protocol.PlayerState
			{
				PlayerId = p.Id,
				Name = p.Name,
				Score = p.Score,
				SurvivalTime = p.SurvivalTime,
				Snake = new Protocol.SnakeState
				{
					Direction = ToProtoDirection(p.Snake.Direction),
					IsAlive = p.Snake.IsAlive

				}
			};
			player.Snake.Body.AddRange(p.Snake.Body.Select(s => new Point { X = s.X, Y = s.Y }));
			return player;
		}));

		msg.Foods.AddRange(w.Foods.Select(f => new FoodState
		{
			Position = new Point { X = f.Position.X, Y = f.Position.Y },
			FoodType = f.Type
		}));

		return msg;
	}

	//客户端收
	public static WorldState FromProtoState(GameState s)
	{
		var players = s.Players.Select(p => new DomainPlayer(
			p.PlayerId,
			p.Name,
			new DomainSnake(
				[.. p.Snake.Body.Select(pt => new DomainPoint(
					pt.X,
					pt.Y
				))],
				FromProtoDirection(p.Snake.Direction),
				p.Snake.IsAlive
			),
			p.Score,
			p.SurvivalTime
		)).ToList();

		var foods = s.Foods.Select(f => new FoodItem
		{
			Position = new DomainPoint(f.Position.X, f.Position.Y),
			Type = f.FoodType
		}).ToList();

		return new WorldState(
			s.ServerTick,
			s.MaxTick,
			s.Width,
			s.Height,
			players,
			foods,
			s.FoodNum
		);
	}

	static Direction ToProtoDirection(DomainDirection d) => d switch
	{
		DomainDirection.Up => Direction.Up,
		DomainDirection.Down => Direction.Down,
		DomainDirection.Left => Direction.Left,
		DomainDirection.Right => Direction.Right,
		_ => Direction.Unspecified
	};

	static DomainDirection FromProtoDirection(Direction d) => d switch
	{
		Direction.Up => DomainDirection.Up,
		Direction.Down => DomainDirection.Down,
		Direction.Left => DomainDirection.Left,
		Direction.Right => DomainDirection.Right,
		_ => DomainDirection.Down
	};
}
