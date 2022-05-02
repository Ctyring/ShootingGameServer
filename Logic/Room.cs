using ShootingGameServer.Logic;

namespace ShootingGameServer.core;

/// <summary>
/// 房间类
/// </summary>
public class Room
{
    public enum Status
    {
        Prepare = 1,
        Fight = 2
    }

    public Status status = Status.Prepare;

    // 最大玩家数量
    public int maxPlayers = 6;
    public Dictionary<string, Player> list = new Dictionary<string, Player>();

    /// <summary>
    /// 添加玩家
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public bool AddPlayer(Player player)
    {
        lock (list)
        {
            if (list.Count >= 6)
            {
                return false;
            }

            PlayerTempData tempData = player.playerTempData;
            tempData.room = this;
            tempData.team = SwitchTeam();
            tempData.status = PlayerTempData.Status.Room;
            if (list.Count == 0)
            {
                // 如果没有人，这个玩家就是房主
                tempData.isOwner = true;
            }

            player.playerTempData = tempData;
            string id = player.id;
            list.Add(id, player);
        }

        return true;
    }

    /// <summary>
    /// 分配队伍
    /// </summary>
    public int SwitchTeam()
    {
        int count1 = 0;
        int count2 = 0;
        foreach (Player player in list.Values)
        {
            if (player.playerTempData.team == 1)
            {
                count1++;
            }

            if (player.playerTempData.team == 2)
            {
                count2++;
            }
        }

        if (count1 <= count2)
        {
            return 1;
        }

        return 2;
    }

    /// <summary>
    /// 删除玩家
    /// </summary>
    /// <param name="id"></param>
    public void DelPlayer(string id)
    {
        lock (list)
        {
            if (!list.ContainsKey(id))
            {
                return;
            }

            bool isOwner = list[id].playerTempData.isOwner;
            list[id].playerTempData.status = PlayerTempData.Status.None;
            list.Remove(id);
            if (isOwner)
            {
                UpdateOwner();
            }
        }
    }

    /// <summary>
    /// 胜负判断
    /// </summary>
    /// <returns>0 未分出胜负 1 阵营1获胜 2 阵营2获胜</returns>
    private int IsWin()
    {
        if (status != Status.Fight)
        {
            return 0;
        }

        int count1 = 0;
        int count2 = 0;
        foreach (Player player in list.Values)
        {
            PlayerTempData ptd = player.playerTempData;
            if (ptd.team == 1 && ptd.hp > 0)
            {
                count1++;
            }

            if (ptd.team == 2 && ptd.hp > 0)
            {
                count2++;
            }
        }
        if (count1 <= 0)
        {
            return 2;
        }

        if (count2 <= 0)
        {
            return 1;
        }

        return 0;
    }

    /// <summary>
    /// 处理战斗结果
    /// </summary>
    public void UpdateWin()
    {
        int isWin = IsWin();
        if (isWin == 0)
        {
            return;
        }

        lock (list)
        {
            status = Status.Prepare;
        }

        foreach (Player player in list.Values)
        {
            player.playerTempData.status = PlayerTempData.Status.Room;
            if (player.playerTempData.team == isWin)
            {
                player.playerData.win++;
            }
            else
            {
                player.playerData.fail++;
            }
        }

        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("Result");
        protocol.AddInt(isWin);
        Broadcast(protocol);
    }
    /// <summary>
    /// 更换房主
    /// </summary>
    public void UpdateOwner()
    {
        lock (list)
        {
            if (list.Count <= 0)
            {
                return;
            }

            foreach (Player player in list.Values)
            {
                player.playerTempData.isOwner = false;
            }

            Player p = list.Values.First();
            p.playerTempData.isOwner = true;
        }
    }

    /// <summary>
    /// 房间内广播
    /// </summary>
    /// <param name="protocolBase"></param>
    public void Broadcast(ProtocolBase protocol)
    {
        foreach (Player player in list.Values)
        {
            player.Send(protocol);
        }
    }

    /// <summary>
    /// 输出房间信息
    /// </summary>
    /// <returns></returns>
    public ProtocolBytes GetRoomInfo()
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("GetRoomInfo");
        protocol.AddInt(list.Count);
        foreach (Player player in list.Values)
        {
            protocol.AddString(player.id);
            protocol.AddInt(player.playerTempData.team);
            protocol.AddInt(player.playerData.win);
            protocol.AddInt(player.playerData.fail);
            int isOwner = player.playerTempData.isOwner ? 1 : 0;
            protocol.AddInt(isOwner);
        }

        return protocol;
    }

    /// <summary>
    /// 判断房间是否能开战
    /// </summary>
    /// <returns></returns>
    public bool CanStart()
    {
        if (status != Status.Prepare)
        {
            return false;
        }

        int count1 = 0;
        int count2 = 0;
        foreach (Player player in list.Values)
        {
            if (player.playerTempData.team == 1)
            {
                count1++;
            }

            if (player.playerTempData.team == 2)
            {
                count2++;
            }
        }

        if (count1 < 1 || count2 < 1)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 初始化战场
    /// 将房间状态设置为Fight
    /// 初始化每个角色的生命值
    /// 计算每个角色的初始位置
    /// 改变房间内玩家的状态
    /// </summary>
    public void StartFight()
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("Fight");
        status = Status.Prepare;
        int teamPos1 = 1;
        int teamPos2 = 1;
        lock (list)
        {
            protocol.AddInt(list.Count);
            foreach (Player player in list.Values)
            {
                player.playerTempData.hp = 200;
                protocol.AddString(player.id);
                protocol.AddInt(player.playerTempData.team);
                if (player.playerTempData.team == 1)
                {
                    protocol.AddInt(teamPos1++);
                }
                else
                {
                    protocol.AddInt(teamPos2++);
                }

                player.playerTempData.status = PlayerTempData.Status.Fight;
            }

            Broadcast(protocol);
        }
    }
}