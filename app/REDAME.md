# developer/app 开发说明（0.6 起）

> 本文件承接 `developer/REDAME.md` 的 Stage1，只描述 **0.6 及以后** 的步骤。
> 0.6 的定义：本机双人对战之后的网络对战，及其后续演进。

---

## 0. 当前状态：单人和本机双人都能玩了

`dotnet build` 0 错误；`dotnet run` 可以完整走「登录 → 主菜单 → 模式选择 → 对局 → 结算 → 回菜单」。

之前「跑不起来」的 6 个问题已全部修掉，记录如下（后续别改回去）：

| 编号 | 原问题 | 修法 | 落点 |
| --- | --- | --- | --- |
| P0-1 | 第 1 帧就撞死自己 | 方向→偏移的符号写反了，且蛇身从头往后铺，`Head` 落在队尾。改为按单位向量把身体摆在头后面 | `Models/Snake/Snake.cs` |
| P0-2 | 空闲时 100% CPU、界面卡死 | `Console.KeyAvailable ? ReadKey() : default` 轮询改成阻塞式 `ReadKey(intercept: true)`；游戏循环靠 `PeriodicTimer` 配速，不再空转 | `Game.Screen.cs`、`Game.Start.cs`、`Game.SetMenu.cs` |
| P0-3 | 登录必打印「成功 + 失败」 | 补 `return`，并顺手修了重名判断不会 continue 的问题 | `GameRegisterSystem.cs`、`Game.RegisterInterface.cs` |
| P0-4 | 设置菜单恒 false、进不去 | 重写菜单：地图大小 / 速度 / 时长 / 改名都能改，改完对下一局生效 | `Game.SetMenu.cs` |
| P0-5 | 第二局开始事件全掉线 | `BeginRound()` 重新注册、`EndRound()` 解绑；`Button`/`Collision` 不再是到处 new 的静态对象 | `Game.Args.cs`、`KeyBoard/*`、`Collision.cs` |
| P0-6 | 多人结束返回值的语义反了 | 一局结束回到模式菜单，不再用 `IsOver` 当「继续玩」 | `Game.Start.cs`、`LocalDoubleSystem.cs` |

实机验证（pty 脚本驱动 + `/proc` 采样）：

- 单人：画面正常刷新、能吃到食物涨分（0→1，长度 3→4）、撞墙后出结算并回菜单；
- 连续两局单人：两局都能结算，说明事件重新挂上了；
- 本机双人：两条蛇同时在场上，一方死亡后出结算与胜者，回模式菜单；
- 停在主菜单 2 秒的 CPU tick = 0；对局中 2 秒 7 tick（约 3.5% 单核），修复前是 200（单核满载）。

---

## 1. 版本进度

| 步骤 | 内容 | 状态 |
| --- | --- | --- |
| 0.1 | 工程骨架、`app.csproj`、`Config/GameSettings.json` | 已完成 |
| 0.2 | 注册 / 登录（`Config`、`Interface/Game.Register`） | 已完成 |
| 0.3 | 主菜单（`Interface/Game.Screen`） | 已完成 |
| 0.4 | 单人模式（`Services/System.cs`） | 已完成，可玩 |
| 0.5 | 本机双人对战（`Services/LocalDoubleSystem.cs`） | 已完成，可玩 |
| **0.6** | **网络对战：MQTT 传输 + protobuf 编码** | 未开始 |
| 0.7 | 服务端权威世界 + 快照广播 | 未开始 |
| 0.8 | 房间 / 建房 / 加入 | 未开始 |
| 0.9 | 断线重连与状态补帧 | 未开始 |
| 0.10 | 结算、复活、排行 | 未开始 |
| 0.11 | 渲染与手感优化 | 未开始 |
| 0.12 | 配置、皮肤、存档 | 未开始 |

---

## 2. 0.6 网络对战最小闭环

**目标**：两个进程（可以同一台机跑两个终端）连同一个 broker、同一个房间，
A 的按键能让 B 的角色动起来。

**已有可用的地基**

- `app.proto`：已定义 `PlayerInput`、`Point`、`PlayerState`、`FoodState`、`GameState`，
  `csharp_namespace = App.Protocol`，`app.csproj` 里 `Protobuf Include` 已经接好，构建时会生成 `obj/Debug/net11.0/App.cs`。
- `Config/GameSettings.json` 的 `NetWorkConfig`：`BrokerHost` / `BrokerPort` / `BrokerUsername` / `RoomId`。
- `World/WorldSnapShot.cs`：`WorldSnapShot` / `PlayerSnapShot` / `FoodSnapShot`，
  `GameWorld.Snapshot()` 已经能吐只读快照——这就是将来要序列化上网的东西。

**要做的事**

1. 依赖：`app.csproj` 增加 MQTT 客户端（`MQTTnet`），建议先不加 gRPC 运行时，`GrpcServices="None"` 保持现状。
2. 新建 `NetWork/` 目录：
   - `MqttTransport.cs`：连接、订阅、发布、断线回调（只做字节数组收发，不掺业务）。
   - `RemoteInputProcessor.cs`：把收到的 `PlayerInput` 转成 `KeyBoard` 里的方向 / 技能事件。
3. 上行：把 `Button` 事件（`KeyBoardController` 已统一收口）的结果封装成 `PlayerInput`
   （`room_id`、`player_id`、`client_tick`、`direction`、`use_dash`），`client_tick` 用 `GameWorld.Tick`。
4. 下行：把本地 `WorldSnapShot` 映射成 proto 的 `GameState` 广播（0.6 阶段允许「各算各的、只同步输入」，
   真正的权威服放到 0.7）。
5. 主题约定：`snake/{roomId}/input/{playerId}`、`snake/{roomId}/state`。
6. 本地回环自测：`dotnet run` 两次，第二个进程用 `--room` 参数或环境变量覆盖 `RoomId`。

**验收标准**

- 两个终端进入同一房间，互相能看到对方蛇头位置变化；
- broker 断开时客户端不崩溃，打印告警并在 0.7 接入重连；
- 网络包体大小可观测（打印每帧字节数）。

---

## 3. 0.7 服务端权威世界 + 快照广播

**目标**：只有一个地方推进 `GameWorld`（服务端），客户端只发输入、只渲染快照。

**要做的事**

1. 拆 `GameWorld` 的职责：`Update*` 只在服务端循环调用；客户端不再本地跑碰撞。
2. `GameWorld.Snapshot()` → `GameState` proto，按 `server_tick` 广播；
   频率与 `TickMilliseconds` 解耦（例如逻辑 20 Hz、广播 10 Hz）。
3. 客户端保留最近 2~3 帧快照做插值，避免闪烁。
4. 输入带 `client_tick`，服务端做「最近输入覆盖」；乱序/过期包直接丢弃。

**验收标准**

- 两个客户端看到的食物、分数、存活状态完全一致；
- 服务端逻辑帧率稳定（打印 tick 漂移 < 5%）；
- 客户端杀进程重开，不影响服务端继续跑。

---

## 4. 0.8 房间与匹配

**目标**：`RoomId` 真正起作用，支持建房、加入、满员提示、玩家列表。

**要做的事**

1. 复用 `Interface/Game.Screen/Game.PlayerLists.cs` 的 `RoomPlayerLists`（现在是雏形，常量 `PlayerMaxCount = 2`）。
2. 新增房间状态机：`Waiting → Playing → Settled`，与 `GameState.HomeScreenState` 解耦。
3. 玩家加入/离开广播，界面上显示「1/2 已就绪」。
4. 主菜单加「4. 房间」入口，进入二级界面（沿用 P0-2 的阻塞式读键）。

**验收标准**

- A 建房、B 输入房号能进；第三人加入提示满员；
- 房主退出后房间能正确销毁或转移。

---

## 5. 0.9 断线重连与状态补帧

**目标**：客户端短暂断网后自动恢复，不丢分、不重置。

**要做的事**

1. `MqttTransport` 增加心跳与自动重连（指数退避）。
2. 客户端重连后先请求一次全量快照，再恢复增量。
3. 服务端记录 `player_id → 最后输入 tick`，超时判定掉线，按 0.10 的规则结算。

**验收标准**

- 手动断开网络 3 秒再恢复，客户端能继续游戏；
- 服务端日志能区分「掉线」与「正常结束」。

---

## 6. 0.10 结算、复活与排行

**目标**：一局有完整的开始/结束/再来一局闭环。

**要做的事**

1. 把 `MultiRules.Winner` 接进网络局的结算（本机双人已经这么做了，网络局复用同一套）。
2. 单人结算已接到 `DefeatSystem.Result`（`Services/Die.cs`），复活功能在这个方法里往上加。
3. 设置菜单的时长预设在 `GameTime/ChangeTime.cs` 的 `SetTime.Options`，加一档改一行即可。
3. 结算界面：胜者、分数、长度、存活时长，按键返回主菜单 / 再来一局。
4. 复活：`DefeatSystem` 里那句「是否复活--待开发」，定义复活代价（扣分 / 冷却）。
5. 排行：先做本地 `Leaderboard.json`，字段对齐 `PlayerScore` + `SurvivalTime`。

**验收标准**

- 一局结束自动进结算，按 R 能立刻再来一局（同时验证 P0-5 已修好）；
- 排行榜能持久化并在重启后读到。

---

## 7. 0.11 渲染与手感

**目标**：画面不闪、不重绘全屏、操作跟手。

**要做的事**

1. 差分渲染：只重画变化的格子（整屏 `Console.Clear()` 是当前卡顿的主因，见 `Services/System.cs` 与 `LocalDoubleSystem.Draw`）。
2. 统一走 `World/View.cs` 的 `MapPrinter`，删掉 `LocalDoubleSystem` 里重复的绘制。
3. 输入与逻辑分帧：按键在帧边界消费，避免一帧吞多个键。
4. 终端尺寸检查：地图超出窗口时给出提示而不是乱码。

**验收标准**

- 25×30 地图下帧率稳定、无明显闪烁；
- 连续快速转向不出现「回身撞自己」以外的异常。

---

## 8. 0.12 配置、皮肤与存档

**目标**：把写死的数值都搬到配置里。

**要做的事**

1. `SkillConfig`（`Config/GameSettings.cs`）真正生效：`Skills` 里的 `cooldownTime = 5.0`
   和「前 5 秒才能冲刺」的逻辑都是硬编码，改为读配置。
2. `GameSettings.MapConfig` 支持设置菜单修改（承接 P0-4）。
3. 皮肤：颜色/字符表单独成 `Skins.json`，由 `View/MapPrinter` 读取。
4. 存档：`PlayerLists`、最高分、设置项落到 `%APPDATA%\snake` 或 `~/.config/snake`。

**验收标准**

- 改 json 不改代码就能调整时长、食物数、技能冷却；
- 换皮肤无需重新编译。

---

## 9. 附：常用命令

```bash
cd developer/app

dotnet build                 # 编译
dotnet run                   # 运行（需要真实终端，ReadKey 在重定向输入下会抛异常）
dotnet build -p:EnforceCodeStyleInBuild=true   # 连同 IDE0058 等风格规则一起检查

# 配置文件随构建拷贝到输出目录
ls bin/Debug/net11.0/Config/GameSettings.json
```

配置项（`Config/GameSettings.json`）：

| 字段 | 含义 | 校验 |
| --- | --- | --- |
| `gameDurationSeconds` | 单局时长 | — |
| `tickMilliseconds` | 逻辑帧间隔 | 不得 < 50 |
| `map.GameMapWidth/Height` | 地图尺寸 | 不得 < 10 |
| `food.GenerateFoodSeconds/Count` | 食物刷新间隔/数量 | 数量不得 < 0 |
| `NetWorkConfig.*` | broker 与房间 | 0.6 起使用 |