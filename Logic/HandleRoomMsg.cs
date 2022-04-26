using ShootingGameServer.core;

namespace ShootingGameServer.Logic;

/// <summary>
/// player消息中的和room相关的部分
/// </summary>
public partial class HandlePlayerMsg
{
    /// <summary>
    /// 获取房间列表
    /// </summary>
    /// <param name="player"></param>
    /// <param name="protocolBase"></param>
    public void MsgGetRoomList(Player player, ProtocolBase protocolBase)
    {
        player.Send(RoomMgr.instance.GetRoomList());
    }

    /// <summary>
    /// 创建房间
    /// </summary>
    /// <param name="player"></param>
    /// <param name="protocolBase"></param>
    public void MsgCreateRoom(Player player, ProtocolBase protocolBase)
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("CreateRoom");
        // 条件检测
        if (player.playerTempData.status != PlayerTempData.Status.None)
        {
            Console.WriteLine("[MsgCreateRoom] 创建房间失败!" + player.id);
            protocol.AddInt(-1);
            player.Send(protocol);
            return;
        }
        RoomMgr.instance.CreateRoom(player);
        protocol.AddInt(0);
        player.Send(protocol);
        Console.WriteLine("[MsgCreateRoom] 创建房间成功!" + player.id);
        return;
    }

    /// <summary>
    /// 加入房间
    /// </summary>
    /// <param name="player"></param>
    /// <param name="protocolBase"></param>
    public void MsgEnterRoom(Player player, ProtocolBase protocolBase)
    {
        // 获取数值
        ProtocolBytes protocol = (ProtocolBytes) protocolBase;
        int start = 0;
        string name = protocol.GetString(start, ref start);
        int index = protocol.GetInt(start, ref start);
        Console.WriteLine("[EnterRoom]" + player.id + "【房间号】" + index);
        protocol = new ProtocolBytes();
        protocol.AddString("EnterRoom");
        // 判断房间是否存在
        if (index < 0 || index >= RoomMgr.instance.list.Count)
        {
            Console.WriteLine("[EnterRoom] 房间不存在！" + player.id);
            protocol.AddInt(-1);
            player.Send(protocol);
            return;
        }

        Room room = RoomMgr.instance.list[index];
        // 判断房间状态
        if (room.status != Room.Status.Prepare)
        {
            Console.WriteLine("[EnterRoom] 房间不在准备状态！" + player.id);
            protocol.AddInt(-1);
            player.Send(protocol);
            return;
        }
        // 添加玩家
        if (room.AddPlayer(player))
        {
            room.Broadcast(room.GetRoomInfo());
            protocol.AddInt(0);
            player.Send(protocol);
        }
        else
        {
            Console.WriteLine("[EnterRoom] 房间添加玩家失败！" + player.id);
            protocol.AddInt(-1);
            player.Send(protocol);
            return;
        }
    }

    /// <summary>
    /// 获取房间信息
    /// </summary>
    /// <param name="player"></param>
    /// <param name="protocolBase"></param>
    public void MsgGetRoomInfo(Player player, ProtocolBase protocolBase)
    {
        // 如果玩家不在房间，直接return
        if (player.playerTempData.status != PlayerTempData.Status.Room)
        {
            Console.WriteLine("[GetRoomInfo] 玩家不在房间" + player.id);
            return;
        }

        Room room = player.playerTempData.room;
        player.Send(room.GetRoomInfo());
    }

    /// <summary>
    /// 离开房间
    /// </summary>
    /// <param name="player"></param>
    /// <param name="protocolBase"></param>
    public void MsgLeaveRoom(Player player, ProtocolBase protocolBase)
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("LeaveRoom");
        // 同样判断是否还在房间
        if (player.playerTempData.status != PlayerTempData.Status.Room)
        {
            Console.WriteLine("[LeaveRoom] 已经离开房间！" + player.id);
            protocol.AddInt(-1);
            player.Send(protocol);
            return;
        }
        // 处理消息
        protocol.AddInt(0);
        player.Send(protocol);
        Room room = player.playerTempData.room;
        RoomMgr.instance.LeaveRoom(player);
        // 广播
        if (room != null)
        {
            room.Broadcast(room.GetRoomInfo());
        }
    }
}