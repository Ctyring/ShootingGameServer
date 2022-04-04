namespace ShootingGameServer.Logic;

[Serializable]
public class PlayerData
{
    public int score = 0;
    public PlayerData()
    {
        score = 100;
    }
}