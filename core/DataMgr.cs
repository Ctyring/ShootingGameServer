using System.Data;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;
using MySql.Data.MySqlClient;
using ShootingGameServer.Logic;

namespace ShootingGameServer.core;

/// <summary>
/// 封装数据库操作类
/// 实现验证用户名密码、注册、读取角色数据等功能
/// </summary>
public class DataMgr
{
    private MySqlConnection sqlConnection;

    // 单例模式
    public static DataMgr instance;

    //数据库配置
    private static string Database = "game";
    private static string DataSource = "127.0.0.1";
    private static string UserId = "root";
    private static string Password = "root";
    private static string Port = "3306";

    /// <summary>
    /// 构造函数
    /// </summary>
    public DataMgr()
    {
        instance = this;
        Connect();
    }

    /// <summary>
    /// 连接数据库
    /// </summary>
    public void Connect()
    {
        string Conn = "Database=" + Database + ";"
                      + "Data Source=" + DataSource + ";"
                      + "User Id=" + UserId + ";"
                      + "Password=" + Password + ";"
                      + "Port=" + Port + ";";
        sqlConnection = new MySqlConnection(Conn);
        try
        {
            sqlConnection.Open();
        }
        catch (Exception e)
        {
            Console.WriteLine("[DataMgr]数据库连接失败！ " + e.Message);
            throw;
        }
    }

    /// <summary>
    /// 判定安全字符，防sql注入
    /// </summary>
    /// <param name="str">用户输入的数据</param>
    /// <returns>是否为安全字符</returns>
    public bool IsSafeStr(string str)
    {
        return !Regex.IsMatch(str, @"[-|;|,|\/|\(|\)|\[|\]|\}|\{|%|@|\*|!|\']");
    }

    /// <summary>
    /// 是否可以注册该用户
    /// </summary>
    /// <param name="id">用户id</param>
    /// <returns>是否可以注册</returns>
    private bool CanRegister(string id)
    {
        // 防sql注入
        if (!IsSafeStr(id))
        {
            return false;
        }

        // 查询 id 是否存在
        string cmdStr = string.Format("select * from user where id = '{0}';", id);
        MySqlCommand command = new MySqlCommand(cmdStr, sqlConnection);
        try
        {
            MySqlDataReader dataReader = command.ExecuteReader();
            bool hasRows = dataReader.HasRows;
            dataReader.Close();
            // 如果有数据就不能注册，否则能注册
            return !hasRows;
        }
        catch (Exception e)
        {
            Console.WriteLine("[DataMgr]CanRegister Fail" + e);
            return false;
        }
    }

    /// <summary>
    /// 注册
    /// </summary>
    /// <param name="id">用户名</param>
    /// <param name="pw">密码</param>
    /// <returns>注册是否成功</returns>
    public bool Register(string id, string pw)
    {
        // 防止sql注入
        if (!IsSafeStr(id) || !IsSafeStr(pw))
        {
            Console.WriteLine("[DataMgr]Register账号或密码使用了非法字符！");
            return false;
        }
        
        // 判断该账号能否注册
        if (!CanRegister(id))
        {
            Console.WriteLine("[DataMgr]Register该账号不能注册");
            return false;
        }
        
        // 写入数据库User表
        string cmdStr = string.Format("insert into user set id = '{0}', pw = '{1}';", id, pw);
        MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConnection);
        try
        {
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("[DataMgr]Register注册失败： " + e);
            return false;
        }
    }

    public bool CreatePlayer(string id)
    {
        // 防sql注入
        if (!IsSafeStr(id))
        {
            return false;
        }
        
        // 序列化
        IFormatter formatter = new BinaryFormatter();
        MemoryStream stream = new MemoryStream();
        PlayerData playerData = new PlayerData();
        try
        {
            // JsonSerializer.Serialize(stream, playerData);
            formatter.Serialize(stream, playerData);
        }
        catch (Exception e)
        {
            Console.WriteLine("[DataMgr]CreatePlayer序列化失败 " + e);
            throw;
        }

        byte[] byteArr = stream.ToArray();
        // 写入数据库
        string cmdStr = string.Format("insert into player set id = '{0}', data = @data", id);
        MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConnection);
        cmd.Parameters.Add("@data", MySqlDbType.Blob);
        cmd.Parameters[0].Value = byteArr;
        try
        {
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("[DataMgr]CreatePlayer创建角色失败  " + e);
            throw;
        }
    }

    /// <summary>
    /// 登录校验
    /// </summary>
    /// <param name="id">账号</param>
    /// <param name="pw">密码</param>
    /// <returns>登录是否成功</returns>
    public bool CheckPassword(string id, string pw)
    {
        // 防止sql注入
        if (!IsSafeStr(id) || !IsSafeStr(pw))
        {
            Console.WriteLine("[DataMgr]CheckPassword账号或密码使用了非法字符！");
            return false;
        }
        
        // 查询数据库
        string cmdStr = string.Format("select * from user where id = '{0}' and pw = '{1}';", id, pw);
        MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConnection);
        try
        {
            MySqlDataReader dataReader = cmd.ExecuteReader();
            bool hasRows = dataReader.HasRows;
            dataReader.Close();
            return hasRows;
        }
        catch (Exception e)
        {
            Console.WriteLine("[DataMgr]CheckPassword登录校验失败" + e);
            return false;
        }
    }

    public PlayerData GetPlayerData(string id)
    {
        PlayerData playerData = null;
        // 防 sql 注入
        if (!IsSafeStr(id))
        {
            return playerData;
        }
        // 查询
        string cmdStr = String.Format("select * from player where id = '{0}';", id);
        MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConnection);
        cmd.CommandType = CommandType.Text;
        byte[] buffer = new byte[1];
        try
        {
            MySqlDataReader dataReader = cmd.ExecuteReader();
            // 如果数据不存在直接返回null
            if (!dataReader.HasRows)
            {
                dataReader.Close();
                return playerData;
            }

            // 读取数据
            dataReader.Read();
            // 获取data长度，1是data
            long len = dataReader.GetBytes(1, 0, null, 0, 0);
            buffer = new byte[len];
            dataReader.GetBytes(1, 0, buffer, 0, (int) len);
            dataReader.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine("[DataMgr]GetPlayerData获取player信息失败" + e);
            throw;
        }
        // 反序列化
        MemoryStream stream = new MemoryStream(buffer);
        try
        {
            BinaryFormatter formatter = new BinaryFormatter();
            playerData = (PlayerData)formatter.Deserialize(stream);
            // playerData = JsonSerializer.Deserialize<PlayerData>(stream);
            return playerData;
        }
        catch (Exception e)
        {
            Console.WriteLine("[DataMgr]GetPlayerData反序列化失败！" + e);
            throw;
        }
    }

    /// <summary>
    /// 保存角色
    /// </summary>
    /// <param name="player">角色</param>
    /// <returns></returns>
    public bool SavePlayer(Player player)
    {
        string id = player.id;
        PlayerData playerData = player.playerData;
        
        // 序列化
        IFormatter formatter = new BinaryFormatter();
        MemoryStream stream = new MemoryStream();
        try
        {
            formatter.Serialize(stream, playerData);
        }
        catch (Exception e)
        {
            Console.WriteLine("[DataMgr]SavePlayer序列化失败 " + e);
            return false;
        }

        byte[] byteArr = stream.ToArray();
        // 写入数据库
        string cmdStr = string.Format("update player set data = @data where id = '{0}';", player.id);
        MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConnection);
        cmd.Parameters.Add("@data", MySqlDbType.Blob);
        cmd.Parameters[0].Value = byteArr;
        try
        {
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("[DataMgr]SavePlayer写入数据库失败" + e);
            throw;
        }
    }
}