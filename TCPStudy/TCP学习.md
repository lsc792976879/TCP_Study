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



#### 代码1：简单服务器客户端示例

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



#### TCPClient类

通过代入IPEndPoint对象，调用Connect进行和服务器的点对点连接，通过GetStream方法返回NetworkStream对象。

```
static void Main(string[] args)
{
    //由服务器的（IP：端口号）建立连接
    Socket tcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    IPAddress ipAddress = new IPAddress(new byte[] { 10, 192, 166, 232 });
    IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 1234);
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

作用：侦听来自TCP网络客户端的连接，Start方法开始侦听，一旦有连接信息，用AcceptTcpClient方法捕获TcpClient对象

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



#### NetworkStream

· 只用用于TCP/IP协议中、是面向连接的、基于流的传递信息，简化Socket开发

· 建立NetworkStream的实例，必须用已经连接的Socket，使用后不会自动关闭提供的Socket，必须在使用构造函数时，指定Socket所有权

· 支持异步读写

· 不支持Position属性或Seek方法来寻找或改变流的位置，

#### TCP网络通讯的粘包、拆包的问题

关键词：sticky，packet，unpacking in communications

粘包的问题是应用层协议开发者的错误设计造成的，忽略了TCP协议的数据传输的核心机制是基于字节流，其本身不含消息包、数据包等概念



1、TCP协议是面向字节流的，可能会组合，分割应用层协议传来的数据

2、应用层协议可能没有定义数据的边界，导致接收方无法拼接数据

我的理解是：“这是第一条数据”，new int[] {2,3,4,5}，”这是第三条数据“，这些会首先全部传入buffer，buffer再传入字节流，因此这三条数据可能会在同一个字节流中。即产生了粘包

Nagle算法，数据进入tcp缓冲区不会立即发送，缓冲区满（填满会把当前待传送的数据，分开传输），或者上一次的数据被确认受到后才发送下一个



二种最常见的解决方案：基于长度；基于终结符

##### 基于长度：

固定长度，所有应用层信息用统一的大小

可变长度，长度可变，但需要应用层协议的协议头加一个负载长度的字段，让它从字节流中分离出来



示例：

HTTP协议的消息边界content-length便是基于此实现的

HTTP使用分块传输（固定长度）机制的时候，http报头不再包含Content-Length，用负载大小为0的作为边界



##### 基于终结符：

在数据包之间设置边界，添加特殊符号，接收端通过这个边界就可以将不同的数据包拆分开。

但是TCP传输的是字节流，任何字节都可能被传输，因此这种方法很危险，而且和正常数据区分很困难。



自定义消息协议类，来封装数据报文



command 指令 	—— 1个字节

Param 参数			—— 1个字节

DataLength			—— 1个int，4字节，表明实际发送的数据长度

MessageData		—— 1个byte[]，全部数据内容消息体

MoreData				—— 1个byte[]，多余的数据



#### MemoryStream类

为系统内存提供流式的读写操作，常作为其他数据流交换时的中间对象操作

它封装一个字节数组，构造实例时使用字节数组做参数，但数组长度无法调整；如果使用默认的无参数，可以用write方法写入，这时的数组长度会自动调整。



#### BinaryWriter类

将基元数据类型写入流，构造时填写需要写入的stream

具体参照其.write方法，支持各种基元数据类型的写入。



#### BinaryReader类

将基元数据类型读出，构造时填写需要读出的stream

.read___()方法

.readInt32() 读取一个int

.readInt64() 读取一个long

.readByte() 读取字节

.readBytes(Int32) 读取指定的字节数

......等等等等





#### 代码2：简单处理分包粘包