using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.MercadoPago
{
    public class MercadoPagoPaymentSettings : ISettings
    {
        public string countryId { get; set; }

        public bool UseLog { get; set; }

        public string client_id { get; set; }

        public string client_secret { get; set; }

        public string excluded_payment_methods { get; set; }

        public string excluded_payment_types { get; set; }

        public int AvailableDays { get; set; }

        public decimal AdditionalFee { get; set; }

        public bool AdditionalFeePercentage { get; set; }

        public bool ManejarTotal { get; set; }

        public bool EnableIpn { get; set; }

        public string IpnUrl { get; set; }

        public string IdTestIPN { get; set; }

    }
}
