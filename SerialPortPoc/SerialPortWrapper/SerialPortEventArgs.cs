using System;

namespace SerialPortPoc.SerialPortWrapper
{
    public class SerialPortEventArgs : EventArgs
    {
        public byte[] Data { get; set; }
        public string StringData { get; set; }
    }
}