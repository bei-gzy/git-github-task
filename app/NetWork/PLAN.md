# 联机实施计划（2026-11-02 · P0 已完成）

> 阶段 2/3 的教程仍然有效；本文记录**当前真实进度**和剩余任务。
> 上一版（18:21）已过期：P0 全部完成，proto 字段也已按方案 B 加上。

---

## 0. 现在到哪一步

**构建：✅ 0 错误 0 警告**

| 文件 | 状态 | 说明 |
| --- | --- | --- |
| `NetWork/MqttTransport.cs` | ✅ | 连接 / 订阅（带 topics）/ 发布 / 收件箱 / Log / Dispose |
| `NetWork/NetOptions.cs` | ✅ | `--host/--join/--ping/--broker/--room/--name` |
| `NetWork/NetHost.cs` | ✅ | P0-2 守卫、P0-3 Host 不阻塞，已修 |
| `NetWork/ProtoCodec.cs` | ✅ | `ToProto` / `FromProto` 对称，含枚举显式映射 |
| `NetWork/SnapShotPump.cs` | ⚠️ | `RunAsync` 可用；`Dispose` 仍抛 NotImplementedException（P1-2） |
| `NetWork/RemoteRender.cs` | ⚠️ | 已接线，但绘制逻辑有 bug（P1-1） |
| `NetWork/RemoteInput.cs` | ⏳ | 空壳，阶段 4 才写 |
| `Services/.../Game.Args.cs` | ✅ | 已调用 `NetHost.Attach(GameWorld)` |
| `program.cs` | ✅ | P0-4 已补 return |

**已实测**：`--host` 能连上 broker、打印 `[mqtt]已连接` / `[mqtt] 订阅`、打印 Host 就绪，并正常进入登录界面。

---

## 1. P0 修复记录（已完成，留档）

### P0-1 删掉 ProtoCodec.ToP ✅

- `ToP` 用了 `GameState { [..集合] }` 这种非法写法，已删除；
- 同时把 `ToProto` 重写成和 `FromProto` 对称的结构（每个类型一对 ToProto/FromProto）；
- 修了两个真 bug：
  1. 方向枚举差 1（`State.Direction.Up=0` vs `Protocol.Direction.Up=1`），原来直接强转会静默错位；
  2. `ToProto` 漏发 `Width/Height/RemainingSeconds`，导致观战端读回来全是 0。

### P0-2 修 NetHost.Attach 的守卫 ✅

原来是 `if (mqttTransport is null || sp is null || ...) return;`，第一次进来 `sp` 必为 null → 永远 return。

现在保留**两个 if**（语义不同，便于排错）：

    // 依赖没准备好，或者不是主机，就不挂
    if (mqttTransport is null || no is null || no.NetMode is not NetMode.Host) return;

    // 已经挂过，幂等返回
    if (sp is not null) return;

> 也可以合并成一条 `if (sp is not null || mqttTransport is null || no is null || no.NetMode is not NetMode.Host) return;`，
> 逻辑等价；分开写是为了以后看日志能判断是哪一类没通过。

### P0-3 RunHostAsync 不能阻塞 ✅

改成连上就返回，让 `program.cs` 继续登录、进游戏：

    static Task RunHostAsync()
    {
        Console.WriteLine("[host] 已连接，进入游戏后开始广播快照");
        return Task.CompletedTask;
    }

**为什么 Host 不能顺便跑 `Drain()`**：`Drain()` 末尾会 `Console.ReadKey` 消费按键，
会把游戏主循环的 WASD 全部吞掉；而且它还会把 `/state` 渲染到主机屏幕上。
收包留给阶段 4 的专用 `DrainInput()`（只处理 `/input/`，不碰 `Console`）。

### P0-4 program.cs 补 return ✅

    if (netOptions.NetMode is not NetMode.None)
    {
        await NetHost.RunAsync(netOptions);

        // 主机继续进游戏（开局时 Attach 启动快照泵）；观战/测连通跑完就退出
        if (netOptions.NetMode is not NetMode.Host)
            return;
    }

### 额外：日志赋值顺序 ✅

`mqttTransport.Log = Console.WriteLine;` 挪到 `ConnectAsync` **之前**，否则连接/订阅阶段的日志全被吞掉。

---

## 2. 剩余任务

### P1：把观战真正跑起来

- [ ] **P1-1 重写 `RemoteRender`（当前最大问题）**
  - 现在的写法：对**每个格子**都遍历所有食物、每只蛇的每一节，并 `Append` 一次 →
    每行字符数 = 1 + 食物数 + Σ蛇长 + 玩家数，**行宽爆炸**，画面是乱的。
  - 正确做法：先建 `char[,] grid`（边界 █ / 空格），再**覆盖**：食物 → 蛇身 → 蛇头；最后按行拼字符串。
  - 蛇头取 `SnakeSegments[^1]`，且要**先画身体再画头**；死亡玩家不画头。
  - 宽高可以用 `GameState.width/height`（proto 已加），不必再读配置文件。

- [ ] **P1-2 `SnapshotPump.Dispose` 别抛异常**
  - 改成 `cts.Cancel(); cts.Dispose();`。

- [ ] **P1-3 Client 退出键**
  - `Drain()` 里的 `Escape` 只是 return 出 `Drain`，退不出循环；在 `RunClientAsync` 里处理 `q`。

- [ ] **P1-4 双终端验收**

      # 终端 1（主机）
      dotnet run -- --host --room snake-t3 --broker broker.emqx.io:1883
      #   登录 -> 1 开始游戏 -> 2 本机双人

      # 终端 2（观战）
      dotnet run -- --join --room snake-t3 --broker broker.emqx.io:1883 --name 小明

  - 验收：终端 2 每 100ms 刷新棋盘，蛇动、分数变、结束提示。

### P2：输入上行（阶段 4）

- [ ] P2-1 `RemoteInput`：`PlayerInput` → 合成 `ConsoleKeyInfo` → `Button.TouchClick`（主机侧）
- [ ] P2-2 Client 采集：WASD/方向键/E → `PlayerInput`（带自增 `client_tick`）
- [ ] P2-3 主机去重：只应用比上次新的 `client_tick`
- [ ] P2-4 验收：观战端按 WASD，主机上的玩家2 会动

### P3：收尾（阶段 5-7）

- [ ] 房间号 / 座位 / 满员提示（复用 `RoomPlayerLists`）
- [ ] 结算与重开（`is_game_over` → 双方结算 → R 重开）
- [ ] 断线重连（MQTTnet 5 无自动重连，自己写退避）
- [ ] README 增加「联机玩法」一节

---

## 3. 阶段表

| 阶段 | 内容 | 状态 |
| --- | --- | --- |
| 阶段 2 | MQTT 连通（Ping 收发 hello） | ✅ 完成（已实测） |
| 阶段 3 | 只读观战 | 🚧 约 75%：Host 发快照已通，Client 渲染要修 P1-1 |
| 阶段 4 | 输入上行 | ⏳ 未开始 |
| 阶段 5 | 房间/座位/满员 | ⏳ 未开始 |
| 阶段 6 | 结算/重开/掉线 | ⏳ 未开始 |
| 阶段 7 | 稳定性与文档 | ⏳ 未开始 |

---

## 4. proto 字段：已选方案 B（已落地）

不再需要拍板，`app.proto` 现在已经有：

    message PlayerState {
      // 1-4 原有
      string name = 5;
      Direction direction = 6;
    }

    message GameState {
      // 1-5 原有
      int32 width = 6;
      int32 height = 7;
      double remaining_seconds = 8;
    }

`ProtoCodec.ToProto` 已发送这些字段，`FromProto` 已读取，往返测试通过（65 字节，逐字段一致）。

---

## 5. 文档导航

| 文档 | 用途 |
| --- | --- |
| [阶段2-MQTT入门与骨架.md](阶段2-MQTT入门与骨架.md) | MQTT 概念 + 从零搭骨架（已落地） |
| [阶段3-快照观战清单.md](阶段3-快照观战清单.md) | 阶段 3 的代码参考（Attach 签名/文件名以本文件为准） |
| 本文件 | 当前真实进度 + 剩余任务 |