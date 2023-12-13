using Dorbit.Filters;
using Dorbit.Models;
using Dorbit.Models.Captchas;
using Dorbit.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dorbit.Controllers;

public class CaptchasController : BaseController
{
    private readonly CaptchaService _captchaService;

    public CaptchasController(CaptchaService captchaService)
    {
        _captchaService = captchaService;
    }

    [HttpGet, Delay(Request = 300)]
    public QueryResult<KeyValuePair<string, string>> Generate([FromQuery] CaptchaGenerateCommand viewModel)
    {
        var dto = new CaptchaGenerateModel
        {
            Width = viewModel.Width,
            Height = viewModel.Height,
        };
        return new QueryResult<KeyValuePair<string, string>>(_captchaService.Generate(dto));
    }
}