using mercadopago;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.MercadoPago.Models;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Nop.Plugin.Payments.MercadoPago.Controllers
{
    public class PaymentMercadoPagoController : BasePaymentController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IStoreContext _storeContext;
        private readonly ILogger _logger;
        private readonly IWebHelper _webHelper;
        private readonly PaymentSettings _paymentSettings;
        private readonly MercadoPagoPaymentSettings _MercadoPagoPaymentSettings;
        private readonly IPictureService _pictureService;
        private readonly ICategoryService _categoryService;
        private readonly ILocalizationService _localizationService;
        #region Ctor
        public PaymentMercadoPagoController(IWorkContext workContext,
            IStoreService storeService,
            ISettingService settingService,
            IPaymentService paymentService,
            IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            IStoreContext storeContext,
            ILogger logger,
            IWebHelper webHelper,
            PaymentSettings paymentSettings,
            MercadoPagoPaymentSettings MercadoPagoPaymentSettings,
            IPictureService pictureService,
            ICategoryService categoryService,
            ILocalizationService localizationService)
        {
            this._workContext = workContext;
            this._storeService = storeService;
            this._settingService = settingService;
            this._paymentService = paymentService;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._storeContext = storeContext;
            this._logger = logger;
            this._webHelper = webHelper;
            this._paymentSettings = paymentSettings;
            this._MercadoPagoPaymentSettings = MercadoPagoPaymentSettings;
            this._pictureService = pictureService;
            this._categoryService = categoryService;
            this._localizationService = localizationService;
        }

        #endregion

        #region Methods
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var mercadoPagoPaymentSettings = _settingService.LoadSetting<MercadoPagoPaymentSettings>(storeScope);
            var countries = MakeListCountrySetting();
            var model = new ConfigurationModel();
            model.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Address.SelectCountry"), Value = "0" });
            foreach (var c in countries)
            {
                model.AvailableCountries.Add(new SelectListItem
                {
                    Text = c.CountryName,
                    Value = c.CountryId,
                    Selected = c.CountryId == model.countryId
                });
            }

            string paymentForms = string.Empty;
            foreach (var c in countries)
            {
                paymentForms += c.CountryName + "<br />Formas de pago - ";
                foreach (var pm in c.PaymentMethods)
                {
                    paymentForms += pm.MethodId + ";";
                }
                paymentForms += "<br />";
            }
            ViewData["PaymentForms"] = paymentForms;

            model.countryId = mercadoPagoPaymentSettings.countryId;
            model.UseLog = mercadoPagoPaymentSettings.UseLog;
            model.client_id = mercadoPagoPaymentSettings.client_id;
            model.client_secret = mercadoPagoPaymentSettings.client_secret;
            model.AdditionalFee = mercadoPagoPaymentSettings.AdditionalFee;
            model.AdditionalFeePercentage = mercadoPagoPaymentSettings.AdditionalFeePercentage;
            model.ManejarTotal = mercadoPagoPaymentSettings.ManejarTotal;
            model.EnableIpn = mercadoPagoPaymentSettings.EnableIpn;
            model.IpnUrl = mercadoPagoPaymentSettings.IpnUrl;
            model.IdTestIPN = mercadoPagoPaymentSettings.IdTestIPN;
            model.AvailableDays = mercadoPagoPaymentSettings.AvailableDays;
            model.excluded_payment_methods = mercadoPagoPaymentSettings.excluded_payment_methods;
            model.excluded_payment_types = mercadoPagoPaymentSettings.excluded_payment_types;

            model.ActiveStoreScopeConfiguration = storeScope;
            if (storeScope > 0)
            {
                model.UseLog_OverrideForStore = _settingService.SettingExists(mercadoPagoPaymentSettings, x => x.UseLog, storeScope);
                model.countryId_OverrideForStore = _settingService.SettingExists(mercadoPagoPaymentSettings, x => x.countryId, storeScope);
                model.client_id_OverrideForStore = _settingService.SettingExists(mercadoPagoPaymentSettings, x => x.client_id, storeScope);
                model.client_secret_OverrideForStore = _settingService.SettingExists(mercadoPagoPaymentSettings, x => x.client_secret, storeScope);
                model.AdditionalFee_OverrideForStore = _settingService.SettingExists(mercadoPagoPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = _settingService.SettingExists(mercadoPagoPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
                model.EnableIpn_OverrideForStore = _settingService.SettingExists(mercadoPagoPaymentSettings, x => x.EnableIpn, storeScope);
                model.ManejarTotal_OverrideForStore = _settingService.SettingExists(mercadoPagoPaymentSettings, x => x.ManejarTotal, storeScope);
                model.IpnUrl_OverrideForStore = _settingService.SettingExists(mercadoPagoPaymentSettings, x => x.IpnUrl, storeScope);
                model.IdTestIPN_OverrideForStore = _settingService.SettingExists(mercadoPagoPaymentSettings, x => x.IdTestIPN, storeScope);
                model.AvailableDays_OverrideForStore = _settingService.SettingExists(mercadoPagoPaymentSettings, x => x.AvailableDays, storeScope);
                model.excluded_payment_methods_OverrideForStore = _settingService.SettingExists(mercadoPagoPaymentSettings, x => x.excluded_payment_methods, storeScope);
                model.excluded_payment_types_OverrideForStore = _settingService.SettingExists(mercadoPagoPaymentSettings, x => x.excluded_payment_types, storeScope);
            }

            return View("~/Plugins/Payments.MercadoPago/Views/PaymentMercadoPago/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var mercadoPagoPaymentSettings = _settingService.LoadSetting<MercadoPagoPaymentSettings>(storeScope);

            //save settings
            mercadoPagoPaymentSettings.countryId = model.countryId;
            mercadoPagoPaymentSettings.UseLog = model.UseLog;
            mercadoPagoPaymentSettings.client_id = model.client_id;
            mercadoPagoPaymentSettings.client_secret = model.client_secret;
            mercadoPagoPaymentSettings.AdditionalFee = model.AdditionalFee;
            mercadoPagoPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            mercadoPagoPaymentSettings.ManejarTotal = model.ManejarTotal;
            mercadoPagoPaymentSettings.EnableIpn = model.EnableIpn;
            mercadoPagoPaymentSettings.IpnUrl = model.IpnUrl;
            mercadoPagoPaymentSettings.IdTestIPN = model.IdTestIPN;
            mercadoPagoPaymentSettings.AvailableDays = model.AvailableDays;
            mercadoPagoPaymentSettings.excluded_payment_methods = model.excluded_payment_methods;
            mercadoPagoPaymentSettings.excluded_payment_types = model.excluded_payment_types;

            if (model.UseLog_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(mercadoPagoPaymentSettings, x => x.UseLog, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(mercadoPagoPaymentSettings, x => x.UseLog, storeScope);

            if (model.countryId_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(mercadoPagoPaymentSettings, x => x.countryId, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(mercadoPagoPaymentSettings, x => x.countryId, storeScope);

            if (model.client_id_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(mercadoPagoPaymentSettings, x => x.client_id, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(mercadoPagoPaymentSettings, x => x.client_id, storeScope);

            if (model.client_secret_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(mercadoPagoPaymentSettings, x => x.client_secret, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(mercadoPagoPaymentSettings, x => x.client_secret, storeScope);

            if (model.AdditionalFee_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(mercadoPagoPaymentSettings, x => x.AdditionalFee, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(mercadoPagoPaymentSettings, x => x.AdditionalFee, storeScope);

            if (model.AdditionalFeePercentage_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(mercadoPagoPaymentSettings, x => x.AdditionalFeePercentage, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(mercadoPagoPaymentSettings, x => x.AdditionalFeePercentage, storeScope);

            if (model.ManejarTotal_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(mercadoPagoPaymentSettings, x => x.ManejarTotal, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(mercadoPagoPaymentSettings, x => x.ManejarTotal, storeScope);

            if (model.EnableIpn_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(mercadoPagoPaymentSettings, x => x.EnableIpn, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(mercadoPagoPaymentSettings, x => x.EnableIpn, storeScope);

            if (model.IpnUrl_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(mercadoPagoPaymentSettings, x => x.IpnUrl, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(mercadoPagoPaymentSettings, x => x.IpnUrl, storeScope);

            if (model.IdTestIPN_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(mercadoPagoPaymentSettings, x => x.IdTestIPN, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(mercadoPagoPaymentSettings, x => x.IdTestIPN, storeScope);


            if (model.AvailableDays_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(mercadoPagoPaymentSettings, x => x.AvailableDays, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(mercadoPagoPaymentSettings, x => x.AvailableDays, storeScope);

            if (model.excluded_payment_methods_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(mercadoPagoPaymentSettings, x => x.excluded_payment_methods, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(mercadoPagoPaymentSettings, x => x.excluded_payment_methods, storeScope);

            if (model.excluded_payment_types_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(mercadoPagoPaymentSettings, x => x.excluded_payment_types, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(mercadoPagoPaymentSettings, x => x.excluded_payment_types, storeScope);

            //now clear settings cache
            _settingService.ClearCache();

            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            return View("~/Plugins/Payments.MercadoPago/Views/PaymentMercadoPago/PaymentInfo.cshtml");
        }

        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();
            return warnings;
        }

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();
            return paymentInfo;
        }

        #endregion

        #region AuxMethods

        public ActionResult MPAction()
        {
            int orderId = 0;
            string strorderId = Request.Form["orderId"];

            try
            {
                orderId = int.Parse(strorderId);
            }
            catch (Exception e)
            {
                orderId = 0;
            }

            if (orderId == 0) //error
                return new RedirectResult(_webHelper.GetStoreLocation() + "Plugins/PaymentMercadoPago/Error.cshtml");

            var order = _orderService.GetOrderById(orderId);
            if (order == null)//error
                return new RedirectResult(_webHelper.GetStoreLocation() + "Plugins/PaymentMercadoPago/Error.cshtml");

            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var mercadoPagoPaymentSettings = _settingService.LoadSetting<MercadoPagoPaymentSettings>(storeScope);
            var countrySetting = CountrySettingByCountryId(_MercadoPagoPaymentSettings.countryId);
            MP mp = new MP(mercadoPagoPaymentSettings.client_id, mercadoPagoPaymentSettings.client_secret);

            string external_reference = string.Format("\"external_reference\": \"{0}\"", order.OrderGuid.ToString());
            string items = "\"items\":[";

            if (!mercadoPagoPaymentSettings.ManejarTotal)
            {
                foreach (var item in order.OrderItems)
                {
                    string urlPicture = string.Empty;
                    string category = string.Empty;
                    var pictures = _pictureService.GetPicturesByProductId(item.ProductId, 1);
                    if (pictures.Count > 0)
                    {
                        urlPicture = _pictureService.GetPictureUrl(pictures[0], storeLocation: _storeContext.CurrentStore.Url);
                    }

                    var defaultProductCategory = _categoryService
                              .GetProductCategoriesByProductId(item.ProductId, _storeContext.CurrentStore.Id)
                              .FirstOrDefault();
                    if (defaultProductCategory != null)
                    {
                        category = defaultProductCategory.Category.Name;
                    }
                    items += "{\"id\": \"" + item.Product.Sku + "\",\"title\": \"" + item.Product.Name + "\",\"currency_id\": \"" + countrySetting.Moneda + "\",\"picture_url\": \"" + urlPicture + "\",\"quantity\": " + item.Quantity.ToString() + ",\"unit_price\": " + string.Format(CultureInfo.InvariantCulture, "{0:0.00}", item.UnitPriceInclTax) + "},";
                }
                if (order.OrderShippingInclTax > 0)
                    items += "{\"id\": \"" + "Mensajeria" + "\",\"title\": \"" + "Envio especializado" + "\",\"currency_id\": \"" + countrySetting.Moneda + "\",\"quantity\": " + "1" + ",\"unit_price\": " + string.Format(CultureInfo.InvariantCulture, "{0:0.00}", order.OrderShippingInclTax) + "},";

                if (order.PaymentMethodAdditionalFeeInclTax > 0)
                    items += "{\"id\": \"" + "MercadoPago" + "\",\"title\": \"" + "Forma de Pago" + "\",\"currency_id\": \"" + countrySetting.Moneda + "\",\"quantity\": " + "1" + ",\"unit_price\": " + string.Format(CultureInfo.InvariantCulture, "{0:0.00}", order.PaymentMethodAdditionalFeeInclTax) + "},";
            }
            else
            {
                items += "{\"id\": \"" + "MercadoPago" + "\",\"title\": \"" + _localizationService.GetLocaleStringResourceByName("Order.Order#") + " " + order.Id + "\",\"currency_id\": \"" + countrySetting.Moneda + "\",\"quantity\": " + "1" + ",\"unit_price\": " + string.Format(CultureInfo.InvariantCulture, "{0:0.00}", order.OrderTotal) + "},";
            }

            items += "]";

            //string payer = "\"payer\": { \"name\": \"" + order.Customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName) + "\",\"surname\": \"" + order.Customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName) + "\",\"email\": \"" + order.Customer.Email + "\",\"phone\": \"" + order.Customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone) + "\",\"date_created\": \"" + order.Customer.CreatedOnUtc.ToString("yyyy-MM-ddTHH:mm:ssK") + "\"}";
            string payer = "\"payer\": { \"name\": \"" + order.Customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName) + "\",\"surname\": \"" + order.Customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName) + "\",\"email\": \"" + order.Customer.Email + "\",\"date_created\": \"" + order.Customer.CreatedOnUtc.ToString("yyyy-MM-ddTHH:mm:ssK") + "\"}";
            string urlSuccess = _webHelper.GetStoreLocation() + "Plugins/PaymentMercadoPago/Success/" + order.Id.ToString() + "/" + order.OrderGuid.ToString(); // Url.RouteUrl(new { Controller = "PaymentMercadoPago", Action = "Success", oId = order.Id, oG = order.OrderGuid });
            string urlFailure = _webHelper.GetStoreLocation() + "Plugins/PaymentMercadoPago/Failure/" + order.Id.ToString() + "/" + order.OrderGuid.ToString(); //Url.RouteUrl(new { Controller = "PaymentMercadoPago", Action = "Failure", oId = order.Id, oG = order.OrderGuid });
            string urlPending = _webHelper.GetStoreLocation() + "Plugins/PaymentMercadoPago/Pending/" + order.Id.ToString() + "/" + order.OrderGuid.ToString(); //Url.RouteUrl(new { Controller = "PaymentMercadoPago", Action = "Pending", oId = order.Id, oG = order.OrderGuid }); 
            string back_url = "\"back_urls\": { \"success\": \"" + urlSuccess + "\", \"failure\": \"" + urlFailure + "\", \"pending\": \"" + urlPending + "\" }";
            string payment_methods = string.Empty;
            string excluded_payment_methods = string.Empty;
            string excluded_payment_types = string.Empty;
            if (!string.IsNullOrEmpty(mercadoPagoPaymentSettings.excluded_payment_methods))
            {
                excluded_payment_methods = "\"excluded_payment_methods\": [";
                string[] excluded_payment_methodsList = mercadoPagoPaymentSettings.excluded_payment_methods.Split(';');
                foreach (string payment_method_id in excluded_payment_methodsList)
                {
                    excluded_payment_methods += "{\"id\": \"" + payment_method_id + "\"}";
                }
                excluded_payment_methods += "],";
            }

            if (!string.IsNullOrEmpty(mercadoPagoPaymentSettings.excluded_payment_types))
            {
                excluded_payment_types = "\"excluded_payment_types\": [";
                string[] excluded_payment_typesList = mercadoPagoPaymentSettings.excluded_payment_types.Split(';');
                foreach (string excluded_payment_types_id in excluded_payment_typesList)
                {
                    excluded_payment_types += "{\"id\": \"" + excluded_payment_types_id + "\"}";
                }
                excluded_payment_types += "],";
            }
            if (!string.IsNullOrEmpty(excluded_payment_methods) || !string.IsNullOrEmpty(excluded_payment_types))
            {
                payment_methods = "\"payment_methods\": {" + excluded_payment_methods + excluded_payment_types + ",}";
            }
            string notification_url = string.Format("\"notification_url\": \"{0}\"", mercadoPagoPaymentSettings.IpnUrl);
            string auto_return = string.Format("\"auto_return\": \"{0}\"", "approved");
            string strpreference = string.Empty;
            if (!order.Customer.Email.ToUpper().Contains("TEST"))
            {
                string sponsor_id = string.Format("\"sponsor_id\": {0}", countrySetting.SponsorId);
                strpreference = "{" + external_reference + "," + items + "," + back_url + "," + payment_methods + "," + sponsor_id + "," + payer + "," + notification_url + "}";
            }
            else
                strpreference = "{" + external_reference + "," + items + "," + back_url + "," + payment_methods + "," + payer + "," + notification_url + "}";

            if (mercadoPagoPaymentSettings.UseLog)
                _logger.Information(string.Format("Mercado Pago Send: {0}", strpreference));

            Hashtable preference = mp.createPreference(strpreference);
            if (preference != null && mercadoPagoPaymentSettings.UseLog)
            {
                _logger.Information(string.Format("preference[Response] mercado pago:{0}", preference["response"].ToString()));
                _logger.Information(string.Format("preference[Status] mercado pago:{0}", preference["status"].ToString()));
            }
            else
            {
                _logger.Information(string.Format("Error mercado pago:{0}", "Error al crear Preference"));
            }
            int intstatus = (int)preference["status"];
            if (intstatus == 201 || intstatus == 200)
            {
                string idMP = (string)((Hashtable)preference["response"])["id"];
                if (!string.IsNullOrEmpty(idMP))
                {
                    order.CaptureTransactionId = idMP;
                    _orderService.UpdateOrder(order);
                }
                string init_point = (string)((Hashtable)preference["response"])["init_point"];

                return Content("<html><script>window.top.location.href = '" + init_point + "'; </script><body onload=\"window.history.forward()\"></body></html>");
            }
            else
            {
                string error = (string)((Hashtable)preference["response"])["message"];
                if (!string.IsNullOrEmpty(error))
                    _logger.InsertLog(LogLevel.Error, "Mercado Pago Error", error);

                var model = new MPActionModel()
                {
                    mp_mode = "redirect",
                    orderId = order.Id,
                    IpnUrl = mercadoPagoPaymentSettings.IpnUrl,
                    EnableIpn = mercadoPagoPaymentSettings.EnableIpn,
                };

                return View("~/Plugins/Payments.MercadoPago/Views/PaymentMercadoPago/MPAction.cshtml", model);
            }
        }

        public ActionResult Error()
        {
            return View("~/Plugins/Payments.MercadoPago/Views/PaymentMercadoPago/Error.cshtml");
        }

        public ActionResult Success(int? oId, Guid? oG)
        {
            Order order = null;
            string RedirectUrl = _webHelper.GetStoreLocation() + "Plugins/PaymentMercadoPago/Error";

            if (oId.HasValue && oG.HasValue)
            {
                //load order by identifier (if provided)
                order = _orderService.GetOrderById(oId.Value);

                if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId || order.OrderGuid != oG.Value)
                {
                    return new RedirectResult(RedirectUrl);
                }

                //order note
                order.OrderNotes.Add(new OrderNote()
                {
                    Note = "Orden Completada (Success)",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });

                _orderService.UpdateOrder(order);
                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });                
            }
            else
                return new RedirectResult(RedirectUrl);
        }

        public ActionResult Failure(int? oId, Guid? oG)
        {
            Order order = null;
            string RedirectUrl = _webHelper.GetStoreLocation() + "Plugins/PaymentMercadoPago/Error";

            if (oId.HasValue && oG.HasValue)
            {
                //load order by identifier (if provided)
                order = _orderService.GetOrderById(oId.Value);

                if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId || order.OrderGuid != oG.Value)
                {
                    return new RedirectResult(RedirectUrl);
                }

                //order note
                order.OrderNotes.Add(new OrderNote()
                {
                    Note = "Orden Cancelada (Failure)",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);

                if (_orderProcessingService.CanCancelOrder(order))//Cancela Orden                
                    _orderProcessingService.CancelOrder(order, false);

                //model
                GenericModel model = new GenericModel() { orderId = order.Id };                
                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
            }
            else
                return new RedirectResult(RedirectUrl);

        }

        public ActionResult Pending(int? oId, Guid? oG)
        {
            Order order = null;
            string RedirectUrl = _webHelper.GetStoreLocation() + "Plugins/PaymentMercadoPago/Error";

            if (oId.HasValue && oG.HasValue)
            {
                //load order by identifier (if provided)
                order = _orderService.GetOrderById(oId.Value);

                if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId || order.OrderGuid != oG.Value)
                {
                    return new RedirectResult(RedirectUrl);
                }
                //order note
                order.OrderNotes.Add(new OrderNote()
                {
                    Note = "Orden Pendiente de Pago (Pending)",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });

                _orderService.UpdateOrder(order);
                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });                
            }
            else
            {
                return new RedirectResult(RedirectUrl);
            }
        }
        
        [ValidateInput(false)]
        public ActionResult IPN()
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var mercadoPagoPaymentSettings = _settingService.LoadSetting<MercadoPagoPaymentSettings>(storeScope);

            string RedirectUrl = _webHelper.GetStoreLocation() + "Plugins/PaymentMercadoPago/Error.cshtml";
            if (!mercadoPagoPaymentSettings.EnableIpn)
                return new RedirectResult(RedirectUrl);

            if (Request["topic"] == "")            
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);            
            
            string id = string.Empty;
            if (Request["topic"] == "payment")            
                id = Request["id"];                           

            if (Request["type"] == "payment")            
                id = Request["data.id"];

            MP mp = new MP(mercadoPagoPaymentSettings.client_id, mercadoPagoPaymentSettings.client_secret);
            if (mercadoPagoPaymentSettings.UseLog)
                _logger.Information(string.Format("Data Id IPN mercado pago:{0}", id));

            if (id == "" || id == "12345")
                return new HttpStatusCodeResult(HttpStatusCode.OK);

            if (Request["topic"] == "payment" || Request["type"] == "payment")
            {                                                                    
                // Get the payment reported by the IPN. Glossary of attributes response in https://developers.mercadopago.com
                Hashtable payment_info = mp.getPaymentInfo(id);
                
                Hashtable response = (Hashtable)payment_info["response"];
                if (mercadoPagoPaymentSettings.UseLog)
                {
                    _logger.Information(string.Format("PaymentInfo[Response] mercado pago:{0}", payment_info["response"].ToString()));
                    _logger.Information(string.Format("PaymentInfo[Status] mercado pago:{0}", payment_info["status"].ToString()));
                }                
                // Show payment information
                if ((int)payment_info["status"] == 200 || (int)payment_info["status"] == 201)
                {                    
                    if (response != null)
                    {
                        Hashtable collection = (Hashtable)response["collection"];
                        var sb = new StringBuilder();
                        sb.AppendLine("Mercado Pago IPN:");
                        sb.AppendLine("id: " + collection["id"]);
                        sb.AppendLine("orderId: " + collection["order_id"]);
                        sb.AppendLine("external_reference: " + collection["external_reference"]);
                        sb.AppendLine("status: " + collection["status"]);
                        sb.AppendLine("Pending reason: " + collection["status_detail"]);
                        sb.AppendLine("transaction_amount: " + collection["transaction_amount"]);
                        _logger.Error(string.Format("Mensaje IPN mercado pago:{0}", sb));

                        if (!string.IsNullOrEmpty(collection["external_reference"].ToString()))
                        {
                            string external_reference = collection["external_reference"].ToString();

                            if (!string.IsNullOrEmpty(external_reference))
                            {
                                var order = _orderService.GetOrderByGuid(Guid.Parse(external_reference));
                                if (order == null)
                                {
                                    external_reference = collection["order_id"].ToString();
                                    order = _orderService.GetOrderByGuid(Guid.Parse(external_reference));
                                }
                                if (order != null)
                                {
                                    //order note
                                    order.OrderNotes.Add(new OrderNote()
                                    {
                                        Note = sb.ToString(),
                                        DisplayToCustomer = false,
                                        CreatedOnUtc = DateTime.UtcNow
                                    });
                                    _orderService.UpdateOrder(order);

                                    string status = collection["status"].ToString();
                                    if (!string.IsNullOrEmpty(status))
                                    {
                                        status = status.ToLower();
                                        switch (status)
                                        {
                                            case "approved":
                                                if (_orderProcessingService.CanMarkOrderAsPaid(order))
                                                {
                                                    order.AuthorizationTransactionCode = collection["order_id"].ToString();
                                                    order.AuthorizationTransactionResult = status;
                                                    order.AuthorizationTransactionId = collection["id"].ToString();
                                                    _orderService.UpdateOrder(order);
                                                    _orderProcessingService.MarkOrderAsPaid(order);
                                                }
                                                break;
                                            case "refunded":
                                                if (_orderProcessingService.CanCancelOrder(order))                                                        
                                                    _orderProcessingService.CancelOrder(order, false);                                                        
                                                break;

                                            case "cancelled":
                                                if (_orderProcessingService.CanCancelOrder(order))                                                        
                                                    _orderProcessingService.CancelOrder(order, false);                                                        
                                                break;
                                            default:
                                                order.OrderNotes.Add(new OrderNote()
                                                {
                                                    Note = "Aviso de Cambio IPN " + status,
                                                    DisplayToCustomer = false,
                                                    CreatedOnUtc = DateTime.UtcNow
                                                });
                                                _orderService.UpdateOrder(order);
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    return new HttpStatusCodeResult(HttpStatusCode.OK);  // regresa que proceso bien la respuesta            
                }
                else
                     return new HttpStatusCodeResult((int)payment_info["status"]);
            }

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        protected List<CountrySetting> MakeListCountrySetting()
        {
            var countrySetting = new List<CountrySetting>();

            countrySetting.Add(new CountrySetting()
            {
                CountryId = "MLM",
                CountryName = "Mexico",
                SponsorId = "275165500",
                Moneda = "MXN",
                PaymentMethods = new List<PaymentMethodSetting>()
                {
                    new PaymentMethodSetting()
                    { 
                        MethodId ="visa",
                        Name = "Visa",
                        PaymentTypeId="credit_card"
                    },
                    new PaymentMethodSetting()
                    { 
                        MethodId ="amex",
                        Name = "American Express",
                        PaymentTypeId="credit_card"
                    },
                    new PaymentMethodSetting()
                    { 
                        MethodId ="master",
                        Name = "Mastercard",
                        PaymentTypeId="Mastercard"
                    },
                    new PaymentMethodSetting()
                    { 
                        MethodId ="bancomer",
                        Name = "BBVA Bancomer",
                        PaymentTypeId = "atm"
                    },
                    new PaymentMethodSetting()
                    { 
                        MethodId ="banamex",
                        Name = "Banamex",
                        PaymentTypeId = "atm"
                    },
                    new PaymentMethodSetting()
                    { 
                        MethodId ="serfin",
                        Name = "Santander",
                        PaymentTypeId="atm"
                    },
                    new PaymentMethodSetting()
                    { 
                        MethodId ="oxxo",
                        Name = "Oxxo",
                        PaymentTypeId = "ticket"
                    },
                    new PaymentMethodSetting()
                    { 
                        MethodId ="account_money",
                        Name = "Dinero en mi cuenta de MercadoPago",
                        PaymentTypeId="account_money"
                    },
                    new PaymentMethodSetting()
                    { 
                        MethodId ="debmaster",
                        Name = "Mastercard Débito",
                        PaymentTypeId = "debit_card"
                    },
                    new PaymentMethodSetting()
                    { 
                        MethodId ="debvisa",
                        Name = "Visa Débito",
                        PaymentTypeId = "debit_card"
                    },
                    new PaymentMethodSetting()
                    { 
                        MethodId ="mercadopagocard",
                        Name = "Tarjeta MercadoPago",
                        PaymentTypeId = "prepaid_card"
                    }                    
                }
            });


            countrySetting.Add(new CountrySetting()
            {
                CountryId = "MLA",
                CountryName = "Argentina",
                SponsorId = "279863060",
                Moneda = "ARS",
                PaymentMethods = new List<PaymentMethodSetting>()
                {
                    new PaymentMethodSetting()
                    { 
                        MethodId ="master",
                        Name = "Mastercard",
                        PaymentTypeId="credit_card"
                    },   
                    new PaymentMethodSetting()
                    { 
                        MethodId = "amex",
                        Name = "American Express",
                        PaymentTypeId = "credit_card"
                    },   
                    new PaymentMethodSetting()
                    { 
                        MethodId = "mercadopago_cc",
                        Name = "Mercado Pago + Banco Patagonia",
                        PaymentTypeId = "credit_card"
                    },   
                    new PaymentMethodSetting()
                    { 
                        MethodId = "naranja",
                        Name = "Naranja",
                        PaymentTypeId = "credit_card"
                    },   
                    new PaymentMethodSetting()
                    { 
                        MethodId = "nativa",
                        Name = "Nativa Mastercard",
                        PaymentTypeId = "credit_card"
                    },   
                    new PaymentMethodSetting()
                    { 
                        MethodId = "cabal",
                        Name = "Cabal",
                        PaymentTypeId = "credit_card",
                    },   
                    new PaymentMethodSetting()
                    { 
                        MethodId = "tarshop",
                        Name = "Tarjeta Shopping",
                        PaymentTypeId= "credit_card",
                    },
                    new PaymentMethodSetting()
                    { 
                        MethodId = "cencosud",
                        Name = "Cencosud",
                        PaymentTypeId= "credit_card",
                    },
                    new PaymentMethodSetting()
                    { 
                        MethodId = "diners",
                        Name = "Diners",
                        PaymentTypeId= "credit_card",
                    },
                    new PaymentMethodSetting()
                    { 
                        MethodId = "pagofacil",
                        Name = "Pago Fácil",
                        PaymentTypeId= "ticket",
                    },
                    new PaymentMethodSetting()
                    { 
                        MethodId = "argencard",
                        Name = "Argencard",
                        PaymentTypeId= "credit_card",
                    },
                    new PaymentMethodSetting()
                    { 
                        MethodId = "maestro",
                        Name = "Maestro",
                        PaymentTypeId= "debit_card",
                    },
                    new PaymentMethodSetting()
                    { 
                        MethodId = "debmaster",
                        Name = "Mastercard Débito",
                        PaymentTypeId= "debit_card",
                    },
                    new PaymentMethodSetting()
                    { 
                        MethodId = "debcabal",
                        Name = "Cabal Débito",
                        PaymentTypeId= "debit_card",
                    },
                    new PaymentMethodSetting()
                    { 
                        MethodId = "rapipago",
                        Name = "Rapipago",
                        PaymentTypeId= "ticket"
                    },
                     new PaymentMethodSetting()
                    { 
                        MethodId = "redlink",
                        Name = "Red Link",
                        PaymentTypeId= "atm"
                    },
                     new PaymentMethodSetting()
                    { 
                        MethodId = "bapropagos",
                        Name = "Provincia NET",
                        PaymentTypeId= "ticket",
                    },
                     new PaymentMethodSetting()
                    { 
                        MethodId = "cargavirtual",
                        Name = "Kioscos y comercios cercanos",
                        PaymentTypeId= "ticket",
                    },
                     new PaymentMethodSetting()
                    { 
                        MethodId = "cordobesa",
                        Name = "Cordobesa",
                        PaymentTypeId= "credit_card",
                    },
                     new PaymentMethodSetting()
                    { 
                        MethodId = "cordial",
                        Name = "Tarjeta Walmart",
                        PaymentTypeId= "credit_card",
                    },
                    new PaymentMethodSetting()
                    { 
                        MethodId = "cmr",
                        Name = "CMR",
                        PaymentTypeId= "credit_card",
                    },
                    new PaymentMethodSetting()
                    { 
                        MethodId = "cobroexpress",
                        Name = "CobroExpress",
                        PaymentTypeId= "ticket",
                    },
                }
            });

            countrySetting.Add(new CountrySetting()
            {
                CountryId = "MLB",
                CountryName = "Brazil",
                SponsorId = "279869774",
                Moneda = "BRL",
                PaymentMethods = new List<PaymentMethodSetting>()
                {
                    new PaymentMethodSetting()
                    { 
                        MethodId = "visa",
                        Name = "Visa",
                        PaymentTypeId= "credit_card",
                    },
                    new PaymentMethodSetting()
                    { 
                        MethodId = "master",
                        Name = "Mastercard",
                        PaymentTypeId= "credit_card",
                    },
                    new PaymentMethodSetting()
                    { 
                        MethodId = "hipercard",
                        Name = "Hipercard",
                        PaymentTypeId= "credit_card",
                    },
                    new PaymentMethodSetting()
                    { 
                        MethodId = "amex",
                        Name = "American Express",
                        PaymentTypeId= "credit_card",
                    },
                    new PaymentMethodSetting()
                    { 
                        MethodId = "diners",
                        Name = "Diners",
                        PaymentTypeId= "credit_card",
                    },new PaymentMethodSetting()
                    { 
                        MethodId = "elo",
                        Name = "Elo",
                        PaymentTypeId= "credit_card",
                    },new PaymentMethodSetting()
                    { 
                        MethodId = "melicard",
                        Name = "Cartão MercadoLivre",
                        PaymentTypeId= "credit_card"
                    },new PaymentMethodSetting()
                    { 
                        MethodId = "bolbradesco",
                        Name = "Boleto",
                        PaymentTypeId= "ticket",
                    },
                }
            });

            countrySetting.Add(new CountrySetting()
            {
                CountryId = "MLC",
                CountryName = "Chile",
                SponsorId = "279870118",
                Moneda = "CLP",
                PaymentMethods = new List<PaymentMethodSetting>()
                {
                    new PaymentMethodSetting()
                    { 
                        MethodId = "visa",
                        Name = "Visa",
                        PaymentTypeId = "credit_card",
                    }, 
                    new PaymentMethodSetting()
                    { 
                        MethodId = "master",
                        Name = "Mastercard",
                        PaymentTypeId = "credit_card",
                    },
                    new PaymentMethodSetting()
                    { 
                        MethodId = "amex",
                        Name = "American Express",
                        PaymentTypeId = "credit_card",
                    },
                    new PaymentMethodSetting()
                    { 
                        MethodId = "magna",
                        Name = "Magna",
                        PaymentTypeId = "credit_card",
                    },
                    new PaymentMethodSetting()
                    { 
                        MethodId = "presto",
                        Name = "Presto",
                        PaymentTypeId = "credit_card",
                    },
                    new PaymentMethodSetting()
                    { 
                        MethodId = "cmr",
                        Name = "CMR",
                        PaymentTypeId = "credit_card"
                    },
                    new PaymentMethodSetting()
                    { 
                        MethodId = "diners",
                        Name = "Diners",
                        PaymentTypeId = "credit_card",
                    },
                    new PaymentMethodSetting()
                    { 
                        MethodId = "servipag",
                        Name = "Sucursales Servipag",
                        PaymentTypeId = "ticket",
                    },
                    new PaymentMethodSetting()
                    { 
                        MethodId = "webpay",
                        Name = "RedCompra (Webpay)",
                        PaymentTypeId = "bank_transfer",
                    },                    
                }
            });

            countrySetting.Add(new CountrySetting()
            {
                CountryId = "MCO",
                CountryName = "Colombia",
                SponsorId = "279874054",
                Moneda = "COP",
                PaymentMethods = new List<PaymentMethodSetting>()
                {
                    new PaymentMethodSetting()
                    { 
                        MethodId = "visa",
                        Name = "Visa",
                        PaymentTypeId = "credit_card",
                    }, 
                    new PaymentMethodSetting()
                    { 
                        MethodId = "amex",
                        Name = "American Express",
                        PaymentTypeId = "credit_card",
                    },
                    new PaymentMethodSetting()
                    { 
                        MethodId = "master",
                        Name = "Mastercard",
                        PaymentTypeId = "credit_card",
                    },
                    new PaymentMethodSetting()
                    { 
                        MethodId = "diners",
                        Name = "Diners",
                        PaymentTypeId = "credit_card",
                    },
                    new PaymentMethodSetting()
                    { 
                        MethodId = "efecty",
                        Name = "Efecty",
                        PaymentTypeId = "ticket",
                    },
                    new PaymentMethodSetting()
                    { 
                        MethodId = "pse",
                        Name = "PSE",
                        PaymentTypeId = "bank_transfer",
                    },
                    new PaymentMethodSetting()
                    { 
                        MethodId = "codensa",
                        Name = "Crédito Fácil Codensa",
                        PaymentTypeId = "credit_card",
                    },
                    new PaymentMethodSetting()
                    { 
                        MethodId = "davivienda",
                        Name = "Davivienda",
                        PaymentTypeId = "ticket",
                    },                    
                }
            });

            countrySetting.Add(new CountrySetting()
            {
                CountryId = "MLV",
                CountryName = "Venezuela",
                SponsorId = "279874709",
                Moneda = "VEF",
                PaymentMethods = new List<PaymentMethodSetting>()
                {
                    new PaymentMethodSetting()
                    { 
                        MethodId = "visa",
                        Name = "Visa",
                        PaymentTypeId = "credit_card",
                    }, 
                    new PaymentMethodSetting()
                    { 
                        MethodId = "master",
                        Name = "Mastercard",
                        PaymentTypeId = "credit_card",
                    }, 
                    new PaymentMethodSetting()
                    { 
                        MethodId = "provincial",
                        Name = "BBVA Provincial",
                        PaymentTypeId = "ticket",
                    }, 
                    new PaymentMethodSetting()
                    { 
                        MethodId = "mercantil",
                        Name = "Banco Mercantil",
                        PaymentTypeId = "atm",
                    }, 
                    new PaymentMethodSetting()
                    { 
                        MethodId = "banesco",
                        Name = "Banco Banesco",
                        PaymentTypeId = "ticket",
                    },                                         
                }
            });

            countrySetting.Add(new CountrySetting()
            {
                CountryId = "MPE",
                CountryName = "Peru",
                SponsorId = "279872580",
                Moneda = "PEN",
                PaymentMethods = new List<PaymentMethodSetting>()
                {
                    new PaymentMethodSetting()
                    { 
                        MethodId = "visa",
                        Name = "Visa",
                        PaymentTypeId = "credit_card",
                    },  
                    new PaymentMethodSetting()
                    { 
                        MethodId = "debvisa",
                        Name = "Visa Débito",
                        PaymentTypeId = "debit_card",
                    },    
                    new PaymentMethodSetting()
                    { 
                        MethodId = "pagoefectivo_atm",
                        Name = "BCP, BBVA Continental u otros",
                        PaymentTypeId = "atm",
                    },    
                    new PaymentMethodSetting()
                    { 
                        MethodId = "pruebasetting",
                        Name = "Prueba Setting 2",
                        PaymentTypeId = "credit_card",
                    },    
                    new PaymentMethodSetting()
                    { 
                        MethodId = "master",
                        Name = "Mastercard",
                        PaymentTypeId = "credit_card",
                    },    
                    new PaymentMethodSetting()
                    { 
                        MethodId = "amex",
                        Name = "American Express",
                        PaymentTypeId = "credit_card",
                    },    
                    new PaymentMethodSetting()
                    { 
                        MethodId = "debmaster",
                        Name = "Mastercard Débito",
                        PaymentTypeId = "debit_card"
                    }                       
                }
            });
            return countrySetting;
        }

        protected CountrySetting CountrySettingByCountryId(string countryId)
        {
            var configListPM = MakeListCountrySetting();
            CountrySetting sConfigPM = new CountrySetting();
            sConfigPM = configListPM.Where(cPM => cPM.CountryId == countryId).First();
            return sConfigPM;
        }


        [PublicStoreAllowNavigation(true)]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetCountrySettingByCountryId(string countryId, bool addSelectStateItem)
        {
            CountrySetting sConfigPM = CountrySettingByCountryId(countryId);            
            
            CountrySettingModel model = new CountrySettingModel()
            {
                CountryId = sConfigPM.CountryId,
                CountryName = sConfigPM.CountryName,
                Moneda = sConfigPM.Moneda,
                SponsorId = sConfigPM.SponsorId,
                PaymentMethods = sConfigPM.PaymentMethods,
            };
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}
