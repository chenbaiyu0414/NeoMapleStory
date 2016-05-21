using System;
using System.Drawing;
using System.IO;
using System.Text;

namespace NeoMapleStory.Core.IO
{
    /// <summary>
    ///     构建被发送的封包
    /// </summary>
    public sealed class OutPacket : IDisposable
    {
        /// <summary>
        ///     默认的封包大小
        /// </summary>
        public const int DefaultBufferSize = 32;

        private readonly MemoryStream m_mStream;
        private readonly BinaryWriter m_mWriter;

        /// <summary>
        ///     初始化 <see cref="OutPacket" /> 的新实例，并以默认封包大小（32字节）创建流。
        /// </summary>
        public OutPacket()
        {
            m_mStream = new MemoryStream(DefaultBufferSize);
            m_mWriter = new BinaryWriter(m_mStream);
        }

        /// <summary>
        ///     初始化 <see cref="OutPacket" /> 的新实例，并以指定的封包大小创建流。
        /// </summary>
        /// <param name="size">封包大小</param>
        public OutPacket(int size)
        {
            m_mStream = new MemoryStream(size);
            m_mWriter = new BinaryWriter(m_mStream);
        }

        /// <summary>
        ///     初始化 <see cref="OutPacket" /> 的新实例，并以指定的封包操作码或封包大小创建流。
        /// </summary>
        /// <param name="opcode">封包操作码</param>
        /// <param name="size">封包大小，默认为32字节</param>
        public OutPacket(short opcode, int size = DefaultBufferSize)
        {
            m_mStream = new MemoryStream(size);
            m_mWriter = new BinaryWriter(m_mStream);
            WriteShort(opcode);
        }

        /// <summary>
        ///     流的当前位置
        /// </summary>
        public int Position
        {
            get { return (int) m_mStream.Position; }
            set
            {
                if (value <= 0)
                    throw new IndexOutOfRangeException();

                m_mStream.Position = value;
            }
        }

        /// <summary>
        ///     将单字节 Boolean 值写入当前流，其中 0 表示 false，1 表示true。
        /// </summary>
        /// <param name="value">要写入的 Boolean 值</param>
        public void WriteBool(bool value)
        {
            m_mWriter.Write(value);
        }

        /// <summary>
        ///     将一个无符号字节（Byte）写入当前流，并将流的位置提高 1 个字节。
        /// </summary>
        /// <param name="value">要写入的无符号字节（Byte）</param>
        public void WriteByte(byte value)
        {
            m_mWriter.Write(value);
        }

        /// <summary>
        ///     将字节数组写入当前流，并将流的位置提高相应的字节数。
        /// </summary>
        /// <param name="value">无符号字节数组</param>
        public void WriteBytes(byte[] value)
        {
            m_mWriter.Write(value);
        }

        /// <summary>
        ///     将 2 字有符号整数（Short）写入当前流，并将流的位置提高 2 个字节。
        /// </summary>
        /// <param name="value">要写入的有符号整数（Short）</param>
        public void WriteShort(short value)
        {
            m_mWriter.Write(value);
        }

        /// <summary>
        ///     将 2 字无符号整数（UShort）写入当前流，并将流的位置提高 2 个字节。
        /// </summary>
        /// <param name="value">要写入的无符号整数（UShort）</param>
        public void WriteUShort(ushort value)
        {
            m_mWriter.Write(value);
        }

        /// <summary>
        ///     将 4 字有符号整数（Int）写入当前流，并将流的位置提高 4 个字节。
        /// </summary>
        /// <param name="value">要写入的有符号整数（Int）</param>
        public void WriteInt(int value)
        {
            m_mWriter.Write(value);
        }

        /// <summary>
        ///     将 8 字有符号整数（Long）写入当前流，并将流的位置提高 8 个字节。
        /// </summary>
        /// <param name="value">要写入的有符号整数（Long）</param>
        public void WriteLong(long value)
        {
            m_mWriter.Write(value);
        }

        /// <summary>
        ///     将一个点结构（Point）写入当前流，并将流的位置提高 4 个字节。
        /// </summary>
        /// <param name="point">将要写入的点结构（Point）</param>
        public void WritePoint(Point point)
        {
            m_mWriter.Write((short) point.X);
            m_mWriter.Write((short) point.Y);
        }

        /// <summary>
        ///     将一个点结构（Point）写入当前流，并将流的位置提高 4 个字节。
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        public void WritePoint(int x, int y)
        {
            m_mWriter.Write((short) x);
            m_mWriter.Write((short) y);
        }

        /// <summary>
        ///     将字符串写入当前流，并将流的位置提高相应的字节数。
        /// </summary>
        /// <param name="value"></param>
        public void WriteString(string value)
        {
            m_mWriter.Write(Encoding.Default.GetBytes(value));
        }

        /// <summary>
        ///     将填充字符串（PaddedString）写入当前流，并将流的位置提高相应的字节数。
        /// </summary>
        /// <param name="value">要写入的填充字符串</param>
        /// <param name="length">填充后的字符串总长度</param>
        /// <param name="paddedLeft">是否将填充字符填充在左边，默认填充在右边</param>
        public void WritePaddedString(string value, int length, bool paddedLeft = false)
        {
            if (value.Length > length)
            {
                value = value.Substring(0, length);
            }
            var bytes = Encoding.Default.GetBytes(value);
            if (paddedLeft)
            {
                WriteZero(length - bytes.Length);
                WriteBytes(bytes);
            }
            else
            {
                WriteBytes(bytes);
                WriteZero(length - bytes.Length);
            }
        }

        /// <summary>
        ///     将 MapleStory 格式的字符串（MapleString）写入当前流，并将流的位置提升相应的字节数。
        /// </summary>
        /// <param name="value">要写入的 MapleString</param>
        public void WriteMapleString(string value)
        {
            if (value == null)
            {
                WriteZero(2);
            }
            else
            {
                var bytes = Encoding.Default.GetBytes(value);
                WriteShort((short) bytes.Length);
                WriteBytes(bytes);
            }
        }

        /// <summary>
        ///     通过指定的格式将 MapleStory 格式的字符串（MapleString）写入当前流，并将流的位置提升相应的字节数。
        /// </summary>
        /// <param name="format">复合格式字符串</param>
        /// <param name="args">要设置格式的对象</param>
        public void WriteMapleString(string format, params object[] args)
        {
            WriteMapleString(string.Format(format, args));
        }

        /// <summary>
        ///     将相应数量的 0 以无符号字节（Byte）的形式写入当前流，并将流的位置提高相应的字节数。
        /// </summary>
        /// <param name="count">要写入 0 的数量</param>
        public void WriteZero(int count = 1)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException("count");

            for (var i = 0; i < count; i++)
                WriteByte(0x00);
        }

        /// <summary>
        ///     将流内容写入字节数组，而与 <see cref="OutPacket.Position" /> 属性无关。
        /// </summary>
        public byte[] ToArray()
        {
            m_mWriter.Flush();
            return m_mStream.ToArray();
        }

        #region IDisposable Support

        private bool m_disposedValue; // 要检测冗余调用

        private void Dispose(bool disposing)
        {
            if (!m_disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                    m_mWriter.Dispose();
                    m_mStream.Dispose();
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。
                m_disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~OutPacket() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }

        #endregion
    }
}