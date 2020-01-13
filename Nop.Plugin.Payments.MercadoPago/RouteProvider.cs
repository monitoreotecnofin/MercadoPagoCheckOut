using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Payments.PayPalStandard
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            //PDT
            routes.MapRoute("Plugin.Payments.MercadoPago.MPAction",
                 "Plugins/PaymentMercadoPago/MPAction",
                 new { controller = "PaymentMercadoPago", action = "MPAction" },
                 new[] { "Nop.Plugin.Payments.MercadoPago.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.MercadoPago.Error",
                 "Plugins/PaymentMercadoPago/Error",
                 new { controller = "PaymentMercadoPago", action = "Error" },
                 new[] { "Nop.Plugin.Payments.MercadoPago.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.MercadoPago.IPN",
                 "Plugins/PaymentMercadoPago/IPN",
                 new { controller = "PaymentMercadoPago", action = "IPN" },
                 new[] { "Nop.Plugin.Payments.MercadoPago.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.MercadoPago.Success",
                "Plugins/PaymentMercadoPago/Success/{oId}/{oG}",
                new { controller = "PaymentMercadoPago", action = "Success", oId = UrlParameter.Optional, oG = UrlParameter.Optional },
                new { oId = @"\d+", oG = @"^(\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\}{0,1})$" },
                new[] { "Nop.Plugin.Payments.MercadoPago.Controllers" });

            routes.MapRoute("Plugin.Payments.MercadoPago.Failure",
                "Plugins/PaymentMercadoPago/Failure/{oId}/{oG}",
                new { controller = "PaymentMercadoPago", action = "Failure", oId = UrlParameter.Optional, oG = UrlParameter.Optional },
                new { oId = @"\d+", oG = @"^(\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\}{0,1})$" },
                new[] { "Nop.Plugin.Payments.MercadoPago.Controllers" });

            routes.MapRoute("Plugin.Payments.MercadoPago.Pending",
                "Plugins/PaymentMercadoPago/Pending/{oId}/{oG}",
                new { controller = "PaymentMercadoPago", action = "Pending", oId = UrlParameter.Optional, oG = UrlParameter.Optional },
                new { oId = @"\d+", oG = @"^(\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\}{0,1})$" },
                new[] { "Nop.Plugin.Payments.MercadoPago.Controllers" });

            //get countrysetting by country ID  (AJAX link)
            routes.MapRoute("GetCountrySettingByCountryId",
                            "Plugins/PaymentMercadoPago/GetCountrySettingByCountryId/",
                            new { controller = "PaymentMercadoPago", action = "GetCountrySettingByCountryId" },
                            new[] { "Nop.Plugin.Payments.MercadoPago.Controllers" });

        }

        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
