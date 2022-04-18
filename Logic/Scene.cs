using ShootingGameServer.core;

namespace ShootingGameServer.Logic;

/// <summary>
/// 场景类
/// </summary>
public class Scene
{
    // 单例
    public static Scene instance;

    public Scene()
    {
        instance = this;
    }

    private List<ScenePlayer> list = new List<ScenePlayer>();

    /// <summary>
    /// 根据id查找场景内的玩家
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private ScenePlayer GetScenePlayer(string id)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].id == id)
            {
                return list[i];
            }
        }
        return null;
    }

    /// <summary>
    /// 添加玩家
    /// </summary>
    /// <param name="id"></param>
    public void AddPlayer(string id)
    {
        // 多个线程可能同时操作到表，需要加锁
        lock (list)
        {
            ScenePlayer p = new ScenePlayer();
            p.id = id;
            list.Add(p);
        }
    }

    /// <summary>
    /// 删除玩家
    /// </summary>
    /// <param name="id"></param>
    public void DelPlayer(string id)
    {
        lock (list)
        {
            ScenePlayer p = GetScenePlayer(id);
            if (p != null)
            {
                list.Remove(p);
            }
        }

        // 删除玩家后进行广播
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("PlayerLeave");
        protocol.AddString(id);
        ServNet.instance.Broadcast(protocol);
    }

    /// <summary>
    /// 发送玩家数据列表
    /// </summary>
    /// <param name="player"></param>
    public void SendPlayerList(Player player)
    {
        int Count = list.Count;
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("GetList");
        protocol.AddInt(Count);
        for (int i = 0; i < Count; i++)
        {
            ScenePlayer p = list[i];
            protocol.AddString(p.id);
            protocol.AddFloat(p.x);
            protocol.AddFloat(p.y);
            protocol.AddFloat(p.z);
            protocol.AddInt(p.score);
        }
        player.Send(protocol);
    }

    /// <summary>
    /// 更新角色信息
    /// </summary>
    /// <param name="id"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="score"></param>
    public void UpdateInfo(string id, float x, float y, float z, int score)
    {
        int count = list.Count;
        ProtocolBytes protocol = new ProtocolBytes();
        ScenePlayer p = GetScenePlayer(id);
        if (p == null)
        {
            return;
        }

        p.x = x;
        p.y = y;
        p.z = z;
        p.score = score;
    }
}