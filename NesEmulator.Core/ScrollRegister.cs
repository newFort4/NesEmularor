namespace NesEmulator.Core
{
    public class ScrollRegister
	{
        public byte ScrollX { get; set; } = 0;
        public byte ScrollY { get; set; } = 0;
        public bool Latch { get; set; } = false;

        public void Write(byte data)
        {
            if (!Latch)
            {
                ScrollX = data;
            } else
            {
                ScrollY = data;
            }

            Latch = !Latch;
        }

        public void ResetLatch() => Latch = false;
    }
}

