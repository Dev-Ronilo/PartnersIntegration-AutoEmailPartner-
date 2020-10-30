using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService" in both code and config file together.
[ServiceContract]
public interface IService
{

    [OperationContract]
    [WebInvoke(Method = "GET",
        ResponseFormat = WebMessageFormat.Json,
        RequestFormat = WebMessageFormat.Json,
        BodyStyle = WebMessageBodyStyle.WrappedRequest,
        UriTemplate = "/getInfoPartners")]

    PartnersInfo getInfoPartners();

    [OperationContract]
    [WebInvoke(Method = "GET",
        ResponseFormat = WebMessageFormat.Json,
        RequestFormat = WebMessageFormat.Json,
        BodyStyle = WebMessageBodyStyle.WrappedRequest,
        UriTemplate = "/isEmailed/?accountToSend={AccoutIDtoSend}")]

    Responces isEmailed(String AccoutIDtoSend);

    [OperationContract]
    [WebInvoke(Method = "GET",
        ResponseFormat = WebMessageFormat.Json,
        RequestFormat = WebMessageFormat.Json,
        BodyStyle = WebMessageBodyStyle.WrappedRequest,
        UriTemplate = "/getEmailMessages/?emailText={message}&emailaddress={emailAdd}")]

    Responces getEmailMessages(String message, String emailAdd);

    [OperationContract]
    [WebInvoke(Method = "GET",
        ResponseFormat = WebMessageFormat.Json,
        RequestFormat = WebMessageFormat.Json,
        BodyStyle = WebMessageBodyStyle.WrappedRequest,
        UriTemplate = "/IfExistinTable/?AccountID={accountid}&emailaddress={emailAdd}")]

    bool IfExistinTable(string accountID,string emailAdd);

    [OperationContract]
    [WebInvoke(Method = "GET",
        ResponseFormat = WebMessageFormat.Json,
        RequestFormat = WebMessageFormat.Json,
        BodyStyle = WebMessageBodyStyle.WrappedRequest,
        UriTemplate = "/InsertToTable/?AccountID={accountid}&accountname={accountName}&emailaddress={emailAdd}")]

    Responces InsertToTable(string accountID, string accountName, string emailAdd);

    [OperationContract]
    [WebInvoke(Method = "GET",
        ResponseFormat = WebMessageFormat.Json,
        RequestFormat = WebMessageFormat.Json,
        BodyStyle = WebMessageBodyStyle.WrappedRequest,
        UriTemplate = "/checkDetails/?accountID={accountId}")]
    Responces checkDetails(string accountID);

    [OperationContract]
    [WebInvoke(Method = "GET",
        ResponseFormat = WebMessageFormat.Json,
        RequestFormat = WebMessageFormat.Json,
        BodyStyle = WebMessageBodyStyle.WrappedRequest,
        UriTemplate = "/upDateTable/?accountID={accountId}")]
    void upDateTable(String accountID);

    [OperationContract]
    [WebInvoke(Method = "GET",
        ResponseFormat = WebMessageFormat.Json,
        RequestFormat = WebMessageFormat.Json,
        BodyStyle = WebMessageBodyStyle.WrappedRequest,
        UriTemplate = "/resetTable")]
    void resetTable();
}

// Use a data contract as illustrated in the sample below to add composite types to service operations.
[DataContract]
public class PartnersInfo
{
    [DataMember]
    public int rescode { get; set; }
    [DataMember]
    public String message { get; set; }
    [DataMember]
    public String errorDetail { get; set; }
    [DataMember]
    public List<partnerThreshold> partnersDetails { get; set; }
}
[DataContract]
public class partnerThreshold
{
    [DataMember]
    public String accountID { get; set; }
    [DataMember]
    public String accountName { get; set; }
    [DataMember]
    public String prefund { get; set; }
    [DataMember]
    public String email { get; set; }
    [DataMember]
    public String emailcontent { get; set; }
    [DataMember]
    public String threshold { get; set; }
}
[DataContract]
public class partnerRunningBal 
{
    [DataMember]
    public String accountID { get; set; }
    [DataMember]
    public String accountName { get; set; }
    [DataMember]
    public String runningBalance { get; set; }
    [DataMember]
    public String amountLoaded { get; set; }
}
[DataContract]
public class Responces
{
    [DataMember]
    public string rescode { get; set; }
    [DataMember]
    public string message { get; set; }
    [DataMember]
    public List<partnerRunningBal> partnerBalance { get; set; }
}