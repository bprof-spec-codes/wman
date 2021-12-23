﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        IPhotoLogic photoLogic;
        IAuthLogic authLogic;
        IAdminLogic adminLogic;

        public AdminController(IPhotoLogic photoLogic, IAuthLogic authLogic, IAdminLogic adminLogic)
        {
            this.authLogic = authLogic;
            this.photoLogic = photoLogic;
            this.adminLogic = adminLogic;
        }
        [HttpPost]
        [Route("Create")]
        public async Task<ActionResult> CreateWorker([FromBody] RegisterDTO model, IFormFile file)
        {
            await this.authLogic.CreateWorker(model);
            await this.photoLogic.AddProfilePhoto(model.Username, file);
            return Ok();
        }
        [HttpPut]
        [Route("Modify/{username}")]
        public async Task<ActionResult> ModifyWorker(string username, [FromBody] WorkerModifyDTO model)
        {
            return Ok(await this.authLogic.UpdateUser(username, model));
        }
        [HttpDelete]
        [Route("Delete")]
        public async Task<ActionResult> DeleteWorker(string username)
        {
            return Ok(await this.authLogic.DeleteUser(username));
        }
        /// <summary>
        /// Set the role of a user, while removing any previous roles he had before
        /// </summary>
        /// <param name="username">Username of the user</param>
        /// <param name="rolename">Name of the role(Admin/Manager/Worker)</param>
        /// <returns></returns>
        [HttpGet]
        [Route("role/set")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> SetRole(string username, string rolename)
        {
            await this.authLogic.SetRoleOfUser(username, rolename);
            return Ok();
        }
        [HttpGet]
        [Route("test")]
        public async Task<ActionResult> testme()
        {
            adminLogic.test(this.authLogic);
            return Ok();
        }
    }
}