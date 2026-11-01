# 联机实施计划（路线 C：主机权威 + MQTT/protobuf）

> 目标形态：**你的机器当主机**（跑唯一一份 `GameWorld`），同学 `git clone` 后在自己的终端跑客户端（只发按键、只画快照）。
> 代码改动策略：**只新增 `NetWork/` 目录，外加 3 行装配**，单人/本机双人的业务逻辑一行不动。

---

## 0. 拓扑与前提

```text
你的机器 (Host)                          同学的机器 (Client)
├─ GameWorld（权威、唯一）               ├─ 不跑模拟
├─ LocalDoubleSystem（p2 由远端控制）    ├─ 收 GameState → 画棋盘
└─ MQTT 客户端 ──► broker ◄── MQTT 客户端 ┘  发 PlayerInput
```

**联机的本质 = 你开「本机双人」模式，但玩家2 的键盘来自网络。**
这样完全复用现有代码：`LocalDoubleSystem` 里 p2 已经存在，只需要把远端按键合成 `ConsoleKeyInfo` 喂给 `Button.TouchClick`。

**broker 三选一（先 A，跑通再考虑 B/C）**

| 方案 | 地址 | 优点 | 缺点 |
| --- | --- | --- | --- |
| A. 公网公共 broker（推荐起步） | `broker.emqx.io:1883` 或 `test.mosquitto.org:1883` | 零安装、跨公网、不用端口映射 | 公开环境，必须用随机房间号 |
| B. 你本机跑 mosquitto | `你的公网IP:1883` | 私密、可控 | 要装 broker + 防火墙/端口映射；家里宽带多为 NAT |
| C. 内网穿透 + WebSocket | `wss://xxx.trycloudflare.com/mqtt` | 免端口映射、可走 443 | 要配 tunnel，MQTT 用 8083/WS |

> 公共 broker 上**一定不要用 `room1`**：房间号用 `snake-` + 6 位随机串（例如 `snake-7f3k2a`），同学们手输同一个。

---

## 1. 阶段 0：先让「别人能跑起来」（最容易翻车的一步）

当前 `app.csproj` 是 `net11.0` + 你本机 SDK `11.0.100-rc.1`。同学没装这个 **预览版 SDK** 时，`dotnet run` 直接失败。

**两种解法（选一）**

| 做法 | 改动 | 适合 |
| --- | --- | --- |
| 降到 `net8.0`（LTS） | `csproj` 里 `TargetFramework` 一行 | 最稳，`MQTTnet 5.2`、`Google.Protobuf 3.36.2`、`Grpc.Tools 2.85.0` 都有 net8.0 资产 |
| 让大家装 .NET 11 SDK | 加 `global.json` 锁定版本 | 想保持 net11 |

建议再加一个 `global.json`（避免同学机器上 SDK 版本漂移）：

```json
{ "sdk": { "version": "8.0.100", "rollForward": "latestFeature" } }
```

**验收**：同学机器上 `git clone` → `dotnet run` 能看到登录界面。

---

## 2. 阶段 1：先验证「联机」本身（不碰游戏逻辑）

**目标**：两台机器通过 MQTT 互发一条消息，证明网络链路通。

新增文件：

```text
NetWork/
 ├─ MqttTransport.cs   // 直接照 snake/NetWork/MqttTransport.cs 搬（改命名空间）
 └─ NetPing.cs         // --ping 模式：发 hello / 收 hello
```

`csproj` 增加：

```xml
<PackageReference Include="MQTTnet" Version="5.2.0.1603" />
```

`program.cs` 最前面加分流（**装配第 1 行**）：

```csharp
if (args.Contains("--ping")) { await NetPing.RunAsync(args); return; }
```

**验收**：你发 `hello`，同学终端打印出来；反向也通。
**主题**：`snake/{room}/hello`。

---

## 3. 阶段 2：网络骨架 + 角色

> 零基础版教程（含 MQTT 名词解释、逐文件代码、排错表）：
> **[阶段2-MQTT入门与骨架.md](阶段2-MQTT入门与骨架.md)**

**本阶段只做「连通性」，不接游戏。**

新增文件（都不碰游戏逻辑）：

| 文件 | 职责 |
| --- | --- |
| `NetWork/NetOptions.cs` | 解析 `--host` / `--join` / `--ping` / `--broker` / `--room` / `--name` |
| `NetWork/MqttTransport.cs` | 连接 / 订阅 / 发布 / 收件箱（纯传输） |
| `NetWork/NetHost.cs` | 三种模式的生命周期：Ping / Host / Client |

**装配（本阶段只加 1 行）**

```csharp
// program.cs
var netOptions = NetOptions.Parse(args, gameSettings.NetWorkConfig.RoomId);
if (netOptions.Mode is not NetMode.None) { await NetHost.RunAsync(netOptions); return; }
```

阶段 3 才会再补 2 行（`NetHost.Attach(args)` + 客户端分支），用来把游戏接进来。

**主题**

| 方向 | 主题 | QoS |
| --- | --- | --- |
| Client → Host | `snake/{room}/input/{playerId}` | 1 |
| Host → Client | `snake/{room}/state` | 0 |
| 双向 | `snake/{room}/hello` | 1 |

**验收**：两个终端进同一房间互发 `hello`；换房号收不到；不带参数启动仍是原来的游戏流程。

---

## 4. 阶段 3：只读观战（Host → Client 快照）

> 完整实现清单（8 项任务 + 代码 + 验收）见 **[阶段3-快照观战清单.md](阶段3-快照观战清单.md)**

- Host：`SnapshotPump` 每 100ms `world.Snapshot()` → `ProtoCodec.ToProto` → publish `GameState`；
- Client：`RemoteRender` 收 `GameState` → 画 25×30 棋盘；
- 装配：`GamePrepareArgs` 构造函数末尾加 1 行 `NetHost.Attach(this);`；
- pump 用**独立定时器**，不挂在游戏循环里，所以单/双人逻辑零改动。

**前置**：先修《阶段3清单》§0 的 3 个已知问题（Guid 格式串、空订阅、Log 没接线）。

**验收**：Host 正常玩，Client 终端 100ms 刷新一次画面，蛇动、分数变、结束提示。

---

## 5. 阶段 4：输入上行（Client → Host）

- Client：在渲染循环里 `Console.KeyAvailable` 轮询，把 W/A/S/D/方向键/E → `PlayerInput`；
- Host：`RemoteInput` 收包 → 合成 `ConsoleKeyInfo`：

```csharp
// 远端 Direction.Up → 玩家2（Arrows 布局）
args.Button.TouchClick(new ConsoleKeyInfo('\0', ConsoleKey.UpArrow, false, false, false));
```

- 去重：`client_tick` 单调递增，Host 只应用更新的；
- 冷却/技能判定仍在 Host（`Skills` 里已有）。

**验收**：同学按 W/A/S/D，你屏幕上的玩家2 会动；按 E 会冲刺。

---

## 6. 阶段 5：房间、座位、身份

- `--room snake-xxxx`：房间号（Host 生成，发给同学）；
- `--name 张三`：显示名 / playerId；
- 座位：Host 固定 p1，第一个加入的 Client 固定 p2；
- 满员：第三个人加入回 `room/full`（复用 `RoomPlayerLists.PlayerMaxCount = 2` 的语义）；
- 离开：MQTT 遗嘱（LWT）或 3s 心跳超时。

**验收**：第二个人加入提示「房间已满」；有人掉线 Host 有提示。

---

## 7. 阶段 6：结算、重开、掉线

- `is_game_over` → Client 弹结算（胜者/分数/长度）；
- Host 按 R → `BeginRound()` → 广播 `snake/{room}/reset`，Client 清屏等待新快照；
- 掉线：15s 无 `client_tick` → 判该方死亡并结算。

**验收**：一局打完 → 重开 → 双方都回到新局。

---

## 8. 阶段 7：稳定性与体验（可后置）

- MQTTnet 5 **没有内置自动重连**：在 `DisconnectedAsync` 里做指数退避重连循环；
- 输入 QoS1 + 去重，快照 QoS0；
- 测量延迟：`hello` 里带时间戳，日志打 RTT；
- 客户端插值（可选）：两帧快照间平滑；
- 日志：`--verbose` 开关。

---

## 9. 交付物

- `NetWork/` 6 个文件 + 3 行装配；
- `README`（或 `REDAME.md`）增加「联机玩法」：主机命令、同学命令、broker 选择、常见问题；
- 两个脚本：`run-host.sh` / `run-client.sh`；
- 演示脚本：一局完整对局的验收步骤。

---

## 10. 验收清单

- [ ] 同学 clone 后 `dotnet run` 能起（SDK 版本问题已解决）
- [ ] `--ping` 两台机器互通
- [ ] `--host` / `--join` 进入同一房间
- [ ] 观战端能看到主机画面
- [ ] 观战端按键能控制玩家2
- [ ] 满员/掉线有提示
- [ ] 一局结束能结算、能重开
- [ ] 主机空闲时 CPU 正常（网络泵不空转）

---

## 11. 风险与对策

| 风险 | 对策 |
| --- | --- |
| 同学没装 .NET 11 预览 SDK | 降到 net8.0，或发 `global.json` + 安装说明 |
| 家里宽带 NAT，同学连不到你 | 先用公共 broker；要私密就上内网穿透 |
| 公共 broker 房间撞车 | 随机 6 位房间号；主题里带房间前缀 |
| 联机后两人操作互相干扰 | Host 只认 p1（自己键盘）+ p2（网络），云端不接管本地键盘 |
| `GameState` 缺宽度/剩余时间 | 客户端先写死 25×30，或后续给 proto 加 4 个字段 |