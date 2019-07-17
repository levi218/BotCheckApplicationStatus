using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CheckApplicationStatus
{
    public partial class Form1 : Form
    {
        private bool configured = false;
        private String registrationNumber = null;
        private String email = null;
        private int interval = 900000;
        public Form1()
        {
            InitializeComponent();
            TryLoadInfo();
        }
        void TryLoadInfo()
        {
            try
            {
                // Create an instance of StreamReader to read from a file.
                // The using statement also closes the StreamReader.
                using (StreamReader sr = new StreamReader("config.cfg"))
                {
                    registrationNumber = sr.ReadLine();
                    email = sr.ReadLine();
                    interval = Int32.Parse(sr.ReadLine());
                    sr.Close();
                }
                timer1.Interval = interval;
                configured = true;
                this.WindowState = FormWindowState.Minimized;
                timer1.Enabled = true;
                Proc();
            }
            catch (Exception e)
            {
                configured = false;
                // Let the user know what went wrong.
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
        }
        private void btnConfig_Click(object sender, EventArgs e)
        {
            registrationNumber = txtRegistrationNumber.Text;
            email = txtEmail.Text;
            interval = Int32.Parse(txtInterval.Text)*1000*60;
            if (email != null && registrationNumber != null && email.Length>0 && registrationNumber.Length>0)
            {
                using (StreamWriter sw = new StreamWriter("config.cfg"))
                {
                    sw.WriteLine(registrationNumber);
                    sw.WriteLine(email);
                    sw.WriteLine(interval);
                    sw.Flush();
                    sw.Close();
                }
                timer1.Interval = interval;
                configured = true;
                this.WindowState = FormWindowState.Minimized;
                timer1.Enabled = true;
                Proc();
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            Proc();
        }
        //WebBrowser browser;
        void Proc()
        {
            using (var client = new WebClient())
            {
                //try
                //   {
                var values = new NameValueCollection();
                values["registrationNumber"] = registrationNumber;
                values["email"] = email;

                var response = client.UploadValues("http://www.russia-edu.ru/", values);

                var responseString = Encoding.UTF8.GetString(response);
                //textBox1.Text = responseString;
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.Load(new StringReader(responseString));
                //textBox1.Text = doc.DocumentNode.InnerHtml;
                //HtmlNodeCollection col = doc.GetElementbyId("main").FirstChild.NextSibling.FirstChild.NextSibling.FirstChild.FirstChild.ChildNodes;
                HtmlNodeCollection col = doc.GetElementbyId("main").FirstChild.NextSibling.NextSibling.NextSibling.NextSibling.NextSibling.FirstChild.NextSibling.FirstChild.NextSibling.NextSibling.NextSibling.FirstChild.NextSibling.FirstChild.NextSibling.ChildNodes;
                foreach (HtmlNode item in col)
                {
                    var classs = from att in item.Attributes
                                 where att.Name == "class"
                                 select att.Value;
                    if (classs.ToArray<String>().Length > 0 && classs.ToArray<String>().First().Equals("current"))
                    {
                        String status = item.FirstChild.NextSibling.FirstChild.NextSibling.InnerText;
                        switch (status)
                        {
                            case "Ввод анкеты":
                                textBox1.Text = status + " - Input - 1";
                                break;
                            case "Анкета введена":
                                textBox1.Text = status + " - Introduced - 2";
                                break;
                            case "На рассмотрении вуза":
                                textBox1.Text = status + " - On approval - 3";
                                break;
                            case "Распределен":
                                textBox1.Text = status + " - Distributed - 4";
                                break;
                            case "Направлен":
                                textBox1.Text = status + " - Directed - 5";
                                break;
                            case "Зачислен":
                                textBox1.Text = status + " - Enrolled - 6";
                                break;
                            default:
                                textBox1.Text = status;
                                break;
                        }

                        notifyIcon1.ShowBalloonTip(5000, "", textBox1.Text, System.Windows.Forms.ToolTipIcon.Info);
                    }
                }
                //    }
                //    catch (Exception ex) { }
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
        }


    }
}
