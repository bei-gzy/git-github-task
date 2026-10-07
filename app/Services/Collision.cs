namespace app.Services;
using app.Models;

class Collsion 
{   
    public bool CheckCollision(SnakeHead snake)
    {    
        var snakeHead = new string[snake.Head.X, snake.Head.Y];
        if (snakeHead[snake.Head.X, snake.Head.Y] == "■")
        {   
            DefeatSystem.Defeat();
            return true;
        }
        else if(snakeHead[snake.Head.X, snake.Head.Y] == "✰")
        {
            snakeHead[snake.Head.X, snake.Head.Y] = " "; 
            return true;
        }

        return false;
    }
}