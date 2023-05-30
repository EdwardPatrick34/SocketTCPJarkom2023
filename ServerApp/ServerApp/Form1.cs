using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ServerApp
{
    public partial class Form1 : Form
    {
        TcpListener serverSocket;
        TcpClient clientSocket;
        int counter = 0;

        Socket listener;
        List<ClsClient> myclient = new List<ClsClient>();
        List<ClsMessage> mymessage = new List<ClsMessage>();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
        }

        public void AppendListbox(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AppendListbox), new object[] { value });
                return;
            }
            listBox1.Items.Add(value); 
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string hostName = Dns.GetHostName();
            string myIP = Dns.GetHostByName(hostName).AddressList[0].ToString();
            IPHostEntry ipHost = Dns.GetHostEntry(myIP); // get ip host
            IPAddress ipAddr = IPAddress.Parse(myIP);

            listBox1.Items.Add("Host name : " + ipHost.HostName);
            listBox1.Items.Add("Ip addresss : " + ipAddr.ToString());
            listener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 8001);
            listener.Bind(localEndPoint);

            listener.Listen(10);

            Thread tr = new Thread(new ThreadStart(this.waitingClientConnection2));
            tr.Start();
        }

        void waitingClientConnection2()
        {
            while (true)
            {
                AppendListbox("Waiting connection ... ");

                Socket clientSocket = listener.Accept();

                ClsClient mc = new ClsClient(clientSocket, listBox1, this);
                myclient.Add(mc);

                string message = "connacp#" + (myclient.Count - 1) + "~"; 
                byte[] messageSent = Encoding.ASCII.GetBytes(message);
                int byteSent = clientSocket.Send(messageSent);

                AppendListbox("size = " + myclient.Count.ToString());
            }
        }

        public void addmessage(String nama, String targetname, String teks)
        {
            if(teks == "pick target user")
            {
                sendchat(nama, targetname);
            }
            else
            {
                mymessage.Add(new ClsMessage(nama, targetname, teks));
                sendchat(nama, targetname);
            }
        }

        public void sendchat(String nama, String targetname)
        {
            int countchat = 0; 
            String message = "chatlst#" + nama + "#" + targetname;
            for (int i = 0; i < mymessage.Count; i++)
            {
                if((mymessage[i].nama == nama && mymessage[i].targetnama == targetname) ||
                    (mymessage[i].nama == targetname && mymessage[i].targetnama == nama))
                {
                    countchat += 1; 
                    message = message + "#" + mymessage[i].nama + ": " + mymessage[i].teks;
                }
            }
            Console.WriteLine("tracking = " + countchat);
            if(countchat > 0)
            {
                byte[] messageSent = Encoding.ASCII.GetBytes(message + "~");
                for (int i = 0; i < myclient.Count; i++)
                {
                    if (myclient[i].nama == nama || myclient[i].nama == targetname)
                    {
                        Console.WriteLine("tracking = " + myclient[i].nama + "-" +
                            message);
                        myclient[i].client.Send(messageSent);
                    }
                }
            }
        }

        public void sendlistuser()
        {
            //bentuk kalimat list#john#budi
            String message = "list";
            for(int i = 0; i < myclient.Count; i++)
            {
                message = message + "#" + myclient[i].nama; 
            }
            //send to all client
            byte[] messageSent = Encoding.ASCII.GetBytes(message + "~");
            for (int i = 0; i < myclient.Count; i++)
            {
                myclient[i].client.Send(messageSent);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(textBox1.Text.ToString() != "")
            {
                byte[] messageSent = Encoding.ASCII.GetBytes("announce#" + textBox1.Text.ToString() + "~");
                for (int i = 0; i < myclient.Count; i++)
                {
                    myclient[i].client.Send(messageSent);
                }
            }
        }
    }

    class ClsMessage
    {
        public String nama;
        public String targetnama;
        public String teks; 

        public ClsMessage(String n, String tn, String te)
        {
            nama = n;
            targetnama = tn;
            teks = te;
        }
    }
    class ClsClient
    {
        public String nama;
        public Socket client;
        public ListBox listbox1;
        public Form1 myform;

        public ClsClient(Socket c, ListBox lb, Form1 mf)
        {
            myform = mf; 
            client = c;
            listbox1 = lb;
            Thread tr = new Thread(new ThreadStart(this.receivedata)); // untuk menangani kiriman data dari client bersangkutan 
            tr.Start();
        }

        void receivedata()
        {
            while(true)
            {
                var buffer = new List<byte>();
                if (client.Available > 0)
                {
                    byte[] messageReceived = new byte[1024];
                    int byteRecv = client.Receive(messageReceived);
                    String kal = Encoding.ASCII.GetString(messageReceived, 0, byteRecv);
                    Console.WriteLine("data = " + kal);

                    if (kal.IndexOf("namauser") >= 0) 
                    {
                        String[] arr = kal.Split('#');
                        int nomerurut = int.Parse(arr[1]);
                        nama = arr[2];
                        myform.sendlistuser();
                    }
                    else if(kal.IndexOf("newmessage") >= 0)
                    {
                        String[] arr = kal.Split('#');
                        String targetname = arr[1];
                        String teks = arr[2];
                        myform.addmessage(nama, targetname, teks);
                    }
                }

                if (buffer.Count > 0)
                {
                    Console.WriteLine("data = " + buffer.Count);
                    Console.WriteLine("data = " + buffer.ToString());
                    //listbox1.Items.Add(buffer.ToArray());
                }
            }
        }
    }
}
