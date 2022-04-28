using ShootingGameServer.core;

namespace ShootingGameServer.Logic;

/// <summary>
/// 角色临时数据类
/// </summary>
[Serializable]
public class PlayerTempData
{
    public PlayerTempData()
    {
        
    }

    public enum Status
    {
        None,
        Room,
        Fight
    }

    public Status status;
    public Room room;
    public int team = 1;
    public bool isOwner = false;
    
    // 战场相关
    public long lastUpdateTime;
    public float posX;
    public float posY;
    public float posZ;
    public long lastShootTime;
    public float hp = 100;
}