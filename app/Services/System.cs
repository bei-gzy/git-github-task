using App.Models;
using App.GameScore;
using App.GameCollision;
using App.KeyBoard;

namespace App.Services;

public class FuncSystem
{
	public static async Task<bool> GameRunSystemAsync(SnakeHead snake, KeyBoardController keyBoardController, Button button, State.Direction dir, CollisionController collisionController, Collision collision, PlayerScore playerscore, GameMap map, Score score)
	{
		while (!State.IsGameOver)
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

			collision.TouchCollision(collision, snake, playerscore, map, score);

			await Task.Delay(150);
		}

		keyBoardController.CancleClicked();
		collisionController.CancelCollision();

		return State.IsGameOver = true;
	}
}
