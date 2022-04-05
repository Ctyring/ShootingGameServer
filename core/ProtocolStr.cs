namespace ShootingGameServer.core;

/// <summary>
/// 字符串协议模型
/// 形式 名称, 参数1, 参数2, 参数3
/// 字符串协议模型有天生的漏洞，只要客户端发送带逗号的信息就会引起混乱，所以只是用来演示
/// </summary>
public class ProtocolStr : ProtocolBase
{
    // 传输的字符串，整个协议都只用字符串表达
    public string str;
    
    /// <summary>
    /// 解码器，解码过程就是将字节流转换为字符串
    /// </summary>
    /// <param name="readBuff">读缓冲区</param>
    /// <param name="start">解码的开始位置</param>
    /// <param name="length">解码的字节长度</param>
    /// <returns></returns>
    public override ProtocolBase Decode(byte[] readBuff, int start, int length)
    {
        ProtocolStr protocol = new ProtocolStr();
        protocol.str = System.Text.Encoding.UTF8.GetString(readBuff, start, length);
        return (ProtocolBase)protocol;
    }

    /// <summary>
    /// 编码器，编码过程就是将字符串转换成字节流
    /// </summary>
    /// <returns></returns>
    public override byte[] Encode()
    {
        byte[] b = System.Text.Encoding.UTF8.GetBytes(str);
        return b;
    }

    /// <summary>
    /// 获取协议名称
    /// </summary>
    /// <returns></returns>
    public override string GetName()
    {
        if (str.Length == 0)
        {
            return "";
        }
        // 第一个逗号前的参数就是协议名称
        return str.Split(',')[0];
    }

    /// <summary>
    /// 协议描述
    /// </summary>
    /// <returns></returns>
    public override string GetDesc()
    {
        // 用整个字符串代表协议描述
        return str;
    }
}