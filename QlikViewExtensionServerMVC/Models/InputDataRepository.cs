using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data.SQLite;
using Newtonsoft.Json;

using myCore = frqtlib.Core;

namespace QlikViewExtensionServerWS.Models
{
    public class InputDataRepository : IInputDataRepository
    {

        public InputDataRepository()
        {
            /*
                CREATE TABLE Data
                (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserName varchar(50),
                    ModificationDate datetime,
                    Bucket varchar(256),
                    BucketCategory varchar(256),
                    Context varchar(2048),
                    Value varchar(1024)
                )
            */
        }

        public IEnumerable<InputData> Get(int? id = null, string userName = null, string bucket = null, string bucketCategory = null)
        {
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=Db/InputOutput.db"))
            using (SQLiteCommand cmd = conn.CreateCommand())
            {
                conn.Open();

                cmd.CommandText =
                    @"SELECT 
                        Id,
                        UserName,
                        ModificationDate,
                        Bucket,
                        BucketCategory,
                        Context,
                        Value
                    FROM
                        Data
                    WHERE
                        1 = 1" +
                        ((id == null) ? "" : " AND Id = @ParamId") +
                        ((userName == null) ? "" : " AND UserName = @ParamUserName") +
                        ((bucket == null) ? "" : " AND Bucket = @ParamBucket") +
                        ((bucketCategory == null) ? "" : " AND BucketCategory = @ParamBucketCategory");

                if (id != null) cmd.Parameters.Add(new SQLiteParameter("@ParamId", id));
                if (userName != null) cmd.Parameters.Add(new SQLiteParameter("@ParamUserName", userName));
                if (bucket != null) cmd.Parameters.Add(new SQLiteParameter("@ParamBucket", bucket));
                if (bucketCategory != null) cmd.Parameters.Add(new SQLiteParameter("@ParamBucketCategory", bucketCategory));

                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    if (id != null && !reader.HasRows) throw new ArgumentOutOfRangeException("Id does not exist in table");

                    while (reader.Read())
                    {
                        yield return new InputData()
                        {
                            Id = reader.GetInt32(0),
                            UserName = (reader.IsDBNull(1)) ? null : reader.GetString(1),
                            ModificationDate = (reader.IsDBNull(2)) ? DateTime.Now : Convert.ToDateTime(reader.GetString(2)),
                            Bucket = (reader.IsDBNull(3)) ? null : reader.GetString(3),
                            BucketCategory = (reader.IsDBNull(4)) ? null : reader.GetString(4),
                            Context = (reader.IsDBNull(5)) ? null : JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(reader.GetString(5)),
                            Value = (reader.IsDBNull(6)) ? null : reader.GetString(6)
                        };
                    }

                }
            }
        }

        public InputData Add(InputData item)
        {
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=Db/InputOutput.db"))
            using (SQLiteCommand cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText =
                    @"INSERT INTO Data (
                        UserName,
                        ModificationDate,
                        Bucket,
                        BucketCategory,
                        Context,
                        Value
                    ) VALUES (
                        @ParamUserName,
                        @ParamModificationDate,
                        @ParamBucket,
                        @ParamBucketCategory,
                        @ParamContext,
                        @ParamValue
                    );
                    select last_insert_rowid();";

                cmd.Parameters.Add(new SQLiteParameter("@ParamUserName", item.UserName));
                cmd.Parameters.Add(new SQLiteParameter("@ParamModificationDate", DateTime.Now));
                cmd.Parameters.Add(new SQLiteParameter("@ParamBucket", item.Bucket));
                cmd.Parameters.Add(new SQLiteParameter("@ParamBucketCategory", item.BucketCategory));
                cmd.Parameters.Add(new SQLiteParameter("@ParamContext", JsonConvert.SerializeObject(item.Context)));
                cmd.Parameters.Add(new SQLiteParameter("@ParamValue", item.Value));

                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    reader.Read();

                    int id = reader.GetInt32(0);

                    return this.Get(id).First();
                }
            }
        }

        public void Remove(int id)
        {
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=Db/InputOutput.db"))
            using (SQLiteCommand cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText =
                    @"DELETE FROM Data
                    WHERE Id = @ParamId";

                cmd.Parameters.Add(new SQLiteParameter("@ParamId", id));

                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    reader.Read();

                    return;
                }
            }
        }

        public bool Update(InputData item)
        {
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=Db/InputOutput.db"))
            using (SQLiteCommand cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText =
                    @"UPDATE Data
                    SET
                        UserName = @ParamUserName,
                        ModificationDate = @ParamModificationDate,
                        Bucket = @ParamBucket,
                        BucketCategory = @ParamBucketCategory,
                        Context = @ParamContext,
                        Value = @ParamValue
                    WHERE
                        Id = @ParamId";

                cmd.Parameters.Add(new SQLiteParameter("@ParamId", item.Id));
                cmd.Parameters.Add(new SQLiteParameter("@ParamUserName", item.UserName));
                cmd.Parameters.Add(new SQLiteParameter("@ParamModificationDate", DateTime.Now));
                cmd.Parameters.Add(new SQLiteParameter("@ParamBucket", item.Bucket));
                cmd.Parameters.Add(new SQLiteParameter("@ParamBucketCategory", item.BucketCategory));
                cmd.Parameters.Add(new SQLiteParameter("@ParamContext", JsonConvert.SerializeObject(item.Context)));
                cmd.Parameters.Add(new SQLiteParameter("@ParamValue", item.Value));

                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    reader.Read();

                    return true;
                }
            }
        }
    }
}