using App.Config;
using App.Game.Register;
using App.Game.Screen;
using App.GameStates;
using App.NetWork;

var gameSettings = GameSettingLoader.Load();
var gameRunningState = new GameState();

var netOptions = NetOptions.Parse(args, gameSettings.NetWorkConfig.RoomId);//args是运行时传进的参数
if (netOptions.NetMode is not NetMode.None)
{
	await NetHost.RunAsync(netOptions);

	//观战/测连通跑完就退出；主机要继续进游戏（开局时 Attach 启动快照泵）
	if (netOptions.NetMode is not NetMode.Host)
		return;

	//主机也没连上 broker：别进游戏，否则快照根本发不出去
	if (!NetHost.IsConnected)
		return;
}

while (gameRunningState.GameRunningState is GameState.GameOnAndOffState.GameOn)
{
	//登录/创建角色
	await GameRegisterSystem.UseRegister();

	//大厅：开始游戏、设置菜单、退出
	await HomeScreen.ScreenFuncSystem(gameRunningState, gameSettings);
}