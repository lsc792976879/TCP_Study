using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TCPClient;

public class MyTCPClient
{
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
}