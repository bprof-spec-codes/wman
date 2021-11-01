﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wman.Logic.DTO_Models;
using Wman.Logic.Interfaces;

namespace Wman.WebAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LabelController : ControllerBase
    {
        ILabelLogic labelLogic;
        public LabelController(ILabelLogic labelLogic)
        {
            this.labelLogic = labelLogic;
        }

        [HttpPost("/CreateLabel")]
        public async Task<ActionResult> CreateLabel([FromBody] CreateLabelDTO label)
        {
            try
            {
                await labelLogic.CreateLabel(label);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error : {ex}");
            }
        }
    }
}