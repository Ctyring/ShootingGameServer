using ShootingGameServer.Logic;

namespace ShootingGameServer.core;

public class Player
{
    // id、连接、玩家数据
    public string id;
    public Conn conn;
    public PlayerData playerData;
    public PlayerTempData playerTempData;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="id">角色id</param>
    /// <param name="conn">连接</param>
    public Player(string id, Conn conn)
    {
        this.id = id;
        this.conn = conn;
        playerTempData = new PlayerTempData();
    }

    /// <summary>
    /// 发送消息方法，是对ServNet类中的Send方法的封装
    /// </summary>
    /// <param name="protocol"></param>
    public void Send(ProtocolBase protocol)
    {
        if (conn == null)
        {
            return;
        }
        ServNet.instance.Send(conn, protocol);
    }

    /// <summary>
    /// 将玩家踢下线
    /// </summary>
    /// <param name="id">被踢下线玩家的id</param>
    /// <param name="protocol">要给玩家发送怎样的消息</param>
    /// <returns></returns>
    public static bool KickOff(string id, ProtocolBase protocol)
    {
        Conn[] conns = ServNet.instance.conns;
        for (int i = 0; i < conns.Length; i++)
        {
            if (conns[i] == null)
            {
                continue;
            }

            if (!conns[i].isUse)
            {
                continue;
            }

            if (conns[i].player == null)
            {
                continue;
            }

            if (conns[i].player.id == id)
            {
                lock (conns[i].player)
                {
                    if (protocol != null)
                    {
                        // 发送消息
                        conns[i].player.Send(protocol);
                    }

                    // 下载并保存数据
                    return conns[i].player.Logout();
                }
            }
        }

        return true;
    }

    /// <summary>
    /// 角色下线
    /// </summary>
    /// <returns></returns>
    public bool Logout()
    {
        // 事件处理，稍后实现
        // ServNet.instance.handlePlayerEvent.OnLogout(this);

        // 先保存数据
        if (!DataMgr.instance.SavePlayer(this))
        {
            return false;
        }

        // 角色下线
        conn.player = null;
        conn.Close();
        return true;
    }
}