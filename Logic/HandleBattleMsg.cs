using ShootingGameServer.core;

namespace ShootingGameServer.Logic;

public partial class HandlePlayerMsg
{
    public void MsgStartFight(Player player, ProtocolBase protocolBase)
    {
        ProtocolBytes protocol = (ProtocolBytes) protocolBase;
        protocol.AddString("StartFight");
        if (player.playerTempData.status != PlayerTempData.Status.Room)
        {
            Console.WriteLine("[StartFight]玩家不在房间!" + player.id);
            protocol.AddInt(-1);
            player.Send(protocol);
            return;
        }

        if (!player.playerTempData.isOwner)
        {
            Console.WriteLine("[StartFight]玩家不是房主" + player.id);
            protocol.AddInt(-1);
            player.Send(protocol);
            return;
        }

        Room room = player.playerTempData.room;
        if (!room.CanStart())
        {
            Console.WriteLine("[StartFight]房间不能开战！" + player.id);
            protocol.AddInt(-1);
            player.Send(protocol);
        }
        
        protocol.AddInt(0);
        player.Send(protocol);
        room.StartFight();
    }

    /// <summary>
    /// 同步角色单元
    /// </summary>
    /// <param name="player"></param>
    /// <param name="protocolBase"></param>
    public void MsgUpdateUnitInfo(Player player, ProtocolBase protocolBase)
    {
        // 获取数值
        int start = 0;
        ProtocolBytes protocol = (ProtocolBytes) protocolBase;
        string name = protocol.GetString(start, ref start);
        float posX = protocol.GetFloat(start, ref start);
        float posY = protocol.GetFloat(start, ref start);
        float posZ = protocol.GetFloat(start, ref start);
        float rotX = protocol.GetFloat(start, ref start);
        float rotY = protocol.GetFloat(start, ref start);
        float rotZ = protocol.GetFloat(start, ref start);
        if (player.playerTempData.status != PlayerTempData.Status.Fight)
        {
            return;
        }

        Room room = player.playerTempData.room;
        player.playerTempData.posX = posX;
        player.playerTempData.posY = posY;
        player.playerTempData.posZ = posZ;
        player.playerTempData.lastUpdateTime = Sys.GetTimeStamp();
        ProtocolBytes protocolRet = new ProtocolBytes();
        protocolRet.AddString("UpdateUnitInfo");
        protocolRet.AddString(player.id);
        protocolRet.AddFloat(posX);
        protocolRet.AddFloat(posY);
        protocolRet.AddFloat(posZ);
        protocolRet.AddFloat(rotX);
        protocolRet.AddFloat(rotY);
        protocolRet.AddFloat(rotZ);
        room.Broadcast(protocolRet);
    }

    /// <summary>
    /// 角色射击
    /// </summary>
    /// <param name="player"></param>
    /// <param name="protocolBase"></param>
    public void MsgShooting(Player player, ProtocolBase protocolBase)
    {
        ProtocolBytes protocol = (ProtocolBytes) protocolBase;
        int start = 0;
        string name = protocol.GetString(start, ref start);
        float posX = protocol.GetFloat(start, ref start);
        float posY = protocol.GetFloat(start, ref start);
        float posZ = protocol.GetFloat(start, ref start);
        float rotX = protocol.GetFloat(start, ref start);
        float rotY = protocol.GetFloat(start, ref start);
        float rotZ = protocol.GetFloat(start, ref start);
        if (player.playerTempData.status != PlayerTempData.Status.Fight)
        {
            return;
        }
        Room room = player.playerTempData.room;
        ProtocolBytes protocolRet = new ProtocolBytes();
        protocolRet.AddString("Shooting");
        protocolRet.AddString(player.id);
        protocolRet.AddFloat(posX);
        protocolRet.AddFloat(posY);
        protocolRet.AddFloat(posZ);
        protocolRet.AddFloat(rotX);
        protocolRet.AddFloat(rotY);
        protocolRet.AddFloat(rotZ);
        room.Broadcast(protocolRet);
    }

    /// <summary>
    /// 伤害处理
    /// </summary>
    /// <param name="player"></param>
    /// <param name="protocolBase"></param>
    public void MsgHit(Player player, ProtocolBase protocolBase)
    {
        ProtocolBytes protocol = (ProtocolBytes) protocolBase;
        int start = 0;
        string name = protocol.GetString(start, ref start);
        string enemyName = protocol.GetString(start, ref start);
        float damage = protocol.GetFloat(start, ref start);
        if (player.playerTempData.status != PlayerTempData.Status.Fight)
        {
            return;
        }
        Room room = player.playerTempData.room;
        if (!room.list.ContainsKey(enemyName))
        {
            Console.WriteLine("[MsgHit]该玩家不在房间内" + enemyName);
        }

        Player enemy = room.list[enemyName];
        if (enemy == null)
        {
            return;
        }

        if (enemy.playerTempData.hp <= 0)
        {
            return;
        }

        enemy.playerTempData.hp -= damage;
        Console.WriteLine("[MsgHit]" + enemy.id + "hp:" + enemy.playerTempData.hp);
        ProtocolBytes protocolRet = new ProtocolBytes();
        protocolRet.AddString("Hit");
        protocolRet.AddString(player.id);
        protocolRet.AddString(enemy.id);
        protocolRet.AddFloat(damage);
        room.Broadcast(protocolRet);
        room.UpdateWin();
    }
}