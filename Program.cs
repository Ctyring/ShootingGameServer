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
        servNet.Start("127.0.0.1", 1234);
        Scene scene = new Scene();
        while (true)
        {
            string str = Console.ReadLine();
            switch (str)
            {
                case "quit":
                    servNet.Close();
                    return;
                case "print":
                    servNet.Print();
                    break;
            }
        }
    }
}