using Microsoft.AspNetCore.Mvc;
using Smartstore.Web.Components;

namespace Smartstore.SwissBitcoinPay.Components
{
    public class SwissBitcoinPayViewComponent : SmartViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("~/Modules/SmartStore.SwissBitcoinPay/Views/Public/PaymentInfo.cshtml");
        }
    }
}
