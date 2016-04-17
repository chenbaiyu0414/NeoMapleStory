using System.Threading;

namespace NeoMapleStory.Core
{
    public class InterLockedInt
    {
        private int _interValue;

        public int Value => _interValue;

        public InterLockedInt(int value)
        {
            _interValue = value;
        }

        /// <summary>
        /// 递增变量
        /// </summary>
        /// <returns></returns>
        public int Increment()
        {
            return Interlocked.Increment(ref _interValue);
        }

        /// <summary>
        /// 递减变量
        /// </summary>
        /// <returns></returns>
        public int Decrement()
        {
            return Interlocked.Decrement(ref _interValue);
        }

        /// <summary>
        /// 对两个32位整数求和并用和替换原值，返回被存储的新值。
        /// </summary>
        /// <param name="value">要添加到整数中的值</param>
        /// <returns>被储存的新值</returns>
        public int Add(int value)
        {
            int result = Interlocked.Add(ref _interValue, value);
            return result;
        }

        /// <summary>
        /// 将原值设置为指定的新值，返回原始值。
        /// </summary>
        /// <param name="value">要被设置的新值</param>
        /// <returns>原始值</returns>
        public int Exchange(int value)
        {
            int result= Interlocked.Exchange(ref _interValue, value);
            return result;
        }

        /// <summary>
        /// 将值归零
        /// </summary>
        public void Reset()
        {
            if (_interValue != 0)
                Exchange(0);
        }
    }
}
