﻿using System.Collections.Generic;
using Dorbit.Framework.Contracts;
using Dorbit.Framework.Contracts.Captchas;
using Dorbit.Framework.Contracts.Results;
using Dorbit.Framework.Filters;
using Dorbit.Framework.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dorbit.Framework.Controllers;

[ApiExplorerSettings(GroupName = "framework")]
[Route("Framework/[controller]")]
public class CaptchasController(CaptchaService captchaService) : BaseController
{
    [HttpGet, AntiDos(AntiDosAttribute.DurationType.Minute, 30)]
    public QueryResult<KeyValuePair<string, string>> Generate([FromQuery] CaptchaRequest request)
    {
        var dto = new CaptchaGenerateModel
        {
            Width = request.Width,
            Height = request.Height,
        };
        return new QueryResult<KeyValuePair<string, string>>(captchaService.Generate(dto));
    }
}