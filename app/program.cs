using App.Models;
using App.Services;
using App.GameScore;
using App.GameCollision;
using App.KeyBoard;

using Game.Register;
//using App.GameEat;

//program <- Snake <- Services
//异步的吃豆？

//时山--登陆创建角色
string? sname = " ";
var player = new Player(sname);
var gameRegister = new GameRegister();
var gameRegisterSystem = new GameRegisterSystem();
UseGameRegisterSystem.ToUseGameRegisterSystem(sname, player, gameRegister, gameRegisterSystem);

//计分
var score = new Score();
var playerscore = new PlayerScore();

var dir = State.Direction.Down;//当前方向
var map = new GameMap();
var snake = new SnakeHead();

//蛇移动
var button = new Button();
//var useDelegateVariable = new DelegateVariable(button, dir, state);
var keyBoardController = new KeyBoardController(button, dir);

//碰撞检测
var collision = new Collision();
// var collisionSystem = new CollisionSystem();
var collsionController = new CollisionController(collision);

if (await FuncSystem.GameRunSystemAsync(snake, keyBoardController, button, dir, collsionController, collision, playerscore, map, score))
{
	return;
}
