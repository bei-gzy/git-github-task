namespace App.NetWork;

//--host / --join / --ping / --broker host[:port] / --room / --name / --help
//解析命令行参数

public enum NetMode { None, Ping, Host, Client }

public sealed class NetOptions
{
	//NetMode -- BrokerHost -- BrokerPort -- Room -- Name
	public NetMode NetMode { get; set; } = NetMode.None;
	public string BrokerHost { get; set; } = "broker.emqx.io";
	public int BrokerPort { get; set; } = 1883;
	public string Room { get; set; } = "snake-dev";
	public string Name { get; set; } = "player";

	public static NetOptions Parse(string[] args, string defaultRoom)//Parse解析
	{
		var no = new NetOptions { Room = defaultRoom };
		for (int i = 0; i < args.Length; i++)
		{
			switch (args[i])
			{
				case "--ping":
					no.NetMode = NetMode.Ping;
					break;
				case "--host":
					no.NetMode = NetMode.Host;
					break;
				case "--join":
					no.NetMode = NetMode.Client;
					break;
				case "--broker":
					string[] m = args[++i].Split(":", 2);
					no.BrokerHost = m[0];
					if (m.Length > 1 && int.TryParse(m[1], out int bp)) no.BrokerPort = bp;
					break;
				case "--room":
					no.Room = args[++i];
					break;
				case "--name":
					no.Name = args[++i];
					break;
			}
		}

		return no;
	}

	public void ReSet()
	{
		NetMode = NetMode.None;
		BrokerHost = "broker.emqx.io";
		BrokerPort = 1883;
		Room = "snake-dev";
		Name = "player";
	}
}

// "用法：",
// "  dotnet run                                       # 正常游戏",
// "  dotnet run -- --host --room 房间号               # 主机",
// "  dotnet run -- --join --room 房间号 --name 名字   # 客户端",
// "  dotnet run -- --ping --room 房间号               # 只测连通",
// "",
// "参数：",
// "  --broker broker.emqx.io[:1883]   不写端口用默认 1883",
// "  --room   snake-abc123            默认取 Config/GameSettings.json 的 RoomId",
// "  --name   张三",
// "",
// "注意:dotnet run 后面必须先写 --，否则参数会被 dotnet 自己吃掉。"
