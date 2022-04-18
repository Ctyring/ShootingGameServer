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
}