using ShootingGameServer.core;

namespace ShootingGameServer.Logic;

/// <summary>
/// 玩家事件类
/// </summary>
public class HandlePlayerEvent
{
    /// <summary>
    /// 上线
    /// </summary>
    /// <param name="player"></param>
    public void OnLogin(Player player)
    {
        Scene.instance.AddPlayer(player.id);
    }

    /// <summary>
    /// 下线
    /// </summary>
    /// <param name="player"></param>
    public void OnLogout(Player player)
    {
        Scene.instance.DelPlayer(player.id);
    }
}