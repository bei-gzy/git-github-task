using App.Models;
namespace App.Services;

class Collision
{
	public static bool CheckWallCollision(SnakeHead snake, Map map) => map.IsWall(snake.Head.X, snake.Head.Y);

	public static bool CheckSnakeCollision(SnakeHead snake) => snake.Snake.Take(snake.Snake.Count - 1).Contains(snake.Head);

	public static bool CheckFoodCollision(SnakeHead snake, Map map) => map.IsFood(snake.Head.X, snake.Head.Y);
}
