using app.Services;

namespace app.Models;

class Map
{
    public const int width = 25;

    public const int height = 30;

    //数组
    private readonly string[,] map = new string[height,width];//[,]表示二维数组，几个“，”表示几纬
    private readonly Random food = new();

    private  static bool IsBorder(int x,int y) => x == 0 || y == 0 || x == width - 1 || y == height - 1; 

    private void InitMap()
    {
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                map[i,j] = IsBorder(j,i) ? "■" : " "; 
            }
        }
    }

    private void GenerateFood()
    {
        int x,y; 
        do
        {
            x = food.Next(width);
            y = food.Next(height);
        } while (!IsWall(x,y) && map[x,y] != " ");
        map[x,y] = "✰";
    }

    public Map()
    {
        InitMap();
        GenerateFood();
    }

    public bool IsWall(int x,int y) => x < 0 || y < 0 || x >= width || y >= height || map[x,y] == "■";

    public bool IsFood(int x,int y) => x > 0 || y >0 || x < width || y < height || map[x,y] == "✰";
 
    //打印地图
    public void PrintMap(SnakeHead snake)
    {
        var body = snake.Snake.ToHashSet();
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if(snake.Head.X == j && snake.Head.Y == i)
                    Console.Write("☻");
                else if(body.Contains(new State.Point(j,i)))
                    Console.Write("◊");
                else
                Console.Write(map[i,j]);
            }
            Console.WriteLine(); 
        }
    }
}