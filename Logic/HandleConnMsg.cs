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
}