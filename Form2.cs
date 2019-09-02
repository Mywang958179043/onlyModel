using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace onlyModel
{
    public partial class Form2 : Form
    {
        //172.18.2.47
        public string connString;
        public string dataBaseName; //数据库名称
        public string tableName; //表名称
        public bool IsMSQL;

        public Form2(string form, bool isMySql)
        {
            InitializeComponent();
            connString = form;
            IsMSQL = isMySql;
            if (isMySql)
            {
                GetMysqlDataBase(connString);
            }
            else
            {
               GetSQLServer(connString);
            }
          
        }

        private void cmbDatabase_SelectedIndexChanged(object sender, EventArgs e)
        {
            string value = cmbDatabase.Text;
            if (IsMSQL)
            {
                string table = "";
                GetMySqlTable(value, table);
            }
            else
            {
                string values = "";
                GetSqlServerTable(values);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataBaseName = cmbDatabase.Text;
            tableName = listBox1.Text;
            if (IsMSQL)
            {
                GetColumn();
            }
            else
            {
                GetSqlServerColumn();
            }
            
        }

        private void txtContent_TextChanged(object sender, EventArgs e)
        {

        }

        //获取mysql数据库
        private void GetMysqlDataBase(string connString)
        {
            try
            {
                MySqlConnection AConn = new MySqlConnection(connString);
                AConn.Open();
                MySqlCommand cmd = new MySqlCommand("show databases", AConn);
                MySqlDataReader reader = cmd.ExecuteReader();
                List<string> list = new List<string>();
                while (reader.Read())//初始索引是-1，执行读取下一行数据，返回值是bool
                {
                    string str = reader["Database"].ToString();
                    list.Add(str);
                }
                reader.Close();
                cmbDatabase.DataSource = list;
                AConn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("数据库加载错误！错误信息：" + ex.Message);
            }
        }

        //获取mysql数据库表
        private void GetMySqlTable(string value,string table)
        {
            MySqlConnection AConn = new MySqlConnection(connString);
            AConn.Open();
            StringBuilder sql = new StringBuilder();
            sql.AppendFormat(" select table_name from information_schema.tables where table_schema='{0}' ",value);
            sql.AppendFormat(" and table_type='base table' ");
            if (table != "")
            {
                sql.AppendFormat(" AND table_name LIKE '%{0}%' ", table);
            }
            MySqlCommand cmd = new MySqlCommand(sql.ToString(), AConn);
            MySqlDataReader reader = cmd.ExecuteReader();
            List<string> list = new List<string>();
            while (reader.Read())//初始索引是-1，执行读取下一行数据，返回值是bool
            {
                string str = reader["table_name"].ToString();
                list.Add(str);
            }
            reader.Close();
            listBox1.DataSource = list;
        }

        //获取mysql数据库字段信息
        private void GetColumn()
        {
            MySqlConnection AConn = new MySqlConnection(connString);
            AConn.Open();

            //列名、类型、注释
            StringBuilder sql = new StringBuilder();
            sql.Append(" SELECT COLUMN_NAME,DATA_TYPE,COLUMN_COMMENT FROM information_schema. COLUMNS WHERE ");
            sql.Append(" table_name = '" + tableName + "'");
            sql.Append(" AND table_schema = '" + dataBaseName + "'");
            MySqlCommand cmd = new MySqlCommand(sql.ToString(), AConn);
            MySqlDataReader reader = cmd.ExecuteReader();

            DataTable dt = new DataTable();
            dt.Columns.Add("COLUMN_NAME", typeof(string));//Type.GetType("System.String")
            dt.Columns.Add("DATA_TYPE", typeof(string));
            dt.Columns.Add("COLUMN_COMMENT", typeof(string));
            dt.Columns.Add("tableName", typeof(string));
            while (reader.Read())//初始索引是-1，执行读取下一行数据，返回值是bool
            {
                DataRow dr = dt.NewRow();
                var sql1 = reader["DATA_TYPE"].ToString();
                dr["COLUMN_NAME"] = reader["COLUMN_NAME"].ToString();
                dr["DATA_TYPE"] = reader["DATA_TYPE"].ToString();
                dr["COLUMN_COMMENT"] = reader["COLUMN_COMMENT"].ToString();
                dr["tableName"] = tableName;
                dt.Rows.Add(dr);
            }
            List<string> tableNameArray = new List<string>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var read = dt.Rows[i];
                tableNameArray.Add(Convert.ToString(read["tableName"]));  //将表名保存到列表中
            }
            tableNameArray = tableNameArray.Distinct().ToList();
            List<string> list = new List<string>();
            int n = 0;
            foreach (string tableName in tableNameArray)
            {
                n = n + 1;
                DataRow[] dr = dt.Select("tableName='" + tableName + "'");
                MysqlModel.ClassBuilder classBuilder = new MysqlModel.ClassBuilder("Model", tableName, dr);
                string sqlType = "mysql";
                string  value= classBuilder.Execute(sqlType);
                txtContent.Text = value;
                this.txtContent.SelectAll();
                this.txtContent.SelectionColor = Color.Black;
                Font font = new Font(FontFamily.GenericMonospace, 8, FontStyle.Regular);
                this.txtContent.SelectionFont = font;
            }
            
        }

        //获取sqlserver数据库信息
        private void GetSQLServer(string connString)
        {
            try
            {
                SqlCommand con;
                SqlConnection Aon = new SqlConnection(connString);
                Aon.Open();
                string sqlDatabase = "Select Name AS dataBaseName FROM Master..SysDatabases orDER BY Name";
                con = new SqlCommand(sqlDatabase, Aon);
                SqlDataReader reader = con.ExecuteReader();
                List<string> list = new List<string>();
                while (reader.Read())
                {
                    string str = reader["dataBaseName"].ToString();
                    list.Add(str);
                }
                Aon.Close();
                cmbDatabase.DataSource = list;
                con.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show("数据库加载错误！错误信息：" + ex.Message);
            }
        }

        //获取sqlserver数据库表信息
        private void GetSqlServerTable(string dataTable)
        {
            try
            {
                string database = cmbDatabase.Text;   //数据库
                SqlCommand con;
                SqlConnection Aon = new SqlConnection(connString);
                Aon.Open();
                StringBuilder sqlDatabase = new StringBuilder();
                sqlDatabase.AppendFormat("use {0};select name AS tableName from sysobjects where xtype='U' ", '"'+database+'"');
                if (dataTable != "")
                {
                    sqlDatabase.AppendFormat(" AND name LIKE '%{0}%' ", dataTable);
                }
                sqlDatabase.AppendFormat(" order by name asc ");
                con = new SqlCommand(sqlDatabase.ToString(), Aon);
                SqlDataReader reader = con.ExecuteReader();
                List<string> list = new List<string>();
                while (reader.Read())
                {
                    string str = reader["tableName"].ToString();
                    list.Add(str);
                }
                listBox1.DataSource = list;
                con.Dispose();
                Aon.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("数据库加载错误！错误信息：" + ex.Message);
            }
        }

        //获取sqlserver的字段信息
        private void GetSqlServerColumn()
        {
            SqlCommand con;
            SqlConnection Aon = new SqlConnection(connString);
            Aon.Open();

            //查询出字段信息、类型
            StringBuilder sql = new StringBuilder();
            sql.AppendFormat("use {0}; SELECT a.name AS [COLUMN_NAME],b.name AS DATA_TYPE FROM syscolumns a,systypes b WHERE a.id= object_id('{1}')"  , '"'+dataBaseName+'"', tableName);
            sql.AppendFormat(" AND a.xtype= b.xtype ");
            sql.AppendFormat(" AND b.name IN ('int','text','bigint','binary','bit','char','datetime','decimal','float','image', ");
            sql.AppendFormat(" 'money','nchar','ntext','numeric','nvarchar','real','smalldatetime','smallint','smallmoney', ");
            sql.AppendFormat(" 'timestamp','tinyint','uniqueidentifier','varbinary','varchar','variant') ");
            con = new SqlCommand(sql.ToString(), Aon);
            SqlDataReader reader = con.ExecuteReader();
            List<Model> list = new List<Model>();
            while (reader.Read())
            {
                Model model = new Model();
                model.COLUMN_NAME = reader["COLUMN_NAME"].ToString();
                model.DATA_TYPE = reader["DATA_TYPE"].ToString();
                list.Add(model);
            }
            reader.Close();

            //查询出字段注释
            sql.Remove(0, sql.Length);
            sql.AppendFormat("use {0}; SELECT c.name as COLUMN_NAME, a.VALUE as COLUMN_COMMENT FROM sys.extended_properties a, sysobjects b," ,'"'+dataBaseName+'"');
            sql.AppendFormat(" sys.columns c WHERE a.major_id = b.id AND c.object_id = b.id AND c.column_id = a.minor_id AND b.name = ('{0}')", tableName);
            SqlCommand Comment = new SqlCommand(sql.ToString(), Aon);
            SqlDataReader reade = Comment.ExecuteReader();
            while (reade.Read())
            {
                string COLUMN_NAME = reade["COLUMN_NAME"].ToString();
                string COLUMN_COMMENT = reade["COLUMN_COMMENT"].ToString();
                foreach (var item in list) 
                {
                    if (item.COLUMN_NAME == COLUMN_NAME) item.COLUMN_COMMENT = COLUMN_COMMENT;
                }
            }
            Aon.Close();

            DataTable dt = new DataTable();
            dt.Columns.Add("COLUMN_NAME", typeof(string));//Type.GetType("System.String")
            dt.Columns.Add("DATA_TYPE", typeof(string));
            dt.Columns.Add("COLUMN_COMMENT", typeof(string));
            dt.Columns.Add("tableName", typeof(string));

            foreach (var item in list)
            {
                DataRow dr = dt.NewRow();
                dr["COLUMN_NAME"] = item.COLUMN_NAME;
                dr["DATA_TYPE"] = item.DATA_TYPE;
                dr["COLUMN_COMMENT"] = item.COLUMN_COMMENT;
                dr["tableName"] = tableName;
                dt.Rows.Add(dr);
            }

            List<string> tableNameArray = new List<string>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var read = dt.Rows[i];
                tableNameArray.Add(Convert.ToString(read["tableName"]));  //将表名保存到列表中
            }
            tableNameArray = tableNameArray.Distinct().ToList();
            int n = 0;
            foreach (string tableName in tableNameArray)
            {
                n = n + 1;
                DataRow[] dr = dt.Select("tableName='" + tableName + "'");
                MysqlModel.ClassBuilder classBuilder = new MysqlModel.ClassBuilder("Model", tableName, dr);
                string sqlType = "sqlserver";
                string value = classBuilder.Execute(sqlType);
                txtContent.Text = value;
                this.txtContent.SelectAll();
                this.txtContent.SelectionColor = Color.Black;
                Font font = new Font(FontFamily.GenericMonospace, 8, FontStyle.Regular);
                this.txtContent.SelectionFont = font;
            }
        }

        //搜索
        private void button1_Click(object sender, EventArgs e)
        {
            string Table = input1.Text;
            if (IsMSQL)
            {
                string DataBase = cmbDatabase.Text;
                GetMySqlTable(DataBase, Table);
                
            }
            else
            {
                GetSqlServerTable(Table);
            }
        }

        //生成类文件
        private void btnGenerateFile_Click(object sender, EventArgs e)
        {
            string classPath = textBox1.Text;

        }

        private void input1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
