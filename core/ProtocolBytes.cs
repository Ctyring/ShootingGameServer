using System.Drawing;

namespace ShootingGameServer.core;

/// <summary>
/// 字节流协议
/// 把所有参数放入byte[]结构中，客户端和服务端按照约定的数据类型和顺序解析各个参数
/// 目前编写的字节流协议支持int、float、string三种数据类型
/// 我们规定第一个参数必须是字符串，代表协议名称
/// </summary>
public class ProtocolBytes : ProtocolBase
{
    // 传输的字节流
    public byte[] bytes;
    
    /// <summary>
    /// 解码器，把字节流形式的消息复制到ProtocolBytes对象中
    /// </summary>
    /// <param name="readBuff">读缓冲区</param>
    /// <param name="start">解码的开始位置</param>
    /// <param name="length">解码的字节长度</param>
    /// <returns>ProtocolBytes对象</returns>
    public override ProtocolBase Decode(byte[] readBuff, int start, int length)
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.bytes = new byte[length];
        Array.Copy(readBuff, start, protocol.bytes, 0, length);
        return protocol;
    }

    /// <summary>
    /// 解码器
    /// </summary>
    /// <returns></returns>
    public override byte[] Encode()
    {
        return bytes;
    }

    /// <summary>
    /// 获取协议名称，获取协议的第一个字符串
    /// </summary>
    /// <returns>协议名称</returns>
    public override string GetName()
    {
        return GetString(0);
    }

    /// <summary>
    /// 获取内容描述，提取每一个字节并组装成字符串
    /// </summary>
    /// <returns></returns>
    public override string GetDesc()
    {
        string str = "";
        if (bytes == null)
        {
            return str;
        }

        for (int i = 0; i < bytes.Length; i++)
        {
            int b = (int) bytes[i];
            str += b.ToString() + "";
        }

        return str;
    }

    /// <summary>
    /// 添加字符串到字节数组
    /// </summary>
    /// <param name="str">字符串消息</param>
    public void AddString(string str)
    {
        Int32 len = str.Length;
        byte[] lenBytes = BitConverter.GetBytes(len);
        byte[] strBytes = System.Text.Encoding.UTF8.GetBytes(str);
        // bytes可能没有初始化，如果没初始化就初始化一下
        if (bytes == null)
        {
            bytes = lenBytes.Concat(strBytes).ToArray();
        }
        else
        {
            bytes = bytes.Concat(lenBytes).Concat(strBytes).ToArray();
        }
    }

    /// <summary>
    /// 从字节数组的start处开始读取字符串
    /// </summary>
    /// <param name="start">起点</param>
    /// <param name="end">下一个数据的起始点</param>
    /// <returns></returns>
    public string GetString(int start, ref int end)
    {
        // 如果start后面的字节数 < 4，则不能读取字符串的大小
        if (bytes == null)
        {
            return "";
        }
        if (bytes.Length < start + sizeof(Int32))
        {
            return "";
        }

        Int32 strLen = BitConverter.ToInt32(bytes, start);
        // 如果字节数组长度不足以读取字符串，那说明出错了
        if (bytes.Length < start + sizeof(Int32) + strLen)
        {
            return "";
        }

        string str = System.Text.Encoding.UTF8.GetString(bytes, start + sizeof(Int32), strLen);
        end = start + sizeof(Int32) + strLen;
        return str;
    }

    /// <summary>
    /// 封装上面的GetString，使得end可以忽略
    /// </summary>
    /// <param name="start"></param>
    /// <returns></returns>
    public string GetString(int start)
    {
        int end = 0;
        return GetString(start, ref end);
    }
    
    /// <summary>
    /// 添加整数方法
    /// </summary>
    /// <param name="num"></param>
    public void AddInt(int num)
    {
        byte[] numBytes = BitConverter.GetBytes(num);
        if (bytes == null)
        {
            bytes = numBytes;
        }
        else
        {
            bytes = bytes.Concat(numBytes).ToArray();
        }
    }

    /// <summary>
    /// 从字节流中读取一个整数
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public int GetInt(int start, ref int end)
    {
        if (bytes == null)
        {
            return 0;
        }

        if (bytes.Length < start + sizeof(Int32))
        {
            return 0;
        }

        end = start + sizeof(Int32);
        return BitConverter.ToInt32(bytes, start);
    }

    /// <summary>
    /// 覆盖上述方法以忽略end
    /// </summary>
    /// <param name="start"></param>
    /// <returns></returns>
    public int GetInt(int start)
    {
        int end = 0;
        return GetInt(start, ref end);
    }
    
    /// <summary>
    /// 添加浮点数方法
    /// </summary>
    /// <param name="num"></param>
    public void AddFloat(float num)
    {
        byte[] numBytes = BitConverter.GetBytes(num);
        if (bytes == null)
        {
            bytes = numBytes;
        }
        else
        {
            bytes = bytes.Concat(numBytes).ToArray();
        }
    }

    /// <summary>
    /// 从字节流中读取一个浮点数
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public float GetFloat(int start, ref int end)
    {
        if (bytes == null)
        {
            return 0;
        }

        if (bytes.Length < start + sizeof(Int32))
        {
            return 0;
        }

        end = start + sizeof(float);
        return BitConverter.ToSingle(bytes, start);
    }

    /// <summary>
    /// 覆盖上述方法以忽略end
    /// </summary>
    /// <param name="start"></param>
    /// <returns></returns>
    public float GetFloat(int start)
    {
        int end = 0;
        return GetFloat(start, ref end);
    }
}