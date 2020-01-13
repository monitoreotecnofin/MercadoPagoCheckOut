using Nop.Web.Framework.Models;
using System.Collections.Generic;

namespace Nop.Plugin.Payments.MercadoPago.Models
{
    public class CountrySettingModel : BaseNopModel
    {
        public CountrySettingModel()
        {
            PaymentMethods = new List<PaymentMethodSetting>();
        }
        public string CountryId { get; set; }

        public string CountryName { get; set; }

        public string Moneda { get; set; }

        public string SponsorId { get; set; }

        public List<PaymentMethodSetting> PaymentMethods { get; set; }
    }
}
