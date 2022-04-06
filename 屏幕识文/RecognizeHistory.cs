using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Collections.ObjectModel;

namespace 屏幕识文
{
    class RecognizeHistory
    {
         //定义全局变量
        public static RecognizeHistory Instance = new RecognizeHistory();
        private SQLiteConnection connection;

        private RecognizeHistory()
        {
            //定义一个string变量（本地）
            string LocalApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string SoftwareName = "屏幕识文";
            string LocalAppData = Path.Combine(LocalApplicationData, SoftwareName);

            //定义变量用在本地系统的相对路径下自动创建一个文件夹，名字叫history
            string historyDir = Path.Combine(LocalAppData, "history");

            // 如果图片文件夹不存在，则创建之
            if (!Directory.Exists(historyDir))
            {
                Directory.CreateDirectory(historyDir);
            }

            //全局变量用于路径的合并和创建一个txt文件，名称是history
             //HistoryFilePath = Path.Combine(historyDir, "history.txt");

            // 1.创建一个数据库文件。
            var HistoryFilePath =  Path.Combine(historyDir, "history.sqlite");
            bool hasFile = File.Exists(HistoryFilePath);

            // 如果文件不存在
            if (!hasFile)
            {
                SQLiteConnection.CreateFile(HistoryFilePath);
            }
            // 2.创建数据库连接。
            var connectionString = new SQLiteConnectionStringBuilder
            {
                DataSource = HistoryFilePath
            };
            connection = new SQLiteConnection(connectionString.ToString());

            // 3.打开连接。
            connection.Open();

            // 如果文件不存在
            if (!hasFile)
            {
                string sql;
                SQLiteCommand command;
                sql = "create table history (id INTEGER PRIMARY KEY AUTOINCREMENT,text text, picture varchar(260))";
                command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();
            }
        }

        //增加记录
        public void Add(RecognizeRecord rr)
        {
            string sql;
            SQLiteCommand command;
            sql = $"insert into history (text, picture) values ('{rr.ResultText}', '{rr.Image}')";
            command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();
        }

        //删除命令
        public void Delete(RecognizeRecord rr)
        {
            string sql;
            SQLiteCommand command;
            sql = $"DELETE FROM history WHERE id={rr.ID}";
            command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();
        }

        //读取数据
        public ObservableCollection<RecognizeRecord> Read()
        {
            var list = new ObservableCollection<RecognizeRecord>();

            string sql;
            SQLiteCommand command;
            sql = "select * from history";
            command = new SQLiteCommand(sql, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                RecognizeRecord rr = new RecognizeRecord();
                rr.ID = (long)reader["id"];
                rr.ResultText = reader["text"] as string;
                rr.Image = reader["picture"] as string;
                list.Insert(0, rr);//从0开始添加到list
            }
            return list;
        }
    }
}
