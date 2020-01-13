
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Localization;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.MercadoPago.Components
{
    [ViewComponent(Name = "PaymentMercadoPago")]
    public class PaymentMercadoPagoViewComponet : NopViewComponent
    {
        #region Fields
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        #endregion

        #region Ctor

        public PaymentMercadoPagoViewComponet(ILocalizationService localizationService,
            IWorkContext workContext)
        {
            this._localizationService = localizationService;
            this._workContext = workContext;
        }

        #endregion

        public IViewComponentResult Invoke()
        {
            return View("~/Plugins/Payments.MercadoPago/Views/PaymentInfo.cshtml");
        }
    }
}
