using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Leaf.xNet;
using Newtonsoft.Json;
using TNG_plugin;
//using CodeBeautify;
using Xml2CSharp;
using System.Globalization;
using Resto.Front.Api.Data.Orders;
using Resto.Front.Api.Data.Assortment;
using Resto.Front.Api;

namespace RequestsTNG
{

    public class Guest
    {
        public string Name { get; set; }
        public string LastName { get; set; }
        public int ProfileID { get; set; }
        public int CardNumber { get; set; }
        public double Balance { get; set; }
        public string Error { get; set; }
    }

    public class PaymentTNG
    {
        public string Status { get; set; }
        public string PostingGUID { get; set; }
        public string PaymentAmount { get; set; }
        public string Account { get; set; }
        public string PaymentInfo { get; set; }
    }

    class RequestsTNG
    {

        public static Guest GetCard(string number)
        {
            try
            {
                Plugin.Log_Info($"Отправка запроса информации о госте по номеру карты {number}");
                string body = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                    "<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">" +
                    "<soap:Body>" +
                    "<FetchProfileRequest xmlns=\"http://club.lassd.hrs.ru/ws\">" +
                    "<ProfileID xmlns=\"\">" + number + "</ProfileID>" +
                    "</FetchProfileRequest>" +
                    "</soap:Body>" +
                    "</soap:Envelope>";

                Guest guest = new Guest();

                var Request = new HttpRequest();
                Request.AddHeader("Content-Type", "text/xml");
                Request.IgnoreProtocolErrors = true;

                Plugin.Log_Info($"Тело запроса:\n {body}", Plugin.Params.ShowFullLogs);

                var Response = Request.Post(Config.Instance.TNGurl, body, "application/json");

                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(Response.ToString());

                Plugin.Log_Info("Запрос гостя прошел успешно.");
                Plugin.Log_Info($"Тело ответа:\n {xDoc.InnerXml}", Plugin.Params.ShowFullLogs);

                XmlElement xRoot = xDoc.DocumentElement;
                if (xRoot != null)
                {
                    // обход всех узлов в корневом элементе
                    foreach (XmlElement xnode in xRoot)
                    {
                        foreach (XmlNode childnode in xnode.ChildNodes)
                        {
                            foreach (XmlNode cchildnode in childnode.ChildNodes)
                            {
                                if (cchildnode.Name == "faultstring")
                                {
                                    guest.Error = cchildnode.InnerText;
                                }

                                if (cchildnode.Name == "ProfileID")
                                {
                                    guest.ProfileID = Int32.Parse(cchildnode.InnerText);
                                }

                                if (cchildnode.Name == "PersonName")
                                {
                                    foreach (XmlNode cchildnode1 in cchildnode.ChildNodes)
                                    {
                                        if (cchildnode1.Name == "ns2:FirstName")
                                        {
                                            guest.Name = cchildnode1.InnerText;
                                        }
                                        if (cchildnode1.Name == "ns2:LastName")
                                        {
                                            guest.LastName = cchildnode1.InnerText;
                                        }
                                    }
                                }
                                var f = false;
                                if (cchildnode.Name == "Accounts")
                                {
                                    foreach (XmlNode cchildnode1 in cchildnode.ChildNodes)
                                    {
                                        if (cchildnode1.Name == "ns4:AccountInfoList")
                                        {
                                            foreach (XmlNode cchildnode2 in cchildnode1.ChildNodes)
                                            {

                                                if (cchildnode2.Name == "ns4:Name" && cchildnode2.InnerText == Config.Instance.AccountName)
                                                {
                                                    f = true;
                                                }
                                                if (f && cchildnode2.Name == "ns4:Balance")
                                                {
                                                    IFormatProvider formatter = new NumberFormatInfo { NumberDecimalSeparator = "." };
                                                    guest.Balance = double.Parse(cchildnode2.InnerText, formatter);
                                                    f = false;
                                                }
                                            }
                                        }
                                    }
                                }
                                if (cchildnode.Name == "Cards")
                                {
                                    foreach (XmlNode cchildnode1 in cchildnode.ChildNodes)
                                    {
                                        if (cchildnode1.Name == "ns4:CardInfoList")
                                        {
                                            foreach (XmlNode cchildnode2 in cchildnode1.ChildNodes)
                                            {
                                                if (cchildnode2.Name == "ns4:Number")
                                                {
                                                    guest.CardNumber = Int32.Parse(cchildnode2.InnerText);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                //var ResponseParam = new Xml2CSharp.Envelope();


                //XmlSerializer serializer = new XmlSerializer(typeof(Xml2CSharp.Envelope));
                //using (StringReader reader = new StringReader(xDoc.InnerXml))
                //{
                //    ResponseParam = (Xml2CSharp.Envelope)serializer.Deserialize(reader);
                //}

                return guest;
            } catch(Exception ex)
            {
                Plugin.Log_Mess_Error(ex.Message);
                Plugin.Log_Error(ex.StackTrace);
                Guest guest = new Guest() { Error = ex.Message };
                return guest;
            }
        }

        public static PaymentTNG Payment(IOrder order, bool typeoperation)
        {
            try
            {
                Plugin.Log_Info($"Оплата заказа {order.Number}");
                string возврат = typeoperation == false ? "-" : "";
                var os = PluginContext.Operations;
                var card = os.TryGetOrderExternalDataByKey(order, "Card");

                string body = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                    "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:ns5=\"http://club.lassd.hrs.ru/ws\" xmlns:ns2=\"http://htng.org/PWS/2008A/SingleGuestItinerary/Common/Types\"" +
                    " xmlns:ns4=\"http://www.hrs.ru/clubng/ws/common/types\" xmlns:ns3=\"http://htng.org/PWS/2008A/SingleGuestItinerary/Name/Types\">" +
                    "<soapenv:Header />" +
                    "<soapenv:Body>" +
                    "<ns5:PostPaymentRequest>" +
                    $"<Number>{card}</Number>" +
                    $"<PostPropertyId>{Config.Instance.PostPropertyId}</PostPropertyId>" +
                    $"<RegisterId source=\"\">{Config.Instance.WorkstationID}</RegisterId>" +
                    $"<RevenueCenterId>{Config.Instance.RevenueCenterId}</RevenueCenterId>" +
                    $"<PaymentAmount>{возврат}{order.ResultSum.ToString().Replace(",", ".")}</PaymentAmount>" +
                    $"<CheckNumber>{order.Number}</CheckNumber>" +
                    $"<CashierEmpName>{order.Waiter.Name}</CashierEmpName>" +
                    $"<CheckGuestCount>{order.Guests.Count}</CheckGuestCount>" +
                    "<menuItemList>";

                foreach (IOrderProductItem item in order.Items)
                {
                    var group = os.TryGetParentByProduct(item.Product) != null ? os.TryGetParentByProduct(item.Product) : null;
                    IProductGroup ggroup = null;
                    if (group != null)
                    {
                        ggroup = os.TryGetParentByProductGroup(group) == null ? os.TryGetParentByProductGroup(group) : null;
                    }
                    body += "<ns4:MenuItem>" +
                        $"<ns4:id>{item.Product.Number}</ns4:id>" +
                        $"<ns4:name>{item.Product.Name}</ns4:name>" +
                        $"<ns4:qty>{item.Amount}</ns4:qty>" +
                        $"<ns4:price>{item.Price.ToString().Replace(",", ".")}</ns4:price>" +
                        $"<ns4:extPrice>{item.ResultSum.ToString().Replace(",", ".")}</ns4:extPrice>" +
                        $"<ns4:discount>0.0</ns4:discount>" +
                        $"<ns4:MajorGroup>{(ggroup != null ? ggroup.Name : "")}</ns4:MajorGroup>" +
                        $"<ns4:SubGroup>{(group != null ? group.Name : "")}</ns4:SubGroup>" +
                        "</ns4:MenuItem>";
                }

                body += "</menuItemList>" +
                    "<Confirm>true</Confirm>" +
                    "</ns5:PostPaymentRequest>" +
                    "</soapenv:Body>" +
                    "</soapenv:Envelope>";

                Plugin.Log_Info($"Тело запроса на оплату заказа: \n{body}", Plugin.Params.ShowFullLogs);
                var Request = new HttpRequest();
                Request.AddHeader("Content-Type", "text/xml");
                var Response = Request.Post(Config.Instance.TNGurl, body, "application/json");

                PaymentTNG payment = new PaymentTNG();

                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(Response.ToString());
                XmlElement xRoot = xDoc.DocumentElement;

                if (xRoot != null)
                {
                    // обход всех узлов в корневом элементе
                    foreach (XmlElement xnode in xRoot)
                    {
                        foreach (XmlNode childnode in xnode.ChildNodes)
                        {
                            foreach (XmlNode cchildnode in childnode.ChildNodes)
                            {
                                if (cchildnode.Name == "Status")
                                {
                                    payment.Status = cchildnode.InnerText;
                                }

                                if (cchildnode.Name == "PostingGUID")
                                {
                                    payment.PostingGUID = cchildnode.InnerText;
                                }

                                if (cchildnode.Name == "PaymentAmount")
                                {
                                    payment.PaymentAmount = cchildnode.InnerText;
                                }

                                if (cchildnode.Name == "Account")
                                {
                                    payment.Account = cchildnode.InnerText;
                                }

                                if (cchildnode.Name == "PaymentInfo")
                                {
                                    payment.PaymentInfo = cchildnode.InnerText;
                                }
                            }
                        }
                    }
                }

                Plugin.Log_Info($"Тело ответа на оплату заказа:\n{xDoc.InnerXml}", Plugin.Params.ShowFullLogs);
                return payment;
            }
            catch (Exception ex)
            {
                PaymentTNG payment = new PaymentTNG() { Status = "FAIL", PaymentInfo = ex.Message };
                return payment;
            }
        }
    }
}
