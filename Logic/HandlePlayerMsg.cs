using ShootingGameServer.core;

namespace ShootingGameServer.Logic;

/// <summary>
/// 处理角色协议
/// 一般用于处理游戏逻辑
/// </summary>
public partial class HandlePlayerMsg
{
    /// <summary>
    /// 获取玩家数据列表
    /// </summary>
    /// <param name="player"></param>
    /// <param name="protoBase"></param>
    public void MsgGetList(Player player, ProtocolBase protoBase)
    {
        Scene.instance.SendPlayerList(player);
    }

    /// <summary>
    /// 更新玩家信息
    /// </summary>
    /// <param name="player"></param>
    /// <param name="protoBase"></param>
    public void MsgUpdateInfo(Player player, ProtocolBase protoBase)
    {
        // 获取数据
        int start = 0;
        ProtocolBytes protocol = (ProtocolBytes) protoBase;
        string protoName = protocol.GetString(start, ref start);
        float x = protocol.GetFloat(start, ref start);
        float y = protocol.GetFloat(start, ref start);
        float z = protocol.GetFloat(start, ref start);
        int score = player.playerData.score;
        Scene.instance.UpdateInfo(player.id, x, y, z, score);
        // 进行广播
        ProtocolBytes protocolRet = new ProtocolBytes();
        protocolRet.AddString("UpdateInfo");
        protocolRet.AddString(player.id);
        protocolRet.AddFloat(x);
        protocolRet.AddFloat(y);
        protocolRet.AddFloat(z);
        protocolRet.AddInt(score);
        ServNet.instance.Broadcast(protocolRet);
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

    /// <summary>
    /// 查询成绩
    /// </summary>
    /// <param name="player"></param>
    /// <param name="protocol"></param>
    public void MsgGetAchieve(Player player, ProtocolBase protocolBase)
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("GetAchieve");
        protocol.AddInt(player.playerData.win);
        protocol.AddInt(player.playerData.fail);
        player.Send(protocol);
        Console.WriteLine("[MsgGetAchieve] " + player.id + player.playerData.win);
    }
}