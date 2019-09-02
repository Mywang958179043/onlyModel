using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace onlyModel
{
    public class MysqlModel
    {
        public class ClassBuilder
        {
            #region 属性
            /// <summary>写入器</summary>
            public StringBuilder StringBuilder = new StringBuilder();
            /// <summary>写入器</summary>
            public TextWriter TextWriter { get; set; }
            /// <summary>数据表</summary>
            public string TableName { get; set; }


            /// <summary>命名空间</summary>
            public string Namespace { get; set; }
            /// <summary>数据行</summary>
            public DataRow[] DataRow { get; set; }


            /// <summary>引用命名空间</summary>
            public HashSet<String> Usings { get; } = new HashSet<String>(StringComparer.OrdinalIgnoreCase);

            /// <summary>纯净类</summary>
            public Boolean Pure { get; set; }

            /// <summary>生成接口</summary>
            public Boolean Interface { get; set; }

            /// <summary>基类</summary>
            public string BaseClass { get; set; }
            public int Indent = 5;//缩进
            public string IndetSpace { get; set; }//缩进空格
            #endregion

            #region /// <summary>构造,实例化</summary>
            public ClassBuilder(string nameSpace, string tableName, DataRow[] dr)
            {
                Namespace = nameSpace;
                TableName = tableName;
                DataRow = dr;
                Usings.Add("System");
                Usings.Add("System.Collections.Generic");
                Usings.Add("System.ComponentModel");
                TextWriter = new StringWriter(StringBuilder); //实例化
            }
            #endregion

            #region /// <summary>主方法,执行生成</summary>
            public string  Execute(string sqlType)
            {
                WriteLine("生成 {0}", TableName);
                //写入文件内容开始
                OnExecuting();
                //字段
                BuildItems(sqlType);
                //文件内容末尾
                OnExecuted();
                string value = OutputFile();
                return value;
            }
            #endregion

            #region /// <summary>生成头部(引用命名空间)</summary>
            protected virtual void OnExecuting()
            {
                // 引用命名空间
                var us = Usings.OrderBy(e => e.StartsWith("System") ? 0 : 1).ThenBy(e => e).ToArray();
                foreach (var item in us)
                {
                    TextWriter.WriteLine("using {0};", item);
                }
                TextWriter.WriteLine(); //命名空间和引用之间换行
                if (!string.IsNullOrEmpty(Namespace))
                {
                    TextWriter.WriteLine("namespace {0}", Namespace);
                    TextWriter.WriteLine("{");
                }
                BuildClassHeader();
            }
            #endregion

            #region/// <summary>实体类头部</summary>
            protected virtual void BuildClassHeader()
            {
                // 头部
                BuildAttribute();
                // 类名和基类
                var cn = GetClassName();
                if (!string.IsNullOrEmpty(BaseClass)) BaseClass = " : " + BaseClass;
                // 类接口
                if (Interface)
                    WriteLine("public interface {0}{1}", cn, BaseClass);
                else
                    WriteLine("public partial class {0}{1}", cn, BaseClass);
                WriteLine("{");
            }
            #endregion

            #region/// <summary>获取类名</summary>
            /// <returns></returns>
            protected virtual String GetClassName()
            {
                var name = TableName;
                if (Interface) name = "I" + name;
                return name;
            }
            #endregion

            #region/// <summary>实体类头部</summary>
            protected virtual void BuildAttribute()
            {
                if (!Pure)
                {
                    SetIndent(3);//设置缩进空格
                }
                TextWriter.WriteLine(_Indent + "public partial class {0}", TableName);
                TextWriter.WriteLine(_Indent + "{");
            }
            #endregion

            #region /// <summary>生成尾部</summary>
            protected virtual void OnExecuted()
            {
                // 类接口
                // WriteLine("}");
                // StringBuilder.Append("}");
                SetIndent(3);
                TextWriter.WriteLine(_Indent + "}");
                if (!string.IsNullOrEmpty(Namespace))
                {
                    TextWriter.WriteLine("}");
                }
            }
            #endregion

            #region/// <summary>生成主体</summary>
            protected virtual void BuildItems(string sqlType)
            {
                SetIndent(6);
                for (int i = 0; i < DataRow.Count(); i++)
                {
                    if (i > 0) WriteLine();
                    BuildItem(DataRow[i], sqlType);
                }
                SetIndent(6);
            }
            #endregion

            #region/// <summary>生成每一项</summary>
            protected virtual void BuildItem(DataRow dr, string sqlType)
            {
                SetIndent(11);
                TextWriter.WriteLine(_Indent + "///<summary>{0}</summary>", Convert.ToString(dr["COLUMN_COMMENT"]).Replace("\r\n", ""));  //输出字段说明,同时替换换行符
                string type = Convert.ToString(dr["DATA_TYPE"]); //数据库中字段类型
                //bool isNullAble = Convert.ToBoolean(dr["isnullable"]); //0.不可为空 1.可为空
                string columnType="";
                if (sqlType == "mysql")
                {
                    columnType = MySqlChangeToCSharpType(Convert.ToString(dr["DATA_TYPE"]));
                }
                else if (sqlType=="sqlserver")
                {
                    columnType =SqlServerChangeToCSharpType(Convert.ToString(dr["DATA_TYPE"]));
                }
                TextWriter.WriteLine(_Indent + "public {0} {1} {2}", columnType, dr["COLUMN_NAME"], "{ get; set;}\r\n");  //字段属性
       
            }
            #endregion

            #region /// <summary>设置缩进</summary>
            /// <param name="add"></param>
            private string _Indent;
            protected virtual void SetIndent(int indentNumber)
            {
                _Indent = "               ";
                if (indentNumber <= 15) //空格长度为15
                {
                    _Indent = _Indent.Substring(0, indentNumber);
                }
                else
                {
                    _Indent = "";
                }
            }

            /// <summary>写入</summary>
            /// <param name="value"></param>
            protected virtual void WriteLine(String value = null)
            {
                if (string.IsNullOrEmpty(value))
                {
                    // Writer.WriteLine();
                    return;
                }
                var v = value;
                if (!string.IsNullOrEmpty(_Indent)) v = _Indent + v;
            }


            /// <summary>写入</summary>
            /// <param name="format"></param>
            /// <param name="args"></param>
            protected virtual void WriteLine(String format, params Object[] args)
            {
                if (!string.IsNullOrEmpty(_Indent)) format = _Indent + format;
            }

            /// <summary>清空，重新生成</summary>
            public void Clear()
            {
                _Indent = null;
            }
            #endregion

            #region /// <summary>返回生成的内容</summary>
            public string OutputFile()
            {
                return StringBuilder.ToString();
            }
            #endregion

            #region /// <summary> MySql数据库中与C#中的数据类型对照/// </summary>
            /// <param name="type"></param>
            /// <returns></returns>
            private string MySqlChangeToCSharpType(string type)
            {
                string reval = string.Empty;
                switch (type.ToLower())
                {
                    case "int":
                        reval = "int";
                        break;
                    case "text":
                        reval = "string";
                        break;
                    case "bigint":
                        reval = "long";
                        break;
                    case "binary":
                        reval = "Byte[]";
                        break;
                    case "bit":
                        reval = "Boolean";
                        break;
                    case "char":
                        reval = "string";
                        break;
                    case "datetime":
                        reval = "DateTime";
                        break;
                    case "decimal":
                        reval = "Decimal";
                        break;
                    case "float":
                        reval = "Double";
                        break;
                    case "image":
                        reval = "Byte[]";
                        break;
                    case "money":
                        reval = "Decimal";
                        break;
                    case "nchar":
                        reval = "string";
                        break;
                    case "ntext":
                        reval = "string";
                        break;
                    case "numeric":
                        reval = "Decimal";
                        break;
                    case "nvarchar":
                        reval = "string";
                        break;
                    case "real":
                        reval = "Single";
                        break;
                    case "smalldatetime":
                        reval = "DateTime";
                        break;
                    case "smallint":
                        reval = "short";
                        break;
                    case "smallmoney":
                        reval = "Decimal";
                        break;
                    case "timestamp":
                        reval = "DateTime";
                        break;
                    case "tinyint":
                        reval = "Byte";
                        break;
                    case "uniqueidentifier":
                        reval = "Guid";
                        break;
                    case "varbinary":
                        reval = "Byte[]";
                        break;
                    case "varchar":
                        reval = "string";
                        break;
                    case "Variant":
                        reval = "Object";
                        break;
                    default:
                        reval = "string";
                        break;
                }
                return reval;
            }
            #endregion

            #region /// <summary> SqlServer数据库中与C#中的数据类型对照/// </summary>
            /// <param name="type"></param>
            /// <returns></returns>
            private string SqlServerChangeToCSharpType(string type)
            {
                string reval = string.Empty;
                switch (type.ToLower())
                {
                    case "int":
                        reval = "int";
                        break;
                    case "text":
                        reval = "string";
                        break;
                    case "bigint":
                        reval = "long";
                        break;
                    case "binary":
                        reval = "byte[]";
                        break;
                    case "bit":
                        reval = "Boolean";
                        break;
                    case "char":
                        reval = "string";
                        break;
                    case "datetime":
                        reval = "DateTime";
                        break;
                    case "decimal":
                        reval = "Decimal";
                        break;
                    case "float":
                        reval = "double";
                        break;
                    case "image":
                        reval = "byte[]";
                        break;
                    case "money":
                        reval = "Decimal";
                        break;
                    case "nchar":
                        reval = "string";
                        break;
                    case "ntext":
                        reval = "string";
                        break;
                    case "numeric":
                        reval = "decimal";
                        break;
                    case "nvarchar":
                        reval = "string";
                        break;
                    case "real":
                        reval = "single";
                        break;
                    case "smalldatetime":
                        reval = "DateTime";
                        break;
                    case "smallint":
                        reval = "int";
                        break;
                    case "smallmoney":
                        reval = "decimal";
                        break;
                    case "timestamp":
                        reval = "DateTime";
                        break;
                    case "tinyint":
                        reval = "byte";
                        break;
                    case "uniqueidentifier":
                        reval = "guid";
                        break;
                    case "varbinary":
                        reval = "byte[]";
                        break;
                    case "varchar":
                        reval = "string";
                        break;
                    case "variant":
                        reval = "object";
                        break;
                    default:
                        reval = "string";
                        break;
                }
                return reval;
            }
            #endregion
        }
    }
}
