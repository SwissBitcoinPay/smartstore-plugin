
namespace SmartStore.SwissBitcoinPay.Models
{

    public struct SwissBitcoinPayInvoiceModel
    {
        public string title;
        public string description;
        public decimal amount;
        public string unit;
        public bool onChain;
        public int delay;
        public string email;
        public string emailLanguage;
        public string redirectAfterPaid;
        public string webhook;
        public SwissBitcoinPayExtraModel extra;
    }

    public struct SwissBitcoinPayExtraModel
    {
        public string customNote;
    }
}