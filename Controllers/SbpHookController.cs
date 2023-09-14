﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Smartstore.Core;
using Smartstore.Core.Checkout.Orders;
using Smartstore.Core.Checkout.Payment;
using Smartstore.Core.Data;
using Smartstore.SwissBitcoinPay.Providers;
using Smartstore.Web.Controllers;
using SmartStore.SwissBitcoinPay.Services;
using SmartStore.SwissBitcoinPay.Settings;

namespace Smartstore.SwissBitcoinPay.Controllers
{
    public class SbpHookController : PublicController
    {
        private readonly ILogger _logger;
        private readonly SmartDbContext _db;
        private readonly SwissBitcoinPaySettings _settings;

        public SbpHookController(IOrderService orderService,
            SwissBitcoinPaySettings settings,
            SmartDbContext db,
           ICommonServices services,
            ILogger logger)
        {
            _logger = logger;
            _db = db;
            _settings = settings;
        }

        [HttpPost]
        public async Task<IActionResult> Process([FromHeader(Name = "sbp-sig")] string SwissBtcPaySig)
        {
            string jsonStr = "";
            try
            {
                jsonStr = await new StreamReader(Request.Body).ReadToEndAsync();
                dynamic jsonData = JsonConvert.DeserializeObject(jsonStr);
                var SwissBtcPaySecret = SwissBtcPaySig.Split('=')[1];

                string Description = jsonData.description;
                var tblDescription = Description.Split("#");
                string OrderGuid = tblDescription[1].Split(":")[1].Trim();
                int StoreID = Convert.ToInt32(tblDescription[2].Split(":")[1].Trim());

                if (String.IsNullOrEmpty(OrderGuid) || StoreID == 0)
                {
                    Logger.Error("Missing fields in request");
                    return StatusCode(StatusCodes.Status422UnprocessableEntity);
                }

                if (!SwissBitcoinPayService.CheckSecretKey(_settings.ApiSecret, jsonStr, SwissBtcPaySecret))
                {
                    Logger.Error("Bad secret key");
                    return StatusCode(StatusCodes.Status400BadRequest);
                }


                bool IsPaid = jsonData.isPaid;
                bool IsExpired = jsonData.isExpired;

                var order = await _db.Orders.FirstOrDefaultAsync(x =>
                    x.PaymentMethodSystemName == PaymentProvider.SystemName &&
                    x.OrderGuid == new Guid(OrderGuid));
                if (order == null)
                {
                    Logger.Error("Missing order");
                    return StatusCode(StatusCodes.Status422UnprocessableEntity);
                }


                if (IsPaid) order.PaymentStatus = PaymentStatus.Paid;
                if (IsExpired)
                {
                    if (order.PaymentStatus != PaymentStatus.Paid)
                        order.PaymentStatus = PaymentStatus.Voided;
                }
                order.HasNewPaymentNotification = true;
                order.AddOrderNote("PaymentStatus: " + order.PaymentStatus.ToString(), true);

                await _db.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                Logger.Error($"{jsonStr} - {ex.Message}");
                return StatusCode(StatusCodes.Status400BadRequest);
            }
        }

    }
}