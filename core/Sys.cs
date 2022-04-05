namespace ShootingGameServer.core;

public class Sys
{
    /// <summary>
    /// 记录时间戳
    /// </summary>
    /// <returns>返回时间戳</returns>
    public static long GetTimeStamp()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalSeconds);
    }
}