using Resto.Front.Api;
using Resto.Front.Api.Attributes;
using Resto.Front.Api.Attributes.JetBrains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using Lemma_Lic;
using Resto.Front.Api.UI;
using Resto.Front.Api.Extensions;
using Resto.Front.Api.Data.View;
using Resto.Front.Api.Data.Orders;
using Resto.Front.Api.Data.Brd;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using Resto.Front.Api.Exceptions;
using System.Reactive.Disposables;

namespace TNG_plugin
{
    [UsedImplicitly]
    [PluginLicenseModuleId(21016318)]

    public sealed class Front_Plugin : IFrontPlugin
    {

        private readonly CompositeDisposable subscriptions;
        public Front_Plugin()
        {
            subscriptions = new CompositeDisposable();
            Plugin.Log_Mess_Warn("Плагин {0} запускается.", Plugin.Name);
            var paymentSystem = new ExternalPaymentProcessorSample();
            subscriptions.Add(paymentSystem);
            try
            {
                subscriptions.Add(PluginContext.Operations.RegisterPaymentSystem(paymentSystem, true));

            }
            catch (LicenseRestrictionException ex)
            {
                PluginContext.Log.Warn(ex.Message);
                return;
            }
            catch (PaymentSystemRegistrationException ex)
            {
                PluginContext.Log.Warn($"Payment system '{paymentSystem.PaymentSystemKey}': '{paymentSystem.PaymentSystemName}' wasn't registered. Reason: {ex.Message}");
                return;
            }

            PluginContext.Log.Info($"Payment system '{paymentSystem.PaymentSystemKey}': '{paymentSystem.PaymentSystemName}' was successfully registered on server.");
            Lemma.CheckLicense(PluginContext.Operations.GetHostRestaurant().IikoUid, Plugin.Name);
            //subscriptions.Push(new Tester());
            var adress = Config.Instance.TNGurl;
            var test = RequestsTNG.RequestsTNG.GetCard("0000");

            if (test.Error == "Card not found")
            {
                Plugin.Log_Info($"Тест связи с {Config.Instance.TNGurl} прошел успешно.");
            }
            else if (test.Error != "")
            {
                Dispose();
            }

            //PluginContext.Notifications.NavigatingToPaymentScreen.Subscribe(x => nav(x.order, x.os, x.vm));
            PluginContext.Notifications.OrderEditCardSlided.Subscribe(x => CardSlide(x.order, x.os, x.vm, x.card));

            //PluginContext.Operations.AddButtonToPluginsMenu("Тест TNG", x => plgnbtn(x.vm, x.printer));

            Plugin.Log_Mess_Info("Плагин {0} успешно запущен.", Plugin.Name);
            var order = PluginContext.Operations.GetOrders(false, true);

            //createguest();



        }

        private void plgnbtn(IViewManager vm, IReceiptPrinter printer)
        {
            string fuck = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + 
                "<ns4:AccountInfoList>" +
                "<ns4:Type>M</ns4:Type>" +
                "<ns4:Code>DEPO</ns4:Code>" +
                "<ns4:Name>Deposit account</ns4:Name>" +
                "<ns4:Balance>0.0</ns4:Balance>" +
                "</ns4:AccountInfoList> ";

            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(fuck);
            Xml2CSharp.AccountInfoList accountInfoList = new Xml2CSharp.AccountInfoList();
            XmlSerializer serializer = new XmlSerializer(typeof(Xml2CSharp.AccountInfoList));
            using (StringReader reader = new StringReader(xDoc.InnerXml))
            {
                accountInfoList = (Xml2CSharp.AccountInfoList)serializer.Deserialize(reader);
            }

            vm.ShowOkPopup("сообщение", accountInfoList.Name);
        }

        public static bool CardSlide(IOrder order, IOperationService os, IViewManager vm, CardInputDialogResult card = null, string stringcard = "")
        {
            var CardTrack = card.Track2 != null ? card.Track2 : stringcard;
            Plugin.Log_Info($"Прокатана карта {CardTrack}");
            var guestinfo = RequestsTNG.RequestsTNG.GetCard(CardTrack);

            var clients = os.SearchClients(os.GetCredentials(), guestinfo.CardNumber.ToString());

            //var balanse = RequestsMCRM.RequestsCRM.GetCard(card);
            IClient client;
            if (clients.Any())
            {
                client = clients.First();
                Plugin.Log_Info($"Найден гость {client.Name}");
            }
            else
            {
                //List<PhoneDto> phones = new List<PhoneDto>();
                client = os.CreateClient(Guid.NewGuid(), $"{guestinfo.Name} {guestinfo.LastName}", null, guestinfo.CardNumber.ToString(), DateTime.Now, os.GetCredentials());
                Plugin.Log_Info($"Создан новый гость {client.Name}");
            }

            IOrder orderr = os.GetOrderById(order.Id);
            var editsession = os.CreateEditSession();
            editsession.AddOrderExternalData("Card", guestinfo.CardNumber.ToString(), true, orderr);
            editsession.AddOrderExternalData("Name", guestinfo.Name, true, orderr);
            editsession.AddOrderExternalData("Balance", guestinfo.Balance.ToString(), true, orderr);
            os.SubmitChanges(os.GetCredentials(), editsession);

            //order = os.GetOrderById(order.Id);
            //os.AddOrderExternalData("Whaiter", data, false, order, os.GetCredentials());
            //order = os.GetOrderById(order.Id);
            os.AddClientToOrder(os.GetCredentials(), order, client);
            Plugin.Log_Mess_Info($"К заказу привязан гость {client.Name}", Plugin.Name);

            return true;
        }

        private void createguest()
        {
            var os = PluginContext.Operations;
            var cliet=os.CreateClient(Guid.NewGuid(), "test", null, "666777", DateTime.Now, os.GetCredentials());
            os.ChangeClientEmails(new List<Resto.Front.Api.Data.Brd.EmailDto>()
            { new EmailDto(){EmailValue="sirwow@ya.ru",IsMain=true} }, cliet, os.GetCredentials());
        }
       

        private void nav(IOrder order, IOperationService os, IViewManager vm)
        {
            try
            {
                
            }  
            catch (Exception ex )
            {

                throw;
            }
         

        }

        public void Dispose()
        {

                PluginContext.Log.Info("Зашли в деструктор плагина");
                // отписываемся от всех событий
                subscriptions?.Dispose();
                // debug!!
                PluginContext.Log.Info("Отписались от событий");
                PluginContext.Log.Info("Плагин остановлен");
                Plugin.Log_Mess_Info("Плагин {0} успешно остановлен.", Plugin.Name);
                throw new NotImplementedException();

            //while (subscriptions.Any())
            //{
            //    var subscription = subscriptions.Pop();
            //    try
            //    {
            //        subscription.Dispose();
            //    }
            //    catch (RemotingException)
            //    {
            //        // nothing to do with the lost connection
            //    }
            //}
            
        }
    }
}
