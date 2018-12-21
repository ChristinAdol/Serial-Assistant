using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Serial_Assistant
{
	public partial class Form1 : Form
	{
		private long receive_count = 0; //接收字节计数
		private long send_count = 0;    //发送字节计数
		private StringBuilder sb = new StringBuilder();    //为了避免在接收处理函数中反复调用，依然声明为一个全局变量

		public class THbuffer     //定义一个检测数据结构体
		{
			public static int T = 0, H = 0;     //T-温度 H-湿度
		}

		private StringBuilder builder = new StringBuilder();    //避免在事件处理方法中反复创建，定义为全局

		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			//读取电脑可用串口并添加到选项中
			comboBox1.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
			//设置选项默认值
			comboBox2.Text = "9600";
			comboBox3.Text = "8";
			comboBox4.Text = "1";
			comboBox5.Text = "None";
		}

		private void button1_Click(object sender, EventArgs e)
		{
			try
			{
				//将可能产生异常的代码放置在try块中
				//根据当前串口属性来判断是否打开
				if (serialPort1.IsOpen)
				{
					//串口已经处于打开状态
					serialPort1.Close();    //关闭串口
					button1.Text = "打开串口";
					button1.BackColor = Color.ForestGreen;
					comboBox1.Enabled = true;
					comboBox2.Enabled = true;
					comboBox3.Enabled = true;
					comboBox4.Enabled = true;
					comboBox5.Enabled = true;
					label8.Text = "串口已关闭";
					label8.ForeColor = Color.Red;
					button2.Enabled = false;        //失能发送按钮
				}
				else
				{
					//串口已经处于关闭状态，则设置好串口属性后打开
					comboBox1.Enabled = false;
					comboBox2.Enabled = false;
					comboBox3.Enabled = false;
					comboBox4.Enabled = false;
					comboBox5.Enabled = false;
					serialPort1.PortName = comboBox1.Text;
					serialPort1.BaudRate = Convert.ToInt32(comboBox2.Text);
					serialPort1.DataBits = Convert.ToInt16(comboBox3.Text);

					if (comboBox4.Text.Equals("None"))
						serialPort1.Parity = System.IO.Ports.Parity.None;
					else if (comboBox4.Text.Equals("Odd"))
						serialPort1.Parity = System.IO.Ports.Parity.Odd;
					else if (comboBox4.Text.Equals("Even"))
						serialPort1.Parity = System.IO.Ports.Parity.Even;
					else if (comboBox4.Text.Equals("Mark"))
						serialPort1.Parity = System.IO.Ports.Parity.Mark;
					else if (comboBox4.Text.Equals("Space"))
						serialPort1.Parity = System.IO.Ports.Parity.Space;

					if (comboBox5.Text.Equals("1"))
						serialPort1.StopBits = System.IO.Ports.StopBits.One;
					else if (comboBox5.Text.Equals("1.5"))
						serialPort1.StopBits = System.IO.Ports.StopBits.OnePointFive;
					else if (comboBox5.Text.Equals("2"))
						serialPort1.StopBits = System.IO.Ports.StopBits.Two;

					serialPort1.Open();     //打开串口
					button1.Text = "关闭串口";
					button1.BackColor = Color.Firebrick;
					label8.Text = "串口已打开";
					label8.ForeColor = Color.Green;
					button2.Enabled = true;        //使能发送按钮

				}
			}
			catch (Exception ex)
			{
				//捕获可能发生的异常并进行处理

				//捕获到异常，创建一个新的对象，之前的不可以再用
				serialPort1 = new System.IO.Ports.SerialPort();
				//刷新COM口选项
				comboBox1.Items.Clear();
				comboBox1.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
				//响铃并显示异常给用户
				System.Media.SystemSounds.Beep.Play();
				button1.Text = "打开串口";
				button1.BackColor = Color.ForestGreen;
				MessageBox.Show(ex.Message);
				comboBox1.Enabled = true;
				comboBox2.Enabled = true;
				comboBox3.Enabled = true;
				comboBox4.Enabled = true;
				comboBox5.Enabled = true;
				
			}
		}

		private void button2_Click(object sender, EventArgs e)
		{
			textBox1.Text = "";		//清空接收文本框

			receive_count = 0;		//接收计数清零
			send_count = 0;			//发送计数
			label7.Text = "Rx:" + receive_count.ToString() + "Bytes";   //刷新界面
			label6.Text = "Tx:" + send_count.ToString()    + "Bytes";   //刷新界面
		}



		//串口接收事件处理
		private void SerialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
		{
			int num = serialPort1.BytesToRead;      //获取接收缓冲区中的字节数
			byte[] received_buf = new byte[num];    //声明一个大小为num的字节数据用于存放读出的byte型数据

			receive_count += num;                   //接收字节计数变量增加nun
			serialPort1.Read(received_buf, 0, num);   //读取接收缓冲区中num个字节到byte数组中

			sb.Clear();     //防止出错,首先清空字符串构造器

			if (num != 0)
			{
				THbuffer.T = received_buf[0];
				THbuffer.H = received_buf[1];
			}

			if (radioButton2.Checked)
			{
				//选中HEX模式显示
				foreach (byte b in received_buf)
				{
					sb.Append(b.ToString("X2") + ' ');    //将byte型数据转化为2位16进制文本显示，并用空格隔开
				}
			}
			else
			{
				//选中ASCII模式显示
				sb.Append(Encoding.ASCII.GetString(received_buf));  //将整个数组解码为ASCII数组
			}
			try
			{
				//因为要访问UI资源，所以需要使用invoke方式同步ui
				Invoke((EventHandler)(delegate
				{
					
					textBox1.AppendText(sb.ToString());
					label7.Text = "Rx:" + receive_count.ToString() + "Bytes";
					label11.Text = ("温度：" + THbuffer.T.ToString() + " ℃");
					label12.Text = ("湿度：" + THbuffer.H.ToString() + " %");
				}
				  )
				);
			}
			catch (Exception ex)
			{
				//响铃并显示异常给用户
				System.Media.SystemSounds.Beep.Play();
				MessageBox.Show(ex.Message);

			}
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			label10.Text = System.DateTime.Now.ToString();
		}
	}
}
