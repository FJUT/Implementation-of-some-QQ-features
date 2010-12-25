/********************************************************************
 *V1.0 2010-12-5   问题:获取好友列表只能获取120个好友
 *每次向服务器发送命名时,如果返回了&RES=20,说明没有正确登录
 *然后更改is_RightLogin的值为false,因此每次引用QQ类函数返回值的时候都要先判断
 *is_RightLogin的值是否为true,否则就得考虑是不是要重新登录下QQ了
 ********************************************************************/
using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Contact
{
    class QQ
    {
        public string num;  //构造函数的QQ号码
        private string pwd; //构造函数的QQ密码

        public string[] online_Face={""};   //在线的头像号码
        public string[] online_Station={""};    //在线的状态
        public string[] online_Number={""}; //在线的号码
        public string[] online_NameK={""};  //在线的昵称

        private WebClient _client = new WebClient();    //用来给服务器发送消息的

        private string postValues;  //发送给服务器的字符串
        private byte[] byteArray;   //把要发送的字符串变成字符数组
        private byte[] pageData;    //接受服务器返回的字符数组
        private string s;   //把返回的字符数组变成字符串

        public string[] MT;    //储存信息类型
        public string[] UN;    //储存信息来源号码
        public string[] MG;    //储存信息内容

        public bool is_RightLogin;  //判断当前用户是否正确登录
        /// <summary>
        /// QQ类的构造函数
        /// </summary>
        /// <param name="QQ_Num">QQ号码</param>
        /// <param name="QQ_Pwd">QQ密码</param>
        public QQ (string QQ_Num, string QQ_Pwd)
        {
            this.num = QQ_Num;
            this.pwd = QQ_Pwd;
        }

        /// <summary>
        /// 登陆QQ
        /// </summary>
        /// <returns>登陆成功就返回True</returns>
        public bool QQ_Login()
        {

            postValues = "VER=1.1&CMD=Login&SEQ=" + DateTime.Now.Ticks.ToString().Substring(7, 7)
                + "&UIN=" + num + "&PS=" + MD5(pwd) + " &M5=1&LC=9326B87B234E7235";
            byteArray = System.Text.Encoding.UTF8.GetBytes(postValues);
            //向服务器POST数据
            UploadData();
            if (Encoding.UTF8.GetString(pageData).Contains("RES=0&RS=0"))
            {
                is_RightLogin = true;
                return true;
            }
            else
                return false;
        }
        public static string MD5(string toCryString)
        {
            MD5CryptoServiceProvider hashmd5;   //using System.Security.Cryptography安全.密码系统
            hashmd5 = new MD5CryptoServiceProvider();
            return BitConverter.ToString(hashmd5.ComputeHash(Encoding.UTF8.GetBytes(toCryString))).Replace("-", "").ToLower();
        }

        /// <summary>
        /// 获取QQ好友列表
        /// </summary>
        /// <returns>返回一个字符串数组,数组最后一个元素是空格</returns>
        public string[] QQ_List()
        {
            postValues = "VER=1.1&CMD=List&SEQ=" + DateTime.Now.Ticks.ToString().Substring(7, 7) + "&UIN=" + num + "&TN=160&UN=0";
            byteArray = System.Text.Encoding.UTF8.GetBytes(postValues);
            //向服务器POST数据
            UploadData();
            s = Encoding.UTF8.GetString(pageData);
            if (!s.Contains("&RES=0"))
                is_RightLogin = false;
            string s2 = s.Remove(0, s.IndexOf("&UN=")+4);
            string[] QQ_Friend_List = s.Split(',');
            return QQ_Friend_List;
        }

        /// <summary>
        /// 更新QQ类中目前在线online_四个字符串数组的值
        /// </summary>
        public void QQ_Query_Stat()
        {
            s = Encoding.UTF8.GetString(pageData);
            if (!s.Contains("&RES=0"))
                is_RightLogin = false;
            StringBuilder sb = new StringBuilder(s);
            sb.Remove(s.IndexOf("&FN="), s.Length - s.IndexOf("&FN="));
            sb.Remove(0,s.IndexOf("&FC=")+4);
            online_Face = sb.ToString().Split(',');

            sb = new StringBuilder(s);
            sb.Remove(s.IndexOf("&UN="), s.Length - s.IndexOf("&UN="));
            sb.Remove(0, s.IndexOf("&ST=") + 4);
            online_Station = sb.ToString().Split(',');

            sb = new StringBuilder(s);
            sb.Remove(s.IndexOf("&NK="), s.Length - s.IndexOf("&NK="));
            sb.Remove(0, s.IndexOf("&UN=") + 4);
            online_Number = sb.ToString().Split(',');

            string ss = s.Remove(0, s.IndexOf("&NK=") + 4);
            online_NameK = ss.Split(',');
        }

        /// <summary>
        /// 输入一个QQ号,查询这个QQ号的信息
        /// </summary>
        /// <param name="search_num">输入一个QQ号,查询该QQ信息</param>
        /// <returns>字符串数组(联系地址,用户年龄,用户邮箱,头像,个人网站,职业,邮箱,联系电话,简介,省份,真实姓名,毕业院校,性别,QQ号,昵称)</returns>
        public string[] QQ_GetInfo(string search_num)
        {
            postValues = "VER=1.1&CMD=GetInfo&SEQ=" + DateTime.Now.Ticks.ToString().Substring(7, 7) + "&UIN=" + num + "&LV=2&UN=" + search_num;
            byteArray = System.Text.Encoding.UTF8.GetBytes(postValues);
            //向服务器POST数据
            UploadData();
            s = Encoding.UTF8.GetString(pageData);
            if (!s.Contains("&RES=0"))
                is_RightLogin = false;
            MatchCollection matches = Regex.Matches(s, "&([^=][^=])=([^&]*)");
            //AD用户的联系地址,AG为用户年龄,EM为用户MAIL
            //FC为用户头像,HP为用户网站,JB为用户职业,PC为用户邮编,PH为用户联系电话
            //PR为用户简介,PV为用户所以的省,RN为用户真实名称,SC为用户毕业院校,SX为用户性别,UN为用户QQ号,NK为用户QQ昵称
            List<string> Info = new List<string>();
            for (int i = 0; i < matches.Count; i++)
                Info.Add(matches[i].Groups[2].ToString());
            Info.RemoveAt(6);   //去除LV=多少, 这表示查询方式,默然就是普通查询
            if (Info[12].ToString() == "0")
                Info[12] = "男";
            else
                Info[12] = "女";
            string[] Inf = Info.ToArray();
            return Inf;
        }

        /// <summary>
        /// 添加好友功能
        /// </summary>
        /// <param name="fir_num">输入一个QQ号,请求加为好友</param>
        /// <returns>0表示已经加为好友,1表示需要验证请求,2表示拒绝</returns>
        public string AddToList(string fir_num)
        {
            postValues = "VER=1.1&CMD=AddToList&SEQ=" + DateTime.Now.Ticks.ToString().Substring(7, 7) + "&UIN=" + num + "&UN=" + fir_num;
            byteArray = System.Text.Encoding.UTF8.GetBytes(postValues);
            //向服务器POST数据
            UploadData();
            s = Encoding.UTF8.GetString(pageData);
            if (!s.Contains("&RES=0"))
                is_RightLogin = false;
            MatchCollection matchs = Regex.Matches(s,"&CD=(.)");
            return matchs[0].Groups[1].ToString();
        }

        /// <summary>
        /// 回应加为好友的响应
        /// </summary>
        /// <param name="fri_Num">请求的QQ号码</param>
        /// <param name="agree_Type">0表示通过验证,1表示拒绝对方,2表示请求加对方为好友</param>
        public void Ack_AddToList(string fri_Num,string agree_Type)
        {
            //WebClient _client = new WebClient();
            postValues = "VER=1.1&CMD=Ack_AddToList&SEQ=" + DateTime.Now.Ticks.ToString().Substring(7, 7) + "&UIN=" + num + "&UN=" + fri_Num + "&CD="+agree_Type+"&RS=";
            byteArray = System.Text.Encoding.UTF8.GetBytes(postValues);
            //向服务器POST数据
            UploadData();
            s=Encoding.UTF8.GetString(pageData);
            if (!s.Contains("&RES=0"))
                is_RightLogin = false;
        }

        /// <summary>
        /// 删除好友,成功返回True
        /// </summary>
        /// <param name="del_num">输入一个QQ号,删除这个QQ好友</param>
        /// <returns></returns>
        public bool DelFromList(string del_num)
        {
            postValues = "VER=1.1&CMD=DelFromList&SEQ=" + DateTime.Now.Ticks.ToString().Substring(7, 7) + "&UIN=" + num + "&UN=" + del_num;
            byteArray = System.Text.Encoding.UTF8.GetBytes(postValues);
            //向服务器POST数据
            UploadData();
            s = Encoding.UTF8.GetString(pageData);
            if (s.Contains("&RES=0"))
                return true;
            else
                return false;
        }

        /// <summary>
        /// 改变QQ当前状态(在线,离线,忙碌)
        /// </summary>
        /// <param name="Stat">输入10在线,20离线,30忙碌</param>
        /// <returns></returns>
        public bool Change_Stat(string stat)
        {
            postValues = "VER=1.1&CMD=Change_Stat&SEQ=" + DateTime.Now.Ticks.ToString().Substring(7, 7) + "&UIN=" + num + "&ST=" + stat;
            byteArray = System.Text.Encoding.UTF8.GetBytes(postValues);
            //向服务器POST数据
            UploadData();
            s = Encoding.UTF8.GetString(pageData);
            if(s.Contains("&RES=0"))
                return true;
            else
                return false;
        }

        /// <summary>
        /// 向一个QQ号码发送消息
        /// </summary>
        /// <param name="msgTo">输入一个QQ号,向他发送消息</param>
        /// <param name="msg">输入消息内容</param>
        /// <returns>成功返回True</returns>
        public bool QQ_SendMsg(string msgTo, string msg)
        {
            postValues = "VER=1.2&CMD=CLTMSG&SEQ=" + DateTime.Now.Ticks.ToString().Substring(7, 7) + "&UIN=" + num + "&UN=" + msgTo + "&MG=" + msg;
            byteArray = System.Text.Encoding.UTF8.GetBytes(postValues);
            //向服务器POST数据
            UploadData();
            s = Encoding.UTF8.GetString(pageData);
            if (s.Contains("&RES=20"))
            {
                is_RightLogin = false;
                return false;
            }
            if (s.Contains("&RES=0"))
                return true;
            else
                return false;
        }

        //待处理
        public void GetMsgEx()
        {
            postValues = "VER=1.1&CMD=GetMsgEx&SEQ=" + DateTime.Now.Ticks.ToString().Substring(7, 7) + "&UIN=" + num;
            byteArray = System.Text.Encoding.UTF8.GetBytes(postValues);
            //向服务器POST数据
            UploadData();
            s = Encoding.UTF8.GetString(pageData);
            if (s.Contains("\r"))
               s =  s.Replace("\r", "\n");
            if (s.Contains("&RES=0"))
            {
                is_RightLogin = true;
                MatchCollection matches = Regex.Matches(s, "&MN=([^&]*)");
                if (matches[0].Groups[1].ToString() != "0") //判断返回的信息数量是否为0条
                {
                    matches = Regex.Matches(s, "&MT=([^&]*)&UN=([^&]*)&MG=([^&]*)");
                    MT = matches[0].Groups[1].ToString().Split(',');   //信息类型
                    UN = matches[0].Groups[2].ToString().Split(',');   //信息来源号码
                    s = s.Remove(0, s.IndexOf("&MG=") + 4);
                    MG = s.Split(',');   //信息内容
                    //将消息内容进行转码
                    for(int i = 0; i<MG.Length-1;i++)
                    {
                        MG[i] = MG[i].Replace("%25", "%");
                        MG[i] = MG[i].Replace("%26", "&");
                        MG[i] = MG[i].Replace("%2c", ",");
                    }
                }
                else
                {
                    MT = null;
                    UN = null;
                    MG = null;
                    is_RightLogin = false;
                }
            }
        }

        /// <summary>
        /// QQ退出登陆,并改变is_RightLogin为False
        /// </summary>
        public void QQ_Logout()
        {
            postValues = "VER=1.1&CMD=Logout&SEQ=" + DateTime.Now.Ticks.ToString().Substring(7, 7) + "&UIN=" + num;
            byteArray = System.Text.Encoding.UTF8.GetBytes(postValues);
            //向服务器POST数据
            UploadData();
            s = Encoding.UTF8.GetString(pageData);
            if (s.Contains("&RES=0"))
                is_RightLogin = false;
        }

        private void UploadData()
        {
            try
            {
                pageData = _client.UploadData("http://tqq.tencent.com:8000", "POST", byteArray);
            }
            catch {}
        }
    }
}
