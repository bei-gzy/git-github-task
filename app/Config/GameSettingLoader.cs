using System.Text.Json;

namespace App.Config;

public static class GameSettingLoader
{
	//GameSettings总的配置初始
	static GameSettings? gameSettings;

	//立下规矩，按下列方式反序列化
	static readonly JsonSerializerOptions Options = new()
	{
		//属性名大小写匹配
		PropertyNameCaseInsensitive = true,
		//允许注释
		ReadCommentHandling = JsonCommentHandling.Skip,
		//允许尾逗号
		AllowTrailingCommas = true
	};

	public static GameSettings Load()
	{
		if (gameSettings is not null)
			return gameSettings;

		//拼路径
		string path = Path.Combine(AppContext.BaseDirectory, "Config", "GameSettings.json");
		//基本就是从/home/gwj/..../..../Config/GameSettings.json

		//存在与否--抛异常
		if (!File.Exists(path))
			throw new FileNotFoundException($"配置文件不存在：{path}");

		//把文本读进string
		string json = File.ReadAllText(path);

		//把字符串变成对象
		gameSettings = JsonSerializer.Deserialize<GameSettings>(json, Options) ?? throw new InvalidDataException("配置文件解析结果为空");

		Validate();
		return gameSettings;
	}

	static void Validate()
	{
		if (gameSettings?.TickMilliseconds < 50)
			throw new ArgumentException("TickMilliseconds不能小于50毫秒");
		if (gameSettings?.MapConfig.GameMapHeight < 10 || gameSettings?.MapConfig.GameMapWidth < 10)
			throw new ArgumentException("地图长度不能小于10");
		if (gameSettings?.FoodConfig.GenerateFoodCount < 0)
			throw new ArgumentException("生成食物数量不能小于0");
	}
}
