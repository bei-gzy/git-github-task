using App.Services;

namespace App.World;

public sealed record WorldSnapShot
(
	ulong Tick,
	int Width,
	int Height,
	IReadOnlyList<PlayerSnapShot> Players,
	IReadOnlyList<FoodSnapShot> Foods,
	double RemainingSeconds,
	bool IsOver
);

public sealed record PlayerSnapShot
(
	string Id,
	string Name,
	IReadOnlyList<State.Point> Segments,
	int Score,
	bool IsAlive,
	State.Direction Direction
);

public sealed record FoodSnapShot
(
	State.Point Position, int Type
);
