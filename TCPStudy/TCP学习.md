#### **任务**

完成一个TCP服务器和客户端，完成服务器的监听、客户端的连接，实现全双工连接（服务器给客户端发消息，客户端给服务器发消息），设计消息结构，妥善处理粘包、分包的处理，设计消息的序列化和反序列化方案（了解MTU、IP消息报体等知识）

了解[WebSocket](https://datatracker.ietf.org/doc/html/rfc6455)协议



#### **Socket类**

socket的含义可以表示很多种不同的意思。下文的IPEndPoint类，实际上也就是套接字socket，但是要和Socket类区分开。



套接字socket = （IP地址：端口号），IP区分主机，端口号区分进程

服务器使用的端口号：熟知端口号（0~1023）和登记端口号（1024~49151）

客户端使用的端口号：临时端口号（49152~65535）



（来自SocketType（Enum））在一个Socket可以发送接收数据前，它必须用addressfamily、sockettype、protocoltype来创建



##### AddressFamily （Enum）

指定寻址方案

InterNetwork  = 2 ：IPv4的地址

InterNetworkV6 = 23：IPv6

##### SocketType （Enum）

指定socket类型

stream = 1 ：支持可靠的、双向的、基于连接的字节流



##### ProtocolType（Enum）

socket使用的协议

ip = 0 ：网际协议

ipv4 = 4

ipv6 = 41

tcp = 6

udp = 17



##### Bind方法

如果需要用一个特定的本地终端，使用Bind方法；然后在调用Listen方法前，必须先调用Bind方法

如果不需要用特定的本地终端，可以使用Connect方法，不需要调用Bind方法了

Bind方法，既可以用于无连接的，也可以用于面向连接的协议。



##### Listen方法

将Socket置于侦听状态，传递的参数是挂起的连接队列的最大长度



##### Accept方法

返回并创建一个新的Socket，此socket的RemoteEndPoint方法，可以得到远程主机的网络地址和端口号

在阻塞模式下，accept方法会被阻塞，直到一个连接尝试进入队列中，

如果在非阻塞模式下使用这个方法，而且没有连接请求在排队，则有一个SocketException异常，SocketException.ErrorCode，



##### Receive方法

该方法将数据读入缓冲区中，

int len = receive(Buffer)



##### connect方法







##### IPEndPoint类

套接字，实际上是一个通信端点

将一个网络端点表示为IP地址+端口号

```
IPAddress ipAddress = new IPAddress(new byte[] { 192, 168, 1, 184 });
IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 6688);
```



#### 简单服务器客户端示例

##### 服务端

```
static async Task Main(string[] args)
{
    //建立Socket，并绑定服务器的套接字
    Socket tcpServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    IPAddress ipAddress = new IPAddress(new byte[] { 10, 192, 166, 232 });
    IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 1234);
    tcpServer.Bind(ipEndPoint);
    while (true)
    {
        //开始监听
        tcpServer.Listen(100);
        Console.WriteLine("开始监听");
        Socket client = await tcpServer.AcceptAsync();
        Console.WriteLine("一个客户端连上服务器了");
        Console.WriteLine("客户端的套接字为：" + client.RemoteEndPoint);
        
        Task newGetTask = GetMessage(client);
        Task newSendTask = SendMessage(client);
    }
        
    tcpServer.Close();
}

static async Task GetMessage(Socket client)
{
    byte[] buffer = new byte[1024];
    int length = client.Receive(buffer);
    string message = Encoding.UTF8.GetString(buffer, 0, length);
    Console.WriteLine("收到了客户端的消息：" + message);
}

static async Task SendMessage(Socket client)
{
    client.Send(Encoding.UTF8.GetBytes("这里是服务器"));
}
```

##### 客户端

```
static void Main(string[] args)
{
   	//由服务器的（IP：端口号）建立连接
    Socket tcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    IPAddress ipAddress = new IPAddress(new byte[] { 10, 192, 166, 232 });
    IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 6688);
    tcpClient.Connect(ipEndPoint);
    Console.WriteLine("链接上服务器");
        
    //发送和接收服务器的消息
    tcpClient.Send(Encoding.UTF8.GetBytes("这里是客户端"));
    byte[] buffer = new byte[1024];
    int length = tcpClient.Receive(buffer);
    string data = Encoding.UTF8.GetString(buffer, 0, length);
    Console.WriteLine("收到服务器消息：" + data);
    
    tcpClient.Close();
}
```





#### TCPListener类

作用：侦听来自TCP网络客户端的连接

服务端可以改为：

```
static void Main(string[] args)
{
    //建立Socket，并绑定服务器的套接字
    IPAddress ipAddress = new IPAddress(new byte[] { 10, 192, 166, 232 });
    IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 6688);
**  TcpListener listener = new TcpListener(ipEndPoint);
        
        
    //开始监听
    listener.Start();
    Console.WriteLine("开始监听");
**  TcpClient sender = listener.AcceptTcpClient();
    Console.WriteLine("一个客户端连上服务器了");
**  Console.WriteLine("客户端的套接字为：" + sender.Client.RemoteEndPoint);
        
    //接发消息
    byte[] buffer = new byte[1024];
**  int length = sender.GetStream().Read(buffer, 0, 1024);
    string message = Encoding.UTF8.GetString(buffer, 0, length);
    Console.WriteLine("收到了客户端的消息：" + message);
**  sender.GetStream().Write(Encoding.UTF8.GetBytes("这里是服务器"));
        
    //流结束时要关闭，遵循先开后关的原则
  	sender.Close();
    listener.Stop();
}
```

