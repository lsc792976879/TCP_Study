using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TCPServer;

public class MyTCPServer
{
    static async Task Main(string[] args)
    {
        //建立Socket，并绑定服务器的套接字
        IPAddress ipAddress = new IPAddress(new byte[] { 10, 192, 166, 232 });
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
        byte[] buffer = new byte[1024];
        int length;
        StringBuilder message = new StringBuilder();
        NetworkStream fileStream = client.GetStream();
        while ((length = await fileStream.ReadAsync(buffer, 0, 1024)) != 0)
        {
            message.Append(Encoding.UTF8.GetString(buffer, 0, length));   
        }
        
        Console.WriteLine("收到了客户端的消息：" + message.ToString());
    }

    static async Task SendMessage(TcpClient client)
    {
        client.GetStream().Write(Encoding.UTF8.GetBytes("这里是服务器"));
    }
}