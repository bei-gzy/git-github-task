using System.Buffers;//高性能分配以及回收
using System.Collections.Concurrent;//线程安全并发合集

using MQTTnet.Protocol;//定义通信库的规则的语言
using MQTTnet;//第三方通信库

namespace App.NetWork;

//包的内容：主题+载荷
public readonly record struct MqttMessage(string Topic, byte[] PayLoad);

//------------------------------------传输层类-----------------------------------
public sealed class MqttTransport : IAsyncDisposable
{
	readonly IMqttClient client;
	//本质队列维护线程进出安全--进出的都是inbox
	readonly ConcurrentQueue<MqttMessage> inbox = new();
	readonly string clientID;

	//-----Log委托------
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
		client.ApplicationMessageReceivedAsync += e =>
		{
			Log?.Invoke($"[mqtt]断开： {e.ReasonCode}");
			return Task.CompletedTask;
		};
	}

	//异步连接进程
	//委托人（host地址，port端口，CancellationToken default外部取消令牌）Broker的数据
	public async Task ConnectAsync(string host, int port, TimeSpan? keepAlive = null, CancellationToken ct = default)
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

		//等待异步连接（调用）--提示成功
		var result = await client.ConnectAsync(options, timeout.Token);

		if (!result.IsSessionPresent)
		{
			Log?.Invoke($"[mqtt]未连接 {host}:{port} clientID = {clientID}");
			return;
		}

		//记录
		Log?.Invoke($"[mqtt]已连接 {host}:{port} clientID = {clientID}");
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
	public bool TryReceive(out MqttMessage message) => inbox.TryDequeue(out message);

	public async ValueTask DisposeAsync()
	{
		try
		{
			if (client.IsConnected)
				await client.DisconnectAsync();
		}
		catch { }

		client.Dispose();//丢弃无用的订阅
	}
}

//反序列化 字节数组 --> 对象
