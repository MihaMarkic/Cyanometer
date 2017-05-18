namespace Raspberry.IO.InterIntegratedCircuit
{
    using System;

    /// <summary>
    /// TODO
    /// </summary>
    public abstract class I2cAction
    {
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="buffer"></param>
        protected I2cAction(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            Buffer = buffer;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public byte[] Buffer { get; private set; }
    }
}
