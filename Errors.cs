namespace Dorbit.Framework;

internal enum Errors
{
    CaptchaSizeIsTooLarg,
    CaptchaNotCorrect,
    CaptchaNotSet,
    TooMuchRequest,
    ServerError,
    AccessDenied,
    UnAuthorized,
    EntityIsReadonly,
    EntityIsUnDeletable,
    TransactionRollback,
    NoMessageProviderFound,
    CanNotChangeVerifiedFile,
    AesKeySizeIsInvalid,
    AesKeySizeMostEqualIvSize
}