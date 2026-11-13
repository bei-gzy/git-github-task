using App.Config;
using App.Services;

namespace App.World;

public sealed class GameWorld
{
	readonly GameSettings gameSettings;

	readonly Random random;

	TimeSpan timeLose = TimeSpan.Zero;

	public GameWorld(GameSettings gameSettings)
	{
		this.gameSettings = gameSettings;
		random = new Random(timeLose.Milliseconds);
		GameDurationSeconds = gameSettings.GameDurationSeconds;
		TickMilliseconds = gameSettings.TickMilliseconds;
		Width = gameSettings.MapConfig.GameMapWidth;
		Height = gameSettings.MapConfig.GameMapHeight;

		BuildWalls();
		GenerateFood();
	}

	//地图 0 1地图逻辑
	bool[,] Walls { get; set; } = new bool[0, 0];

	public int Width { get; private set; }

	public int Height { get; private set; }

	public void Resize(int width, int height)
	{
		Width = Math.Max(10, width);
		Height = Math.Max(10, height);
		BuildWalls();
		Foods.Clear();
		GenerateFood();
	}

	void BuildWalls()
	{
		Walls = new bool[Height, Width];
		for (int y = 0; y < Height; y++)
		{
			for (int x = 0; x < Width; x++)
			{
				Walls[y, x] = x == 0 || x == Width - 1 || y == 0 || y == Height - 1;//初始化这些是true
			}
		}
	}

	//食物
	public void UpdateFood(TimeSpan delta)//输入时间变化值
	{
		timeLose += delta;
		if (timeLose.TotalSeconds < gameSettings.FoodConfig.GenerateFoodSeconds)
			return;
		timeLose = TimeSpan.Zero;
	}

	//时间
	public TimeSpan ElapsedTime { get; private set; }

	public double GameDurationSeconds { get; set; }

	public int TickMilliseconds { get; set; }

	public void UpdateTime(TimeSpan delta)
	{
		ElapsedTime += delta;
		Tick++;
		if (ElapsedTime.TotalSeconds >= GameDurationSeconds)
			IsOver = true;
	}

	//玩家
	readonly List<Player> players = [];

	public IReadOnlyList<Player> Players => players;

	public Player? LocalPlayer { get; private set; }

	public Player AddPlayer(Player player, bool isLocal = false)
	{
		if (FindPlayer(player.Id) is not null)
			throw new InvalidOperationException("玩家ID重复: " + player.Id);

		players.Add(player);
		if (isLocal)
			LocalPlayer = player;

		return player;
	}

	public Player? FindPlayer(string id) => players.FirstOrDefault(p => p.Id == id);

	//锁
	public object Lock { get; } = new();

	public void Restart()
	{
		timeLose = TimeSpan.Zero;
		ElapsedTime = TimeSpan.Zero;
		Tick = 0;
		IsOver = false;
		QuitRequired = false;
		foreach (var player in players)
			player.ResetForNewGame();
		players.Clear();
		LocalPlayer = null;
		Foods.Clear();
		GenerateFood();
	}

	//只读快照
	public WorldSnapShot Snapshot()
	{
		lock (Lock)
		{
			var playerSnapshots = new List<PlayerSnapShot>(players.Count);
			foreach (var player in players)
			{
				playerSnapshots.Add(new PlayerSnapShot(
					player.Id,
					player.Name ?? "",
					[.. player.MSnake.Bodies],
					player.MScore.MyScore,
					player.IsAlive,
					player.MSnake.Nextdir));
			}

			var foodSnapshots = new List<FoodSnapShot>(Foods.Count);
			foreach (var food in Foods)
				foodSnapshots.Add(new FoodSnapShot(food.Position, food.Type));

			return new WorldSnapShot(
				Tick,
				Width,
				Height,
				playerSnapshots,
				foodSnapshots,
				Math.Max(0, GameDurationSeconds - ElapsedTime.TotalSeconds),
				IsOver);
		}
	}
}
