// See https://aka.ms/new-console-template for more information

using ShootingGameServer.mysql;

class MainClass
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        SqlUtil.Init();
    }
}