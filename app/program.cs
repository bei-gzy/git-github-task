// using App.Application.UI;
// using App.Infrastructure;
// using App.Infrastructure.Config;
// using App.InputAndOutput;

// var gameSettings = GameSettingLoader.Load();
// var netOptions = NetOptions.Parse(args,
// 	gameSettings.NetWorkConfig.RoomId,
// 	gameSettings.NetWorkConfig.PlayerName,
// 	gameSettings.NetWorkConfig.BrokerHost,
// 	gameSettings.NetWorkConfig.BrokerPort);

// IRenderer renderer = new ConsoleRenderer();
// IRoundResultView resultView = new ConsoleResultView();
// var menu = new MenuController(renderer, resultView, new Random(), netOptions);

// using var cts = new CancellationTokenSource();
// Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

// try
// {
// 	await menu.RunAsync(cts.Token);
// }
// finally
// {
// 	Console.ResetColor();
// }
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
using R3;

class Program
{
	static void Main() { }

	// //开局弹射买弹
	// const int AmmoPrice = 20;//可变单价

	// public static Observable<Unit> CreatePurchaseTrigger(//组合的无内容信号Unit，触发使用unit.Default
	// 	Observable<GameState> stage,//状态
	// 	Observable<Zone> zone,//区域
	// 	Observable<int> currency) => Observable.CombineLatest(
	// 		stage,
	// 		zone,
	// 		currency,
	// 		(stage, zone, currency) =>
	// 		stage is GameState.Ready &&
	// 		zone is Zone.Resupply &&
	// 		currency >= AmmoPrice)
	// 			.Where(canPurChase => canPurChase)
	// 			.Select(_ => Unit.Default);

	// public enum GameState
	// {
	// 	Ready,
	// 	Started,
	// 	End
	// }

	// public enum Zone
	// {
	// 	Start,
	// 	Resupply,
	// 	Highland
	// }

	// const int Time = 1;

	// //大pao车命中Pitch
	// public static Observable<float> CreateCriticalPitchFlow(
	// Observable<int> ammoCount, // 剩余弹丸数量
	// Observable<float> pitch, // 当前pitch
	// Observable<Unit> onLob) => ammoCount//弹丸数量减少时推出发射，从pitch中采样出onlob，同时这个onlob得是顺时值；
	// 	.Pairwise()
	// 	.Where(p => p.Previous > p.Current)
	// 	.Select(_ => onLob.TakeUntil(Observable.Timer(TimeSpan.FromSeconds(Time))))//动态创建全新窗口，监听调用
	// 	.Switch()//只留新窗口
	// 	.WithLatestFrom(pitch, (_, pitch) => pitch);//吊射命中

	// //飞镖
	// public record Data
	// (
	// 	string StatusText,
	// 	int CountDown,//倒计时
	// 	int RemainingChances,
	// 	GameState State
	// );

	// public record Source
	// (
	// 	bool? GateOpened,
	// 	GameState? State,
	// 	bool Tick
	// );

	// //ready阶段的gateOpened全部舍弃;只记录比赛开始后30s和4min节点
	// public static (Observable<string> statusText, Observable<int> countDown, Observable<int> gateCount) CreateDartRelatedFlow(
	// 	Observable<bool> gateOpened,
	// 	Observable<GameState> state
	// )
	// {
	// 	//定期触发流 -- interval
	// 	var ticker = Observable.Interval(TimeSpan.FromSeconds(1), TimeProvider.System);

	// 	//闸门是否开启和比赛阶段变化，都会影响Data，相当于是采样多因素，With的Plus版本:Merge + Scan
	// 	var source = Observable.Merge(
	// 		gateOpened.Select(isOpen => new Source(isOpen, null, false)),
	// 		state.Select(stage => new Source(null, stage, false)),
	// 		ticker.Select(_ => new Source(null, null, true))
	// 	/*
	// 	(true, null)
	// 	(false, null)
	// 	(null, state.Started)
	// 	(null, state.Other)
	// 	*/
	// 	);

	// 	var open = source//状态机子
	// 		.Scan(new Data("比赛未开始", 0, 2, GameState.Ready), (data, input) =>
	// 		{
	// 			// return input switch
	// 			// {

	// 			// };

	// 			if (!input.Tick)
	// 			{
	// 				var currentState = input.State ?? data.State;
	// 				if (currentState is GameState.Ready) return data;

	// 				if (currentState is GameState.Started)
	// 				{
	// 					if (input.GateOpened is true && data.RemainingChances > 0)
	// 					{
	// 						return data with
	// 						{
	// 							State = currentState,
	// 							StatusText = "允许发射",
	// 							CountDown = 30,
	// 							RemainingChances = data.RemainingChances - 1
	// 						};
	// 					}

	// 					if (input.GateOpened is false && data.StatusText is not "冷却中")
	// 					{
	// 						return data with
	// 						{
	// 							State = currentState,
	// 							StatusText = "冷却中",
	// 							CountDown = 15
	// 						};
	// 					}
	// 				}

	// 				if (currentState is GameState.End) return data with { StatusText = "比赛结束", State = currentState };
	// 			}
	// 			else if (input.Tick && data.CountDown is 1)
	// 			{
	// 				return data with { StatusText = "闸门关闭", CountDown = data.CountDown - 1 };
	// 			}
	// 			else if (input.Tick && data.CountDown > 0)
	// 			{
	// 				return data with { CountDown = data.CountDown - 1 };
	// 			}

	// 			return data;
	// 		});

	// 	//监听启动流的结果
	// 	var statusText = open.Select(d => d.StatusText).DistinctUntilChanged();
	// 	var countDown = open.Select(d => d.CountDown).DistinctUntilChanged();
	// 	var gateCount = open.Select(d => d.RemainingChances).DistinctUntilChanged();

	// 	return (statusText, countDown, gateCount);
	// }

	//终端应用，组合技能 顺序读取按键
	public enum KeyInfo
	{
		Up,
		Down,
		Left,
		Right,
		A,
		B,
		C,
		E,
		F
	}

	public record ComboState
	(
		int MatchIndex,
		bool Triggered//记录是否触发的意思是按完8个
	);

	//Up、Down、Left、Right、A、B、A、B
	public static Observable<Unit> CreateComboFlow(
		Observable<KeyInfo> pressedKey,
		IReadOnlyList<KeyInfo> targetCombo) => pressedKey
			.Chunk(targetCombo.Count, 1)//1是每一下触发一次
			.Where(w => w.Length == targetCombo.Count && w.SequenceEqual(targetCombo))
			.Select(_ => Unit.Default);

	//打开菜单按钮 -- 长按和点按
	public enum MenuOperation { Open, Close, Confirm }

	public static Observable<MenuOperation> CreateOperationFlow(
		Observable<bool> pressed
	)
	{
		var press = pressed.Where(p => p);
		var release = pressed.Where(p => !p);
		var longPress = press
			.Select(_ => Observable.Timer(TimeSpan.FromSeconds(3)).TakeUntil(release))
			.Switch()
			.Select(_ => true);

		var menuOpen = Observable.Merge(press, longPress)

		return Observable.Merge()
			.Select();
	}
	//未开的前提下按钮只要被按下 -> 开启菜单
	//开启的前提下，短按 -> 菜单关闭； 长按 -> 确认 tf=》关闭

	// public enum ButtonEvent { pressDown, Release, LongPress }

	// public record OpenType
	// (
	// 	bool IsOpen,
	// 	bool IsPress,
	// 	bool OpenByLongPress
	// );

	// public record MenuState
	// (
	// 	OpenType Type,
	// 	MenuOperation? Emit
	// );

	// public static Observable<MenuOperation> CreateOperationFlow(
	// 	Observable<bool> pressed
	// )
	// {
	// 	var pressDown = pressed.Where(p => p).Select(_ => ButtonEvent.pressDown);
	// 	var release = pressed.Where(p => !p).Select(_ => ButtonEvent.Release);
	// 	var longPress = pressDown
	// 		.Select(_ => Observable.Timer(TimeSpan.FromSeconds(3)).TakeUntil(release))//计时器,Timer设定3秒，TakeUntil设置终点，release作为条件
	// 		.Switch()//更新计时器
	// 		.Select(_ => ButtonEvent.LongPress);//长按的前提是按下

	// 	return Observable.Merge(pressDown, longPress, release)
	// 		.Scan(new MenuState(new OpenType(false, false, false), null), (state, e) =>
	// 		{
	// 			var s = state.Type;
	// 			return e switch//按下 -- 松开 或者 按下 （长按） -- 松开
	// 			{
	// 				ButtonEvent.pressDown => new MenuState(s with { IsPress = true }, null),//f t f

	// 				ButtonEvent.LongPress => s.IsOpen
	// 					? new MenuState(s with { OpenByLongPress = true }, null)//f t t
	// 					: new MenuState(s with { IsOpen = true, OpenByLongPress = true }, null),//t t t

	// 				ButtonEvent.Release => (s.IsOpen, s.IsPress, s.OpenByLongPress) switch
	// 				{
	// 					(false, true, _) => new MenuState(s with { IsOpen = true }, MenuOperation.Open),
	// 					(true, true, false) => new MenuState(s with { IsOpen = false, IsPress = false }, MenuOperation.Close),//关闭
	// 					(true, true, true) => new MenuState(s with { OpenByLongPress = false }, MenuOperation.Confirm),
	// 					_ => new MenuState(s with { IsOpen = false, IsPress = false, OpenByLongPress = false }, MenuOperation.Close)
	// 				},

	// 				_ => state
	// 			};
	// 		})
	// 		.Where(s => s.Emit is not null)
	// 		.Select(s => s.Emit!.Value);
	// }
}
