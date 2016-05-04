using System;
using System.Collections.Generic;
using System.Text;
using IBM.Data.DB2;
using System.Configuration;
using System.Data;

namespace db2Common
{
    public class Db2Common
    {


      public DB2Connection GetConn() {
          string connstr = ConfigurationManager.ConnectionStrings["connstr_db2"].ToString();
          return  new DB2Connection(connstr);
      } // GetConn

      public DataSet getDS(string querystring)
      {
          DataSet ds = new DataSet();
          DB2Connection conn = GetConn();
          conn.Open();
          DB2DataAdapter da = new DB2DataAdapter(querystring, conn);
          da.Fill(ds);
          return ds;
      } // getDS



    } //Db2Common
} // end namespace
