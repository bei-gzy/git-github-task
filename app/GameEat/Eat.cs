// using App.Models;
// namespace App.GameEat;

// public class Eat
// {
// 	public static Action<int>? EatWhat;

// 	public static int? LogEat() => EatWhat is not null ? (EatWhat?.Invoke()) : 0;
// }

// public class EatSystem
// {
// 	public static void LogEatMethod(int getScore, SnakeHead snake) => getScore = GameMap.FoodType(snake.Head.X, snake.Head.Y);

// 	public static void DoEatSystem() => Eat.EatWhat += EatSystem.LogEatMethod();

// 	public static void CancelEatSystem() => Eat.EatWhat -= EatSystem.LogEatMethod();

// }
