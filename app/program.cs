//1.地图上的豆的随机性问题
//2.蛇的连续移动问题，如何保持身体沿着头的路径--身头分别封装
//3.蛇的固定移动速度
using app.Models;
using app.Services;

//program -> Snake -> Services

string? sname;
Console.WriteLine("Please set your snake's name: ");
while (true)
{
    sname = Console.ReadLine();
    if(string.IsNullOrEmpty(sname))
    {
        Console.WriteLine("That 's name is empty,please try again: ");
        continue;
    }
    else
    {
        break;
    }
}

State.GamingState state = State.GamingState.GamingStateRunning;
var map = new Map();
var collision = new Collision(); 
State.Direction dir = State.Direction.Up;
State score = new State();

while (state == State.GamingState.GamingStateRunning)
{
    Console.WriteLine("——————————————Weclome TO Snake Eat Game——————————————");
    Console.WriteLine("法则:WASD控制方向,按Esc退出游戏");
    var snake = new SnakeHead()
    {
        //头是1，吃一豆加一个身体单位，记号加一
        HId = 0,
        Name = sname,
    };
    map.PrintMap(snake);


    while(Console.ReadKey(intercept: true).Key != ConsoleKey.Escape)
    {
        State.Direction nextdir = dir;
        ConsoleKeyInfo key = Console.ReadKey(intercept: true); 
        nextdir = key.Key switch
        {
            ConsoleKey.W when dir != State.Direction.Down => State.Direction.Up,
            ConsoleKey.S when dir != State.Direction.Up => State.Direction.Down,
            ConsoleKey.A when dir != State.Direction.Right => State.Direction.Left,
            ConsoleKey.D when dir != State.Direction.Left => State.Direction.Right,
            _ => dir
        };
        dir = nextdir;
        
        snake.Move(nextdir);

        if(collision.CheckWallCollision(snake,map))
        {
            state = State.GamingState.GamingGameover;
            DefeatSystem.Defeat(score);
            if(Console.ReadKey(intercept: true).Key == ConsoleKey.Spacebar)
            {
                state = State.GamingState.GamingStateRunning;
            }
            break;
        }
        if(collision.CheckSnakeCollision(snake))
        {
            state = State.GamingState.GamingGameover;
            DefeatSystem.Defeat(score);
            if(Console.ReadKey(intercept: true).Key == ConsoleKey.Spacebar)
            {
                state = State.GamingState.GamingStateRunning;
            }
            break;
        }
        if(collision.CheckFoodCollision(snake,map))
        {
            score.SetScore(score.GetScore() + 1);
        }
    }
    Console.Clear();
    Thread.Sleep(200);
}


















// 只有普通语句 → 生成 void Main()
// 含 await → 生成 Task Main()（由你剪贴板里第一段话可知）
// 含 return → 生成 int Main()，即退出码（剪贴板第二段）
// 同时含 await 和 return → Task<int> Main()