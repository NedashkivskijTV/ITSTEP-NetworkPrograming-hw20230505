using ComputerComponentsLibrary;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Server_ProtocolUDP
{
    public partial class Form1 : Form
    {
        // ������, �� ��������������������� ��� �������� �����
        Task reciever;
        // ������ ������� - ���������� �����������
        IPAddress address = Dns.GetHostAddresses(Dns.GetHostName())[2];
        // ���� �������
        int port = 11000;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnStartServer_Click(object sender, EventArgs e)
        {
            // ������� ������ �� ������� �������������� �� ������� ���������� �볺���,
            // ������� ��������� ����� ������ � ������� ����

            if (reciever != null)
            {
                return;
            }

            reciever = Task.Run(async () => // ����������� �����
            {
                // ��������� �볺��� �� ���� �������, ���� ���� �������������� �� ������� ���������� �볺���
                UdpClient listener = new UdpClient(new IPEndPoint(address, port));

                // ʳ����� ����� �볺���, ��� ���� ����������� ���������� ��� �������� ����� �� �볺���
                IPEndPoint iPEndPoint = null;

                // ���� ���������������
                while (true)
                {
                    // ��������� ������ �� ��������� ������ �� �볺���
                    byte[] buff = listener.Receive(ref iPEndPoint);
                    // ��������� ���������� ��������� �� ��������� ����������
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("-----------------------------------------");
                    sb.AppendLine($"{buff.Length} received from {iPEndPoint} at {DateTime.Now.ToString()}");
                    string clientRequest = Encoding.Default.GetString(buff);
                    sb.AppendLine(clientRequest);
                    // ��������� ����� � ��������� ��������� �������
                    // ������� ������ � �볺���� ���������� � �������� ������, � ��������� ������ � ���������,
                    // ��������������� ����� BeginInvoke(), ����� ������� Action ���������� string
                    tbServerStatistics.BeginInvoke(new Action<string>(AddText), sb.ToString());

                    // ²������� ²���²Ĳ �� ����� �볺��� ----------------------------------------------------
                    // ��������� ������ ��� �������� �����
                    byte[] bufferForSending = null;
                    // ��������� ����� ��� ��������� ������������ ��'����
                    string dataForSending = "";

                    // ϳ�������� �����
                    if (clientRequest.Equals("GETCategory"))
                    {
                        // ϳ�������� �� �������� �������� ������в�
                        dataForSending = JsonSerializer.Serialize<List<CategoryComponent>>(ComponentsShop.GetCategoriesList());
                    }
                    else
                    {
                        // ϳ�������� �� �������� �������� ��������Ҳ� �������� �� ������
                        
                        // ����������� ��� ���������� ���������� ���� �������� ��� ���� ����������� �����
                        int index = -1;
                        List<Component> componentsList = new List<Component>();
                        if (int.TryParse(clientRequest, out index))
                        {
                            componentsList = ComponentsShop.GetComponentsList().Where(c => c.Category.Id == (index + 1)).ToList();
                        }
                        dataForSending = JsonSerializer.Serialize<List<Component>>(componentsList);
                    }

                    bufferForSending = Encoding.Default.GetBytes(dataForSending);


                    // ³������� ����� -----------------------------------------------------------------------
                    UdpClient udpClient = null;

                    try
                    {
                        udpClient = new UdpClient();
                        IPEndPoint remoteEndpoint = iPEndPoint;
                        await udpClient.SendAsync(bufferForSending, bufferForSending.Length, remoteEndpoint);

                        StringBuilder sbSenb = new StringBuilder();
                        sbSenb.AppendLine($"{bufferForSending.Length} was sending to {iPEndPoint} at {DateTime.Now.ToString()}");
                        //sbSenb.AppendLine(dataForSending);
                        tbServerStatistics.BeginInvoke(new Action<string>(AddText), sbSenb.ToString());
                    }
                    catch (SocketException ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    finally
                    {
                        udpClient.Close();
                    }
                }
            });

            // ��������� ����������� ��� ���� �������
            Text = "Server was started !";
            tbServerStatistics.Text = $"Server was started at {DateTime.Now.ToString()}\r\n";
        }

        private void AddText(string str)
        {
            StringBuilder sb = new StringBuilder(tbServerStatistics.Text);
            sb.Append(str);
            tbServerStatistics.Text = sb.ToString();
        }
    }
}