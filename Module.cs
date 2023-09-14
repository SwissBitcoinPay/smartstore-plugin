global using System;
global using System.ComponentModel.DataAnnotations;
global using System.Linq;
global using System.Threading.Tasks;
global using FluentValidation;
global using Smartstore.Core.Localization;
global using Smartstore.Web.Modelling;
using Smartstore.Engine.Modularity;
using SmartStore.SwissBitcoinPay.Settings;

namespace Smartstore.SwissBitcoinPay
{
    internal class Module : ModuleBase
    {
        public override async Task InstallAsync(ModuleInstallationContext context)
        {
            await SaveSettingsAsync<SwissBitcoinPaySettings>(new SwissBitcoinPaySettings
            {
                ApiUrl = "https://api.swiss-bitcoin-pay.ch"
            });
            await ImportLanguageResourcesAsync();
            await base.InstallAsync(context);
        }

        public override async Task UninstallAsync()
        {
            await DeleteSettingsAsync<SwissBitcoinPaySettings>();

            await DeleteLanguageResourcesAsync();
            await DeleteLanguageResourcesAsync("Plugins.Payment.SwissBitcoinPay");

            await base.UninstallAsync();
        }
    }
}
