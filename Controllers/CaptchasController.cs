using Dorbit.Framework.Filters;
using Dorbit.Framework.Models;
using Dorbit.Framework.Models.Captchas;
using Dorbit.Framework.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dorbit.Framework.Controllers;

public class CaptchasController : BaseController
{
    private readonly CaptchaService captchaService;

    public CaptchasController(CaptchaService captchaService)
    {
        this.captchaService = captchaService;
    }

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