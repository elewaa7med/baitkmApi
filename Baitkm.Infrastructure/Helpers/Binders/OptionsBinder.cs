using Baitkm.Enums;
using Baitkm.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Baitkm.Infrastructure.Helpers.Binders
{
    public class OptionsBinder : IOptionsBinder
    {
        private readonly IOptions<ErrorMessagesEnglish> english;
        private readonly IOptions<ErrorMessageArabian> arabian;

        public OptionsBinder(IOptions<ErrorMessagesEnglish> english,
            IOptions<ErrorMessageArabian> arabian)
        {
            this.english = english;
            this.arabian = arabian;
        }

        public static ErrorMessages ErrorMessages { get; set; }

        public void Get(ref ErrorMessages error)
        {
            ErrorMessages = error;
        }

        public ErrorMessages Error()
        {
            return ErrorMessages;
        }

        public ErrorMessagesEnglish GetErrorEnglish()
        {
            return english.Value;
        }
        public ErrorMessageArabian GetArabian()
        {
            return arabian.Value;
        }

        public ErrorMessages GetErrorMessages(Language language)
        {
            if (language == Language.English)
                return english.Value;
            else
                return arabian.Value;
        }
    }
}