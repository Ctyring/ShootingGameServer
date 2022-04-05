namespace ShootingGameServer.core;

/// <summary>
/// 协议基类
/// </summary>
public class ProtocolBase
{
    /// <summary>
    /// 解码器，解码readBuff中从start开始的length长度的字节
    /// </summary>
    /// <param name="readBuff">读缓冲区</param>
    /// <param name="start">解码的开始位置</param>
    /// <param name="length">解码的字节长度</param>
    /// <returns></returns>
    public virtual ProtocolBase Decode(byte[] readBuff, int start, int length)
    {
        return new ProtocolBase();
    }

    /// <summary>
    /// 编码器
    /// </summary>
    /// <returns></returns>
    public virtual byte[] Encode()
    {
        return new byte[] {};
    }

    /// <summary>
    /// 协议名称，用于消息分发
    /// 消息分发会把不同协议名称的协议交给不同的函数来处理
    /// </summary>
    /// <returns>协议名称</returns>
    public virtual string GetName()
    {
        return "";
    }

    /// <summary>
    /// 描述 用于调试时比较直观地显示协议的内容
    /// </summary>
    /// <returns></returns>
    public virtual string GetDesc()
    {
        return "";
    }
}