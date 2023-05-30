using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientApp
{
    public partial class Form1 : Form
    {
        String packetData = ""; 
        Thread tr;
        Socket client;
        String namaTerpilihListbox = "";
        String lama = "";
        List<String> daftaruser = new List<String>(); 

        public Form1()
        {
            InitializeComponent();
        }
        public void AppendListbox1(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AppendListbox1), new object[] { value });
                return;
            }
            if (value == "clear") { 
                if(listBox1.SelectedIndex >= 0)
                {
                    lama = listBox1.SelectedItem.ToString();
                }
                listBox1.Items.Clear(); 
            }
            else { 
                listBox1.Items.Add(value); 
            }
        }
        public void AppendListbox2(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AppendListbox2), new object[] { value });
                return;
            }
            if (value == "clear") { listBox2.Items.Clear(); }
            else { listBox2.Items.Add(value); }
        }
        public void ChangeNotification(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(ChangeNotification), new object[] { value });
                return;
            }
            label1.Text = value; 
        }
        private void button1_Click(object sender, EventArgs e)
        {
        }
        private void button2_Click(object sender, EventArgs e)
        {
        }
        private void button3_Click(object sender, EventArgs e)
        {
            if(textBox1.Text.ToString() == "")
            { MessageBox.Show("Nomer Ip harus diisikan"); }
            else if(textBox3.Text.ToString() == "")
            { MessageBox.Show("Username harus diisikan");  }
            else
            {
                textBox1.Enabled = false; textBox3.Enabled = false; button3.Enabled = false;
                button4.Enabled = true; 

                String nomerIp = textBox1.Text.ToString();
                IPHostEntry ipHost = Dns.GetHostEntry(nomerIp);
                IPAddress ipAddr = IPAddress.Parse(nomerIp);

                client = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                // Connect Socket to the remote
                // endpoint using method Connect()
                IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 8001);
                client.Connect(localEndPoint);
                // We print EndPoint information
                // that we are connected
                //listBox1.Items.Add("Socket connected to -> {0} " + client.RemoteEndPoint.ToString());

                tr = new Thread(new ThreadStart(this.receivedata));
                tr.Start();
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            if(textBox2.Text.ToString() == "")
            { MessageBox.Show("teks harus diisikan");  }
            else if(listBox1.SelectedIndex == -1)
            { MessageBox.Show("Pilih dulu user yang mau diajak bicara"); }
            else
            {
                string message = "newmessage#" + 
                    listBox1.SelectedItem.ToString() + "#" + textBox2.Text.ToString();
                byte[] messageSent = Encoding.ASCII.GetBytes(message);
                int byteSent = client.Send(messageSent);
                listBox2.Items.Add(textBox3.Text.ToString() + ": " + textBox2.Text.ToString());
            }
        }
        void receivedata()
        {
            while(true)
            {
                byte[] messageReceived = new byte[1024];
                int byteRecv = client.Receive(messageReceived);
                String kal = Encoding.ASCII.GetString(messageReceived, 0, byteRecv);
                packetData = packetData + kal; 

                if(packetData.IndexOf("~") >= 0)
                {
                    String[] ttt = packetData.Split('~');
                    packetData = ttt[ttt.Length - 1]; // perintah terakhir yg
                                                     // gk punya tilde yg masih belom bisa dieksekusi karena belom berakhir

                    for(int k = 0; k < ttt.Length; k++)
                    {
                        kal = ttt[k]; 
                        if (kal.IndexOf("connacp") >= 0)
                        {
                            String[] arr = kal.Split('#');
                            string message = "namauser#" + arr[1] + "#" + textBox3.Text.ToString();
                            byte[] messageSent = Encoding.ASCII.GetBytes(message);
                            int byteSent = client.Send(messageSent);
                            Console.WriteLine(message);
                        }
                        else if(kal.IndexOf("announce") >= 0)
                        {
                            String[] arr = kal.Split('#');
                            ChangeNotification(arr[1]); 
                        }
                        else if (kal.IndexOf("list") >= 0)
                        {
                            //AppendListbox1("clear");
                            String[] arr = kal.Split('#');
                            for (int i = 1; i < arr.Length; i++)
                            {
                                if (arr[i] != textBox3.Text.ToString())
                                {
                                    bool flag = false; 
                                    for(int j = 0; j < daftaruser.Count && !flag; j++)
                                    { if(daftaruser[j] == arr[i]) { flag = true; } }

                                    if(!flag)
                                    {
                                        AppendListbox1(arr[i]);
                                        daftaruser.Add(arr[i]); 
                                    }
                                }
                            }
                        }
                        else if (kal.IndexOf("chatlst") >= 0)
                        {
                            String[] arr = kal.Split('#');
                            String nama = arr[1];
                            String targetname = arr[2];
                            if (namaTerpilihListbox != "")
                            {
                                if (namaTerpilihListbox == nama || namaTerpilihListbox == targetname)
                                {
                                    AppendListbox2("clear");
                                    for (int i = 3; i < arr.Length; i++)
                                    {
                                        AppendListbox2(arr[i]);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        private void button5_Click(object sender, EventArgs e)
        {
            Console.WriteLine("\n =========================== \n"); 
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            button4.Enabled = false;
            listBox1.Items.Clear();
            listBox2.Items.Clear();
        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox2.Items.Clear();
            namaTerpilihListbox = listBox1.SelectedItem.ToString();
            string message = "newmessage#" + namaTerpilihListbox + "#pick target user";
            byte[] messageSent = Encoding.ASCII.GetBytes(message);
            int byteSent = client.Send(messageSent);
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if(client != null)
            {
                client.Disconnect(false);
                tr.Abort(); // untuk mematikan thread supaya waktu program ditutup, maka thread nya juga dihapus
            }
        }
    }
}
