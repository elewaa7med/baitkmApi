using Baitkm.Enums;
using Baitkm.Infrastructure.Options;

namespace Baitkm.Infrastructure.Helpers.Binders
{
    public interface IOptionsBinder
    {
        ErrorMessagesEnglish GetErrorEnglish();
        ErrorMessageArabian GetArabian();
        ErrorMessages GetErrorMessages(Language language);
        void Get(ref ErrorMessages error);
        ErrorMessages Error();
    }
}