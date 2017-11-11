using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class frmMain : Form
    {
        private const int PORT = 8888;
        private IPEndPoint IP;
        private Socket client;
        //private string dataSend;
        private string dataReceive;
        //private string hostName;
        //private string filePath;
        List<Obj> lstObj = new List<Obj>();
        List<Obj> listDataSend = new List<Obj>();
        private string pathLog = Application.StartupPath + "\\logfile";
        public frmMain()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;

        }
        public void OpenConnect()
        {
            IP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), PORT);
            //IP = new IPEndPoint(IPAddress.Parse("172.28.16.40"), PORT);
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

            try
            {
                client.Connect(IP);
            }
            catch (Exception)
            {
                MessageBox.Show("Connect error", "System Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.CloseConnect();
                Application.Exit();
            }
            Thread listen = new Thread(Receive);
            listen.IsBackground = true;
            listen.Start();
        }
        public void CloseConnect()
        {
            if (client != null)
            {
                client.Close();
            }
        }
        public void Send(string data)
        {
            try
            {
                if (data != string.Empty)
                {
                    client.Send(Serialize(data));
                }

            }
            catch
            {
                MessageBox.Show("Mất kết nối", "Errors", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void Receive()
        {
            try
            {
                while (true)
                {
                    byte[] data = new byte[1024 * 5000];
                    client.Receive(data);
                    dataReceive = Deserialize(data).ToString();
                    View();
                }
            }
            catch
            {
                CloseConnect();
            }

        }
        public byte[] Serialize(object obj)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, obj);
                byte[] bytes = stream.ToArray();
                stream.Flush();
                return bytes;
            }

        }
        object Deserialize(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
            {
                //BinaryFormatter formatter = new BinaryFormatter();
                //return formatter.Deserialize(stream);
                stream.Position = 0;
                object desObj = new BinaryFormatter().Deserialize(stream);
                stream.Flush();
                return desObj;
            }
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //Send(hostName + "*CLOSE#");
            CloseConnect();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            OpenConnect();
            LoadSetting();
            //hostName = Dns.GetHostName();
        }
        public void LoadSetting()
        {
            if (!Directory.Exists("logfile"))
            {
                Directory.CreateDirectory("logfile");
            }
            //if (!Directory.Exists(Application.StartupPath + "\\logfile"))
            //{
            //    Directory.CreateDirectory(Application.StartupPath + "\\logfile");
            //}
            // Load Model type
            List<string> list = new List<string>();
            list = File.ReadLines(Application.StartupPath + "\\setup\\SETTING.txt").Skip(1).Take(1).FirstOrDefault().Split(',').ToList();
            cbbType.DataSource = list;
            List<string> listCustomer = Directory.GetFiles(Application.StartupPath + "\\setup", "*.txt")
                                    .Select(Path.GetFileNameWithoutExtension)
                                    .Where(r => !r.Contains("SETTING"))
                                    .ToList();
            cbbCustomer.DataSource = listCustomer;

            string filePathModel = Application.StartupPath + "\\setup\\" + cbbCustomer.Text.ToLower() + ".txt";
            GetModel(filePathModel);
            webBrowser1.DocumentText = "<h2 style='color: red'><marquee>Chú ý xác nhận khi đã nhận được hàng !</marquee></h2>";
            dgvView.ScrollBars = ScrollBars.None;
            this.dgvView.MouseWheel += DgvView_MouseWheel;

        }

        private void DgvView_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0 && dgvView.FirstDisplayedScrollingRowIndex > 0)
            {
                dgvView.FirstDisplayedScrollingRowIndex--;
            }
            else if (e.Delta < 0)
            {
                dgvView.FirstDisplayedScrollingRowIndex++;
            }
        }

        public void GetModel(string path)
        {
            List<string> lst = new List<string>();
            lst = File.ReadLines(path).ToList();
            lst.Sort();
            cbbModel.DataSource = lst;
            var source = new AutoCompleteStringCollection();
            source.AddRange(lst.ToArray());
            cbbModel.AutoCompleteCustomSource = source;
        }
        private void button13_Click(object sender, EventArgs e)
        {
            // Validate
            if (!ValidateWo(txtWO.Text.Trim()))
            {
                MessageBox.Show("WO không được bỏ trống", "Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                txtWO.Focus();
                return;
            }
            if (!ValidateQuantity(txtSl.Text.Trim()))
            {
                MessageBox.Show("Số lượng sai định dạng", "Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                txtSl.Focus();
                return;
            }
            if (!ValidateModel(cbbModel.Text.Trim(), cbbCustomer.Text))
            {
                MessageBox.Show("Sai Model", "Errors", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                cbbModel.Focus();
                return;
            }
            string data = string.Empty;
            string customer = cbbCustomer.Text;
            string wo = txtWO.Text;
            string model = cbbModel.Text;
            string quantity = txtSl.Text.Trim();
            string type = cbbType.Text;
            string timeCall = DateTime.Now.ToString("HH:mm:ss");
            Obj obj = new Obj();
            // Add data to datagridview
            DataGridViewRow row = (DataGridViewRow)dgvView.Rows[0].Clone();
            row.Cells[0].Value = obj.customer = customer;
            row.Cells[1].Value = obj.wo = wo;
            row.Cells[2].Value = obj.model = model;
            row.Cells[3].Value = obj.quantity = quantity;
            row.Cells[4].Value = obj.type = type;
            row.Cells[5].Value = obj.timeCall = timeCall;
            row.Cells[7].Value = "Đang gọi";
            row.Cells[8].Value = "Cancel";
            row.DefaultCellStyle.BackColor = Color.Yellow;
            lstObj.Add(obj);
            dgvView.Rows.Add(row);
            // Sort by time call
            dgvView.Sort(this.dgvView.Columns["clTimeCall"], ListSortDirection.Descending);

            data = string.Join("*", customer, wo, model, quantity, type, timeCall);
            Send(data);
        }
        /// <summary>
        /// Tìm button
        /// </summary>
        /// <param name="control"></param>
        /// <param name="type"></param>
        /// <returns>All button</returns>
        public void View()
        {
            Console.WriteLine(dataReceive);
            if (dataReceive.Contains("SERVER CLOSE"))
            {
                CloseConnect();
                Application.Exit();
            }
            else
            {
                List<string> lst = dataReceive.Split('*').ToList();
                Obj obj = lstObj.Find(r => r.customer.Contains(lst[0]) && r.model.Contains(lst[1]) && r.timeCall.Equals(lst[2]));
                obj.timeResponseStart = lst[3];
                foreach (DataGridViewRow item in dgvView.Rows)
                {
                    if (item.Cells[0].Value.ToString().Equals(obj.customer) && item.Cells[2].Value.ToString().Equals(obj.model) && item.Cells[5].Value.ToString().Equals(obj.timeCall))
                    {
                        item.Cells[6].Value = obj.timeResponseStart;
                        item.DefaultCellStyle.BackColor = Color.LightGreen;
                        item.Cells[7].Value = "Đang trả";
                        item.Cells[8].Value = "OK";
                        break;
                    }
                }
            }

        }
        public void LoadState(string path)
        {
            List<objState> list = new List<objState>();
            if (File.Exists(path))
            {
                File.ReadLines(path).ToList().ForEach(r =>
                {
                    string[] col = r.Split(',');
                    objState obj = new objState() { name = col[0], color = col[1] };
                    list.Add(obj);
                });

            }
        }

        private void reportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmReport frmReport = new frmReport();
            frmReport.ShowDialog();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmAbout frmAbout = new frmAbout();
            frmAbout.ShowDialog();
        }


        private void cbbCustomer_SelectedIndexChanged(object sender, EventArgs e)
        {
            string filePathModel = Application.StartupPath + "\\setup\\" + cbbCustomer.Text + ".txt";
            GetModel(filePathModel);
        }

        public bool ValidateWo(string wo)
        {
            if (string.IsNullOrEmpty(wo))
            {
                return false;
            }
            return true;
        }
        public bool ValidateQuantity(string quantity)
        {
            Regex regex = new Regex(@"^\d+$");
            if (regex.IsMatch(quantity))
            {
                return true;
            }
            return false;
        }

        public bool ValidateModel(string model, string customer)
        {
            string path = Application.StartupPath + "\\Setup\\" + customer + ".txt";
            if (File.ReadLines(path).Contains(model))
                return true;
            return false;
        }
        private void dgvView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;
            string nameFile = pathLog + "\\" + DateTime.Now.ToString("ddMMyy") + ".txt";
            if (senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn &&
                e.RowIndex >= 0 && senderGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null)
            {
                string confirm = senderGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                string customer = senderGrid.Rows[e.RowIndex].Cells[0].Value.ToString();
                string model = senderGrid.Rows[e.RowIndex].Cells[2].Value.ToString();
                string timeCall = senderGrid.Rows[e.RowIndex].Cells[5].Value.ToString();
                Obj obj = lstObj.Find(r => r.customer.Equals(customer) && r.model.Equals(model) && r.timeCall.Equals(timeCall));
                string timeResponseStart = obj.timeResponseStart = senderGrid.Rows[e.RowIndex].Cells[6].Value == null ? "#NA" : DateTime.Now.ToString("HH:mm:ss");
                string timeResponseEnd = obj.timeResponseEnd = DateTime.Now.ToString("HH:mm:ss");
                string data = confirm.Contains("Cancel") ? string.Join("*", obj.customer, obj.model, obj.timeCall, "CANCEL") : string.Join("*", obj.customer, obj.model, obj.timeCall, "FINISH");
                obj.status = confirm.Contains("Cancel") ? "CANCEL" : "OK";
                Send(data);
                Common.SaveLog(nameFile, obj);
                dgvView.Rows.RemoveAt(e.RowIndex);
                //obj.timeReponseStart = timeResponseStart;
            }
        }

    }
}
