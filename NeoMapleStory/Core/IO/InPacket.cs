using System;
using System.IO;
using System.Text;

namespace NeoMapleStory.Core.IO
{
    /// <summary>
    /// 读取接收到的封包
    /// </summary>
     public class InPacket:IDisposable
    {
        private readonly MemoryStream _mBuffer;
        private readonly BinaryReader _mReader;

        /// <summary>
        /// 流的当前位置
        /// </summary>
        public long Position => _mReader.BaseStream.Position;

        /// <summary>
        /// 剩余可读的流字节数量
        /// </summary>
        public long AvailableCount => _mReader.BaseStream.Length - _mReader.BaseStream.Position;

        /// <summary>
        /// 初始化 <see cref="InPacket"/> 的新实例，该类用于提供对指定流的读操作。
        /// </summary>
        /// <param name="packet">从中创建当前流的无符号字节组</param>
        public InPacket(byte[] packet)
        {
            _mBuffer = new MemoryStream(packet);
            _mReader = new BinaryReader(_mBuffer);
        }

        private void CheckLength(int length)
        {
            if (_mReader.BaseStream.Position + length > _mBuffer.Length || length < 0)
                throw new IndexOutOfRangeException();
        }

        /// <summary>
        /// 从当前流中读取Boolean值，并使该流的当前位置提升 1 个字节。
        /// </summary>
        public bool ReadBool()
        {
            return _mReader.ReadBoolean();
        }

        /// <summary>
        /// 从当前流中读取下一个字节，并使该流的当前位置提升 1 个字节。
        /// </summary>
        public byte ReadByte()
        {
            return _mReader.ReadByte();
        }

        /// <summary>
        /// 从当前流中读取指定的字节数以写入字节数组中，并使该流的当前位置提升相应的字节数。
        /// </summary>
        public byte[] ReadBytes(int count)
        {
            CheckLength(count);
            return _mReader.ReadBytes(count);
        }

        /// <summary>
        /// 从当前流中读取 2 字节有符号整数（Short），并使该流的当前位置提升 2 个字节。
        /// </summary>
        public short ReadShort()
        {
            CheckLength(2);
            return _mReader.ReadInt16();
        }

        /// <summary>
        /// 从当前流中读取 2 字节无符号整数（UShort），并使该流的当前位置提升 2 个字节。
        /// </summary>
        public ushort ReadUShort()
        {
            CheckLength(2);
            return _mReader.ReadUInt16();
        }

        /// <summary>
        /// 从当前流中读取 4 字节有符号整数（Int），并使该流的当前位置提升 4 个字节。
        /// </summary>
        public int ReadInt()
        {
            CheckLength(4);
            return _mReader.ReadInt32();
        }

        /// <summary>
        /// 从当前流中读取 8 字节有符号整数（Long），并使该流的当前位置提升 8 个字节。
        /// </summary>
        public long ReadLong()
        {
            CheckLength(8);
            return _mReader.ReadInt64();
        }

        /// <summary>
        /// 从当前流中读取指定的字节数并转化为字符串，并使该流的当前位置提升相应字节数。
        /// </summary>
        public string ReadString(int count)
        {
            CheckLength(count);
            return Encoding.Default.GetString(ReadBytes(count));
        }

        /// <summary>
        /// 先从当前流中读取 2 字节作为字符串长度，然后读取相应长度的字符串，并使该流的当前位置提升 字符串长度 + 2 个字节。
        /// </summary>
        public string ReadMapleString()
        {
            short count = ReadShort();
            return ReadString(count);
        }

        /// <summary>
        /// 从当前流中跳过指定的字节数，并使该流的当前位置提升相应的字节数。
        /// </summary>
        public void Skip(int count)
        {
            CheckLength(count);
            _mReader.BaseStream.Seek(count, SeekOrigin.Current);
        }

        /// <summary>
        /// 将流内容写入字节数组，而与 <see cref="InPacket.Position"/> 属性无关。
        /// </summary>
        public byte[] ToArray()
        {
            return _mBuffer.ToArray();
        }

        #region IDisposable Support
        private bool _disposedValue; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                    _mReader.Dispose();
                    _mBuffer.Dispose();
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。
                _disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~InPacket() {
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
