using App.Config;
using App.Game.Register;
using App.Game.Screen;
using App.GameStates;

var gameSettings = GameSettingLoader.Load();
var gameRunningState = new GameState();

while (gameRunningState.GameRunningState is GameState.GameOnAndOffState.GameOn)
{
	//登录/创建角色
	await GameRegisterSystem.UseRegister();

	//大厅：开始游戏、设置菜单、退出
	await HomeScreen.ScreenFuncSystem(gameRunningState, gameSettings);
}

