using System;
using System.Collections.Generic;
using System.Text;

namespace Baitkm.DTO.ViewModels
{
    public class RateResult
    {
        public int Id { get; set; }
        public string Iso { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
        public int Numeric_code { get; set; }
    }

    public class RateBaseResult
    {
        public List<RateResult> Data { get; set; }
        public string Message { get; set; }
        public bool Status { get; set; }
    }

    public class BaseExchangeResponseModel
    {
        public ExchangeResponseModel Data { get; set; }
        public string Message { get; set; }
        public bool Status { get; set; }
    }

    public class ExchangeResponseModel
    {
        public decimal Result { get; set; }
        public int Amount { get; set; }
        public string Current_date { get; set; }
    }
}
