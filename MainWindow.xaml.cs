using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CodingChallengeV2Client
{
    /// <summary>
    ///
    /// Identity One Coding Challenge V2
    /// 
    /// Note that the below code is left intentionally incomplete, and is only meant as a starting point for participants
    /// of the Identity One Coding Challenge.  It is expected that the completed challenge will demonstrate the applicants 
    /// skills in Object Oriented Programming, understanding of the .NET framework, basic concepts of TCP/IP programming,
    /// use of Threading, error & exception handling, etc.
    ///  
    /// The code below is meant to be modified/changed.   The applicant is encouraged to implement the challenge using 
    /// the patterns and constructs of their choosing, as long as the requirements of the challenge are met.
    /// 
    /// Please ensure your code is tested against both servers!   We provide an "error" server for you test how your code
    /// will handle various error condidtions.
    /// 
    /// Please also note that there aren't bugs in the server implementations -- Re-read the requirements and then look for
    /// a bug in your implementation :)
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        private Random random = new Random(); //create random data
        private Byte[] buffer; //fill with data to be sent to the server

        //instance that will connect to server and send/receive data
        SenderReceiver sr = new SenderReceiver();
        
        //instance that will parse data from the response and give info about
        //whether the process was successful as well as set the buffer to use the new encoded data
        Response resp;

        //holder for encoded data from the response
        byte[] newBuffer = null;

        private void btnGenerateTestData_Click(object sender, RoutedEventArgs e)
        {

            //verify user input
            if (!uint.TryParse(txtTestDataSize.Text, out uint numBytesResult) || numBytesResult <= 0 
                || numBytesResult >= GlobalConstants.MAX_REQUEST_SIZE)
            {
                lstStatus.Items.Insert(0, new OutputCode(DateTime.UtcNow.ToString("HH:mm:ss.ffff"), "Error", "Number of bytes should only contain numbers and be between 1 and 1,048567."));
                txtTestDataSize.Text = (GlobalConstants.DEFAULT_DATA_SIZE).ToString();
            }
            else
            {
                //create buffer with size indicated by the user and fill with random values
                buffer = new byte[numBytesResult];
                random.NextBytes(buffer);

                //reset encode/decode cycle count
                if (sr != null)
                {
                    sr.ResetEncodes();
                    lblEncodeCycleCount.Content = sr.numOfEncodes;
                }
            }
        }

        private void btnEncryptData_Click(object sender, RoutedEventArgs e)
        {
            //don't allow UI to be clicked until encode finishes
            this.IsEnabled = false;
            
            //check that both the buffer size and future encoded buffer size don't exceed the limit
            if (Validation.CheckParams(txtServerHostname.Text, txtServerPort.Text, buffer, txtTimeout.Text, GlobalConstants.ENCODE_OP))
            {
                //update status box and send data
                lstStatus.Items.Insert(0, Validation.thisCode);
                SendDataToServer(txtServerHostname.Text, Convert.ToUInt16(txtServerPort.Text), buffer, Convert.ToInt32(txtTimeout.Text), GlobalConstants.ENCODE_OP);
            } else
            {
                lstStatus.Items.Insert(0, Validation.thisCode);
                this.IsEnabled = true;
            }
        }

        private void btnDecryptData_Click(object sender, RoutedEventArgs e)
        {
            if (Validation.CheckParams(txtServerHostname.Text, txtServerPort.Text, buffer, txtTimeout.Text, GlobalConstants.DECODE_OP))
            {
                //only allow decode if data has been encoded already
                if (sr.numOfEncodes >= 1)
                {
                    //update status box
                    lstStatus.Items.Insert(0, Validation.thisCode);
                    //don't allow UI to be clicked until decode finishes
                    this.IsEnabled = false;

                    //send data
                    SendDataToServer(txtServerHostname.Text, Convert.ToUInt16(txtServerPort.Text), buffer, Convert.ToInt32(txtTimeout.Text), GlobalConstants.DECODE_OP);
                }
                else
                {
                    //update status box
                    lstStatus.Items.Insert(0, new OutputCode(DateTime.UtcNow.ToString("HH:mm:ss.ffff"), "Information", "There is no encoded data."));
                }
            }
            else
            {
                //update status box and enable UI
                lstStatus.Items.Insert(0, Validation.thisCode);
                this.IsEnabled = true;
            }
        }

        private async void SendDataToServer(string ipOrHostname, ushort port, byte[] payload, int timeout, byte operation)
        {
            //set up client to connect to server; pass in timeout value in milliseconds
            sr.SetProperties(ipOrHostname, port, payload, timeout);

            //create the request structure before connecting to server so that data is ready
            //myConnect() should only worry about connecting to the server and reading/writing data
            sr.CreateRequest(operation);

            //use task to connect to server so that program doesn't halt
            await Task.Run(() => sr.MyConnect()).ContinueWith( task =>
            {
                //if data was read (whether there was error or not) pass this data on to be parsed
                //if data was read with no error, new data will be returned as newBuffer
                //if there was an error, the original buffer will be retained
                resp = new Response(sr.totalDataReceived);

                //update status box depending on status code received from response
                lstStatus.Items.Insert(0, OutputCode.StatusOutput((Codes)Enum.Parse(typeof(Codes), resp.GetStatus().ToString())));
                //if there was no error increment cycle and update the buffer
                if ((Codes)Enum.Parse(typeof(Codes), resp.GetStatus().ToString()) == Codes.Zero)
                {
                    //update cycle count
                    if (operation == GlobalConstants.ENCODE_OP)
                    {
                        sr.IncrementEncodes();
                        lblEncodeCycleCount.Content = sr.numOfEncodes;
                    }
                    else
                    {
                        sr.DecrementEncodes();
                        lblEncodeCycleCount.Content = sr.numOfEncodes;
                    }

                    newBuffer = resp.GetData();
                    buffer = newBuffer;
                    newBuffer = null;
                }

                //reenable the UI after process finishes
                this.IsEnabled = true;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public MainWindow()
        {
            InitializeComponent();

            btnGenerateTestData_Click(this, null);
        }
    }
}
