namespace Radikoj.Models.AccountViewModels
{
    public enum SignInErrorType
    {
        NotFound,
        InvalidToken,
        SendEmailFailed,
        ExpiredToken,
        Disabled,
    }
}
