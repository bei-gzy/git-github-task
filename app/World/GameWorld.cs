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
		random = new Random();
		GameDurationSeconds = gameSettings.GameDurationSeconds;
		TickMilliseconds = gameSettings.TickMilliseconds;
		Width = gameSettings.MapConfig.GameMapWidth;
		Height = gameSettings.MapConfig.GameMapHeight;

		BuildWalls();
		FillFood();
	}

	//地图
	bool[,] Walls { get; set; } = new bool[0, 0];

	public int Width { get; private set; }

	public int Height { get; private set; }

	/// <summary>菜单里改地图尺寸用；会重建围墙并清空食物</summary>
	public void Resize(int width, int height)
	{
		Width = Math.Max(10, width);
		Height = Math.Max(10, height);
		BuildWalls();
		Foods.Clear();
		FillFood();
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

	public bool IsWall(int x, int y) => x < 0 || y < 0 || x >= Width || y >= Height || Walls[y, x];

	public void PrintMap()
	{
		if (LocalPlayer is null) return;
		MapPrinter.PrintMap(this, LocalPlayer.MSnake);
	}
	//食物
	List<FoodItem> Foods { get; } = [];

	public int FoodTarget => gameSettings.FoodConfig.GenerateFoodCount;

	public int FoodNum => Foods.Count;

	public FoodItem? FoodAt(int x, int y)
	{
		foreach (var food in Foods)
		{
			if (food.Position.X == x && food.Position.Y == y)
				return food;
		}

		return null;
	}

	public void UpdateFood(TimeSpan delta)//输入时间变化值
	{
		timeLose += delta;
		if (timeLose.TotalSeconds < gameSettings.FoodConfig.GenerateFoodSeconds)
			return;
		timeLose = TimeSpan.Zero;
		FillFood();
	}

	void FillFood()
	{
		while (Foods.Count < FoodTarget)
		{
			int before = Foods.Count;
			GenerateFood();
			if (Foods.Count == before) return;//地图塞不下了，别死循环
		}
	}

	public void GenerateFood()
	{
		for (int guard = 0; guard < Width * Height; guard++)//总次数
		{
			int x = random.Next(1, Math.Max(2, Width - 1));
			int y = random.Next(1, Math.Max(2, Height - 1));
			if (IsWall(x, y) || FoodAt(x, y) is not null)
				continue;
			Foods.Add(new FoodItem(new State.Point(x, y), 1));
			return;
		}
	}

	public void RemoveFood(int x, int y) => Foods.RemoveAll(food => food.Position.X == x && food.Position.Y == y);

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

	/// <summary>开新一局前清空玩家，由 GamePrepareArgs 重新注册当前账号</summary>
	public void ResetPlayers()
	{
		players.Clear();
		LocalPlayer = null;
	}

	//状态
	public bool IsOver { get; private set; }

	public void MarkGameOver() => IsOver = true;

	public bool QuitRequired { get; private set; }

	public void QuitGame() => QuitRequired = true;

	public ulong Tick { get; private set; }

	public void Restart()
	{
		timeLose = TimeSpan.Zero;
		ElapsedTime = TimeSpan.Zero;
		Tick = 0;
		IsOver = false;
		QuitRequired = false;

		foreach (var player in players)
			player.ResetForNewGame();

		Foods.Clear();
		FillFood();
	}

	//只读快照：给渲染器，以后也给网络层
	public WorldSnapShot Snapshot()
	{
		var playerSnapshots = new List<PlayerSnapShot>(players.Count);
		foreach (var player in players)
		{
			playerSnapshots.Add(new PlayerSnapShot(
				player.Id,
				player.Name ?? "",
				[.. player.MSnake.Snake],
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
