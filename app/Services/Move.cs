namespace app.Services;
using app.Models;

class Operation()
{   
    // 移动操作
    public void UncontrolMove(int x, int y,string dirction)
    {
        //一开始默认向上？  

        //传入移动前的位置方向

    }

    public void WMove(int X, int Y)
    {
        State.Point p = new State.Point(X, Y-1);
        
    }   

    public void SMove(int X, int Y)
    {
        
    }

    public void DMove(int X, int Y) 
    {
        
    }

    public void AMove(int X, int Y)
    {
        
    }

    // 固定速度
    public static double Speed() => 1.0;
}