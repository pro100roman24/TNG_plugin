using Resto.Front.Api;
using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Data.Orders;
using Resto.Front.Api.Data.Organization;
using Resto.Front.Api.Data.Payments;
using Resto.Front.Api.Data.Security;
using Resto.Front.Api.Data.View;
using Resto.Front.Api.Exceptions;
using Resto.Front.Api.UI;
using System;
using System.Reactive.Disposables;

namespace TNG_plugin
{
    internal class ExternalPaymentProcessorSample : IPaymentProcessor, IDisposable
    {
        private const string paymentSystemKey = Plugin.Name;
        public string PaymentSystemKey => paymentSystemKey;
        public string PaymentSystemName => paymentSystemKey;
        private readonly CompositeDisposable subscriptions;
        public ExternalPaymentProcessorSample()
        {
            subscriptions = new CompositeDisposable
            {
                PluginContext.Notifications.CafeSessionClosing.Subscribe(x => CafeSessionClosing(x.printer, x.vm)),
                PluginContext.Notifications.CafeSessionOpening.Subscribe(x => CafeSessionOpening(x.printer, x.vm)),
                PluginContext.Notifications.NavigatingToPaymentScreen.Subscribe(x => NavigatingToPaymentScreen(x.order, x.os, x.pos, x.vm)),
            };
        }

        private void NavigatingToPaymentScreen(IOrder order, IOperationService os, IPointOfSale pos, IViewManager vm)
        {
            
        }

        private void CafeSessionOpening(IReceiptPrinter printer, IViewManager vm)
        {
            
        }

        private void CafeSessionClosing(IReceiptPrinter printer, IViewManager vm)
        {
            
        }

        public bool CanPaySilently(decimal sum, Guid? orderId, Guid paymentTypeId, IPaymentDataContext context)
        {
            return false;
        }

        public void CollectData(Guid orderId, Guid paymentTypeId, [NotNull] IUser cashier, IReceiptPrinter printer, IViewManager viewManager, IPaymentDataContext context)
        {
            
        }

        public void Dispose()
        {
            try
            {
                subscriptions?.Dispose();
            }
            catch (NotImplementedException)
            {

                throw;
            }
        }

        public void EmergencyCancelPayment(decimal sum, Guid? orderId, Guid paymentTypeId, Guid transactionId, [NotNull] IPointOfSale pointOfSale, [NotNull] IUser cashier, IReceiptPrinter printer, IViewManager viewManager, IPaymentDataContext context)
        {
            throw new NotImplementedException();
        }

        public void EmergencyCancelPaymentSilently(decimal sum, Guid? orderId, Guid paymentTypeId, Guid transactionId, [NotNull] IPointOfSale pointOfSale, [NotNull] IUser cashier, IReceiptPrinter printer, IPaymentDataContext context)
        {
            throw new NotImplementedException();
        }

        public void OnPaymentAdded([NotNull] IOrder order, [NotNull] IPaymentItem paymentItem, [NotNull] IUser cashier, [NotNull] IOperationService operationService, IReceiptPrinter printer, [NotNull] IViewManager viewManager, IPaymentDataContext context)
        {
            var card = operationService.TryGetOrderExternalDataByKey(order, "Card");
            var name = operationService.TryGetOrderExternalDataByKey(order, "Name");
            var balance = operationService.TryGetOrderExternalDataByKey(order, "Balance");

            if (card == null)
            {
                var settings = new ExtendedInputDialogSettings
                {
                    EnableCardSlider = true,
                    EnablePhone = false,
                    EnableNumericString = true,
                    TabTitleNumericString = "Введите номер карты/браслета",
                };

                var dialogResult = viewManager.ShowExtendedInputDialog(
                "Авторизация гостя",
                "Прокатайте карту гостя.",
                settings);

                if (dialogResult is CardInputDialogResult qrcode)
                {
                    PluginContext.Log.InfoFormat($"Карта: {qrcode.Track2}", Plugin.Name);
                    var response = Front_Plugin.CardSlide(order, operationService, viewManager, qrcode);
                }
                if (dialogResult is StringInputDialogResult numeric)
                {
                    PluginContext.Log.InfoFormat($"Ввели номер карты: {numeric.Result}", Plugin.Name);

                    var response = Front_Plugin.CardSlide(order, operationService, viewManager, null, numeric.Result);
                }
                if (dialogResult == null)
                {
                    PluginContext.Log.InfoFormat("Пользователь отказался от ввода данных", Plugin.Name);
                    throw new PaymentActionCancelledException();
                }
            }
            else
            {
                operationService.AddNotificationMessage($"К заказу привязан гость:\nИмя - {name} \nНомер карты - {card}\nБаланс - {balance}", Plugin.Name);
            }
        }

        public bool OnPreliminaryPaymentEditing([NotNull] IOrder order, [NotNull] IPaymentItem paymentItem, [NotNull] IUser cashier, [NotNull] IOperationService operationService, IReceiptPrinter printer, [NotNull] IViewManager viewManager, IPaymentDataContext context)
        {
            throw new NotImplementedException();
        }

        public void Pay(decimal sum, [NotNull] IOrder order, [NotNull] IPaymentItem paymentItem, Guid transactionId, [NotNull] IPointOfSale pointOfSale, [NotNull] IUser cashier, [NotNull] IOperationService operationService, IReceiptPrinter printer, [NotNull] IViewManager viewManager, IPaymentDataContext context)
        {
            Plugin.Log_Info($"Оплата заказа {order.Number}");
            var pay = RequestsTNG.RequestsTNG.Payment(order, true);
            if (pay.Status == "SUCCESS")
            {
                Plugin.Log_Mess_Info($"Оплата заказа {order.Number} прошла успешно, заказ отправлен в TNG");
            }
            else
            {
                Plugin.Log_Error($"При оплате заказа {order.Number} типом оплаты {paymentItem.Type.Name} возникла ошибка: \n {pay.PaymentInfo} \n Проверьте интернет соединение, настройки плагина и повторите попытку.");
                throw new PaymentActionFailedException($"При оплате заказа {order.Number} типом оплаты {paymentItem.Type.Name} возникла ошибка: \n {pay.PaymentInfo} \n Проверьте интернет соединение, настройки плагина и повторите попытку.");
            }
        }

        public void PaySilently(decimal sum, [NotNull] IOrder order, [NotNull] IPaymentItem paymentItem, Guid transactionId, [NotNull] IPointOfSale pointOfSale, [NotNull] IUser cashier, IReceiptPrinter printer, IPaymentDataContext context)
        {
            throw new NotImplementedException();
        }

        public void ReturnPayment(decimal sum, Guid? orderId, Guid paymentTypeId, Guid transactionId, [NotNull] IPointOfSale pointOfSale, [NotNull] IUser cashier, IReceiptPrinter printer, [NotNull] IViewManager viewManager, IPaymentDataContext context)
        {
            var order = PluginContext.Operations.GetOrderById(orderId.Value);
            var paytype = PluginContext.Operations.TryGetPaymentTypeById(paymentTypeId);
            Plugin.Log_Info($"Возврат заказа {order.Number}");
            var pay = RequestsTNG.RequestsTNG.Payment(order, false);
            if (pay.Status == "SUCCESS")
            {
                Plugin.Log_Mess_Info($"Возврат заказа {order.Number} прошёл успешно, заказ отправлен в TNG");
            }
            else
            {
                Plugin.Log_Error($"При возврате заказа {order.Number} типом оплаты {paytype.Name} возникла ошибка: \n {pay.PaymentInfo} \n Проверьте интернет соединение, настройки плагина и повторите попытку.");
                throw new PaymentActionFailedException($"При возврате заказа {order.Number} типом оплаты {paytype.Name} возникла ошибка: \n {pay.PaymentInfo} \n Проверьте интернет соединение, настройки плагина и повторите попытку.");
            }
        }

        public void ReturnPaymentWithoutOrder(decimal sum, Guid? orderId, Guid paymentTypeId, [NotNull] IPointOfSale pointOfSale, [NotNull] IUser cashier, IReceiptPrinter printer, [NotNull] IViewManager viewManager)
        {
            throw new NotImplementedException();
        }
    }
}