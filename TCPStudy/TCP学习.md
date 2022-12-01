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

服务器

```
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TCPServer;

public class MyMessagePackage
{
    /// <summary>
    /// 记录即将发来的消息的头部，头部长6字节
    /// command是控制信息
    /// paramter是参数信息
    /// dataLength是文件的大小
    /// </summary>
    public const int HeadLength = 6;
    private byte _command;
    private byte _parameter;
    private int _dataLength; //最大允许内容的完整长度为4GB

    public int Command
    {
        get { return _command; }
    }

    public int Parameter
    {
        get { return _parameter; }
    }

    public int DataLength
    {
        get { return _dataLength; }
    }
    
    /// <summary>
    /// message用于存该包的数据
    /// </summary>
    public byte[] message;

    /// <summary>
    /// 将头部和数据转字节流，以便封包发送
    /// </summary>
    /// <returns></returns>
    public byte[] ToBytesStream()
    {
        try
        {
            //数据转字节流
            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
            binaryWriter.Write(this._command);
            binaryWriter.Write(this._parameter);
            binaryWriter.Write(this._dataLength);
            binaryWriter.Write(this.message);
            
            var bytesStream = memoryStream.ToArray();
            binaryWriter.Close();
            memoryStream.Close();
            return bytesStream;
        }
        catch (Exception e)
        {
            Console.WriteLine("\n 封包时出现异常 ");
            Console.WriteLine("Source :{0} ", e.Source);
            Console.WriteLine("Message :{0} ", e.Message);
            return (new byte[0]);
        }
    }

    /// <summary>
    /// 拆包，将属于本数据的字节拆出来
    /// </summary>
    /// <param name="buffer"></param>
    public byte[] ToMyPackage(byte[] buffer)
    {
        try
        {
            if (buffer == null || buffer.Length < 6)
            {
                Console.WriteLine("头部没传输完，拆包失败");
                return buffer;
            }
            
            MemoryStream memoryStream = new MemoryStream(buffer);
            BinaryReader binaryReader = new BinaryReader(memoryStream);
            this._command = binaryReader.ReadByte();
            this._parameter = binaryReader.ReadByte();
            this._dataLength = binaryReader.ReadInt32();
            int restDataLength = buffer.Length - HeadLength - this._dataLength;
            
            if (restDataLength < 0)
            {
                Console.WriteLine("正文没传输完，拆包失败");
                return buffer;
            }
            
            this.message = binaryReader.ReadBytes(this._dataLength);
            var restMessage = binaryReader.ReadBytes(restDataLength);
            binaryReader.Close();
            memoryStream.Close();
            
            return memoryStream.ToArray();
        }
        catch (Exception e)
        {
            Console.WriteLine("\n 拆包时出现异常 ");
            Console.WriteLine("Source :{0} ", e.Source);
            Console.WriteLine("Message :{0} ", e.Message);
            return buffer;
        }
    }
    
    /// <summary>
    /// 获取头部信息存储
    /// </summary>
    /// <param name="buffer"></param>
    public void GetHeadInfo(byte[] buffer)
    {
        try
        {
            if (buffer == null || buffer.Length < 6)
            {
                Console.WriteLine("头部没传输完，读取头部失败");
            }
            MemoryStream memoryStream = new MemoryStream(buffer);
            BinaryReader binaryReader = new BinaryReader(memoryStream);
            this._command = binaryReader.ReadByte();
            this._parameter = binaryReader.ReadByte();
            this._dataLength = binaryReader.ReadInt32();
            binaryReader.Close();
            memoryStream.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine("\n 获取头部信息时出现异常 ");
            Console.WriteLine("Source :{0} ", e.Source);
            Console.WriteLine("Message :{0} ", e.Message);
        }
    }
    
}

public class MyTCPServer
{
    
    static async Task Main(string[] args)
    {
        //建立Socket，并绑定服务器的套接字
        IPAddress ipAddress = new IPAddress(new byte[] { 10, 192, 130, 51 });
        IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 1234);
        TcpListener listener = new TcpListener(ipEndPoint);
        while (true)
        {
            //开始监听
            listener.Start(10);
            Console.WriteLine("开始监听");
            TcpClient client = await listener.AcceptTcpClientAsync();
            Console.WriteLine("一个客户端连上了服务器");
            Console.WriteLine("客户端的套接字为：" + client.Client.RemoteEndPoint);
            
            Task newGetTask = GetMessage(client);
            Task newSendTask = SendMessage(client);
        }
    }

    static async Task GetMessage(TcpClient client)
    {
        try
        {
            byte[] buffer = new byte[1024];
            byte[] message = new byte[] { };
            int length;
            MyMessagePackage mypackage = new MyMessagePackage();
            NetworkStream fileStream = client.GetStream();

            while ((length = await fileStream.ReadAsync(buffer, 0, 1024)) != 0)
            {
                message = AppendMessage(message, 0, message.Length, buffer, 0, length);

                if (message.Length < MyMessagePackage.HeadLength) continue;
                mypackage.GetHeadInfo(message);

                if (mypackage.DataLength + MyMessagePackage.HeadLength < message.Length) break;
                message = mypackage.ToMyPackage(message);

                Console.WriteLine("收到了客户端的消息：" + Encoding.Unicode.GetString(mypackage.message));
            }
        }
        catch(Exception e)
        {
            Console.WriteLine("\n 读取数据时出现异常 ");
            Console.WriteLine("Source :{0} ", e.Source);
            Console.WriteLine("Message :{0} ", e.Message);
        }
    }

    static async Task SendMessage(TcpClient client)
    {
        client.GetStream().Write(Encoding.UTF8.GetBytes("这里是服务器"));
    }

    static private byte[] AppendMessage(byte[] previousMessage, int start, int end, byte[] comingMessage, int start2, int end2)
    {
        MemoryStream memoryStream = new MemoryStream();
        memoryStream.Write(previousMessage,start,end);
        memoryStream.Write(comingMessage,start2,end2);
        var result = memoryStream.ToArray();
        memoryStream.Close();
        return result;
    }
}
```





#### 大端小端（big/small Endian）

大端：低地址位 存储值的高位

小端：低地址位 存储值的低位

例如 0x12345678

大端模式下，低地址是12 ，高地址是78。先传过去的是12，再是34，再是56，最后传输的是78

小端模式下，低地址是78，高地址是12。

所有网络协议都是采用大端方式传输数据的，所有大端方式又称为网络字节序。



BitConverter.IsLittleEndian 可以知道C#在windows上是小端模式存储

binarywriter，通过把int x = 864 = 0x03 96 写入stream，把stream转为byte[]，再输出可以发现

低地址：bytes[0] = 96 

高地址：bytes[1] = 3

所以binarywriter写入，采用的小端模式

在BinaryWriter.cs中搜Endian，只搜得到Write***LittleEndian的关键字，是把value作为小端写入

通过看底层的定义可知，调用的write是把value作为小端，Writes an [Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32?view=net-6.0) into a span of bytes, as little endian.

![1669862523184](C:\Users\lsc\AppData\Roaming\Typora\typora-user-images\1669862523184.png)

![1669864418043](C:\Users\lsc\AppData\Roaming\Typora\typora-user-images\1669864418043.png)



补充：内联函数



binaryread.readBytes的实现，好像就是直接read，不考虑端序，因此我传过来的是小端端序的，如果客户端的是大端端序的话，就需要做个翻转

![1669864052221](C:\Users\lsc\AppData\Roaming\Typora\typora-user-images\1669864052221.png)