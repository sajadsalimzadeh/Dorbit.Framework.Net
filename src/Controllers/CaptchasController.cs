using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Devor.Framework.Models;
using Devor.Framework.Filters;
using Devor.Framework.Models.Captchas;
using Devor.Framework.Services;

namespace Devor.Framework.Controllers
{
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
}
