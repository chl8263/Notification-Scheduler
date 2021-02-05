using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace NotiScheduler.Helper {

    public class DBHelper {

        private SqlConnection conn;

        public DBHelper () {
        }

        public void setConnection(string db, string country) {
            if (db == "SAP") {
                if (conn != null) {
                    closeConnection(conn);
                }
                conn = getSAPSqlConnection(country);
            }
        }

        public void closeConnection () {
            if (conn != null) {
                conn.Close();
            }
        }

        protected SqlConnection getSAPSqlConnection (string country) {
            string usConnString = System.Configuration.ConfigurationManager.ConnectionStrings["SAPUSConnection"].ConnectionString;
            string caConnString = System.Configuration.ConfigurationManager.ConnectionStrings["SAPCAConnection"].ConnectionString;
            string upConnString = System.Configuration.ConfigurationManager.ConnectionStrings["SAPUPConnection"].ConnectionString;
            SqlConnection conn;
            if (country == "CA") {
                conn = new SqlConnection(caConnString);
            } else if (country == "US") {
                conn = new SqlConnection(usConnString);
            } else {
                conn = new SqlConnection(upConnString);
            }
            conn.Open();
            return conn;
        }

        protected void closeConnection (SqlConnection conn) {
            if (conn != null) {
                conn.Close();
            }
        }

        protected DataTable getDatabyQuery (String query, SqlParameter[] opArr) {
            SqlCommand cmd = new SqlCommand(query, conn);
            if (opArr != null) {
                foreach (SqlParameter op in opArr) {
                    cmd.Parameters.Add(op);
                }
            }
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            conn.Close();
            da.Dispose();

            return dt;
        }

        protected DataTable getDatabySP (String query, SqlParameter[] opArr) {
            SqlCommand cmd = new SqlCommand(query, conn);
            if (opArr != null) {
                foreach (SqlParameter op in opArr) {
                    cmd.Parameters.Add(op);
                }
            }
            cmd.CommandType = CommandType.StoredProcedure;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            conn.Close();
            da.Dispose();

            return dt;
        }

        protected SqlDataReader getRecordSet (String query) {
            return getRecordSet(query, null);
        }

        protected SqlDataReader getRecordSet (String query, SqlParameter[] opArr) {
            SqlCommand cmd = new SqlCommand(query, conn);
            if (opArr != null) {
                foreach (SqlParameter op in opArr) {
                    cmd.Parameters.Add(op);
                }
            }
            SqlDataReader resultSet = cmd.ExecuteReader();
            return resultSet;
        }

        protected SqlDataReader getRecordSetBySP (String query) {
            return getRecordSetBySP(query, null);
        }

        protected SqlDataReader getRecordSetBySP (String query, SqlParameter[] opArr) {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            cmd.Connection = conn;
            cmd.CommandTimeout = 600;
            cmd.CommandType = CommandType.StoredProcedure;
            if (opArr != null) {
                foreach (SqlParameter op in opArr) {
                    cmd.Parameters.Add(op);
                }
            }
            SqlDataReader resultSet = cmd.ExecuteReader();
            cmd.Dispose();
            return resultSet;
        }

        protected bool setSaveData (String query) {
            return setSaveData(query, null);
        }

        protected bool setSaveData (String query, SqlParameter[] opArr) {
            bool result = false;
            SqlCommand cmd = new SqlCommand(query, conn);
            if (opArr != null) {
                foreach (SqlParameter op in opArr) {
                    cmd.Parameters.Add(op);
                }
            }
            if (cmd.ExecuteNonQuery() > 0) {
                result = true;
            }
            cmd.Dispose();
            return result;
        }

        protected bool setSaveDataBySP (String storedProc, SqlParameter[] opArr) {
            bool result = false;
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = storedProc;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 600;
            cmd.Connection = conn;
            if (opArr != null) {
                foreach (SqlParameter op in opArr) {
                    cmd.Parameters.Add(op);
                }
            }
            if (cmd.ExecuteNonQuery() > 0) {
                result = true;
            }
            cmd.Dispose();
            return result;
        }

        protected string getStringData (String sqlQuery, SqlParameter[] opArr) {
            SqlDataReader rdr = null;
            string result = null;
            try {
                rdr = getRecordSet(sqlQuery, opArr);
                if (rdr.HasRows) {
                    while (rdr.Read()) {
                        result = rdr.GetString(0);
                    }
                }
            } catch (SqlException ex) {
                MailHelper.AddSystemIssueString(GetType().Name, ex.ToString());
                throw ex;
            } finally {
                if (rdr.IsClosed == false) {
                    rdr.Close();
                }
            }
            return result;
        }

        protected Int32 getIntData (String sqlQuery, SqlParameter[] opArr) {
            SqlDataReader rdr = null;
            Int32 result = 0;
            try {
                rdr = getRecordSet(sqlQuery, opArr);
                if (rdr.HasRows) {
                    while (rdr.Read()) {
                        result = Convert.ToInt32(rdr.GetValue(0));
                    }
                }
            } catch (SqlException ex) {
                MailHelper.AddSystemIssueString(GetType().Name, ex.ToString());
                throw ex;
            } finally {
                if (rdr.IsClosed == false) {
                    rdr.Close();
                }
            }
            return result;
        }

        protected string getStringDataBySP (String sqlQuery, SqlParameter[] opArr) {
            SqlDataReader rdr = null;
            string result = null;
            try {
                rdr = getRecordSetBySP(sqlQuery, opArr);
                if (rdr.HasRows) {
                    while (rdr.Read()) {
                        result = rdr.GetString(0);
                    }
                }
            } catch (SqlException ex) {
                MailHelper.AddSystemIssueString(GetType().Name, ex.ToString());
                throw ex;
            } finally {
                if (rdr.IsClosed == false) {
                    rdr.Close();
                }
            }
            return result;
        }

        protected Int32 getIntDataBySP (String sqlQuery, SqlParameter[] opArr) {
            SqlDataReader rdr = null;
            Int32 result = 0;
            try {
                rdr = getRecordSetBySP(sqlQuery, opArr);
                if (rdr.HasRows) {
                    while (rdr.Read()) {
                        result = Convert.ToInt32(rdr.GetValue(0));
                    }
                }
            } catch (SqlException ex) {
                MailHelper.AddSystemIssueString(GetType().Name, ex.ToString());
                throw ex;
            } finally {
                if (rdr.IsClosed == false) {
                    rdr.Close();
                }
            }
            return result;
        }

        protected SqlParameter setParam (string var1, string var2) {
            return new SqlParameter(var1, var2);
        }
        protected SqlParameter setParam (string var1, int var2) {
            return new SqlParameter(var1, var2);
        }
        protected SqlParameter setParam (string var1, double var2) {
            return new SqlParameter(var1, var2);
        }
        protected SqlParameter setParam (string var1, DateTime var2) {
            return new SqlParameter(var1, var2);
        }
        protected SqlParameter setParam (string var1, bool var2) {
            return new SqlParameter(var1, var2);
        }

    }
}
