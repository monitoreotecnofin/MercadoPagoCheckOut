using Nop.Core.Plugins;
using Nop.Services.Payments;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Nop.Core.Domain.Orders;
using Nop.Services.Directory;
using Nop.Core.Domain.Directory;
using Nop.Core;
using Nop.Services.Orders;
using Nop.Services.Tax;
using Nop.Services.Localization;
using Nop.Services.Configuration;
using System.Web;
using Nop.Web.Framework;
using Nop.Core.Domain.Payments;

namespace Nop.Plugin.Payments.MercadoPago
{
    public class MercadoPagoPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields
        private readonly MercadoPagoPaymentSettings _mercadoPagoPaymentSettings;
        private readonly ISettingService _settingService;
        //private readonly ICurrencyService _currencyService;
        //private readonly CurrencySettings _currencySettings;
        private readonly IWebHelper _webHelper;
        //private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        //private readonly ITaxService _taxService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        //private readonly HttpContextBase _httpContext;
        private readonly ILocalizationService _localizationService;
        #endregion
        #region Ctor
        public MercadoPagoPaymentProcessor(ILocalizationService localizationService,
            IOrderTotalCalculationService orderTotalCalculationService,
            ISettingService settingService,
            IWebHelper webHelper,
            MercadoPagoPaymentSettings mercadoPagoPaymentSettings)
        {
            this._localizationService = localizationService;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._settingService = settingService;
            this._webHelper = webHelper;
            this._mercadoPagoPaymentSettings = mercadoPagoPaymentSettings;
        }
        
        #endregion
        #region Properties
        public bool SupportCapture
        {
            get
            {
                return false;
            }
        }

        public bool SupportPartiallyRefund
        {
            get
            {
                return false;
            }
        }

        public bool SupportRefund
        {
            get
            {
                return false;
            }
        }

        public bool SupportVoid
        {
            get
            {
                return false;
            }
        }

        public RecurringPaymentType RecurringPaymentType
        {
            get
            {
                return RecurringPaymentType.NotSupported;
            }
        }

        public PaymentMethodType PaymentMethodType
        {
            get
            {
                return PaymentMethodType.Redirection;
            }
        }

        public bool SkipPaymentInfo
        {
            get
            {
                return true;
            }
        }

        public string PaymentMethodDescription
        {
            //return description of this payment method to be display on "payment method" checkout step. good practice is to make it localizable
            //for example, for a redirection payment method, description may be like this: "You will be redirected to PayPal site to complete the payment"
            get { return _localizationService.GetResource("Plugins.Payments.MercadoPago.PaymentMethodDescription"); }
        }
        #endregion
        #region Methods
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            //let's ensure that at least 5 seconds passed after order is placed
            //P.S. there's no any particular reason for that. we just do it
            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalSeconds < 5)
                return false;

            return true;
        }

        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();
            result.AddError("Capture method not supported");
            return result;
        }

        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            var result = this.CalculateAdditionalFee(_orderTotalCalculationService, cart,
                _mercadoPagoPaymentSettings.AdditionalFee, _mercadoPagoPaymentSettings.AdditionalFeePercentage);

            return result;
        }

        public ProcessPaymentRequest GetPaymentInfo(IFormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();
            return paymentInfo;
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentMercadoPago/Configure";
        }

        public void GetPublicViewComponent(out string viewComponentName)
        {
            viewComponentName = "PaymentMercadoPago";
        }

        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return false;
        }

        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            //Recupera Informacion
            string orderid = Convert.ToString(postProcessPaymentRequest.Order.Id);

            //Redirect to IframePage
            RemotePost IDMRemotePost = new RemotePost();
            IDMRemotePost.FormName = "MercadoPago";
            IDMRemotePost.Url = _webHelper.GetStoreLocation() + "Plugins/PaymentMercadoPago/MPAction";
            IDMRemotePost.Add("orderId", orderid);
            IDMRemotePost.Post();

        }

        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.NewPaymentStatus = PaymentStatus.Pending;
            return result;
        }

        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            result.AddError("Refund method not supported");
            return result;
        }

        public IList<string> ValidatePaymentForm(IFormCollection form)
        {
            var warnings = new List<string>();
            return warnings;
        }

        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();
            result.AddError("Void method not supported");
            return result;
        }

        public override void Install()
        {
            //settings
            var settings = new MercadoPagoPaymentSettings()
            {
                countryId = "MLM",
                UseLog = true,
                client_id = "client_id",
                client_secret = "client_secret",
                AdditionalFee = 3.99M,
                AdditionalFeePercentage = true,
                IpnUrl = _webHelper.GetStoreLocation() + "Plugins/PaymentMercadoPago/IPN",
                EnableIpn = true,
                AvailableDays = 3,
                IdTestIPN = "12345",
            };
            _settingService.SaveSetting(settings);

            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.RedirectionTip", "Por este medio  puedes pagar en tiendas <strong>OXXO</strong>, hacer depósitos o transferencias bancarias. <br/>Serás redireccionado a Mercado Pago para finalizar el pedido.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.UseLog", "Usa Log");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.UseLog.Hint", "Habilita el Log.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.test_mode", "Habilita Sponsor");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.test_mode.Hint", "Habilita que el PlugIn fue descargado de NopCommerce");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.client_id", "Id de Cliente en MercadoPago");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.client_id.Hint", "Id de cliente que se obtuvo de mercado pago");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.client_secret", "Secreto");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.client_secret.Hint", "Cadena de caracteres secreta en MercadoPago");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.mp_mode", "Modo de MercadoPago");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.mp_mode.Hint", "Modo de desplegarse la ventana de Mercado Pago, modal, popup, blank, redirect");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.AdditionalFee", "Cargo adicional");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.AdditionalFee.Hint", "Monto para el cargo adicional");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.AdditionalFeePercentage", "Usar porcentaje");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.AdditionalFeePercentage.Hint", "Habilita si se requiere manejar por porcentaje");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.EnableIpn", "Habilita IPN");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.EnableIpn.Hint", "Habilita Instant Payment Notification");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.IpnUrl", "URL del IPN");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.IpnUrl.Hint", "URL del IPN");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.moneda", "Moneda: Argentina(ARS-USD)  Brasil(BRL) Mexico(MXN) Venezuela(VEF) Colombia(COP)");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.AvailableDays", "Dias disponibles para el pedido");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.Error", "Ha ocurrido un error en el proceso");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.Success", "Ha concluido el pedido de manera exitosa");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.Pending", "El Pedido se encuentra en proceso de confirmación del pago.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.Failure", "Ocurrio un error durante el proceso y el pedido fue Cancelado, por favor intente nuevamente.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.IdTestIPN", "Id de Test IPN");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.IdTestIPN.Hint", "Id Para configurar IPN");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.excluded_payment_methods", "Metodos de pago excluidos");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.excluded_payment_types", "Tipos de pago excluidos");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MercadoPago.PaymentMethodDescription", "Seras redireccionado a Mercado Pago para completar el pago.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.ManejarTotal", "Maneja solo Totales.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.ManejarTotal.Hint", "Maneja Totales no manda items.");

            base.Install();
        }

        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<MercadoPagoPaymentSettings>();

            //locales
            this.DeletePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.RedirectionTip");
            this.DeletePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.UseLog");
            this.DeletePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.UseLog.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.test_mode");
            this.DeletePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.test_mode.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.client_id");
            this.DeletePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.client_id.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.client_secret");
            this.DeletePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.client_secret.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.mp_mode");
            this.DeletePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.mp_mode.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.AdditionalFee");
            this.DeletePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.AdditionalFee.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.AdditionalFeePercentage");
            this.DeletePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.AdditionalFeePercentage.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.EnableIpn");
            this.DeletePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.EnableIpn.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.IpnUrl");
            this.DeletePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.IpnUrl.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.moneda");
            this.DeletePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.AvailableDays");
            this.DeletePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.Error");
            this.DeletePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.Success");
            this.DeletePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.Pending");
            this.DeletePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.Failure");

            this.DeletePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.excluded_payment_methods");
            this.DeletePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.excluded_payment_types");
            this.DeletePluginLocaleResource("Plugins.Payments.MercadoPago.PaymentMethodDescription");
            this.DeletePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.ManejarTotal");
            this.DeletePluginLocaleResource("Plugins.Payments.MercadoPago.Fields.ManejarTotal.Hint");
            base.Uninstall();
        }

        #endregion
    }
}
