using ShootingGameServer.core;

namespace ShootingGameServer.Logic;

/// <summary>
/// 房间管理器
/// </summary>
public class RoomMgr
{
    public static RoomMgr instance;

    public RoomMgr()
    {
        instance = this;
    }

    public List<Room> list = new List<Room>();

    /// <summary>
    /// 创建房间
    /// </summary>
    /// <param name="player">房主</param>
    public void CreateRoom(Player player)
    {
        Room room = new Room();
        lock (list)
        {
            list.Add(room);
            room.AddPlayer(player);
        }
    }

    /// <summary>
    /// 玩家离开
    /// </summary>
    /// <param name="player"></param>
    public void LeaveRoom(Player player)
    {
        PlayerTempData tempData = player.playerTempData;
        // 防止反复离开
        if (tempData.status == PlayerTempData.Status.None)
        {
            return;
        }

        Room room = tempData.room;
        lock (list)
        {
            room.DelPlayer(player.id);
            if (room.list.Count == 0)
            {
                list.Remove(room);
            }
        }
    }

    /// <summary>
    /// 输出房间列表
    /// </summary>
    /// <returns></returns>
    public ProtocolBytes GetRoomList()
    {
        Console.WriteLine("[DEBUG] 广播");
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("GetRoomList");
        int count = list.Count;
        protocol.AddInt(count);
        for (int i = 0; i < count; i++)
        {
            Room room = list[i];
            protocol.AddInt(room.list.Count);
            protocol.AddInt((int)room.status);
        }

        return protocol;
    }
}