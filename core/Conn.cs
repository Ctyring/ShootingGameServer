using System.Net.Sockets;

namespace ShootingGameServer.core;

/// <summary>
/// 客户端连接类
/// </summary>
public class Conn
{
    // 常量 缓冲区大小
    public const int BUFFER_SIZE = 1024;
    // 与客户端连接的socket
    public Socket socket;
    // 是否使用
    public bool isUse = false;
    // 读缓冲区
    public byte[] readBuff = new byte[BUFFER_SIZE];
    // 读缓冲区已经占用的大小
    public int buffCount = 0;
    // 粘包分包
    // 粘包分包现象(TCP协议本身的机制)
    // 粘包：如果发送的网络数据包太小，TCP会合并较小的数据包再发送
    // 分包：如果发送的网络数据包比较大，TCP可能会将它拆分成多个
    // 解决方法：在每个数据包前加上字节长度
    public byte[] lenBytes = new byte[sizeof(UInt32)];
    public Int32 magLength = 0;
    // 心跳时间
    public long lastTickTime = long.MinValue;
    // 对应的player
    public Player player;
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public Conn()
    {
        // 初始化读缓冲区
        readBuff = new byte[BUFFER_SIZE];
    }

    /// <summary>
    /// 初始化函数
    /// </summary>
    /// <param name="socket"></param>
    public void Init(Socket socket)
    {
        this.socket = socket;
        isUse = true;
        buffCount = 0;
        // 心跳处理 记录客户端连接时间作为第一次心跳
        lastTickTime = Sys.GetTimeStamp();
    }

    /// <summary>
    /// 获取剩余的缓冲区大小
    /// </summary>
    /// <returns></returns>
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

        if (player != null)
        {
            // 玩家退出处理，稍后实现
            return;
        }
        
        Console.WriteLine("断开连接:" + GetAdress());
        socket.Close();
        isUse = false;
    }

    /// <summary>
    /// 发送协议
    /// </summary>
    /// <param name="protocolBase"></param>
    public void Send(ProtocolBase protocolBase)
    {
        ServNet.instance.Send(this, protocolBase);
    }
}