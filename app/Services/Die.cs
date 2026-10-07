namespace app.Services;

class DefeatSystem()
{
    public static void Defeat()
    {
        Console.Clear();
        Console.ResetColor();
        
        int score = new State{}.Score;

        string[] DefeatMessage =
        {
            "Gaming Over!",
            "得分: " + score,
            "按下空格键重新开始游戏！",
            "按下Esc键退出游戏!"
        };
        //空格
        //delete
    }
    //是否复活--待开发
}