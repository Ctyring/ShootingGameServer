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
        // 如果玩家在房间内，就要通知下线
        if (player.playerTempData.status == PlayerTempData.Status.Room)
        {
            Room room = player.playerTempData.room;
            RoomMgr.instance.LeaveRoom(player);
            if (room != null)
            {
                room.Broadcast(room.GetRoomInfo());
            }
        }
    }
}