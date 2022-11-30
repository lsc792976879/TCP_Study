using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TCPClient;

public class MyMessagePackage
{
    /// <summary>
    /// 记录即将发来的消息的头部，头部长6字节
    /// command是控制信息
    /// paramter是参数信息
    /// dataLength是文件的大小
    /// </summary>
    public const int HeadLength = 6;
    public byte command;
    public byte parameter;
    public int dataLength; //最大允许内容的完整长度为4GB

    
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
            binaryWriter.Write(this.command);
            binaryWriter.Write(this.parameter);
            binaryWriter.Write(this.dataLength);
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
            this.command = binaryReader.ReadByte();
            this.parameter = binaryReader.ReadByte();
            this.dataLength = binaryReader.ReadInt32();
            int restDataLength = buffer.Length - HeadLength - this.dataLength;
            
            if (restDataLength < 0)
            {
                Console.WriteLine("正文没传输完，拆包失败");
                return buffer;
            }
            
            this.message = binaryReader.ReadBytes(this.dataLength);
            var restMessage = binaryReader.ReadBytes(restDataLength);
            binaryReader.Close();
            memoryStream.Close();
            
            return restMessage;
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
            this.command = binaryReader.ReadByte();
            this.parameter = binaryReader.ReadByte();
            this.dataLength = binaryReader.ReadInt32();
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

public class MyTCPClient
{
    static void Main(string[] args)
    {
        //由服务器的（IP：端口号）建立连接
        IPAddress ipAddress = new IPAddress(new byte[] { 10, 192, 130, 51 });
        IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 1234);
        TcpClient tcpClient = new TcpClient();
        tcpClient.Connect(ipEndPoint);
        Console.WriteLine("链接上服务器");

        
        Task newSendTask = SendMessage(tcpClient);
        Task newGetTask = GetMessage(tcpClient);
        for(int i = 0; i < 100; i++) Thread.Sleep(1000);
        tcpClient.Close();
    }
    
    static async Task SendMessage(TcpClient client)
    {
        MyMessagePackage myPackage = new MyMessagePackage();
        NetworkStream networkStream = client.GetStream();
        myPackage.message = Encoding.UTF8.GetBytes("这里是客户端");
        myPackage.dataLength = myPackage.message.Length;
        myPackage.command = 0;
        myPackage.parameter = 0;
        for (int i = 0; i < 10; i++)
        {
            networkStream.Write(myPackage.ToBytesStream());   
        }
    }
    
    static async Task GetMessage(TcpClient client)
    {
        try
        {
            byte[] buffer = new byte[10];
            byte[] message = new byte[] { };
            int length;
            MyMessagePackage mypackage = new MyMessagePackage();
            NetworkStream fileStream = client.GetStream();

            while ((length = await fileStream.ReadAsync(buffer, 0, 10)) != 0)
            {
                message = AppendMessage(message, 0, message.Length, buffer, 0, length);
                Console.WriteLine("收到字节流，当前缓冲区总长度：" + message.Length);
                
                while (message.Length >= MyMessagePackage.HeadLength)
                {
                    mypackage.GetHeadInfo(message);
                    
                    if (mypackage.dataLength + MyMessagePackage.HeadLength > message.Length) break;
                    message = mypackage.ToMyPackage(message);
                    
                    Console.WriteLine("收到了客户端的消息：" + Encoding.UTF8.GetString(mypackage.message));
                    
                    Console.WriteLine("完成拆包，剩下部分的长度：" + message.Length);
                }
            }
        }
        catch(Exception e)
        {
            Console.WriteLine("\n 读取数据时出现异常 ");
            Console.WriteLine("Source :{0} ", e.Source);
            Console.WriteLine("Message :{0} ", e.Message);
        }
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