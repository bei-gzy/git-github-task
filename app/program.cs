using App.Config;
using App.Interface;
using App.Game.Screen;
using App.GameStates;
using App.NetWork;
using App.Rendering;
using App.Services;

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
			await NetLoop.RunAsync(session, renderer, async () => ClientReadKey.Poll());
			return;
		}
	}

	while (gameRunningState.GameRunningState is GameState.GameOnAndOffState.GameOn)
	{
		//大厅：开始游戏、设置菜单、退出
		await HomeScreen.ScreenFuncSystem(gameRunningState, gameSettings, session, renderer, resultView);
	}
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
