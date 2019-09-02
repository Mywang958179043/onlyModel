using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace onlyModel
{
    public partial class Form1 : Form
    {
        string connString = string.Empty;
        public Form1()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            bool isMySql = checkBox1.Checked;
            if (isMySql)
            {
                connString = string.Format("server={0};port=3306;user id={1};password={2};charset=utf8;", txtServer.Text, txtUser.Text, txtPassword.Text);
            }
            else
            {
                if (ckbSystemUser.Checked)
                {
                    connString = string.Format("Data Source={0};Initial Catalog={1};Integrated Security=true;", txtServer.Text);
                }
                else
                {
                    connString = string.Format("Data Source={0};Integrated Security=false;User={1};Password={2};", txtServer.Text, txtUser.Text, txtPassword.Text);
                }
            }

            try
            {
                if (isMySql)
                {
                    MySqlConnection AConn = new MySqlConnection(connString);
                    AConn.Open();
                    AConn.Close();
                    this.Close();
                }
                else
                {
                    SqlConnection con = new SqlConnection(connString);
                    con.Open();
                    con.Close();
                    this.Close();
                }
                JumpForm2(connString,isMySql);
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                MessageBox.Show("连接失败！错误信息：" + ex.Message);
            }
        }

        private void JumpForm2(string connString,bool isMySql)
        {
            //打开另一个窗口的同时关闭当前窗口
            Thread th = new Thread(delegate () { new Form2(connString, isMySql).ShowDialog(); });
            th.Start();
            this.Close();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
