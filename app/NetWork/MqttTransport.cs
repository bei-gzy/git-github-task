using System.Buffers;//高性能分配以及回收
using System.Collections.Concurrent;//线程安全并发合集

using MQTTnet.Protocol;//定义通信库的规则的语言
using MQTTnet;//第三方通信库

namespace App.NetWork;

//包的内容：主题+载荷
public readonly record struct MqttMessage(string Topic, byte[] PayLoad);

//------------------------------------传输层类(收发装置）-----------------------------------
public sealed class MqttTransport : IAsyncDisposable
{
	readonly IMqttClient client;
	//本质队列维护线程进出安全--进出的都是inbox
	readonly ConcurrentQueue<MqttMessage> inbox = new();
	readonly string clientID;

	//-----Log委托(日志)------
	public Action<string>? Log { get; set; }

	//构造函数内付出值委托人、包箱、委托人ID
	public MqttTransport(string clientId)
	{
		//构造函数内建工厂
		clientID = clientId;
		var factory = new MqttClientFactory();
		client = factory.CreateMqttClient();

		//委托人调用接受事件（封装装箱操作）,将topic和payload
		client.ApplicationMessageReceivedAsync += e =>
		{
			//后台回调收包  放进线程给主线程后续调用
			inbox.Enqueue(new MqttMessage(e.ApplicationMessage.Topic, e.ApplicationMessage.Payload.ToArray()));
			return Task.CompletedTask;
		};

		//连接失败的反馈
		client.DisconnectedAsync += e =>
		{
			Log?.Invoke($"[mqtt]断开： {e.Reason}");
			if (e.Exception is not null)
				Log?.Invoke($"[mqtt]原因:{e.Exception.Message}");
			return Task.CompletedTask;
		};
	}

	//是否真的连上（Host 用它决定要不要进游戏）
	public bool IsConnected => client.IsConnected;

	//异步连接：返回是否成功，连不上只打日志，不抛异常
	public async Task<bool> ConnectAsync(string host, int port, TimeSpan? keepAlive = null, CancellationToken ct = default)
	{
		//连接选项，构建拼出MqttClientptions对象
		var options = new MqttClientOptionsBuilder()
			.WithTcpServer(host, port)//Tcp路线连接broker
			.WithClientId(clientID)
			.WithCleanSession(true)
			.WithKeepAlivePeriod(keepAlive ?? TimeSpan.FromSeconds(15))//保证持续连接
			.Build();

		//自销毁timeout计时器，链不通取消
		using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
		timeout.CancelAfter(TimeSpan.FromSeconds(8));

		try
		{
			var result = await client.ConnectAsync(options, timeout.Token);

			if (result.ResultCode is not MqttClientConnectResultCode.Success)
			{
				Log?.Invoke($"[mqtt]连接失败 {host}:{port} -> {result.ResultCode}");
				return false;
			}
		}
		catch (Exception ex)
		{
			//Connection refused = 端口没人监听；超时 = 网络或防火墙
			Log?.Invoke($"[mqtt]连不上 {host}:{port}：{ex.InnerException?.Message ?? ex.Message}");
			Log?.Invoke("[mqtt]检查：broker 起了吗？端口对吗？本机没有 broker 时可用 --broker broker.emqx.io:1883");
			return false;
		}

		//记录
		Log?.Invoke($"[mqtt]已连接 {host}:{port} clientID = {clientID}");
		return true;
	}

	//订阅
	public async Task SubscribeAsync(CancellationToken ct = default, params string[] topics)//param可变参数
	{
		if (topics.Length is 0) return;

		//构造订阅选项
		var b = new MqttClientSubscribeOptionsBuilder();//建造者，用来拼装一次订阅请求
		foreach (string t in topics)
		{
			_ = b.WithTopicFilter(t, MqttQualityOfServiceLevel.AtLeastOnce);//主题过滤器 + Qos
		}

		//发起订阅
		var result = await client.SubscribeAsync(b.Build(), ct);

		//检查订阅
		foreach (var item in result.Items)
		{
			if (item.ResultCode is not MqttClientSubscribeResultCode.GrantedQoS0
				and not MqttClientSubscribeResultCode.GrantedQoS1
				and not MqttClientSubscribeResultCode.GrantedQoS2)
			{
				Log?.Invoke($"[mqtt] 订阅失败：{item.ResultCode}");
			}
		}

		Log?.Invoke($"[mqtt] 订阅 {string.Join(", ", topics)}");
	}

	//序列化 对象 --> 二进制字节数组
	public async Task<bool> PublishAsync(string topic, byte[] payload, CancellationToken ct = default)
	{
		if (!client.IsConnected) { Log?.Invoke($"[mqtt]未连接，发布跳过： {topic}"); return false; }

		var msg = new MqttApplicationMessageBuilder()
			.WithTopic(topic)
			.WithPayload(payload)
			.WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce)
			.WithRetainFlag(false)
			.Build();

		using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
		timeout.CancelAfter(TimeSpan.FromSeconds(5));
		var result = await client.PublishAsync(msg, timeout.Token);
		if (!result.IsSuccess)
		{
			Log?.Invoke($"[mqtt] 发布失败 {topic}: {result.ReasonCode}");
			return false;
		}

		return true;
	}

	//接收
	public bool TryReceive(out string topic, out byte[] payload)
	{
		if (inbox.TryDequeue(out var item))
		{
			topic = item.Topic;
			payload = item.PayLoad;
			return true;
		}

		topic = "";
		payload = [];
		return false;
	}

	public async ValueTask DisposeAsync()
	{
		try
		{
			if (client.IsConnected)
				await client.DisconnectAsync();
		}
		catch
		{
			client.Dispose();//丢弃无用的订阅
		}
	}
}

/*
                    ┌──────────── MqttTransport（收发室）────────────┐
  你的游戏主循环 ──▶ │ PublishAsync ──▶ client（快递员）──▶ Broker    │
                    │                                             │
  你的游戏主循环 ◀── │ TryReceive ◀── inbox（收件箱传送带）◀── client │
                    └─────────────────────────────────────────────┘
                                        ▲
                        后台回调线程 ApplicationMessageReceivedAsync
*/