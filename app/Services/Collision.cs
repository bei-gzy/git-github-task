namespace app.Services;
using app.Models;

class Collision 
{   
   public bool CheckWallCollision(SnakeHead snake,Map map) => map.IsWall(snake.Head.X,snake.Head.Y);

    public bool CheckSnakeCollision(SnakeHead snake) => snake.Snake.Contains(snake.Head); 

    public bool CheckFoodCollision(SnakeHead snake,Map map) => map.IsFood(snake.Head.X,snake.Head.Y);
}