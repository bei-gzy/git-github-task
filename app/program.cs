using App.Application.UI;
using App.Infrastructure;
using App.Infrastructure.Config;
using App.InputAndOutput;

var gameSettings = GameSettingLoader.Load();
var netOptions = NetOptions.Parse(args, gameSettings.NetWorkConfig.RoomId);

IRenderer renderer = new ConsoleRenderer();
IRoundResultView resultView = new ConsoleResultView();
var menu = new MenuController(renderer, resultView, new Random(), netOptions);

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

try
{
	await menu.RunAsync(cts.Token);
}
finally
{
	Console.ResetColor();
}
/*
UI (Console)                入口 / 菜单 / 结算
   │
GameRunner                 一局：输入 → 推进 → 渲染 → 快照
   ├─ IInputSource          本地键盘 / 远端网络（都产出 PlayerIntent）
   ├─ IRenderer             Console 渲染（MapPrinter）
   └─ Mqtt                  网络广播
   │
Domain                      GameWorld / Player / Snake / Rules（无 Console、无网络）
   │
Infrastructure              MQTT / ProtoCodec / Config
*/
