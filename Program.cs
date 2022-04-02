// See https://aka.ms/new-console-template for more information

using ShootingGameServer.mysql;
using ShootingGameServer.Server;

class MainClass
{
    public static void Main(string[] args)
    {
        SqlUtil.Init();
        MainServer mainServer = new MainServer();
        mainServer.Start("127.0.0.1", 1234);

        while (true)
        {
            string str = Console.ReadLine();
            switch (str)
            {
                case "quit":
                    return;
            }
        }
    }
}