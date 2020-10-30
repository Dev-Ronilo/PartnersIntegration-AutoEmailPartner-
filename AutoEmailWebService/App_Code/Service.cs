using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using MySql.Data.MySqlClient;
using System.Globalization;
using log4net;
using log4net.Config;
using System.Data;
using System.Data.SqlClient;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;


// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service" in code, svc and config file together.
public class Service : IService
{
    private string host, from, password, hmail, email, status;
    private bool ifexist;
    private string path2,currency,isEmail;
    private DBConnect dbcon;

    private static readonly ILog autolog = LogManager.GetLogger(typeof(Service));
     
    public Service () {
        log4net.Config.XmlConfigurator.Configure();
        path2 = "C:\\kpconfig\\AutoEmailConfig.ini";
        IniFile ini = new IniFile(path2);
        
        currency = ini.IniReadValue("SelectedCurrency", "currency");
    }

    private void Connect(String settings)
    {
        try 
        {
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            XmlConfigurator.Configure();

            IniFile ini = new IniFile(path2);

            String Serv = ini.IniReadValue(settings, "server");
            String DB = ini.IniReadValue(settings, "database"); ;
            String UID = ini.IniReadValue(settings, "uid"); ;
            String Password = ini.IniReadValue(settings, "password"); ;
            String pool = ini.IniReadValue(settings, "pool");
            Int32 maxcon = Convert.ToInt32(ini.IniReadValue(settings, "maxcon"));
            Int32 mincon = Convert.ToInt32(ini.IniReadValue(settings, "mincon"));
            Int32 tout = Convert.ToInt32(ini.IniReadValue(settings, "tout"));
            dbcon = new DBConnect(Serv, DB, UID, Password, pool, maxcon, mincon, tout);

            host = from = password = string.Empty;
            autolog.Info("Connected To: " + Serv);
        }
        catch (Exception ex) 
        {
            autolog.Fatal(ex.ToString());
        }

    }
    public Responces getEmailMessages(String message,String emailAdd)
    {
        IniFile ini = new IniFile(path2);
        try
        {
            List<String> admins = getEmailAdmin();
            host = ini.IniReadValue("Email Config", "host");
            hmail = ini.IniReadValue("Email Config", "hmail");
            from = ini.IniReadValue("Email Config", "from");
            password = ini.IniReadValue("Email Config", "password");

            SmtpClient client = new SmtpClient(host);
            MailMessage msg = new MailMessage();
            
            admins.Add(emailAdd);
            msg.To.Add(admins[0].ToString());
            for (int i = 1; i < admins.Count; i++) 
            {
                msg.CC.Add(admins[i].ToString());
            }
            msg.From = new MailAddress(hmail);
            msg.Subject = "Partner AutoEmailer";
            msg.Body = "<br/>" + message + "<br/>";
            msg.IsBodyHtml = true;

            client.Port = 587;
            client.UseDefaultCredentials = false;    
            client.Credentials = new NetworkCredential(from, password);  
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = false;
            
            
            client.Send(msg);
            return new Responces { rescode = "1", message = "Success" };

            autolog.Info(":  emailed to " + emailAdd);
        }
        catch (Exception x) 
        {
            return new Responces { rescode = "0", message = x.ToString()};
            autolog.Error(x.ToString());
        }
       
    }
    public Responces checkDetails(string accountID)
    {
        List<partnerRunningBal> partRunBal = new List<partnerRunningBal>();
        IniFile ini = new IniFile(path2);

        string Settings = ini.IniReadValue("PartnerSettings", "settings");
        string[] config = Settings.Split(',');
        char[] chr = {'[',']'};
        string splitAccountID = accountID.Trim(chr);
        string[] arrayAccountID = splitAccountID.Split(',');
        try 
        {
            foreach (var item in config)
            {
                Connect(item);
                using (MySqlConnection conn = dbcon.getConnection())
                {
                    conn.Open();
                    using (MySqlCommand cmd = conn.CreateCommand())
                    {
                        foreach (var account in arrayAccountID)
                        {
                            cmd.CommandText = "SELECT a.accountid, accountname,b.runningbalance, amountloaded FROM `kpadminpartners`.`accountlist` a " +
                                "INNER JOIN `kpadminpartners`.`accountdetail` b ON a.accountid=b.accountid WHERE b.accountid = " + account + " AND currency = '" + currency + "' AND a.isactive = 1";
                            MySqlDataReader dtrdr = cmd.ExecuteReader();
                            if (dtrdr.HasRows)
                            {
                                while (dtrdr.Read())
                                {
                                    partRunBal.Add(new partnerRunningBal
                                    {
                                        accountID = dtrdr["accountid"].ToString(),
                                        accountName = dtrdr["accountname"].ToString(),
                                        runningBalance = dtrdr["runningbalance"].ToString(),
                                        amountLoaded = dtrdr["amountloaded"].ToString()
                                    });
                                }
                            }
                            dtrdr.Close();
                        }
                        conn.Close();
                    }
                }
            }
            return new Responces { rescode = "1", message = "Success", partnerBalance = partRunBal };
            autolog.Info("Success: " + partRunBal);
        }
        catch (Exception ex) 
        {
            return new Responces { rescode = "0", message = ex.ToString(), partnerBalance = null };
            autolog.Fatal(ex.ToString());
        }
    }
    public PartnersInfo getInfoPartners() 
    {
        Connect("DBConfig DomesticPartner");
        List<partnerThreshold> partnersInfo = new List<partnerThreshold>();
        try 
        {
            using (MySqlConnection conn = dbcon.getConnection())
            {
                conn.Open();
                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.Parameters.Clear();
                    cmd.CommandText = "SELECT accountid,accountname,prefund,threshold,email,emailcontent FROM kpadminpartners.partnersthreshold_notif WHERE currency = '" + currency + "'";
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        partnersInfo.Add(new partnerThreshold
                        {
                            accountID = rdr["accountid"].ToString(),
                            accountName = rdr["accountname"].ToString(),
                            prefund = rdr["prefund"].ToString(),
                            threshold = rdr["threshold"].ToString(),
                            email = rdr["email"].ToString(),
                            emailcontent = rdr["emailcontent"].ToString()

                        });
                    }
                    rdr.Close();
                }
                conn.Close();
            }
            return new PartnersInfo { rescode = 1, message = "Sucess", partnersDetails = partnersInfo };
            autolog.Info("Sucess: " + partnersInfo);
        }
        catch (Exception ex) 
        {
            return new PartnersInfo { rescode = 0, message = ex.ToString(), partnersDetails = partnersInfo };
            autolog.Error(": " + ex.ToString());
        }
    }
    public Responces isEmailed(String AccoutIDtoSend) 
    {
        Connect("DBConfig DomesticPartner");
        try {
             using (MySqlConnection conn = dbcon.getConnection())
                {
                    conn.Open();
                    using (MySqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = "SELECT accountid,isEmailed FROM `kpadminpartners`.`autoEmailPartners` WHERE currency = '" + currency + "' AND accountid = '" + AccoutIDtoSend + "'"; 
                        MySqlDataReader rdr = cmd.ExecuteReader();
                        if (rdr.HasRows) 
                        {
                            while (rdr.Read()) 
                            {
                                isEmail = rdr["isEmailed"].ToString();
                            }
                            rdr.Close();
                        }
                    }
                    conn.Close();
                    autolog.Info((isEmail == "0")?"not yet emailed - " + AccoutIDtoSend:"Already emailed - "+ AccoutIDtoSend);
                }
            }
        catch(Exception ex)
        {
            return new Responces { rescode = null, message = ex.ToString() };
            autolog.Error("Error: " + ex.ToString());
        }
        return new Responces { rescode = isEmail, message = (isEmail == "0") ? "not yet emailed - " + AccoutIDtoSend : "Already emailed - " + AccoutIDtoSend };
    }
    public List<String> getEmailAdmin() 
    {
        List<String> adminEmail = new List<string>();
        Connect("DBConfig DomesticPartner");
        
        using (MySqlConnection conn = dbcon.getConnection())
        {
            conn.Open();
            using (MySqlCommand cmd = conn.CreateCommand())
            {
                cmd.Parameters.Clear();
                cmd.CommandText = "SELECT emailAdd FROM `kpadminpartners`.`autoEmailPartners` WHERE groupDept = 6";
                MySqlDataReader rdr = cmd.ExecuteReader();
                if (rdr.HasRows)
                {
                    while (rdr.Read()) 
                    {
                        adminEmail.Add(rdr["emailAdd"].ToString());
                    }
                    rdr.Close();
                }
            }
            conn.Close();
        }
        return adminEmail;
    }
    public bool IfExistinTable(string accountID, string emailAdd)
    { 
        Connect("DBConfig DomesticPartner");

        using (MySqlConnection conn = dbcon.getConnection())
        {
            conn.Open();
            using (MySqlCommand cmd = conn.CreateCommand())
            {
                cmd.Parameters.Clear();
                cmd.CommandText = "SELECT emailAdd FROM `kpadminpartners`.`autoEmailPartners` WHERE accountid = '" + accountID + "'";
                MySqlDataReader rdr = cmd.ExecuteReader();
                 if(!rdr.HasRows)
                 {
                     ifexist = false;
                 }
                 else
                 {
                     while (rdr.Read())
                     {
                         email = rdr["emailAdd"].ToString();
                         
                     }
                     if (email != emailAdd)
                     {
                         cmd.Parameters.Clear();
                         cmd.CommandText = "UPDATE `kpadminpartners`.`autoEmailPartners` SET emailAdd = '" + emailAdd + "' WHERE accountid = '" + accountID + "'";
                         rdr.Close();
                         cmd.ExecuteNonQuery();
                         autolog.Info("Successfull updated email for: " + accountID);
                     }
                     ifexist = true;
                 }
                
            }
            conn.Close();
        }
        return ifexist;
        autolog.Info((ifexist == true) ? accountID + " - Existing in Database" : accountID + " is not Existing in Database");
    }
    public Responces InsertToTable(string accountID, string accountName, string emailAdd) 
    {
        Connect("DBConfig DomesticPartner");
        int emailedStatus = 0;
        try
        {
            using (MySqlConnection conn = dbcon.getConnection())
            {
                conn.Open();
                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.Parameters.Clear();
                    cmd.CommandText = "SELECT emailAdd FROM `kpadminpartners`.`autoEmailPartners` WHERE accountid = '" + accountID + "'";
                    MySqlDataReader rdr = cmd.ExecuteReader();

                    if (!rdr.HasRows)
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = "INSERT INTO `kpadminpartners`.`autoEmailPartners`(emailAdd,accountName,IsEmailed,DateEmailed,groupDept,accountID,currency) VALUES(@emailAdds,@accountNames,@isemailed,@DateEmailed,@groupDept,@accountid,@currencies)";
                        cmd.Parameters.Add("emailAdds", emailAdd);
                        cmd.Parameters.Add("accountNames", accountName);
                        cmd.Parameters.Add("isemailed", emailedStatus);
                        cmd.Parameters.Add("DateEmailed", DateTime.Now);
                        cmd.Parameters.Add("groupDept", 1);
                        cmd.Parameters.Add("accountid", accountID);
                        cmd.Parameters.Add("currencies", currency);
                        rdr.Close();
                        cmd.ExecuteNonQuery();
                        ifexist = true;
                    }
                    status = "Success in Inserting data for: ";
                    autolog.Info(status + " " + accountID);
                }
                conn.Close();
            }
           
        }
        catch (Exception ex) 
        {
            status = "Error: " +ex.ToString();
            autolog.Error(status);
        }
        return new Responces { rescode = (ifexist == true) ? "1" : "0" ,message = status};
    }
    public void upDateTable(String accountID) 
    {
        Connect("DBConfig DomesticPartner");
        try
        {
            using (MySqlConnection conn = dbcon.getConnection())
            {
                conn.Open();
                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.Parameters.Clear();
                    cmd.CommandText = "SELECT emailAdd FROM `kpadminpartners`.`autoEmailPartners` WHERE accountid = '" + accountID + "'";
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = "UPDATE `kpadminpartners`.`autoEmailPartners` SET isEmailed = " + 1 + " WHERE accountid = '" + accountID + "'";
                        rdr.Close();
                        cmd.ExecuteNonQuery();
                    }
                }
                conn.Close();
            }
            autolog.Info("SuccesFully Updated - " + accountID);
        }
        catch (Exception ex) 
        {
            autolog.Error("Error - " + ex.ToString());
        }
      
    }
    public void resetTable()
    {
        Connect("DBConfig DomesticPartner");
        try 
        {
            using (MySqlConnection conn = dbcon.getConnection())
            {
                conn.Open();
                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.Parameters.Clear();
                    cmd.CommandText = "SELECT emailAdd FROM `kpadminpartners`.`autoEmailPartners` WHERE isEmailed = 1 ";
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = "UPDATE `kpadminpartners`.`autoEmailPartners` SET isEmailed = " + 0 + " WHERE isEmailed = 1";
                        rdr.Close();
                        cmd.ExecuteNonQuery();
                    }
                }
                conn.Close();
            }
            autolog.Info("Success in Resetting");
        }
        catch (Exception ex) 
        {
            autolog.Error("Failed to Reset: " + ex.ToString());
        }
   
    }

}
