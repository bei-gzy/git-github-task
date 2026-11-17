using App.Config;
using App.Interface;
using App.NetWork;
using App.Rendering;

var gameSettings = GameSettingLoader.Load();
var gameRunningState = new GameState();
NetSession? session = null;

try
{
	//登录/创建角色
	await GameRegisterSystem.UseRegister();
	var netOptions = NetOptions.Parse(args, gameSettings.NetWorkConfig.RoomId);//args是运行时传进的参数
	IRenderer renderer = new ConsoleRenderer(Console.Out);
	IRoundResultView resultView = new ConsoleResultView(Console.Out);
	if (netOptions.NetMode is not NetMode.None)
	{
		session = await NetSession.StartAsync(netOptions);
		if (session is null) return;
		if (netOptions.NetMode is not NetMode.Host)
		{
			await NetLoop.RunAsync(session, renderer, ClientReadKey.Poll);
			return;
		}
	}

	while (gameRunningState.GameRunningState is GameState.GameOnAndOffState.GameOn)
		await HomeScreen.ScreenFuncSystem(gameRunningState, gameSettings, session, renderer, resultView);
}
finally
{
	try
	{
		if (session is not null)
			await session.DisposeAsync();
	}
	finally
	{
		Console.ResetColor();
	}
}

public static async Task<NetSession?> StartAsync(NetOptions no)//通过异步完成初始化并完成连接和订阅
{
	//房间名 + “-” + 32位无连字符GUID  ：N不带连字符   表达时 ： [..18]范围运算符，也叫格式
	var mt = new MqttTransport($"{no.Room}-{Guid.NewGuid().ToString("N")[..18]}")
	{
		Log = Console.WriteLine
	};
	if (!await mt.ConnectAsync(no.BrokerHost, no.BrokerPort)) return null;
	try
	{
		await mt.SubscribeAsync(default, $"snake/{no.Room}/#");
	}
	catch
	{
		await mt.DisposeAsync();
		return null;
	}

	return new NetSession(mt, no);
}

public static async Task RunAsync(NetSession ns, IRenderer renderer, Func<PlayerIntent?>? poll)
{
	if (ns is null) return;
	var no = ns.NetOptions;
	switch (no!.NetMode)
	{
		case NetMode.Ping: await RunPingAsync(ns, renderer); return;
		case NetMode.Client: await RunClientAsync(ns, renderer, poll); return;
		case NetMode.Host: break;
		default: break;
	}
}

/*
UI (Console)                入口 / 菜单 / 结算
   │
RoundRunner                 一局：输入 → 推进 → 渲染 → 快照
   ├─ IInputSource          本地键盘 / 远端网络（都产出 PlayerIntent）
   ├─ IRenderer             Console 渲染（MapPrinter）
   └─ ISnapshotSink         网络广播
   │
Domain                      GameWorld / Player / Snake / Rules（无 Console、无网络）
   │
Infrastructure              MQTT / ProtoCodec / Config
*/
