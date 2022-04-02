using System.Net.Sockets;

namespace ShootingGameServer.Server;

/// <summary>
/// 客户端链接类
/// </summary>
public class Connector
{
    // 常量 缓冲区大小
    public const int BUFFER_SIZE = 1024;
    // Socket
    public Socket socket;
    // 是否使用
    public bool isUse = false;
    // Buff 缓冲区
    public byte[] readBuff = new byte[BUFFER_SIZE];
    public int buffCount = 0;

    /// <summary>
    /// 构造函数
    /// </summary>
    public Connector()
    {
        readBuff = new byte[BUFFER_SIZE];
    }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="socket"></param>
    public void Init(Socket socket)
    {
        this.socket = socket;
        isUse = true;
        buffCount = 0;
    }

    /// <summary>
    /// 计算缓冲区剩余字节数
    /// </summary>
    /// <returns>缓冲区剩余字节数</returns>
    public int BuffRemain()
    {
        return BUFFER_SIZE - buffCount;
    }

    /// <summary>
    /// 获取客户端ip地址和端口
    /// </summary>
    /// <returns>客户端ip地址和端口，失败返回无法获取地址</returns>
    public string GetAdress()
    {
        if (!isUse)
        {
            return "无法获取地址！";
        }

        return socket.RemoteEndPoint.ToString();
    }

    /// <summary>
    /// 关闭连接
    /// </summary>
    public void Close()
    {
        if (!isUse)
        {
            return;
        }
        Console.WriteLine("断开连接:" + GetAdress());
        socket.Close();
        isUse = false;
    }
}