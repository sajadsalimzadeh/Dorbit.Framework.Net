using System.Collections.Generic;
using Dorbit.Framework.Contracts;
using Dorbit.Framework.Contracts.Captchas;
using Dorbit.Framework.Contracts.Results;
using Dorbit.Framework.Filters;
using Dorbit.Framework.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dorbit.Framework.Controllers;

public class CaptchasController(CaptchaService captchaService) : BaseController
{
    [HttpGet, Delay(Request = 300)]
    public QueryResult<KeyValuePair<string, string>> Generate([FromQuery] CaptchaGenerateCommand viewModel)
    {
        var dto = new CaptchaGenerateModel
        {
            Width = viewModel.Width,
            Height = viewModel.Height,
        };
        return new QueryResult<KeyValuePair<string, string>>(captchaService.Generate(dto));
    }
}