// See https://aka.ms/new-console-template for more information

using System.Data;
using MySql.Data.MySqlClient;
using ShootingGameServer.core;
using ShootingGameServer.Logic;
using ShootingGameServer.mysql;

class MainClass
{
    public static void Main(string[] args)
    {
        RoomMgr roomMgr = new RoomMgr();
        DataMgr dataMgr = new DataMgr();
        ServNet servNet = new ServNet();
        servNet.proto = new ProtocolBytes();
        servNet.Start(9888);
        Scene scene = new Scene();
        Thread.Sleep(Timeout.Infinite);
    }
}