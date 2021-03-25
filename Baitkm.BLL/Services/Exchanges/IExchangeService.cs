namespace Baitkm.BLL.Services.Exchanges
{
    public interface IExchangeService
    {
        decimal CurrencyRate(int exId, int currencyRequestid, decimal amount);
    }
}