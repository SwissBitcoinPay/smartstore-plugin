namespace SmartStore.SwissBitcoinPay.Models
{
    [LocalizedDisplay("Plugins.Smartstore.SwissBitcoinPay.")]
    public class ConfigurationModel : ModelBase
    {

        [LocalizedDisplay("*ApiUrl")]
        [Url]
        //[Required]
        public string ApiUrl { get; set; }

        [LocalizedDisplay("*ApiKey")]
        //[Required]
        public string ApiKey { get; set; }

        [LocalizedDisplay("*ApiSecret")]
        //[Required]
        public string ApiSecret { get; set; }

        [LocalizedDisplay("Admin.Configuration.Payment.Methods.AdditionalFee")]
        public decimal AdditionalFee { get; set; }

        [LocalizedDisplay("Admin.Configuration.Payment.Methods.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
    }


}