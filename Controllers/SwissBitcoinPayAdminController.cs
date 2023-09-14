using Microsoft.AspNetCore.Mvc;
using Smartstore.ComponentModel;
using Smartstore.Core.Common.Services;
using Smartstore.Core.Security;
using Smartstore.Engine.Modularity;
using Smartstore.Web.Controllers;
using Smartstore.Web.Modelling.Settings;
using SmartStore.SwissBitcoinPay.Models;
using SmartStore.SwissBitcoinPay.Settings;

namespace Smartstore.SwissBitcoinPay.Controllers
{
    [Area("Admin")]
    [Route("[area]/swissbitcoinpay/{action=index}/{id?}")]
    public class SwissBitcoinPayAdminController : ModuleController
    {

        private readonly IProviderManager _providerManager;
        private readonly ICurrencyService _currencyService;

        public SwissBitcoinPayAdminController(IProviderManager providerManager, ICurrencyService currencyService)
        {
            _providerManager = providerManager;
            _currencyService = currencyService;
        }

        [LoadSetting, AuthorizeAdmin]
        public IActionResult Configure(int storeId, SwissBitcoinPaySettings settings)
        {
            var model = MiniMapper.Map<SwissBitcoinPaySettings, ConfigurationModel>(settings);
            ViewBag.Provider = _providerManager.GetProvider("SmartStore.SwissBitcoinPay").Metadata;
            ViewBag.StoreCurrencyCode = _currencyService.PrimaryCurrency.CurrencyCode ?? "EUR";
            return View(model);
        }

        [HttpPost, SaveSetting, AuthorizeAdmin]
        public IActionResult Configure(int storeId, ConfigurationModel model, SwissBitcoinPaySettings settings)
        {
            if (!ModelState.IsValid)
            {
                return Configure(storeId, settings);
            }

            ModelState.Clear();
            MiniMapper.Map(model, settings);

            return RedirectToAction(nameof(Configure));
        }

    }
}