using Java.Lang;
using Java.Nio;
using Java.Nio.Charset;
using Serial;
using System;
using NativeSerialPort = Serial.Serial;
using SerialPortBuilder = Serial.Serial.Builder;

namespace SerialPort.SerialPortWrapper
{
    /// <summary>
    /// Native Serial Port wrapper. More information: https://github.com/chzhong/serial-android.
    /// </summary>
    public class SerialPort
    {
        /// <summary>
        /// This is the native SerialPort. We can use this as a SerialPort.
        /// </summary>
        private NativeSerialPort _serialPort;

        /// <summary>
        /// Stop the reading from the Serial Port.
        /// </summary>
        private bool _stopOnReceive = true;

        private int _howManyDelayOnReceivingThread = 200;

        /// <summary>
        /// Read from the Serial Port and send the readed datas to the subscribers.
        /// </summary>
        public event EventHandler<SerialPortEventArgs> OnReceived;

        protected virtual void OnReceived_Event(SerialPortEventArgs e)
        {
            OnReceived?.Invoke(this, e);
        }

        /// <summary>
        /// Read from the Serial Port.
        /// </summary>
        private System.Threading.Thread OnReceiveThread;

        /// <summary>
        /// Create new SerialPort from the native library: libserial-release.aar.
        /// Creates a Serial object and opens the port if a port is specified, otherwise it remains closed until serial::Serial::open is called.
        /// @throws SerialException Generic serial error.
        /// @throws SerialIOException I/O error.
        /// @throws IllegalArgumentException Invalid arguments are given.
        /// </summary>
        /// <param name="device">A string containing the address of the serial port, which would be something like 'COM1' on Windows and '/dev/ttyS0' on Linux.</param>
        /// <param name="baudrate">An unsigned 32-bit integer that represents the baudrate.</param>
        /// <param name="stopbits">Number of stop bits used, default is stopbits_one, possible values are: stopbits_one, stopbits_one_point_five, stopbits_two.</param>
        /// <param name="parity">Method of parity, default is parity_none, possible values are: parity_none, parity_odd, parity_even.</param>
        /// <param name="byteSize">Size of each byte in the serial transmission of data, default is eightbits, possible values are: fivebits, sixbits, sevenbits, eightbits.</param>
        /// <param name="flowControl">Flowcontrol Type of flowcontrol used, default is flowcontrol_none, possible values are: flowcontrol_none, flowcontrol_software, flowcontrol_hardware.</param>
        /// <param name="timeout">A serial::Timeout struct that defines the timeout conditions for the serial port. inter_byte_timeout,read_timeout_constant,read_timeout_multiplier,write_timeout_constant,write_timeout_multiplier</param>
        public SerialPort(
            string device,
            int baudrate,
            Stopbits stopbits,
            Parity parity,
            ByteSize byteSize,
            FlowControl flowControl,
            Timeout timeout)
        {
            SerialPortBuilder Serial = new SerialPortBuilder(
                device,
                baudrate,
                stopbits,
                parity,
                byteSize,
                timeout,
                flowControl);

            OnReceiveThread = new System.Threading.Thread(() =>
            {
                while (!_stopOnReceive)
                {
                    System.Threading.Thread.Sleep(_howManyDelayOnReceivingThread);

                    var buffer = Read();

                    if (buffer != null && buffer.Length > 0)
                    {
                        OnReceived(this, new SerialPortEventArgs() { Data = buffer });
                    }
                }
            });

            OnReceiveThread.IsBackground = true;
            OnReceiveThread.Start();

            _serialPort = Serial.NativeSerialPort;
        }

        /// <summary>
        /// Change the Delay on the Receiving Thread.
        /// </summary>
        /// <param name="time"></param>
        public void ChangeDelayOnReceivingThread(int time)
        {
            _howManyDelayOnReceivingThread = time;
        }

        /// <summary>
        /// Start the Receiving Thread.
        /// </summary>
        public void StartOnReceiveThread()
        {
            if (OnReceiveThread.IsAlive)
            {
                OnReceiveThread.Abort();
            }

            OnReceiveThread.IsBackground = true;
            OnReceiveThread.Start();
        }

        /// <summary>
        /// Read a given amount of bytes from the serial port into a given buffer. The read function will return in one of three cases: The number of requested bytes was read. In this case the number of bytes requested will match the size_t returned by read. A timeout occurred, in this case the number of bytes read will not  match the amount requested, but no exception will be thrown.  One of two possible timeouts occurred: The inter byte timeout expired, this means that number of milliseconds elapsed between receiving bytes from the serial port exceeded the inter byte timeout. The total timeout expired, which is calculated by multiplying the read timeout multiplier by the number of requested bytes and then added to the read timeout constant. If that total number of milliseconds elapses after the initial call to read a timeout will occur. An exception occurred, in this case an actual exception will be thrown.
        /// @throws SerialIOException I/O Error.
        /// </summary>
        /// <param name="buffer">An array of at least the requested size.</param>
        /// <param name="offset">the offset of the buffer to receive data.</param>
        /// <param name="size">How many bytes to be read.</param>
        /// <returns>A size_t representing the number of bytes read as a result of the call to read.</returns>
        public int Read(byte[] buffer, int offset, int size)
        {
            return _serialPort.Read(buffer, offset, size);
        }

        /// <summary>
        /// Read all data available from the serial port.
        /// @throws SerialIOException I/O Error.
        /// </summary>
        /// <returns>A buffer that contains all available data.</returns>
        public byte[] Read()
        {
            return _serialPort.Read();
        }

        /// <summary>
        /// Read a given amount of bytes from the serial port into a give buffer.
        /// @throws SerialIOException I/O Error.
        /// </summary>
        /// <param name="buffer">A reference to a std::vector of uint8_t.</param>
        /// <param name="size">A size_t defining how many bytes to be read.</param>
        /// <returns>A size_t representing the number of bytes read as a result of the call to read.</returns>
        public int Read(ByteBuffer buffer, int size)
        {
            return _serialPort.Read(buffer, size);
        }

        /// <summary>
        /// Read a given amount of bytes from the serial port into a give buffer.
        /// @throws SerialIOException I/O Error.
        /// </summary>
        /// <param name="buffer">A reference to a std::string.</param>
        /// <param name="size">A size_t defining how many bytes to be read.</param>
        /// <param name="charset">The charset of the data.</param>
        /// <returns>A size_t representing the number of bytes read as a result of the call to read.</returns>
        public int Read(StringBuilder buffer, int size /*= 1*/, Charset charset)
        {
            return _serialPort.Read(buffer, size, charset);
        }

        /// <summary>
        /// Read a given amount of bytes from the serial port and return a string containing the data.
        /// @throws SerialIOException I/O Error.
        /// </summary>
        /// <param name="size">A size_t defining how many bytes to be read.</param>
        /// <param name="charset">A std::string containing the data read from the port.</param>
        /// <returns></returns>
        public string Read(int size, Charset charset)
        {
            return _serialPort.Read(size, charset);
        }

        /// <summary>
        /// Reads in a line or until a given delimiter has been processed. Reads from the serial port until a single line has been read.
        /// @throws SerialIOException I/O Error.
        /// </summary>
        /// <param name="buffer">A std::string reference used to store the data.</param>
        /// <param name="size">A maximum length of a line, defaults to 65536 (2^16).</param>
        /// <param name="eol">A string to match against for the EOL.</param>
        /// <returns>A size_t representing the number of bytes read.</returns>
        public int ReadLine(StringBuilder buffer, int size /*= 65536*/, string eol /*= "\n"*/)
        {
            return _serialPort.Readline(buffer, size, eol);
        }

        /// <summary>
        /// Reads in a line or until a given delimiter has been processed. Reads from the serial port until a single line has been read.
        /// @throws SerialIOException I/O Error.
        /// </summary>
        /// <param name="size">A maximum length of a line, defaults to 65536 (2^16).</param>
        /// <param name="eol">A string to match against for the EOL.</param>
        /// <returns>A std::string containing the line.</returns>
        public string ReadLine(int size /*= 65536*/, string eol /*= "\n"*/)
        {
            return _serialPort.Readline(size, eol);
        }

        /// <summary>
        /// Reads in multiple lines until the serial port times out. This requires a timeout > 0 before it can be run.It will read until a Timeout occurs and return a list of strings.
        /// @throws SerialIOException I/O Error.
        /// </summary>
        /// <param name="size">A maximum length of combined lines, defaults to 65536 (2^16)</param>
        /// <param name="eol">A string to match against for the EOL.</param>
        /// <returns>A array containing the lines.</returns>
        public string[] ReadLines(int size /*= 65536*/, string eol /*= "\n"*/)
        {
            return _serialPort.Readlines(size, eol);
        }

        /// <summary>
        /// Write a byte[] to the serial port.
        /// @throws SerialIOException I/O Error.
        /// </summary>
        /// <param name="data">A const reference containing the data to be written to the serial port.</param>
        /// <param name="size">A size_t that indicates how many bytes should be written from the given data buffer.</param>
        /// <returns>A size_t representing the number of bytes actually written to.</returns>
        public int Write(byte[] data, int size)
        {
            return _serialPort.Write(data, size);
        }

        /// <summary>
        /// Write the string on the SerialPort.
        /// </summary>
        /// <param name="text"></param>
        public void Write(string text)
        {
            _serialPort.Write(text);
        }

        /// <summary>
        /// Write the string and a new line on the SerialPort.
        /// </summary>
        /// <param name="text"></param>
        public void WriteLine(string text)
        {
            _serialPort.Write(text + "\n");
        }

        /// <summary>
        /// Flush the input and output buffers.
        /// </summary>
        public void Flush()
        {
            _serialPort.Flush();
        }

        /// <summary>
        /// Flush only the input buffer.
        /// </summary>
        public void FlushInput()
        {
            _serialPort.FlushInput();
        }

        /// <summary>
        /// Flush only the output buffer.
        /// </summary>
        public void FlushOutput()
        {
            _serialPort.FlushOutput();
        }

        /// <summary>
        /// Set the RTS handshaking line to the true level.
        /// </summary>
        public void SetRTS()
        {
            _serialPort.SetRTS();
        }

        /// <summary>
        /// Set the RTS handshaking line to the given level.
        /// </summary>
        /// <param name="level"></param>
        public void SetRTS(bool level)
        {
            _serialPort.SetRTS(level);
        }

        /// <summary>
        /// Set the DTR handshaking line to the given level.
        /// </summary>
        public void SetDTR()
        {
            _serialPort.SetDTR();
        }

        /// <summary>
        /// Set the DTR handshaking line to the given level.
        /// </summary>
        /// <param name="level"></param>
        public void SetDTR(bool level)
        {
            _serialPort.SetDTR(level);
        }

        /// <summary>
        /// Sends the RS-232 break signal.
        /// </summary>
        /// <param name="duration"></param>
        public void SendBreak(int duration)
        {
            _serialPort.SendBreak(duration);
        }

        /// <summary>
        /// Set the break condition to true level.
        /// </summary>
        public void SetBreak()
        {
            _serialPort.SetBreak();
        }

        /// <summary>
        /// Set the break condition to a given level.
        /// </summary>
        /// <param name="level"></param>
        public void SetBreak(bool level)
        {
            _serialPort.SetBreak(level);
        }

        /// <summary>
        /// Close the Serial Port.
        /// </summary>
        public void Close()
        {
            _serialPort.Close();
        }

        /// <summary>
        /// Stop the Receiving Thread.
        /// </summary>
        public void StopReceive()
        {
            if (OnReceiveThread.IsAlive)
            {
                OnReceiveThread.Abort();
            }
        }

        /// <summary>
        /// Blocks until CTS, DSR, RI, CD changes or something interrupts it.
        /// Can throw an exception if an error occurs while waiting.
        /// You can check the status of CTS, DSR, RI, and CD once this returns.
        /// Uses TIOCMIWAIT via ioctl if available (mostly only on Linux) with a
        /// resolution of less than +-1ms and as good as +-0.2ms.  Otherwise a
        /// polling method is used which can give +-2ms.
        /// @throws SerialException
        /// </summary>
        /// <returns>Returns true if one of the lines changed, false if something else occurred.</returns>
        public bool WaitForChange()
        {
            return _serialPort.WaitForChange();
        }
    }
}