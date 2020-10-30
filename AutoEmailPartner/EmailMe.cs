using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using CallRestAPI;
using AutoEmailPartner.Responces;
using Newtonsoft.Json;
using System.Reflection;
using Newtonsoft.Json.Linq;
using ThreadSafeCall;

namespace AutoEmailPartner
{
    public partial class EmailMe : Form
    {
        //private static readonly log4net.ILog log = log4net.LogManager.GetLogger("EmailMe");
        public String path = "C:\\kpconfig\\AutoEmailAgent.ini";
        IniFile ini;
        private string firstHour, secondHour, thirdHour, resetHour,hours;
        bool state;
        public EmailMe()
        {
            InitializeComponent();
            ini = new IniFile(path);
            hours = ini.IniReadValue("Time Checking", "time");
            string[] splitHours = hours.Split(',');
            firstHour = splitHours[0].ToString();
            secondHour = splitHours[1].ToString();
            thirdHour = splitHours[2].ToString();
            resetHour = splitHours[3].ToString();
            
        }
        public Uri getBaseAddress()
        {
            string url = ini.IniReadValue("BaseAddress", "baseAddress");
            Uri uri = new Uri(url);
            return uri;
        }
        private void EmailBtn_Click(object sender, EventArgs e)
        {
            if (EmailBTN.Text == "Start")
            {
               
                        timer1.Start();
                        EmailBTN.SafeInvoke(d => d.Text = "Stop");
                        state = true;
                        Task.Factory.StartNew(() =>
                        {
                            doWhileAuto();
                        });
            }
            else 
            {
                state = false;
                timer1.Stop();
                EmailBTN.SafeInvoke(d => d.Text = "Start");
            }
        }
        public String sendRequest(Uri uri)
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            string jsonString = null;

            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "GET";
            request.ContentType = "application/json";
            request.Timeout = Timeout.Infinite;

            var webresponse = (HttpWebResponse)request.GetResponse();
            Stream response = webresponse.GetResponseStream();
            using (StreamReader sr = new StreamReader(response))
            {
                jsonString = sr.ReadToEnd();
                sr.Close();
            } 

            return jsonString;
        }
        public String SendEmail(string messageFormat,string emailAdd) 
        {
            Uri strUrl = new Uri(getBaseAddress().ToString() + ("/getEmailMessages/?emailText=" + messageFormat + "&emailaddress=" + emailAdd));
           string resJson = sendRequest(strUrl);

           return resJson;
        }
        public String GetMessageFormat(string messageBody, string partnername,string prefund, string threshold, string RunningBalance) 
        {
            String FormattedMsg;
            FormattedMsg = messageBody.Replace("(Partners Name)", partnername).Replace("(Prefund)", prefund).Replace("(Threshold)", threshold + "%").Replace("(Running Balance)",RunningBalance);
            return FormattedMsg;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            
            label1.SafeInvoke(d => d.Text = DateTime.Now.ToString("T"));
            
        }

        private void EmailMe_Load(object sender, EventArgs e)
        {
            EmailBTN.PerformClick();
        }
        public void AutoEmail() 
        {

            Indication.SafeInvoke(d => d.Text = "Sending Email");
            List<String> partTOemail = new List<String>();
            String json, respJson, emailedJson = null;
            double runningBalance, percentage, threshold, minFund = 0.000;
            Uri strUrl = new Uri(getBaseAddress().ToString() + ("/getInfoPartners"));
            json = sendRequest(strUrl);
            PartnersInfo partnersInfo = JsonConvert.DeserializeObject<PartnersInfo>(json);

            //Getting All accountID 
            foreach (var item in partnersInfo.partnersDetails)
            {
                partTOemail.Add(item.accountID);
            }

            //Checking account ID in every Partner's database
            String accountIDs = JsonConvert.SerializeObject(partTOemail);
            Uri newUrl = new Uri(getBaseAddress().ToString() + "/checkDetails/?accountID=" + accountIDs);
            respJson = sendRequest(newUrl);
            respartBal resPart = JsonConvert.DeserializeObject<respartBal>(respJson);

            //Calculating the threshold for each Partners
            foreach (var select in partnersInfo.partnersDetails)
            {
                foreach (var compare in resPart.partnerBalance)
                {
                    if (select.accountID == compare.accountID)
                    {
                        //Check running balance
                        runningBalance = double.Parse(compare.runningBalance);

                        threshold = double.Parse(select.threshold);
                        percentage = threshold / 100;

                        //Getting the minimum required amount
                        //Minimum Required Amount for Prefund = Amount Loaded x percentage of threshold 
                        minFund = double.Parse(compare.amountLoaded) * percentage;
                        if (runningBalance <= minFund)
                        {
                            //send email to partner
                            Uri isExitAccountId = new Uri(getBaseAddress().ToString() + ("/IfExistinTable/?AccountID=" + select.accountID + "&emailaddress=" + select.email));
                            String strBool = sendRequest(isExitAccountId);
                            if (strBool == "false")
                            {
                                Uri insertTotable = new Uri(getBaseAddress().ToString() + ("/InsertToTable/?AccountID=" + select.accountID + "&accountname=" + select.accountName + "&emailaddress=" + select.email));
                                String isEXIST = sendRequest(insertTotable);
                                responce resp = JsonConvert.DeserializeObject<responce>(isEXIST);
                                log((resp.rescode == "1") ? resp.message + select.accountID : resp.message);
                            }
                            Uri isEmailed = new Uri(getBaseAddress().ToString() + ("/isEmailed/?accountToSend=" + select.accountID));
                            emailedJson = sendRequest(isEmailed);
                            Emailed emailx = JsonConvert.DeserializeObject<Emailed>(emailedJson);
                            if (emailx.rescode == "0")
                            {
                                SendEmail(GetMessageFormat(select.emailcontent, select.accountName, compare.amountLoaded, select.threshold, compare.runningBalance), select.email);
                                Uri updateIsEmailed = new Uri(getBaseAddress().ToString() + ("/upDateTable/?accountID=" + select.accountID));
                                string strUpdate = sendRequest(updateIsEmailed);
                            }
                            log((emailx.rescode == "") ? emailx.message : emailx.message);
                        }
                    }
                }
            }
            log("Success in Checking and Sending of email - " + DateTime.Now.ToString());
        }
        public void doWhileAuto() 
        {
            do
            {
                try
                {
                    
                        if (label1.Text == firstHour || label1.Text == secondHour || label1.Text == thirdHour)
                        {
                            log("Starting to Send Email - " + DateTime.Now.ToString());
                            AutoEmail();
                            Indication.SafeInvoke(d => d.Text = "Done Sending Email");

                        }
                        else if (label1.Text == resetHour)
                        {
                            log("Resetting - " + DateTime.Now.ToString());
                            Uri newUrl = new Uri(getBaseAddress().ToString() + "/resetTable");
                            Indication.SafeInvoke(d => d.Text = "Resetting");
                            string reset = sendRequest(newUrl);
                            Indication.SafeInvoke(d => d.Text = "Done Reset");
                            Thread.Sleep(5000);
                            Indication.SafeInvoke(d => d.Text = "");
                        }
                }
                catch (Exception ex)
                {
                    Indication.SafeInvoke(d => d.Text = "Error Found");
                    log(ex.ToString());
                }
            }
            while (state == true);
        }
        private void log(String message) 
        {
            string path = @"C:\AutoEmailAppAppLogs\";
            System.IO.Directory.CreateDirectory(path);
            StreamWriter logWriter;

            if (!File.Exists(path + "AutoEmail.txt"))
            {
                File.Create(path + "AutoEmail.txt").Dispose();
            }
            logWriter = new StreamWriter(path + "AutoEmail.txt", true, Encoding.ASCII);
            //logWriter.WriteLine(DateTime.Now.ToString());
            logWriter.Write(message);
            logWriter.WriteLine();
            logWriter.WriteLine();
            logWriter.Close();
        }
    }
}
