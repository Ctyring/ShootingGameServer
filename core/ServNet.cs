using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Net.Sockets;

namespace ShootingGameServer.core;

/// <summary>
/// 网络连接处理类
/// </summary>
public class ServNet
{
    // 服务端监听socket
    public Socket listenfd;
    // 客户端连接
    public Conn[] conns;
    // 最大连接数
    public int maxConn = 50;
    // 单例模式 --- 方便调用
    public static ServNet instance;

    /// <summary>
    /// 构造函数
    /// </summary>
    public ServNet()
    {
        instance = this;
    }
    
    /// <summary>
    /// 获取连接池索引
    /// </summary>
    /// <returns>连接池索引，如果获取失败返回负数</returns>
    public int NewIndex()
    {
        if (conns == null)
        {
            return -1;
        }

        for (int i = 0; i < conns.Length; i++)
        {
            if (conns[i] == null)
            {
                conns[i] = new Conn();
                return i;
            }
            else if (conns[i].isUse == false)
            {
                return i;
            }
        }

        return -1;
    }
    
    /// <summary>
    /// 开启服务器
    /// </summary>
    /// <param name="host">ip地址</param>
    /// <param name="port">端口号</param>
    public void Start(string host, int port)
    {
        // 初始化连接池
        conns = new Conn[maxConn];
        for (int i = 0; i < maxConn; i++)
        {
            conns[i] = new Conn();
        }
        
        // Socket
        listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        
        // Bind
        IPAddress ipAddress = IPAddress.Parse(host);
        IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);
        listenfd.Bind(ipEndPoint);
        
        // Listen
        listenfd.Listen(maxConn);
        
        // Accept
        listenfd.BeginAccept(AcceptCb, null);
        Console.WriteLine("服务器启动成功！");
    }

    /// <summary>
    /// Accept 回调函数
    /// </summary>
    /// <param name="ar"></param>
    private void AcceptCb(IAsyncResult ar)
    {
        try
        {
            Socket socket = listenfd.EndAccept(ar);
            int index = NewIndex();
            if (index < 0)
            {
                // 如果连接池已满就拒绝连接
                socket.Close();
                Console.WriteLine("警告！连接已满！");
            }
            else
            {
                // 给新的连接分配连接池对象
                Conn conn = conns[index];
                conn.Init(socket);
                string adr = conn.GetAdress();
                Console.WriteLine("客户端 --- " + adr + " --- 已经连接");
                Console.WriteLine("连接池ID：" + index);
                // 异步接收客户端数据
                conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(),
                    SocketFlags.None, ReceiveCb, conn);
                // 再次调用BeginAccept实现循环
                listenfd.BeginAccept(AcceptCb, null);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("AccepyCb失败：" + e);
            throw;
        }
    }
    
    /// <summary>
    /// Receive 回调函数
    /// </summary>
    /// <param name="ar"></param>
    private void ReceiveCb(IAsyncResult ar)
    {
        //获取BeiginReceive传入的对象
        Conn conn = (Conn)ar.AsyncState;
        try
        {
            // 获取接收的字节数
            int count = conn.socket.EndReceive(ar);
            // 如果信息为 0 则关闭信号
            if (count <= 0)
            {
                Console.WriteLine("收到" + conn.GetAdress() + "断开连接");
                conn.Close();
                return;
            }

            // 粘包分包处理
            conn.buffCount += count;
            ProcessData(conn);

            // 开始数据处理
            string str = System.Text.Encoding.UTF8.GetString(conn.readBuff, 0, count);
            Console.WriteLine("收到" + conn.GetAdress() + "数据：" + str);
            str = conn.GetAdress() + ":" + str;
            byte[] bytes = System.Text.Encoding.Default.GetBytes(str);
            
            // 广播
            for (int i = 0; i < conns.Length; i++)
            {
                if (conns[i] == null)
                {
                    continue;
                }

                if (!conns[i].isUse)
                {
                    continue;
                }
                
                // 只将信息发送给所有正在使用的连接
                Console.WriteLine("将消息转播给 " + conns[i].GetAdress());
                conns[i].socket.Send(bytes);
            }

            // 继续监听接收，实现循环
            conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(), SocketFlags.None, ReceiveCb, conn);
        }
        catch (Exception e)
        {
            Console.WriteLine(conn.GetAdress() +  " 断开连接！" + e);
            conn.Close();
            throw;
        }
    }

    /// <summary>
    /// 判断缓冲区的数据能否满足一条消息所需的数据量
    /// </summary>
    /// <param name="conn">客户端连接</param>
    private void ProcessData(Conn conn)
    {
        // 如果缓冲区的数据长度小于4个字节(sizeof(Int32))，它一定不是一条完整的消息
        if (conn.buffCount < sizeof(Int32))
        {
            return;   
        }
        
        // 如果消息长度大于4个字节，先通过BitConverter获取消息长度
        // 参数说明：将readBuff的前四个字节复制到lenBytes中，就是获取一个完整数据的长度
        Array.Copy(conn.readBuff, conn.lenBytes, sizeof(Int32));
        // 将字节数组转换为int数据类型
        conn.magLength = BitConverter.ToInt32(conn.lenBytes, 0);
        
        // 然后再判断缓冲区长度是否满足需求
        if (conn.buffCount < conn.magLength + sizeof(Int32))
        {
            // 如果不满足就先不处理
            return;
        }
        
        // 如果满足要求则读取和处理消息
        string str = System.Text.Encoding.UTF8.GetString(conn.readBuff, sizeof(Int32), conn.magLength);
        Console.WriteLine("收到消息[" + conn.GetAdress() + "]" + str);
        Send(conn, str);
        
        // 清除已处理的消息
        int count = conn.buffCount - conn.magLength - sizeof(Int32);
        // 把数据向前复制，就覆盖掉了已经处理的数据
        Array.Copy(conn.readBuff, sizeof(Int32) + conn.magLength, conn.readBuff, 0, count);
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="conn">客户端连接</param>
    /// <param name="str">消息内容</param>
    public void Send(Conn conn, string str)
    {
        // 将字符串转换成bytes数组
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);
        // 获取字符串长度
        byte[] length = BitConverter.GetBytes(bytes.Length);
        // 将长度拼接到字符串前面
        byte[] sendBuff = length.Concat(bytes).ToArray();
        try
        {
            conn.socket.BeginSend(sendBuff, 0, sendBuff.Length, SocketFlags.None, null, null);
        }
        catch (Exception e)
        {
            Console.WriteLine("[ServNet]发送消息失败：");
        }
    }

    /// <summary>
    /// 关闭服务端程序
    /// </summary>
    public void Close()
    {
        for (int i = 0; i < conns.Length; i++)
        {
            Conn conn = conns[i];
            if (conn == null)
            {
                continue;
            }

            if (!conn.isUse)
            {
                continue;
            }

            // 使用lock是为了避免线程竞争
            // 服务端框架中至少会有以下线程处理同一连接
            // 主线程(close)
            // 异步 socket 回调
            // 心跳的定时器线程
            lock (conn)
            {
                conn.Close();
            }
        }
    }
}