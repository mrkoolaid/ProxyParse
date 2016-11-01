/// ProxyParse by mrkoolaid
/// Monday October 31, 2016 @ 6:45PM
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProxyParse
{
    public partial class frmMain : Form
    {
        public Thread proxyThread;
        public delegate void SetProxyItem(ListViewItem l);

        public frmMain()
        {
            InitializeComponent();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //save current proxies in a tmp file?
            Environment.Exit(0);
        }

        /// <summary>
        /// 
        /// </summary>
        public void proxyWork()
        {
            //List<ProxyInfo> proxies = new List<ProxyInfo>() { };
            string src = string.Empty;
            Regex regx;
            List<string> sites = new List<string>() { };
            sites.Add("https://incloak.com/proxy-list/");
            sites.Add("http://www.sslproxies.org/");

            foreach (string url in sites)
            {
                string[] split = url.Split('/');
                foreach (string part in split)
                {
                    status.Text = "Looking for proxies..";
                    //.com, .net, .org, .co.uk, .jp
                    if (Regex.IsMatch(part, "(.co|.net|.org|.jp)"))
                    {
                        switch (part)
                        {
                            default: break;

                            case "incloak.com":
                                status.Text = "Analyzing " + part;
                                src = getSrc(url);

                                //get addresses
                                regx = new Regex(@"(\d{1,3}[.]\d{1,3}[.]\d{1,3}[.]\d{1,3})");
                                MatchCollection addresses = regx.Matches(src);

                                regx = new Regex("(\\d{1,5})<\\/td><td><div>");
                                MatchCollection ports = regx.Matches(src);

                                for (int i = 0; i < ports.Count; i++)
                                {
                                    int p1 = i + 1;
                                    ListViewItem lvi = new ListViewItem();
                                    lvi.Text = addresses[p1].Captures[0].Value;
                                    lvi.SubItems.Add(ports[i].Groups[1].Captures[0].Value);
                                    lvi.SubItems.Add(DateTime.Now.ToLongTimeString());
                                    lvi.SubItems.Add(part);
                                    setProxyItem(lvi);

                                    status.Text = "Loading proxies from " + part;
                                }
                                break;

                            case "www.sslproxies.org":
                                status.Text = "Analyzing " + part;
                                src = getSrc(url);

                                regx = new Regex(@"<td>(\d{1,3}[.]\d{1,3}[.]\d{1,3}[.]\d{1,3})<\/td><td>(\d{1,5})<\/td>");
                                MatchCollection sslProxies = regx.Matches(src);

                                for (int i = 0; i < sslProxies.Count; i++)
                                {
                                    Match m = sslProxies[i];
                                    ListViewItem lvi = new ListViewItem();
                                    lvi.Text = m.Groups[1].Captures[0].Value;
                                    lvi.SubItems.Add(m.Groups[2].Captures[0].Value);
                                    lvi.SubItems.Add(DateTime.Now.ToLongTimeString());
                                    lvi.SubItems.Add(part);
                                    setProxyItem(lvi);

                                    status.Text = "Loading proxies from " + part;
                                }

                                break;
                        }
                    }
                }
            }

            status.Text = "Finished loading proxies @ " + DateTime.Now.ToShortTimeString();
        }

        public void startImport()
        {
            proxyThread = new Thread(new ThreadStart(proxyWork));
            proxyThread.Start();
        }

        public void setProxyItem(ListViewItem lvi)
        {
            if (this.lvProxies.InvokeRequired)
            {
                SetProxyItem spi = new SetProxyItem(setProxyItem);
                this.Invoke(spi, new object[] { lvi });
            }
            else
            {
                this.lvProxies.Items.Add(lvi);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string getSrc(string url)
        {
            Random r = new Random(DateTime.Now.Millisecond);

            string[] userAgents = {
                "runscope/0.1",
                "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)",
                "Mozilla/5.0 (compatible; Bingbot/2.0; +http://www.bing.com/bingbot.htm)",
                "Mozilla/5.0 (compatible; Yahoo! Slurp; http://help.yahoo.com/help/us/ysearch/slurp)",
                "DuckDuckBot/1.0; (+http://duckduckgo.com/duckduckbot.html)",
                "Mozilla/5.0 (compatible; Baiduspider/2.0; +http://www.baidu.com/search/spider.html)",
                "Mozilla/5.0 (compatible; YandexBot/3.0; +http://yandex.com/bots)",
                "Sogou Pic Spider/3.0( http://www.sogou.com/docs/help/webmasters.htm#07)",
                "Sogou head spider/3.0( http://www.sogou.com/docs/help/webmasters.htm#07)",
                "Sogou web spider/4.0(+http://www.sogou.com/docs/help/webmasters.htm#07)",
                "Sogou Orion spider / 3.0(http://www.sogou.com/docs/help/webmasters.htm#07)",
                "Sogou - Test - Spider / 4.0(compatible; MSIE 5.5; Windows 98)",
                "Mozilla/5.0 (compatible; Konqueror/3.5; Linux) KHTML/3.5.5 (like Gecko) (Exabot-Thumbnails)",
                "Mozilla/5.0 (compatible; Exabot / 3.0; +http://www.exabot.com/go/robot)",
                "facebookexternalhit/1.0 (+http://www.facebook.com/externalhit_uatext.php)",
                "facebookexternalhit/1.1 (+http://www.facebook.com/externalhit_uatext.php)",
                "ia_archiver (+http://www.alexa.com/site/help/webmasters; crawler@alexa.com)"
            };

            string randUserAgent = userAgents[r.Next(0,userAgents.Length)];

            string src = string.Empty;
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.UserAgent = randUserAgent;
                req.Accept = "*/*";

                WebResponse res = req.GetResponse();
                StreamReader rdr = new StreamReader(res.GetResponseStream());

                while (!rdr.EndOfStream)
                {
                    src += rdr.ReadLine();
                }

                rdr.Close();
                res.Close();
            }
            catch (WebException wex)
            {
                status.Text = wex.Message;
            }
            catch (Exception ex)
            {
                status.Text = ex.Message;
            }

            return src;
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            startImport();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmAbout frm = new frmAbout();
            frm.Show();
        }

        private void toolStripLabel1_Click(object sender, EventArgs e)
        {
            panelOptions.SendToBack();
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panelOptions.BringToFront();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            //
        }
    }
}
