# 阶段 2：搭网络骨架（MQTT 零基础版）

> 目标：**不改游戏逻辑**，只新增 NetWork/ 下的三个文件 + program.cs 一行分流，
> 让两个终端（两台机器）能进同一个「房间」，互相收到消息。
> 这一步**还不接游戏**——先把「邮局」跑通，再往里寄信。

---

## 0. 五分钟看懂 MQTT（够用就行）

MQTT 就是一套「邮局 + 订阅报纸」的协议：

    你的程序 A                          邮局(broker)                 你的程序 B
    ---------                          ------------                 ---------
    publish(snake/房号/state, 数据) --> 按 topic 分发 --> subscribe(snake/房号/#)

| 名词 | 一句话解释 | 在你项目里 |
| --- | --- | --- |
| **broker** | 转发消息的服务器（邮局） | broker.emqx.io:1883 或自己跑 mosquitto |
| **topic** | 消息的「地址」，用 / 分层 | snake/{房间号}/state、snake/{房间号}/input/{玩家} |
| **publish** | 往某个 topic 发 | 主机发快照、客户端发按键 |
| **subscribe** | 订阅某个 topic（支持通配符 #） | snake/{房间号}/# 订阅房间里所有消息 |
| **QoS** | 送达保证：0 最多一次 / 1 至少一次 / 2 恰好一次 | 输入用 1，快照用 0 |
| **clientId** | 客户端身份，**重复会被踢下线** | 房间号-随机串 |
| **retain** | 保留最后一条，新订阅者立刻收到 | 游戏状态**不要**用 retain |

> 为什么不用自己写 TCP？broker 已经帮你处理了多客户端路由、断线检测、QoS 重发、通配符订阅。
> 你只管 publish / subscribe。

---

## 1. 先选一个 broker（三种，按顺序试）

### A. 公共 broker（推荐，零安装）

| 地址 | 说明 |
| --- | --- |
| broker.emqx.io:1883 | 明文 TCP，最省事 |
| test.mosquitto.org:1883 | 另一个公共选择 |
| broker.emqx.io:8883 | TLS，需要额外配置证书，先别碰 |

**先验证网络通不通**（不装任何软件）：

    # 能连上会打印 OK；超时就说明网络或防火墙挡了 1883
    timeout 5 bash -c 'cat < /dev/null > /dev/tcp/broker.emqx.io/1883' && echo OK

### B. 自己机器跑 mosquitto（要给别人连，得配监听）

    docker run -d --name mqtt -p 1883:1883 eclipse-mosquitto:2 \
      sh -c "printf 'listener 1883 0.0.0.0\nallow_anonymous true\n' > /mosquitto/config/mosquitto.conf && mosquitto -c /mosquitto/config/mosquitto.conf"

> 注意：mosquitto 默认只监听 127.0.0.1，不写 listener 1883 0.0.0.0 的话，**同一局域网的同学也连不上**。

### C. 家里宽带没有公网 IP

用内网穿透（cloudflared / frp）把 1883 暴露出去，或者干脆用方案 A。

---

## 2. 阶段 2 要交付什么

新增三个文件（**都不碰现有游戏代码**）：

    NetWork/
     |- NetOptions.cs    // 解析 --host / --join / --ping / --broker / --room / --name
     |- MqttTransport.cs // MQTT 连接、订阅、发布、收件箱（纯传输）
     |- NetHost.cs       // 生命周期：按模式跑 Ping / Host / Client

装配：**program.cs 只加 1 行分流**。

    program.cs
      |- 解析参数 -> NetOptions
      |- Mode != None -> NetHost.RunAsync(options); return;   <-- 新增
      `- 否则就是原来的游戏流程（一行不动）

**验收**：两个终端进同一房间，互发 hello，双方日志都能打印，且 dotnet build 0 错误。

---

## 3. 分步搭建（每步都能编译、都有验收）

### 步骤 1：确认包

app.csproj 里已经有 MQTTnet 5.2.0.1603（你加的），确认这一行：

    <PackageReference Include="MQTTnet" Version="5.2.0.1603" />

    cd developer/app
    dotnet restore
    dotnet build          # 应该 0 错误

### 步骤 2：NetOptions.cs（参数解析）

    namespace App.NetWork;

    public enum NetMode { None, Ping, Host, Client }

    public sealed class NetOptions
    {
        public NetMode Mode { get; set; } = NetMode.None;
        public string BrokerHost { get; set; } = "broker.emqx.io";
        public int BrokerPort { get; set; } = 1883;
        public string Room { get; set; } = "snake-dev";
        public string Name { get; set; } = "player";

        public static NetOptions Parse(string[] args, string defaultRoom)
        {
            var o = new NetOptions { Room = defaultRoom };

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--ping": o.Mode = NetMode.Ping; break;
                    case "--host": o.Mode = NetMode.Host; break;
                    case "--join": o.Mode = NetMode.Client; break;
                    case "--broker":
                        var hp = args[++i].Split(':', 2);
                        o.BrokerHost = hp[0];
                        if (hp.Length > 1) o.BrokerPort = int.Parse(hp[1]);
                        break;
                    case "--room": o.Room = args[++i]; break;
                    case "--name": o.Name = args[++i]; break;
                }
            }

            return o;
        }
    }

**验收**：dotnet build 通过。

### 步骤 3：MqttTransport.cs（只管收发）

    using MQTTnet;
    using MQTTnet.Protocol;
    using System.Buffers;                    // Payload 在 MQTTnet 5 是 ReadOnlySequence<byte>，ToArray() 来自这里
    using System.Collections.Concurrent;

    namespace App.NetWork;

    /// <summary>MQTT 最小封装：连接 / 订阅 / 发布 / 收件箱，不含任何游戏逻辑。</summary>
    public sealed class MqttTransport
    {
        readonly IMqttClient client;
        readonly string clientId;

        // 收到的消息先入队，主线程按自己的节奏取（别在回调里做重活）
        readonly ConcurrentQueue<(string Topic, byte[] Payload)> inbox = new();

        public bool IsConnected => client.IsConnected;

        public MqttTransport(string clientId)
        {
            this.clientId = clientId;

            var factory = new MqttClientFactory();
            client = factory.CreateMqttClient();

            client.ApplicationMessageReceivedAsync += e =>
            {
                inbox.Enqueue((e.ApplicationMessage.Topic, e.ApplicationMessage.Payload.ToArray()));
                return Task.CompletedTask;
            };

            client.DisconnectedAsync += _ =>
            {
                Console.WriteLine("[mqtt] 连接已断开");
                return Task.CompletedTask;
            };
        }

        public async Task ConnectAsync(string host, int port)
        {
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(host, port)
                .WithClientId(clientId)
                .WithCleanSession(true)                    // 每次连都是干净会话，新手先这样
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(15))
                .WithTimeout(TimeSpan.FromSeconds(10))
                .Build();

            await client.ConnectAsync(options);
            Console.WriteLine($"[mqtt] 已连接 {host}:{port}  clientId={clientId}");
        }

        public async Task SubscribeAsync(string topic, MqttQualityOfServiceLevel qos = MqttQualityOfServiceLevel.AtLeastOnce)
        {
            var options = new MqttClientSubscribeOptionsBuilder()
                .WithTopicFilter(topic, qos)
                .Build();

            await client.SubscribeAsync(options);
            Console.WriteLine($"[mqtt] 已订阅 {topic} (QoS {(int)qos})");
        }

        public async Task PublishAsync(string topic, byte[] payload,
                                       MqttQualityOfServiceLevel qos = MqttQualityOfServiceLevel.AtMostOnce,
                                       bool retain = false)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(qos)
                .WithRetainFlag(retain)                    // 游戏状态别 retain
                .Build();

            await client.PublishAsync(message);                // 必须 await
        }

        public bool TryReceive(out string topic, out byte[] payload)
        {
            if (inbox.TryDequeue(out var item))
            {
                topic = item.Topic;
                payload = item.Payload;
                return true;
            }

            topic = "";
            payload = [];
            return false;
        }

        public async Task DisconnectAsync()
        {
            if (client.IsConnected)
                await client.DisconnectAsync();
        }
    }

> 这些 API 都按 MQTTnet 5.2 的真实签名写过（三个文件我实际编译过：0 错误 0 警告）：
> WithTcpServer(host, port)、WithTopicFilter(topic, qos)、WithPayload(byte[])、
> client.IsConnected、client.DisconnectAsync()。
>
> 一个容易漏的点：MQTTnet 5 里 Payload 是 ReadOnlySequence<byte>，ToArray() 来自
> using System.Buffers;，漏了会报 CS0411「无法推断 ToArray 的类型参数」。

**验收**：dotnet build 通过。

### 步骤 4：NetHost.cs（三种模式）

    namespace App.NetWork;

    public static class NetHost
    {
        static MqttTransport? transport;
        static NetOptions? options;

        public static bool IsClient => options?.Mode is NetMode.Client;

        public static async Task RunAsync(NetOptions o)
        {
            options = o;
            transport = new MqttTransport($"{o.Room}-{Guid.NewGuid():N}"[..18]);
            await transport.ConnectAsync(o.BrokerHost, o.BrokerPort);
            await transport.SubscribeAsync($"snake/{o.Room}/#");

            Console.WriteLine($"[net] 模式={o.Mode}  房间={o.Room}  我是={o.Name}");
            Console.WriteLine($"[net] 另一台用：dotnet run -- --{(o.Mode is NetMode.Host ? "join" : "host")} --room {o.Room}");

            switch (o.Mode)
            {
                case NetMode.Ping:   await RunPingAsync(o);   break;
                case NetMode.Host:   await RunHostAsync(o);   break;
                case NetMode.Client: await RunClientAsync(o); break;
            }
        }

        // 阶段 2 只验证连通性：下面三个都是「打印 + 定时发」
        static async Task RunPingAsync(NetOptions o)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(2));
            while (await timer.WaitForNextTickAsync())
            {
                var text = $"hello from {o.Name} @ {DateTime.Now:HH:mm:ss}";
                await transport!.PublishAsync($"snake/{o.Room}/hello",
                    System.Text.Encoding.UTF8.GetBytes(text));
                Console.WriteLine($"[ping] 发送: {text}");
                Drain();
            }
        }

        static async Task RunHostAsync(NetOptions o)
        {
            Console.WriteLine("[host] 等待客户端加入…（Ctrl+C 退出）");
            using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));
            while (await timer.WaitForNextTickAsync()) Drain();
        }

        static async Task RunClientAsync(NetOptions o)
        {
            Console.WriteLine("[client] 已进入房间，等待主机消息…（Ctrl+C 退出）");
            using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));

            var once = false;
            while (await timer.WaitForNextTickAsync())
            {
                Drain();
                if (once) continue;
                once = true;
                await transport!.PublishAsync($"snake/{o.Room}/hello",
                    System.Text.Encoding.UTF8.GetBytes($"hi, {o.Name} 已加入"));
            }
        }

        static void Drain()
        {
            while (transport!.TryReceive(out var topic, out var payload))
                Console.WriteLine($"[recv] {topic}: {System.Text.Encoding.UTF8.GetString(payload)}");
        }
    }

**验收**：dotnet build 通过。

### 步骤 5：program.cs 加 1 行分流

在你现有 program.cs 里，加载配置之后、进入游戏循环之前加：

    using App.NetWork;

    var gameSettings = GameSettingLoader.Load();

    // —— 联机模式：整个进程交给网络层，不进入游戏（阶段 2 只验证连通性）——
    var netOptions = NetOptions.Parse(args, gameSettings.NetWorkConfig.RoomId);
    if (netOptions.Mode is not NetMode.None)
    {
        await NetHost.RunAsync(netOptions);
        return;
    }

    // 下面保持原样
    var gameRunningState = new GameState();
    while (...) { ... }

> NetWorkConfig 在 Config/GameSettings.cs 里，字段名是 RoomId / BrokerHost / BrokerPort。

### 步骤 6：验证（同一台机器就能测）

终端 A（主机）：

    cd developer/app
    dotnet run -- --host --room snake-abc123 --broker broker.emqx.io

终端 B（客户机，或另一台电脑）：

    dotnet run -- --join --room snake-abc123 --broker broker.emqx.io --name 张三

期望看到：

    # A
    [mqtt] 已连接 broker.emqx.io:1883  clientId=snake-abc123-1a2b3c
    [mqtt] 已订阅 snake/snake-abc123/# (QoS 1)
    [net] 模式=Host  房间=snake-abc123  我是=player
    [host] 等待客户端加入…
    [recv] snake/snake-abc123/hello: hi, 张三 已加入

    # B
    [net] 模式=Client  房间=snake-abc123  我是=张三
    [client] 已进入房间，等待主机消息…

---

## 4. 阶段 2 的验收清单

- [ ] dotnet build 0 错误
- [ ] --host 能连上 broker 并打印 clientId
- [ ] --join 能连上同一房间
- [ ] 双方能看到对方的 hello（[recv] 行）
- [ ] 关掉一方，另一方有「连接已断开」日志
- [ ] 换一个房间号（--room snake-zzz）就收不到消息（证明 topic 隔离）
- [ ] 不带参数 dotnet run 仍然是登录 -> 主菜单（游戏本体没受影响）

---

## 5. 新手最容易踩的 10 个坑

| 现象 | 原因 | 解决 |
| --- | --- | --- |
| 连接超时 | 端口/防火墙/NAT | 先跑第 1 节的 /dev/tcp 测试；公共 broker 用 1883 不要 8883 |
| 刚连上就被踢 | 两个进程 clientId 相同 | 用 房间号-随机串（步骤 4 已这么做） |
| 收不到但对方说发了 | 房间号不一致 / topic 拼写 | 双方打印自己的 room 核对 |
| 别人连不上你本机的 mosquitto | 只监听了 127.0.0.1 | 配置 listener 1883 0.0.0.0 |
| 新客户端一进来就收到旧消息 | 用了 retain | 发布时 WithRetainFlag(false) |
| 偶尔收到重复消息 | QoS 1 是「至少一次」 | 用 client_tick 去重（阶段 4） |
| 消息偶尔丢 | QoS 0 且没 await | 输入用 QoS 1，PublishAsync 必须 await |
| 中文乱码 | 没统一 UTF-8 | Encoding.UTF8；protobuf 是二进制别当字符串 |
| 公共 broker 有人乱入 | 房间号太常见 | snake- + 6 位随机串，别用 room1 |
| 程序退出后消息还在 | retain 残留 | 发布时 retain=false，或发一条空 payload 清 retained |
| 编译报 CS0411（ToArray 推断失败） | 少了 using System.Buffers | 见步骤 3 的 using 列表 |

---

## 6. 下一阶段（阶段 3）要接什么

阶段 2 结束时，你手上有一个**能收发的 MQTT 通道**。阶段 3 才把游戏接上去：

1. SnapshotPump：独立定时器每 100ms 调 world.Snapshot() -> ProtoCodec.ToProto -> 发 snake/{room}/state；
2. RemoteRender：客户端收 GameState -> 画字符棋盘；
3. RemoteInput：客户端按键 -> PlayerInput -> 主机 Button.TouchClick。

为此还会多 2 行装配（NetHost.Attach(args) 和客户端分支），阶段 3 的文档再讲。

---

## 7. 本阶段新增代码的位置

| 文件 | 作用 | 碰游戏逻辑吗 |
| --- | --- | --- |
| NetWork/NetOptions.cs | 参数解析 | 否 |
| NetWork/MqttTransport.cs | 连接/订阅/发布/收件箱 | 否 |
| NetWork/NetHost.cs | 三种模式的生命周期 | 否 |
| program.cs | +1 行分流 | 只加分流，不改原流程 |
| app.csproj | MQTTnet 包 | 已有 |