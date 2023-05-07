using ComputerComponentsLibrary;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Client_ProtocolUDP
{
    public partial class Form1 : Form
    {
        // ������ �������
        IPAddress address = IPAddress.Parse("192.168.56.1");
        // ���� �������
        int port = 11000;
        // ����� ��� �������� ������������ ����� �� ���������� ComboBox
        bool isComboBoxLoad = false;

        public Form1()
        {
            InitializeComponent();

            //Process.Start("ServerRecipeBook.exe");
        }

        private async void btnSendRequest_Click(object sender, EventArgs e)
        {
            // ��������� �볺���
            UdpClient udpClient = null;
            try
            {
                // ³������� ������ -----------------------------------------
                udpClient = new UdpClient();

                // ��������� ����� ��� ��������� ������ (��� ������������ �������� �������� ��� ComboBox
                // ��� ������������ �������� ���������� ��� ������������ � DataGridView)
                string data = isComboBoxLoad ? "" + cbCategoryName.SelectedIndex.ToString() : "GETCategory";
                //string data = "" + cbCategoryName.SelectedIndex.ToString();
                
                // ������� ��������� ������� ������ ������� �� ...
                //MessageBox.Show(data);
                //MessageBox.Show("cbCategoryName - " + cbCategoryName.Items);

                // ��������� ������ ��� �������� ����� �������
                byte[] buff = Encoding.Default.GetBytes(data);
                // ʳ����� ����� ��� ���������� �볺���
                IPEndPoint remoteEndpoint = new IPEndPoint(address, port);
                
                // ²������� ����� �� ������ 
                // ������� ��������������� UDP-��������, ���������� �� ������� / �� ���������������
                // - �������� ���������� ����� ����� Send �� ���� ��������� �������
                await udpClient.SendAsync(buff, buff.Length, remoteEndpoint);

                // ��������� ������ �� ����� ------------------------------------------------
                // ��������� ������
                byte[] buffer = null;
                // ��������� �����
                UdpReceiveResult bufferTemp = await udpClient.ReceiveAsync();
                // �������������� ����� �� ������
                buffer = bufferTemp.Buffer;
                // ��������� ����� � ������ �����
                string recievedData = Encoding.Default.GetString(buffer);

                // ������� ������ �������
                if (isComboBoxLoad)
                {
                    // ������������ ����� - ��������� �������� ��������Ҳ� (������ �� ��������)
                    List<Component> components = JsonSerializer.Deserialize<List<Component>>(recievedData);

                    // ��������� ����� � ��������� ��������� �������
                    // ������� ������ � �볺���� ���������� � �������� ������, � ��������� ������ � ���������,
                    // ��������������� ����� BeginInvoke(), ����� ���������� ������� Action 
                    dgvComponents.BeginInvoke(new Action<List<Component>>(ListUpdate), components);
                } 
                else
                {
                    // ������������ ����� - ��������� �������� ������в� 
                    List<CategoryComponent> categoryComponents = JsonSerializer.Deserialize<List<CategoryComponent>>(recievedData);

                    // ��������� ���������� ComboBox - ��� ������ ������� ������
                    cbCategoryName.BeginInvoke(new Action<List<CategoryComponent>>(ComboBoxUpdate), categoryComponents);    
                    isComboBoxLoad = true; 
                }

            }
            catch (SocketException ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                // �������� �볺���
                udpClient.Close();
            }

        }

        // ��������� ���������� - ������ ������� ������ (���� ��������� ������ ������� �� ��������� ����� �볺��� (GETCategory))
        private void ComboBoxUpdate(List<CategoryComponent> categories)
        {
            // ������������ �������� �������� �� ���������� ComboBox
            cbCategoryName.DataSource = null;
            cbCategoryName.DisplayMember = "Name";
            cbCategoryName.ValueMember = "Name";
            cbCategoryName.DataSource = categories;
        }

        // ��������� ���������� ���������� - ������ ����������
        // (���� ������ ����������� �� ��������� �������� ������ �������)
        private void ListUpdate(List<Component> components)
        {
            dgvComponents.DataSource = null;
            dgvComponents.DataSource = components;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // �������� ������������ ������ �������� ���� ������������ �����
            // - ��� ����� �����������
            btnSendRequest_Click(null, null);
        }
    }
}