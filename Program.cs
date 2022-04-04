// See https://aka.ms/new-console-template for more information

using System.Data;
using MySql.Data.MySqlClient;
using ShootingGameServer.core;
using ShootingGameServer.Logic;
using ShootingGameServer.mysql;
using ShootingGameServer.Server;

class MainClass
{
    public static void Main(string[] args)
    {
        DataMgr dataMgr = new DataMgr();
        
        // 注册
        bool ret = dataMgr.Register("cty", "1217");
        if (ret)
        {
            Console.WriteLine("注册成功");
        }
        else
        {
            Console.WriteLine("注册失败");
        }
        
        // 创建玩家
        ret = dataMgr.CreatePlayer("cty");
        if (ret)
        {
            Console.WriteLine("创建玩家成功");
        }
        else
        {
            Console.WriteLine("创建玩家失败");
        }
        
        // 获取玩家数据
        PlayerData pd = dataMgr.GetPlayerData("cty");
        if (pd != null)
        {
            Console.WriteLine("获取玩家成功 分数是 " + pd.score);
        }
        else
        {
            Console.WriteLine("获取玩家数据失败");
        }
        
        // 更改玩家数据
        pd.score += 10;
        
        // 保存数据
        Player p = new Player();
        p.id = "cty";
        p.data = pd;
        dataMgr.SavePlayer(p);
        
        // 重新读取
        pd = dataMgr.GetPlayerData("cty");
        if (pd != null)
        {
            Console.WriteLine("获取玩家成功 分数是 " + pd.score);
        }
        else
        {
            Console.WriteLine("获取玩家数据失败");
        }
    }
}