using ShootingGameServer.core;

namespace ShootingGameServer.Logic;

/// <summary>
/// 处理连接协议
/// 一般用来处理登录过程中的逻辑
/// </summary>
public partial class HandleConnMsg
{
    /// <summary>
    /// 心跳处理,无协议参数
    /// </summary>
    /// <param name="conn">连接</param>
    /// <param name="protocol">协议</param>
    public void MsgHeatBeat(Conn conn, ProtocolBase protocol)
    {
        conn.lastTickTime = Sys.GetTimeStamp();
        Console.WriteLine("[更新心跳时间]" + conn.GetAdress());
    }

    /// <summary>
    /// 注册
    /// 协议参数：str 用户名 str 密码
    /// 返回协议：-1表示失败， 0表示成功
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="protocol"></param>
    public void MsgRegister(Conn conn, ProtocolBase protoBase)
    {
        int start = 0;
        ProtocolBytes protocol = (ProtocolBytes) protoBase;
        string protoName = protocol.GetString(start, ref start);
        string id = protocol.GetString(start, ref start);
        string pw = protocol.GetString(start, ref start);
        string strFormat = "[收到注册协议]" + conn.GetAdress();
        Console.WriteLine(strFormat + "用户名：" + id + "密码：" + pw);
        // 构建返回协议
        protocol = new ProtocolBytes();
        protocol.AddString("Register");
        //注册
        if (DataMgr.instance.Register(id, pw))
        {
            protocol.AddInt(0);
        }
        else
        {
            protocol.AddInt(-1);
        }
        // 创建角色
        DataMgr.instance.CreatePlayer(id);
        // 把消息发送给客户端
        conn.Send(protocol);
    }

    /// <summary>
    /// 登录
    /// 协议参数：str 用户名 str 密码
    /// 返回协议：-1表示失败， 0表示成功
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="protoBase"></param>
    public void MsgLogin(Conn conn, ProtocolBase protoBase)
    {
        // 获取参数
        int start = 0;
        ProtocolBytes protocol = (ProtocolBytes) protoBase;
        string protoName = protocol.GetString(start, ref start);
        string id = protocol.GetString(start, ref start);
        string pw = protocol.GetString(start, ref start);
        string strFormat = "[收到登录协议]" + conn.GetAdress();
        Console.WriteLine(strFormat + "用户名：" + id + "密码：" + pw);
        // 构建返回协议
        protocol = new ProtocolBytes();
        protocol.AddString("Login");
        // 验证
        if (!DataMgr.instance.CheckPassword(id, pw))
        {
            protocol.AddInt(-1);
            conn.Send(protocol);
            return;
        }
        // 是否已经登录
        ProtocolBytes protocolLogout = new ProtocolBytes();
        protocolLogout.AddString("Logout");
        if (!Player.KickOff(id, protocolLogout))
        {
            protocol.AddInt(-1);
            conn.Send(protocol);
            return;
        }
        // 获取玩家数据
        PlayerData playerData = DataMgr.instance.GetPlayerData(id);
        if (playerData == null)
        {
            protocol.AddInt(-1);
            conn.Send(protocol);
            return;
        }
        conn.player = new Player(id, conn);
        conn.player.playerData = playerData;
        
        // 事件触发
        ServNet.instance.handlePlayerEvent.OnLogin(conn.player);
        // 返回
        protocol.AddInt(0);
        conn.Send(protocol);
        return;
    }
    
    /// <summary>
    /// 下线
    /// 返回0正常下线
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="protoBase"></param>
    public void MsgLogout(Conn conn, ProtocolBase protoBase)
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("Logout");
        protocol.AddInt(0);
        if (conn.player == null)
        {
            conn.Send(protocol);
            conn.Close();
        }
        else
        {
            conn.Send(protocol);
            conn.player.Logout();
        }
    }

    /// <summary>
    /// 获取分数功能
    /// 协议参数：
    /// 返回协议：int 分数
    /// </summary>
    /// <param name="player"></param>
    /// <param name="protoBase"></param>
    public void MsgGetScore(Player player, ProtocolBase protoBase)
    {
        ProtocolBytes protocolRet = new ProtocolBytes();
        protocolRet.AddString("GetScore");
        protocolRet.AddInt(player.playerData.score);
        player.Send(protocolRet);
        Console.WriteLine("MsgGetScore " + player.id + player.playerData.score);
    }

    /// <summary>
    /// 增加分数，没有协议参数
    /// </summary>
    /// <param name="player"></param>
    /// <param name="protoBase"></param>
    public void MsgAddScore(Player player, ProtocolBase protoBase)
    {
        // 获取数值
        int start = 0;
        ProtocolBytes protocol = (ProtocolBytes) protoBase;
        string protoName = protocol.GetString(start, ref start);
        
        // 处理
        player.playerData.score += 1;
        Console.WriteLine("MsgAddScore" + player.id + " " + player.playerData.score.ToString());
    }
}