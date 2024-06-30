namespace Dorbit.Framework;

internal enum Errors
{
    CaptchaSizeIsTooLarge,
    CaptchaNotCorrect,
    CaptchaNotSet,
    TooMuchRequest,
    ServerError,
    UnAuthorize,
    AuthenticationFailed,
    EntityIsReadonly,
    EntityIsUnDeletable,
    TransactionRollback,
    SendMessageFailed,
}