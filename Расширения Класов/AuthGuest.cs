using Resto.Front.Api.Data.View;
using System;
using Leaf.xNet;
using Newtonsoft.Json;
using Resto.Front.Api;
using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Data.Cheques;
using Resto.Front.Api.Data.Orders;
using Resto.Front.Api.Data.Organization;
using Resto.Front.Api.Data.Payments;
using Resto.Front.Api.Data.Security;
using Resto.Front.Api.Exceptions;
using Resto.Front.Api.Extensions;
using Resto.Front.Api.UI;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
//using RequestsMCRM;
using System.Runtime.CompilerServices;

namespace TNG_plugin
{
    public class Param
    {
        public string Card;
        public int cf;
        public Guest ResponseClass;
    }

    public class Guest
    {

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("card_number")]
        public string CardNumber { get; set; }

        [JsonProperty("balance")]
        public string Balance { get; set; }

        [JsonProperty("check_summ")]
        public int CheckSumm { get; set; }

        [JsonProperty("check_count")]
        public int CheckCount { get; set; }

        [JsonProperty("average_check")]
        public int AverageCheck { get; set; }

        [JsonProperty("last_date")]
        public string LastDate { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }


    public class AuthGuest
    {
        public string CardSlide;

        public Param Request(IViewManager viewManager, IOperationService os, IOrder order)
        {
            PluginContext.Log.Info("Авторизация гостя...");
            var r = new Param();
            if (CardSlide == null)
            {
                PluginContext.Log.Info("Выводим окно для ввода карты гостя");
                var settings = new ExtendedInputDialogSettings  //Настраиваем окно для ввода номера карты/трека/номера телефона
                {
                    EnableCardSlider = true,
                    EnableNumericString = true,
                    TabTitleNumericString = "Введите номер или прокатайте карту",
                    EnablePhone = true,
                    TabTitlePhone = "Введите номер телефона"
                };
                var input = viewManager.ShowExtendedInputDialog(
                    "Авторизация",
                    "Необходимо авторизовать пользователя по карте или номеру телефона",
                    settings);

                CardInputDialogResult carddialogresult;
                PhoneInputDialogResult numberdialogresult;
                StringInputDialogResult numberstring;

                if (input == null)
                {
                    PluginContext.Log.Info("Пользователь отменил ввод карты");
                    return null;
                }
                else
                {

                    PluginContext.Log.Info("Ввели данные, распознаем трек");
                    if (input as CardInputDialogResult != null)
                    {
                        carddialogresult = input as CardInputDialogResult;
                        r.Card = carddialogresult.Track2.Replace(";", "").Replace("?", "").Replace("ж", "").Replace(",", "");
                        r.cf = 2;
                        PluginContext.Log.Info($"Прокатана карта: {r.Card}");
                    }
                    else if (input as StringInputDialogResult != null)
                    {
                        numberstring = input as StringInputDialogResult;
                        r.Card = numberstring.Result;
                        r.cf = 2;
                        PluginContext.Log.Info($"Введен номер карты: {r.Card}");
                    }
                    else
                    {
                        numberdialogresult = input as PhoneInputDialogResult;
                        r.Card = numberdialogresult.PhoneNumber;
                        r.cf = 8;
                        PluginContext.Log.Info($"Введен номер телефона: {r.Card}");

                    }
                }
            }
            else
            {
                PluginContext.Log.Info($"Карта: {CardSlide}");
                r.Card = CardSlide;
                r.cf = 2;
            }

            if (r.Card != null)
            {

                PluginContext.Log.Info("Выполняем запрос информации о госте на сервере MCRM...");
                viewManager.ChangeProgressBarMessage("Выполняем запрос информации о госте на сервере MCRM...");
               // r.ResponseClass = RequestsCRM.GetCard(r.Card);  //запрос информации о госте на сервере MCRM

                if (r.ResponseClass.Status == "ok")
                {
                    os.AddNotificationMessage($"Карта распознана. Гость: {r.ResponseClass.FirstName}, Телефон: +7{r.ResponseClass.Phone} Баланс: {Convert.ToDecimal(r.ResponseClass.Balance) / 100}", "Marketing CRM PRO", TimeSpan.FromSeconds(3));
                    //var Guestname = r.ResponseClass.FirstName;
                    PluginContext.Log.Info($"Ответ получен. Гость: {r.ResponseClass.FirstName}, Телефон: {r.ResponseClass.Phone} Баланс: {Convert.ToDecimal(r.ResponseClass.Balance) / 100}");
                    try
                    {
                        viewManager.ChangeProgressBarMessage("Ответ получен. Пытаемся найти или создать гостя в БД");
                        PluginContext.Log.Info("Пробуем найти гостя в БД iiko");
                        if (os.SearchClients(os.GetCredentials(), r.Card).Any()) //Если в БД iiko есть хоть один гость с таким номером карты
                        {
                            var client = os.SearchClients(os.GetCredentials(), r.Card, 0, (Resto.Front.Api.Data.Search.ClientFields)r.cf).First();
                            PluginContext.Log.Info($"Гость найден, id: {client.Id}");
                            PluginContext.Log.Info("Добавляем гостя в заказ");
                            os.AddClientToOrder(os.GetCredentials(), order, client);
                        }
                        else
                        {
                            PluginContext.Log.Info($"Гость не найден, попробуем создать");
                            var phones = new List<Resto.Front.Api.Data.Brd.PhoneDto>();
                            var TelOrCard = "f";
                            if (r.ResponseClass.Phone != null)
                                TelOrCard = "+7" + r.ResponseClass.Phone;
                            else
                            {
                                TelOrCard = r.Card;
                                while (TelOrCard.Length < 11)
                                    TelOrCard = "0" + TelOrCard;
                                TelOrCard = "+" + TelOrCard;
                            }
                            var item = new Resto.Front.Api.Data.Brd.PhoneDto
                            {
                                IsMain = true,
                                PhoneValue = TelOrCard
                            };
                            phones.Add(item);

                            try
                            {
                                var client = os.CreateClient(Guid.NewGuid(), $"{r.ResponseClass.FirstName} {r.ResponseClass.LastName}", phones, r.Card, DateTime.Now, os.GetCredentials());
                                PluginContext.Log.Info("Гость успешно создан в БД iiko");
                                try
                                {
                                    os.AddClientToOrder(os.GetCredentials(), order, client);
                                    PluginContext.Log.Info("Гость добавлен к заказу");
                                }
                                catch (Exception ex)
                                {
                                    PluginContext.Log.Error(string.Format("Не удалось привязать гостя {0} {1} к заказу {2}, в методе AddClientToOrder произошла ошибка \n {3} \n {4}", client.Name, client.Phones.First().Value, order.Number, ex.Message, ex.StackTrace));
                                }
                                PluginContext.Operations.AddNotificationMessage($"Гость {client.Name} создан в БД iiko и добавлен к заказу", "Marketing CRM PRO");
                            }
                            catch (Exception ex)
                            {
                                PluginContext.Log.Error($"Не удалось создать гостя, произошла ошибка: {ex.Message}");
                            };

                            //var client = os.SearchClients(TelOrCard, 0, (Resto.Front.Api.Data.Search.ClientFields)8).First();

                        }
                        viewManager.ChangeProgressBarMessage("Выполняем запись данных");
                        try
                        {
                            PluginContext.Log.Info($"Записываем ExternalData в заказ {order.Number}. Card {r.Card}, Name {r.ResponseClass.FirstName}, Balance {Convert.ToDecimal(r.ResponseClass.Balance) / 100}");
                            IOrder orderr = os.GetOrderById(order.Id);
                            var editsession = os.CreateEditSession();
                            editsession.AddOrderExternalData("Card", r.Card, true, orderr);
                            editsession.AddOrderExternalData("Name", r.ResponseClass.FirstName, true, orderr);
                            editsession.AddOrderExternalData("Balance", r.ResponseClass.Balance, true, orderr);
                            os.SubmitChanges(os.GetCredentials(), editsession);
                            PluginContext.Log.Info("ExternalData успешно добавлена");
                        }
                        catch (Exception ex) { PluginContext.Log.Error("Произошла ошибка при добавлении ExternalData " + ex.Message); };
                    }
                    catch (Exception ex)
                    {
                        PluginContext.Log.Error("Произошла непредвиденная ошибка, гость не привязан к заказу! (.)(.)" + ex.Message + ex.StackTrace + string.Format("\n id заказа - {0}, номер - {1}", (object)order.Id, (object)order.Number));

                    }
                }
                else
                {
                    PluginContext.Operations.AddErrorMessage($"Не удалось найти гостя в MCRM\n {r.ResponseClass.Message}", "Marketing CRM PRO", TimeSpan.FromSeconds(5));
                    PluginContext.Log.Warn($"Не удалось найти гостя в MCRM\n {r.ResponseClass.Message}");
                }

            }
            else
            {
                PluginContext.Operations.AddWarningMessage("Не было введено данных гостя", "Marketing CRM PRO", TimeSpan.FromSeconds(1));
                PluginContext.Log.Info("Ввод данных гостя отменен");
            }
            return r;
        }
    }
}