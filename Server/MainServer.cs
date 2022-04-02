using System.Net;
using System.Net.Sockets;

namespace ShootingGameServer.Server;
/// <summary>
/// 异步Socket类
/// </summary>
public class MainServer
{
    // 服务端socket
    public Socket listenfd;
    // 客户端连接 对象池
    public Connector[] connectors;
    // 最大连接数
    public int maxConn = 50;

    /// <summary>
    /// 获取连接池索引
    /// </summary>
    /// <returns>连接池索引，如果获取失败返回负数</returns>
    public int NewIndex()
    {
        if (connectors == null)
        {
            return -1;
        }

        for (int i = 0; i < connectors.Length; i++)
        {
            if (connectors[i] == null)
            {
                connectors[i] = new Connector();
                return i;
            }
            else if (connectors[i].isUse == false)
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
        connectors = new Connector[maxConn];
        for (int i = 0; i < maxConn; i++)
        {
            connectors[i] = new Connector();
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
                Connector connector = connectors[index];
                connector.Init(socket);
                string adr = connector.GetAdress();
                Console.WriteLine("客户端 --- " + adr + " --- 已经连接");
                Console.WriteLine("连接池ID：" + index);
                // 异步接收客户端数据
                connector.socket.BeginReceive(connector.readBuff, connector.buffCount, connector.BuffRemain(),
                    SocketFlags.None, ReceiveCb, connector);
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
        Connector connector = (Connector)ar.AsyncState;
        try
        {
            // 获取接收的字节数
            int count = connector.socket.EndReceive(ar);
            // 如果信息为 0 则关闭信号
            if (count <= 0)
            {
                Console.WriteLine("收到" + connector.GetAdress() + "断开连接");
                connector.Close();
                return;
            }

            // 开始数据处理
            string str = System.Text.Encoding.UTF8.GetString(connector.readBuff, 0, count);
            Console.WriteLine("收到" + connector.GetAdress() + "数据：" + str);
            str = connector.GetAdress() + ":" + str;
            byte[] bytes = System.Text.Encoding.Default.GetBytes(str);
            
            // 广播
            for (int i = 0; i < connectors.Length; i++)
            {
                if (connectors[i] == null)
                {
                    continue;
                }

                if (!connectors[i].isUse)
                {
                    continue;
                }
                
                // 只将信息发送给所有正在使用的连接
                Console.WriteLine("将消息转播给 " + connectors[i].GetAdress());
                connectors[i].socket.Send(bytes);
            }

            // 继续监听接收，实现循环
            connector.socket.BeginReceive(connector.readBuff, connector.buffCount, connector.BuffRemain(), SocketFlags.None, ReceiveCb, connector);
        }
        catch (Exception e)
        {
            Console.WriteLine(connector.GetAdress() +  " 断开连接！" + e);
            connector.Close();
            throw;
        }
    }
}