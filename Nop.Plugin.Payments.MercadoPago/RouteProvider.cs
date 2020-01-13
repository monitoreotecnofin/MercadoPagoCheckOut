using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using System.Web;

namespace Nop.Plugin.Payments.MercadoPago
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {            
            routeBuilder.MapRoute("Plugin.Payments.MercadoPago.MPAction",
                 "Plugins/PaymentMercadoPago/MPAction",
                 new { controller = "PaymentMercadoPago", action = "MPAction" });

            routeBuilder.MapRoute("Plugin.Payments.MercadoPago.Error",
                 "Plugins/PaymentMercadoPago/Error",
                 new { controller = "PaymentMercadoPago", action = "Error" });

            routeBuilder.MapRoute("Plugin.Payments.MercadoPago.IPN",
                 "Plugins/PaymentMercadoPago/IPN",
                 new { controller = "PaymentMercadoPago", action = "IPN" });

            routeBuilder.MapRoute("Plugin.Payments.MercadoPago.Success",
                "Plugins/PaymentMercadoPago/Success/{oId?}/{oG?}",
                new { controller = "PaymentMercadoPago", action = "Success" });
            //new { oId = @"\d+", oG = @"^(\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\}{0,1})$" });

            routeBuilder.MapRoute("Plugin.Payments.MercadoPago.Failure",
                "Plugins/PaymentMercadoPago/Failure/{oId?}/{oG?}",
                new { controller = "PaymentMercadoPago", action = "Failure" }); //,
            //new { oId = @"\d+", oG = @"^(\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\}{0,1})$" });

            routeBuilder.MapRoute("Plugin.Payments.MercadoPago.Pending",
                "Plugins/PaymentMercadoPago/Pending/{oId?}/{oG?}",
                new { controller = "PaymentMercadoPago", action = "Pending" }); //,
                //new { oId = @"\d+", oG = @"^(\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\}{0,1})$" });
            /*
            routeBuilder.MapRoute("Plugin.Payments.MercadoPago.Success",
                "Plugins/PaymentMercadoPago/Success/{oId:regex(\\d+)}/{oG:regex(^\\{{{{0,1}}([0-9a-fA-F]){{8}}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){{4}}-([0-9a-fA-F]){12}\\}}{{0,1}}$)}",
                new { controller = "PaymentMercadoPago", action = "Success" });

            routeBuilder.MapRoute("Plugin.Payments.MercadoPago.Failure",
                "Plugins/PaymentMercadoPago/Failure/{oId:regex(\\d+)}/{oG:regex(^\\{{{{0,1}}([0-9a-fA-F]){{8}}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){{4}}-([0-9a-fA-F]){12}\\}}{{0,1}}$)}",
                new { controller = "PaymentMercadoPago", action = "Failure" });

            routeBuilder.MapRoute("Plugin.Payments.MercadoPago.Pending",
                "Plugins/PaymentMercadoPago/Pending/{oId:regex(\\d+)}/{oG:regex(^\\{{{{0,1}}([0-9a-fA-F]){{8}}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){{4}}-([0-9a-fA-F]){12}\\}}{{0,1}}$)}",
                new { controller = "PaymentMercadoPago", action = "Pending" });
                */

            //get countrysetting by country ID  (AJAX link)
            routeBuilder.MapRoute("GetCountrySettingByCountryId",
                            "Plugins/PaymentMercadoPago/GetCountrySettingByCountryId/",
                            new { controller = "PaymentMercadoPago", action = "GetCountrySettingByCountryId" });

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
