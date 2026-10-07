namespace app.Models;

public class Map
{
    //打印地图
    public static void PrintMap()
    {
        const int width = 25;
        const int height = 30;
        //数组
        string[,] map = new string[width,height];//[,]表示二维数组，几个“，”表示几纬
        Console.WriteLine("——————————————Weclome TO Snake Eat Game——————————————");
        Console.WriteLine("法则:WASD控制方向,按Esc退出游戏");
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if(i == 0 || i == height -1)
                {
                    map[i,j] = "■";
                }
                else if(j == 0 || j == width -1)
                {
                    map[i,j] = "■";
                }
                else
                {
                    map[i,j] = " ";
                }
            }
        }
    }
}