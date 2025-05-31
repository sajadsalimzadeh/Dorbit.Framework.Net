namespace Dorbit.Framework;

public enum FrameworkErrors
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
    AesKeySizeIsInvalid,
    AesKeySizeMostEqualIvSize
}