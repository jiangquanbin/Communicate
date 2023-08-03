using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Communicate.Core.Entities;
using ModBusCommunication;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private ModbusServer server = new ModbusServer();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }



        private void LoadData(string FileName)
        {
            using (FileStream streamReader = new FileStream(Application.StartupPath + $"/DataSource/{FileName}Data.xlsx", FileMode.Open))
            {
                XSSFWorkbook hssfworkbook = new XSSFWorkbook(streamReader);
                ISheet sheet = hssfworkbook.GetSheetAt(0);
                for (int i = 0; i <= sheet.LastRowNum; i++)
                {
                    IRow row = sheet.GetRow(i);
                    int rowindex = dataGridView1.Rows.Add();
                    if (row != null)
                    {
                        for (int j = 0; j < dataGridView1.ColumnCount - 3; j++)
                        {
                            dataGridView1.Rows[rowindex].Cells[j].Value = row.GetCell(j).ToString();
                        }
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int DriveIndex = 0;
            string IPAddress = "";

            // if (comboBox1.Text == "OmronFinsTCP")
            // {
            //     server = new OmronFinsTCPServer();
            //     OmronFinsTCPServer server1 = (OmronFinsTCPServer)server;
            //     server1.onDataChangeList += Server_onDataChangeList;
            //     for (int i = 0; i < dataGridView1.Rows.Count; i++)
            //     {
            //         if (IPAddress != dataGridView1.Rows[i].Cells[0].Value.ToString())
            //         {
            //             DriveIndex++;
            //             IPAddress = dataGridView1.Rows[i].Cells[0].Value.ToString();
            //             server1.AddDrive(IPAddress, Convert.ToInt32(dataGridView1.Rows[i].Cells[1].Value.ToString()), DriveIndex, Convert.ToByte(dataGridView1.Rows[i].Cells[2].Value.ToString()));
            //         }
            //         server1.AddDBPoint(DriveIndex, dataGridView1.Rows[i].Cells[3].Value.ToString(), dataGridView1.Rows[i].Cells[4].Value.ToString(), i);
            //     }
            //     server1.Connection();
            // }
            // else if (comboBox1.Text == "OmronCIP")
            // {
            //     server = new OmronServer();
            //     OmronServer server1 = (OmronServer)server;
            //     server1.onDataChangeList += OmronServer_onDataChangeList; ;
            //     for (int i = 0; i < dataGridView1.Rows.Count; i++)
            //     {
            //         if (IPAddress != dataGridView1.Rows[i].Cells[0].Value.ToString())
            //         {
            //             DriveIndex++;
            //             IPAddress = dataGridView1.Rows[i].Cells[0].Value.ToString();
            //             server1.AddDrive(IPAddress, Convert.ToInt32(dataGridView1.Rows[i].Cells[1].Value.ToString()), Convert.ToByte(dataGridView1.Rows[i].Cells[2].Value.ToString()), DriveIndex);
            //         }
            //         server1.AddDBPoint(DriveIndex, dataGridView1.Rows[i].Cells[3].Value.ToString(), dataGridView1.Rows[i].Cells[4].Value.ToString(), i);
            //     }
            //     server1.Connection();
            // }
            if (comboBox1.Text == "ModbusTCP")
            {
                server = new ModbusServer();
                ModbusServer server1 = (ModbusServer)server;
                server1.onDataChange += ModbusServer_onDataChangeList; ;
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    if (IPAddress != dataGridView1.Rows[i].Cells[0].Value.ToString())
                    {
                        DriveIndex++;
                        IPAddress = dataGridView1.Rows[i].Cells[0].Value.ToString();
                        server1.AddDrive(DriveIndex,IPAddress, Convert.ToInt32(dataGridView1.Rows[i].Cells[1].Value.ToString()), Convert.ToByte(dataGridView1.Rows[i].Cells[2].Value.ToString()));
                    }
                    server1.AddItem(DriveIndex, dataGridView1.Rows[i].Cells[3].Value.ToString(), dataGridView1.Rows[i].Cells[4].Value.ToString(), i);
                }
                server1.Connection();
            }
            // else if (comboBox1.Text == "ModbusRTU")
            // {
            //     server = new ModbusRtuServer();
            //     ModbusRtuServer server1 = (ModbusRtuServer)server;
            //     server1.onDataChangeList += ModbusRtuServer_onDataChangeList; ;
            //     for (int i = 0; i < dataGridView1.Rows.Count; i++)
            //     {
            //         if (IPAddress != dataGridView1.Rows[i].Cells[0].Value.ToString())
            //         {
            //             DriveIndex++;
            //             IPAddress = dataGridView1.Rows[i].Cells[0].Value.ToString();
            //             string[] values = dataGridView1.Rows[i].Cells[1].Value.ToString().Split(',');
            //             server1.AddDrive(IPAddress, Convert.ToInt32(values[0]), Convert.ToInt32(values[1]), Convert.ToInt32(values[2]), Convert.ToInt32(values[3]), Convert.ToByte(dataGridView1.Rows[i].Cells[2].Value.ToString()), DriveIndex);
            //         }
            //         server1.AddModbusAddress(DriveIndex, dataGridView1.Rows[i].Cells[3].Value.ToString(), dataGridView1.Rows[i].Cells[4].Value.ToString(), i);
            //     }
            //     server1.Connection();
            // }
            // else if (comboBox1.Text == "BACnetIP")
            // {
            //     server = new BacNetServer();
            //     BacNetServer server1 = (BacNetServer)server;
            //     server1.onDataChangeList += BacNetServer_onDataChangeList;
            //     for (int i = 0; i < dataGridView1.Rows.Count; i++)
            //     {
            //         if (IPAddress != dataGridView1.Rows[i].Cells[0].Value.ToString())
            //         {
            //             DriveIndex++;
            //             IPAddress = dataGridView1.Rows[i].Cells[0].Value.ToString();
            //             string ServerPort = dataGridView1.Rows[i].Cells[1].Value.ToString();
            //             string[] info = dataGridView1.Rows[i].Cells[2].Value.ToString().Split('|');
            //
            //             string LocalIPAddress = info[0];
            //             int LocalPort = Convert.ToInt32(info[1]);
            //             string driverid = info[2];
            //             string Networkno = info[3];
            //             string MacAddress = info[4];
            //             server1.AddDrive(LocalIPAddress, LocalPort, IPAddress, Convert.ToInt32(ServerPort), Convert.ToUInt16(driverid), Convert.ToUInt16(Networkno), DriveIndex, MacAddress);
            //         }
            //         server1.AddModbusAddress(DriveIndex, dataGridView1.Rows[i].Cells[3].Value.ToString(), dataGridView1.Rows[i].Cells[4].Value.ToString(), i);
            //     }
            //     server1.Connection();
            // }
            // else if (comboBox1.Text == "S7")
            // {
            //     server = new SiemensServer();
            //     SiemensServer server1 = (SiemensServer)server;
            //     server1.onDataChangeList += Server1_onDataChangeList;
            //     for (int i = 0; i < dataGridView1.Rows.Count; i++)
            //     {
            //         if (IPAddress != dataGridView1.Rows[i].Cells[0].Value.ToString())
            //         {
            //             DriveIndex++;
            //             IPAddress = dataGridView1.Rows[i].Cells[0].Value.ToString();
            //             string values = dataGridView1.Rows[i].Cells[1].Value.ToString();
            //             server1.AddDrive(IPAddress, Convert.ToInt32(values), "S7-1500", DriveIndex);
            //         }
            //         server1.AddDBPoint(DriveIndex, dataGridView1.Rows[i].Cells[3].Value.ToString(), dataGridView1.Rows[i].Cells[4].Value.ToString(), i);
            //     }
            //     server1.Connection();
            // }
        }

        private void Server1_onDataChangeList(List<MonitorItem> Tag)
        {
            foreach (var TagItem in Tag)
            {
                dataGridView1.Rows[(int)TagItem.id].Cells[5].Value = TagItem.ItemValue?.ToString();
                dataGridView1.Rows[(int)TagItem.id].Cells[6].Value = TagItem.UpdateDate.ToString();
                dataGridView1.Rows[(int)TagItem.id].Cells[7].Value = TagItem.Quality.ToString();
            }
        }

        private void OmronServer_onDataChangeList(List<MonitorItem> Tag)
        {
            foreach (var TagItem in Tag)
            {
                dataGridView1.Rows[(int)TagItem.id].Cells[5].Value = TagItem.ItemValue?.ToString();
                dataGridView1.Rows[(int)TagItem.id].Cells[6].Value = TagItem.UpdateDate.ToString();
                dataGridView1.Rows[(int)TagItem.id].Cells[7].Value = TagItem.Quality.ToString();
            }
        }

        private void Server_onDataChangeList(List<MonitorItem> Tag)
        {
            foreach (var TagItem in Tag)
            {
                dataGridView1.Rows[(int)TagItem.id].Cells[5].Value = TagItem.ItemValue?.ToString();
                dataGridView1.Rows[(int)TagItem.id].Cells[6].Value = TagItem.UpdateDate.ToString();
                dataGridView1.Rows[(int)TagItem.id].Cells[7].Value = TagItem.Quality.ToString();
            }
        }
        private void ModbusServer_onDataChangeList(List<MonitorItem> Tag)
        {
            foreach (var TagItem in Tag)
            {
                dataGridView1.Rows[(int)TagItem.id].Cells[5].Value = TagItem.ItemValue?.ToString();
                dataGridView1.Rows[(int)TagItem.id].Cells[6].Value = TagItem.UpdateDate.ToString();
                dataGridView1.Rows[(int)TagItem.id].Cells[7].Value = TagItem.Quality.ToString();
            }
        }

        private void ModbusRtuServer_onDataChangeList(List<MonitorItem> Tag)
        {
            foreach (var TagItem in Tag)
            {
                dataGridView1.Rows[(int)TagItem.id].Cells[5].Value = TagItem.ItemValue?.ToString();
                dataGridView1.Rows[(int)TagItem.id].Cells[6].Value = TagItem.UpdateDate.ToString();
                dataGridView1.Rows[(int)TagItem.id].Cells[7].Value = TagItem.Quality.ToString();
            }
        }
        private void BacNetServer_onDataChangeList(List<MonitorItem> Tag)
        {
            foreach (var TagItem in Tag)
            {
                dataGridView1.Rows[(int)TagItem.id].Cells[5].Value = TagItem.ItemValue?.ToString();
                dataGridView1.Rows[(int)TagItem.id].Cells[6].Value = TagItem.UpdateDate.ToString();
                dataGridView1.Rows[(int)TagItem.id].Cells[7].Value = TagItem.Quality.ToString();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text == "OmronFinsTCP")
            {
                // ((OmronFinsTCPServer)server).Dispose();
            }
            else if (comboBox1.Text == "OmronCIP")
            {
                // ((OmronServer)server).Dispose();
            }
            else if (comboBox1.Text == "ModbusTCP")
            {
                ((ModbusServer)server).Dispose();
            }
            else if (comboBox1.Text == "ModbusRTU")
            {
                // ((ModbusRtuServer)server).Dispose();
            }
            else if (comboBox1.Text == "BACnetIP")
            {
                // ((BacNetServer)server).Dispose();
            }
            else if (comboBox1.Text == "S7")
            {
                // ((SiemensServer)server).Dispose();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            LoadData(comboBox1.Text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text == "OmronFinsTCP")
            {
            }
            else if (comboBox1.Text == "OmronCIP")
            {
                // ((OmronServer)server).Write(Convert.ToInt64(txt_DriveNo.Text), Convert.ToInt64(txt_TagNo.Text), txt_Data.Text);
            }
            else if (comboBox1.Text == "ModbusRTU")
            {
                // ((ModbusRtuServer)server).Write(Convert.ToInt64(txt_DriveNo.Text), Convert.ToInt64(txt_TagNo.Text), txt_Data.Text);
            }
        }
    }
}
