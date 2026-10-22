using App.Models;
using App.Services;
using App.GameScore;
using App.GameCollision;
using App.KeyBoard;
using App.GameTime;

namespace App.Game.Screen;

public class GamePrepareArgs
{
	//计分
	public Score Score { get; } = new Score();
	public PlayerScore PlayerScore { get; } = new PlayerScore();

	public static State.Direction Direction { get; } = State.Direction.Down;
	public GameMap GameMap { get; } = new GameMap();
	public SnakeHead Snake { get; } = new SnakeHead();

	//蛇移动
	public static Button Button { get; } = new Button();
	public KeyBoardController KeyBoardController { get; } = new KeyBoardController(Button, Direction);

	//碰撞检测
	public static Collision Collision { get; } = new Collision();
	public CollisionController CollisionController { get; } = new CollisionController(Collision);

	//时间
	public GameTimes Time { get; } = new GameTimes();
}

// public class GamePrepare
// {
// 	public event EventHandler<GamePrepareArgs>? GameArgs;

// 	protected virtual void OnGamePrepare(GamePrepareArgs gamePrepareArgs) => GameArgs?.Invoke(this, gamePrepareArgs);
// }

