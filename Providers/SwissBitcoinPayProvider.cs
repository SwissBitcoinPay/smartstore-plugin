using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Smartstore.Core;
using Smartstore.Core.Checkout.Cart;
using Smartstore.Core.Checkout.Orders;
using Smartstore.Core.Checkout.Payment;
using Smartstore.Core.Common.Services;
using Smartstore.Core.Configuration;
using Smartstore.Core.Identity;
using Smartstore.Core.Widgets;
using Smartstore.Engine.Modularity;
using Smartstore.Http;
using Smartstore.SwissBitcoinPay.Components;
using Smartstore.SwissBitcoinPay.Controllers;
using SmartStore.SwissBitcoinPay.Models;
using SmartStore.SwissBitcoinPay.Services;
using SmartStore.SwissBitcoinPay.Settings;

namespace Smartstore.SwissBitcoinPay.Providers
{
    [SystemName("SmartStore.SwissBitcoinPay")]
    [FriendlyName("SwissBitcoinPay")]
    [Order(1)]
    public class PaymentProvider : PaymentMethodBase, IConfigurable
    {

        private readonly ICommonServices _services;
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly ILocalizationService _localizationService;
        private readonly ICurrencyService _currencyService;
        private readonly ISettingFactory _settingFactory;

        public PaymentProvider(
            ICommonServices services,
            ILocalizationService localizationService,
            ICustomerService customerService,
            ISettingFactory settingFactory,
            ICurrencyService currencyService,
            IOrderService orderService)
        {
            _localizationService = localizationService;
            _services = services;
            _currencyService = currencyService;
            _customerService = customerService;
            _settingFactory = settingFactory;
            _orderService = orderService;
        }

        public ILogger Logger { get; set; } = NullLogger.Instance;

        public static string SystemName => "SmartStore.SwissBitcoinPay";
        public override bool SupportCapture => false;
        public override bool SupportPartiallyRefund => false;
        public override bool SupportRefund => false;
        public override bool SupportVoid => false;
        public override bool RequiresInteraction => false;
        public override PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        public RouteInfo GetConfigurationRoute()
            => new(nameof(SwissBitcoinPayAdminController.Configure), "SwissBitcoinPay", new { area = "Admin" });


        public override Widget GetPaymentInfoWidget()
            => new ComponentWidget(typeof(SwissBitcoinPayViewComponent));

        public override async Task<(decimal FixedFeeOrPercentage, bool UsePercentage)> GetPaymentFeeInfoAsync(ShoppingCart cart)
        {
            var settings = await _settingFactory.LoadSettingsAsync<SwissBitcoinPaySettings>(_services.StoreContext.CurrentStore.Id);
            return (settings.AdditionalFee, settings.AdditionalFeePercentage);
        }

        public override async Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.NewPaymentStatus = PaymentStatus.Pending;

            try
            {

                var myStore = _services.StoreContext.CurrentStore;
                var settings = await _settingFactory.LoadSettingsAsync<SwissBitcoinPaySettings>(myStore.Id);
                var myCustomer = await _customerService.GetAuthenticatedCustomerAsync();

                var apiService = new SwissBitcoinPayService();
                result.AuthorizationTransactionResult = apiService.CreateInvoice(settings, new PaymentDataModel()
                {
                    CurrencyCode = _currencyService.PrimaryCurrency.CurrencyCode,
                    Amount = processPaymentRequest.OrderTotal,
                    BuyerEmail = "" + myCustomer.Email,
                    BuyerName = myCustomer.FirstName + " " + myCustomer.LastName,
                    OrderID = processPaymentRequest.OrderGuid.ToString(),
                    StoreID = myStore.Id,
                    CustomerID = myCustomer.Id,
                    Description = "From " + myStore.Name,
                    RedirectionURL = myStore.Url + "checkout/completed",
                    Lang = _services.WorkContext.WorkingLanguage.LanguageCulture,
                    WebHookURL = myStore.Url + "SbpHook/Process"
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw new PaymentException(ex.Message);
            }

            return await Task.FromResult(result);
        }

        public override Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            if (postProcessPaymentRequest.Order.PaymentStatus == PaymentStatus.Pending)
            {
                // Specify redirection URL here if your provider is of type PaymentMethodType.Redirection.
                // Core redirects to this URL automatically.
                postProcessPaymentRequest.RedirectUrl = postProcessPaymentRequest.Order.AuthorizationTransactionResult;
            }
            return Task.CompletedTask;
        }
    }
}
