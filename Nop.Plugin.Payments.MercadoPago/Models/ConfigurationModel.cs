using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Nop.Plugin.Payments.MercadoPago.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public ConfigurationModel()
        {
            this.AvailableCountries = new List<SelectListItem>();
        }
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Account.Fields.Country")]
        public string countryId { get; set; }
        public IList<SelectListItem> AvailableCountries { get; set; }
        public bool countryId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.MercadoPago.Fields.UseLog")]
        public bool UseLog { get; set; }
        public bool UseLog_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.MercadoPago.Fields.client_id")]
        public string client_id { get; set; }
        public bool client_id_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.MercadoPago.Fields.client_secret")]
        public string client_secret { get; set; }
        public bool client_secret_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.MercadoPago.Fields.excluded_payment_methods")]
        public string excluded_payment_methods { get; set; }
        public bool excluded_payment_methods_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.MercadoPago.Fields.excluded_payment_types")]
        public string excluded_payment_types { get; set; }
        public bool excluded_payment_types_OverrideForStore { get; set; }


        [NopResourceDisplayName("Plugins.Payments.MercadoPago.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.MercadoPago.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.MercadoPago.Fields.EnableIpn")]
        public bool EnableIpn { get; set; }
        public bool EnableIpn_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.MercadoPago.Fields.ManejarTotal")]
        public bool ManejarTotal { get; set; }
        public bool ManejarTotal_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.MercadoPago.Fields.IpnUrl")]
        public string IpnUrl { get; set; }
        public bool IpnUrl_OverrideForStore { get; set; }
        
        [NopResourceDisplayName("Plugins.Payments.MercadoPago.Fields.IdTestIPN")]
        public string IdTestIPN { get; set; }
        public bool IdTestIPN_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.MercadoPago.Fields.AvailableDays")]
        public int AvailableDays { get; set; }
        public bool AvailableDays_OverrideForStore { get; set; }

    }
}
