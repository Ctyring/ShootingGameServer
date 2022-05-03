using System.ComponentModel;
using MySql.Data.MySqlClient;

namespace ShootingGameServer.mysql;

public class SqlUtil
{
    // 连接数据库
    private static MySqlConnection sqlConn;
    
    //数据库配置
    private static string Database = "game";
    private static string DataSource = "47.117.163.229";
    private static string UserId = "game";
    private static string Password = "wESjJWDisWzfbEt3";
    private static string Port = "3306";
    
    /// <summary>
    /// 初始化数据库配置并开启数据库连接
    /// </summary>
    public static void Init()
    {
        string Conn = "Database=" + Database + ";"
                      + "Data Source=" + DataSource + ";"
                      + "User Id=" + UserId + ";"
                      + "Password=" + Password + ";"
                      + "Port=" + Port + ";"
                      + "Allow User Variables=True;"
                      + "SslMode = none;";
        sqlConn = new MySqlConnection(Conn);
        try
        {
            sqlConn.Open();
        }
        catch (Exception e)
        {
            Console.WriteLine("数据库连接失败！ " + e);
            throw;
        }
    }
}