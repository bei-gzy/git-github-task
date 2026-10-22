using App.Game.Register;
using App.Interface.State;
using App.Game.Screen;

//program <- Snake <- Services

var gameRunningState = new GameState();
while (gameRunningState.GameRunningState is GameState.GameOnAndOffState.GameOn)
{
	//时山--登陆创建角色
	await GameRegisterSystem.UseRegister();

	//游戏大厅界面，选择模式，调整设置，退出游戏
	await HomeScreen.ScreenFuncSystem(gameRunningState);

}

