using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace WebApiRulesEngine
{
    public class Employee1
    {
        public static DataSet DataSource { get; internal set; }
        public int ID { get; set; }
            public string First_Name { get; set; }
            public string Last_Name { get; set; }
            public string Gender { get; set; }
            public Nullable<int> Sal { get; set; }

        internal static void DataBind()
        {
            throw new NotImplementedException();
        }
    }
}