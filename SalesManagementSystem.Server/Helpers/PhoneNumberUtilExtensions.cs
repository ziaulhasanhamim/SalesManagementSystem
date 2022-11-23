namespace SalesManagementSystem.Server.Helpers;

using System.Diagnostics.CodeAnalysis;
using PhoneNumbers;

static class PhoneNumberUtilHelpers
{
    public static bool TryParse(this PhoneNumberUtil util, string number, string? regionCode, [NotNullWhen(true)]out PhoneNumber? phoneNumber)
    {
        try
        {
            var tempPhoneNumber = util.Parse(number, regionCode);
            if (util.IsValidNumber(tempPhoneNumber))
            {
                phoneNumber = tempPhoneNumber;
                return true;
            }
        }
        catch
        {
        }
        phoneNumber = null;
        return false;
    }
}