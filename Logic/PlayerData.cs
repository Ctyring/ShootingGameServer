namespace ShootingGameServer.Logic;

/// <summary>
/// 角色数据类
/// </summary>
[Serializable]
public class PlayerData
{
    public int score = 0;
    public PlayerData()
    {
        score = 100;
    }

    public PlayerTempData tempData;
}