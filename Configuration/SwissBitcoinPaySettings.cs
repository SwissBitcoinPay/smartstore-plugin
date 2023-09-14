using Smartstore.Core.Configuration;

namespace SmartStore.SwissBitcoinPay.Settings
{
    public class SwissBitcoinPaySettings : ISettings
    {
        /// <summary>
        /// The url of the Swiss Bitcoin Pay API
        /// </summary>
        public string ApiUrl { get; set; }

        /// <summary>
        /// The API Key value generated in your Swiss Bitcoin Pay account
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// The Api Secret value generated in your Swiss Bitcoin Pay account
        /// </summary>
        public string ApiSecret { get; set; }

        public decimal AdditionalFee { get; set; }

        public bool AdditionalFeePercentage { get; set; }
    }
}