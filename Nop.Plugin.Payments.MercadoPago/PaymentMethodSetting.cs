using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.MercadoPago
{
    public class PaymentMethodSetting
    {
        public string MethodId { get; set; }

        public string Name { get; set; }

        public string PaymentTypeId { get; set; }
    }
}
