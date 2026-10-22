using App.Models;
using App.GameScore;
using App.GameCollision;
using App.KeyBoard;
using App.GameTime;

namespace App.Services;

public class FuncSystem
{
	public static async Task<bool> GameRunSystemAsync(GameTimes time, SnakeHead snake, KeyBoardController keyBoardController, Button button, State.Direction dir, CollisionController collisionController, Collision collision, PlayerScore playerscore, GameMap map, Score score)
	{
		time.Start();

		while (!State.IsGameOver && time.IsPauseTime)
		{
			Console.Clear();
			Console.WriteLine("——————————————Weclome TO Snake Eat Game——————————————");
			Console.WriteLine("法则:WASD控制方向,按Esc退出游戏");
			GameMap.PrintMap(snake);

			if (Console.KeyAvailable)
			{
				button.TouchClick(Console.ReadKey(intercept: true));
			}

			if (keyBoardController.ExitState == State.GamingState.Exite)
			{
				break;
			}

			snake.Move(keyBoardController.Direction);

			collision.TouchCollision(snake, playerscore, map, score);

			await Task.Delay(200);

			//时间截至
			await
		}

		keyBoardController.CancleClicked();
		collisionController.CancelCollision();

		return State.IsGameOver = true;
	}
}
