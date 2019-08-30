using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OJServer
{
    public partial class Server : Form
    {
        private DataTable dt = null;
        public Server()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            while (true)
            {
                dt = DBHelper.ExecuteTable("select * from Solution where status='wait'");
                if (dt.Rows.Count == 0)
                {
                    continue;
                }
                CJudge(dt.Rows[0][2].ToString(), dt.Rows[0][1].ToString(), dt.Rows[0][0].ToString());

            }
        }

        private void CJudge(string user_id, string problem_id, string solution_id)
        {
            if (File.Exists("test/" + user_id + "/" + problem_id + "/a.exe"))
            {
                File.Delete("test/" + user_id + "/" + problem_id + "/a.exe");
            }

            if (File.Exists("test/" + user_id + "/" + problem_id + "/myout.txt"))
            {
                File.Delete("test/" + user_id + "/" + problem_id + "/myout.txt");
            }

            Process p = new Process();
            //设定程序名
            p.StartInfo.FileName = "cmd.exe";
            //关闭Shell的使用
            p.StartInfo.UseShellExecute = false;
            //重定向标准输入
            p.StartInfo.RedirectStandardInput = true;
            //重定向标准输出
            p.StartInfo.RedirectStandardOutput = true;
            //重定向错误输出
            p.StartInfo.RedirectStandardError = true;
            //设置不显示窗口
            p.StartInfo.CreateNoWindow = true;

            // 保存源文件
            // System.IO.File.WriteAllText(Application.StartupPath + @"/test/"+user_id+"/"+problem_id+"/a.c", txt_coder.Text, Encoding.Default);
            // 获取gcc 执行文
            //string strPath = Application.StartupPath + @"\mingw\bin\gcc.exe 1000.c";
            p.Start();
            // 切换到test目录下
            p.StandardInput.WriteLine("cd test/" + user_id + "/" + problem_id + "/");
            //执行gcc命令
            p.StandardInput.WriteLine("gcc a.c");

            // 等待编译完成
            Thread.Sleep(2000);
            // 文件重定向，用的管道
            p.StandardInput.WriteLine("a.exe<../../../problems/" + problem_id + "/in.txt>myout.txt");
            // 检查生成的myout.txt 和 指定的out.txt的数据对比
            p.StandardInput.WriteLine("FC myout.txt ../../../problems/" + problem_id + "/out.txt");
            //int m = p.VirtualMemorySize;
            string memory = (p.WorkingSet64 / 1024).ToString(); //(p.WorkingSet64 / 1024 / 1024).ToString() + "M (" + (p.WorkingSet64 / 1024).ToString() + "KB)";
            int time = p.UserProcessorTime.Seconds;
            //label2.Text = t.ToString();
            p.StandardInput.WriteLine("exit");  //  退出
            string str = p.StandardOutput.ReadToEnd();   //  cmd显示的字符串，放入str中
            p.Close();
            p.Dispose();

            // 测试显示····
            //MessageBox.Show(str);
            // 提交次数
            int submit = Convert.ToInt32(DBHelper.ExecuteTable("select count(*) from Solution where problem_id=" + problem_id).Rows[0][0].ToString());
            ++submit;
            // 解决次数
            int accepted = Convert.ToInt32(DBHelper.ExecuteTable("select count(*) from Solution where problem_id=" + problem_id + " and result=1").Rows[0][0].ToString());

            // 用户提交次数
            int userSubmit = Convert.ToInt32(DBHelper.ExecuteTable("select count(*) from Solution where user_id='" + user_id + "'").Rows[0][0].ToString());
            ++userSubmit;
            // 用户解决次数
            int useraAcepted = Convert.ToInt32(DBHelper.ExecuteTable("select distinct problem_id from Solution where result=1 and user_id='" + user_id + "'").Rows.Count.ToString());




            if (!File.Exists("test/" + user_id + "/" + problem_id + "/a.exe"))//编译未成功
            {
                // 添加solution信息
                DBHelper.ExeSql("update Solution set time=" + time + ",memory=" + memory
              + ",result='0',language='c',status='judged' where solution_id='" + solution_id + "'");
                // 更新problem信息
                DBHelper.ExeSql("update Problem set submit=" + submit + ",accepted=" + accepted + " where problem_id=" + problem_id);
                // 更新用户信息
                DBHelper.ExeSql("update Users set submit=" + userSubmit + ",solved=" + useraAcepted + " where user_id='" + user_id + "'");
            }
            else
            {
                // 如果out.txt与myout.txt相同，cmd显示无差异，不同，则显示5个*，详细自己在cmd测试fc命令
                if (str.Contains("*"))
                {  //失败
                    DBHelper.ExeSql("update Solution set time=" + time + ",memory=" + memory
                        + ",result='-1',language='c',status='judged' where solution_id='" + solution_id + "'");
                    // 更新problem信息
                    DBHelper.ExeSql("update Problem set submit=" + submit + ",solved=" + accepted + " where problem_id=" + problem_id);
                    // 更新用户信息
                    DBHelper.ExeSql("update Users set submit=" + userSubmit + ",solved=" + useraAcepted + " where user_id='" + user_id + "'");
                }
                else  //成功
                {
                    DBHelper.ExeSql("update Solution set time=" + time + ",memory=" + memory
                   + ",result='1',language='c',status='judged' where solution_id='" + solution_id + "'");
                    ++accepted;
                    // 更新problem信息
                    DBHelper.ExeSql("update Problem set submit=" + submit + ",solved=" + accepted + " where problem_id=" + problem_id);
                    ++useraAcepted;
                    // 更新用户信息
                    DBHelper.ExeSql("update Users set submit=" + userSubmit + ",solved=" + useraAcepted + " where user_id='" + user_id + "'");
                }
            }


        }
    }
}
