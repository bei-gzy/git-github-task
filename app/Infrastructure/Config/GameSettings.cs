namespace App.Config;

//配置模型
public sealed class GameSettings
{
	public double GameDurationSeconds { get; init; } = 60.0;
	public int TickMilliseconds { get; init; } = 200;
	public MapConfig MapConfig { get; init; } = new();
	public FoodConfig FoodConfig { get; init; } = new();
	public NetWorkConfig NetWorkConfig { get; init; } = new();
	public List<SkillConfig> SkillConfig { get; init; } = [];
}

public sealed class MapConfig
{
	public int GameMapWidth { get; init; } = 25;
	public int GameMapHeight { get; init; } = 30;
}

public sealed class FoodConfig
{
	public double GenerateFoodSeconds { get; init; } = 5.0;
	public int GenerateFoodCount { get; init; } = 3;
}

public sealed class NetWorkConfig//MQTT通讯
{
	public string BrokerHost { get; init; } = "localhost";
	public int BrokerPort { get; init; } = 1883;
	public string BrokerUsername { get; init; } = "player";
	public string RoomId { get; init; } = "room1";
}

public sealed class SkillConfig
{
	public string SkillName { get; init; } = "dash";
	public double CoolDownSeconds { get; init; } = 5.0;
	public double DurationSeconds { get; init; } = 4.0;
}
