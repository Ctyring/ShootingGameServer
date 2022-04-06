﻿// See https://aka.ms/new-console-template for more information

using System.Data;
using MySql.Data.MySqlClient;
using ShootingGameServer.core;
using ShootingGameServer.Logic;
using ShootingGameServer.mysql;

class MainClass
{
    public static void Main(string[] args)
    {
        ServNet servNet = new ServNet();
        servNet.proto = new ProtocolBytes();
        servNet.Start("127.0.0.1", 1234);
        Console.ReadLine();
    }
}