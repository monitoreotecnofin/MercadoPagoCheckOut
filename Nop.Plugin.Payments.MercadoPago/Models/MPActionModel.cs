using Nop.Web.Framework.Mvc.Models;

namespace Nop.Plugin.Payments.MercadoPago.Models
{
    public class MPActionModel : BaseNopModel
    {
        public string mp_mode { get; set; }

        public string initpoint { get; set; }

        public int orderId { get; set; }

        public bool EnableIpn { get; set; }

        public string IpnUrl { get; set; }

    }
}
