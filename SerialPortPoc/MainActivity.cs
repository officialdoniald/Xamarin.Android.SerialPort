using Android.App;
using Android.Widget;
using Android.OS;
using Java.Lang;
using SerialPort.SerialPortWrapper;
using Serial;

namespace SerialPortPoc
{
    [Activity(Label = "SerialPortPoc", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private TextView getTextView;
        private Button sendButton;

        private SerialPort.SerialPortWrapper.SerialPort LibSerialPort;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            sendButton = FindViewById<Button>(Resource.Id.sendButton);

            sendButton.Click += SendButton_Click;

            getTextView = FindViewById<TextView>(Resource.Id.getTextView);
            getTextView.Text = "Data from Serial Port...\n";

            OpenLibSerialPort();
        }

        /// <summary>
        /// Open a LibSerial Serial Port.
        /// </summary>
        /// <returns></returns>
        private void OpenLibSerialPort()
        {
            LibSerialPort = new SerialPort.SerialPortWrapper.SerialPort(
               "/dev/ttyS3",
               115200,
               Stopbits.One,
               Parity.None,
               ByteSize.EightBits,
               FlowControl.Software,
               new Timeout(50,50,50,50,50));

            LibSerialPort.OnReceived += SerialPort_OnReceived;
        }

        private void SerialPort_OnReceived(object sender, SerialPortEventArgs e)
        {
            RunOnUiThread(()=> {
                getTextView.Append("Data:");
            });

            foreach (var item in e.Data)
            {
                System.Diagnostics.Debug.WriteLine(item);

                RunOnUiThread(() => {
                    getTextView.Append(item + " ");
                });
            }
            RunOnUiThread(() => {
                getTextView.Append("\n");
            });
        }

        private void SendButton_Click(object sender, System.EventArgs e)
        {
            LibSerialPort.WriteLine("Hello Serial Port!");
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (LibSerialPort != null)
            {
                LibSerialPort.Close();
            }
        }
    }
}