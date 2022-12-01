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
    public const int HeadLength = 7;
    public byte command;
    public byte parameter;
    public byte isLittleEndian;
    public int dataLength; //最大允许内容的完整长度为4GB
    
    
    /// <summary>
    /// message用于存该包的数据
    /// </summary>
    public byte[] message;

    public MyMessagePackage()
    {
        this.isLittleEndian =(byte)( BitConverter.IsLittleEndian ? 1 : 0);
    }

    public MyMessagePackage(byte command, byte parameter, byte[] message)
    {
        this.command = command;
        this.parameter = parameter;
        this.message = message;
        this.isLittleEndian =(byte)( BitConverter.IsLittleEndian ? 1 : 0);
        this.dataLength = message.Length;
    }

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
            binaryWriter.Write(this.isLittleEndian);
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
            if (buffer == null || buffer.Length < MyMessagePackage.HeadLength)
            {
                Console.WriteLine("头部没传输完，拆包失败");
                return buffer;
            }
            
            MemoryStream memoryStream = new MemoryStream(buffer);
            BinaryReader binaryReader = new BinaryReader(memoryStream);
            for (int i = 0; i < MyMessagePackage.HeadLength; i++) binaryReader.ReadByte();
            
            this.message = binaryReader.ReadBytes(this.dataLength);
            
            int restDataLength = buffer.Length - HeadLength - this.dataLength;
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
            if (buffer == null || buffer.Length < MyMessagePackage.HeadLength)
            {
                Console.WriteLine("头部没传输完，读取头部失败");
            }
            MemoryStream memoryStream = new MemoryStream(buffer);
            BinaryReader binaryReader = new BinaryReader(memoryStream);
            this.command = binaryReader.ReadByte();
            this.parameter = binaryReader.ReadByte();
            this.isLittleEndian = binaryReader.ReadByte();
            this.dataLength = binaryReader.ReadInt32();
            if (this.isLittleEndian != (byte) (BitConverter.IsLittleEndian ? 1 : 0))
            {
                var tmp = BitConverter.GetBytes(this.dataLength);
                Array.Reverse(tmp);
                this.dataLength = BitConverter.ToInt32(tmp);
            }
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
            byte[] buffer = new byte[10];
            byte[] message = new byte[] { };
            int length;
            MyMessagePackage mypackage = new MyMessagePackage();
            NetworkStream fileStream = client.GetStream();

            while ((length = await fileStream.ReadAsync(buffer, 0, 10)) != 0)
            {
                message = AppendMessage(message, 0, message.Length, buffer, 0, length);
                while (message.Length >= MyMessagePackage.HeadLength)
                {
                    mypackage.GetHeadInfo(message);
                    if (mypackage.dataLength + MyMessagePackage.HeadLength > message.Length) break;
                    message = mypackage.ToMyPackage(message);
                    Console.WriteLine("收到了客户端的消息：{0}" , Encoding.UTF8.GetString(mypackage.message));
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

    static async Task SendMessage(TcpClient client)
    {
        MyMessagePackage myPackage = new MyMessagePackage(0,0,Encoding.UTF8.GetBytes("这里是服务器"));
        NetworkStream networkStream = client.GetStream();
        for (int i = 0; i < 1000; i++)
        {
            networkStream.Write(myPackage.ToBytesStream());   
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