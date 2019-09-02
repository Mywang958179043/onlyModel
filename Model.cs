using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace onlyModel
{
    public class Model
    {
        /// <summary>
        /// 字段信息
        /// </summary>
        public string COLUMN_NAME { get; set; }

        /// <summary>
        /// 字段类型
        /// </summary>
        public string DATA_TYPE { get; set; }

        /// <summary>
        /// 字段注释
        /// </summary>
        public string COLUMN_COMMENT { get; set; }
    }
}
