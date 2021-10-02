﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wman.Data.DB_Models;
using Wman.Logic.Classes;
using Wman.Logic.DTO_Models;
using Wman.Logic.Interfaces;

namespace Wman.WebAPI.Controllers
{

    [ApiController]

    [Route("[controller]")]
    public class AuthController : Controller
    {
        IAuthLogic authLogic;

        public AuthController(IAuthLogic authLogic)
        {
            this.authLogic = authLogic;
        }
        /// <summary>
        /// Create a new user
        /// </summary>
        /// <param name="model">Login model</param>
        /// <returns>ActionResult</returns>
        [HttpPost]
        
        public async Task<ActionResult> CreateUser([FromBody] userDTO model) 
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            IdentityResult result;
            try
            {
                result = await authLogic.CreateUser(model);

                if (result.Succeeded) return Ok("User created successfully");
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new { Error = ex.Message });
                throw;
            }
            return BadRequest(result.Errors);
        }
        /// <summary>
        /// Get a list of all users
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<userDTO>>> GetAllUsers()
        {
            try
            {
                return Ok(Converter.MassConvert( await authLogic.GetAllUsers()));
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            
        }

        /// <summary>
        /// Get one specific user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("username")]
        //[Route("getOne")]
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult<userDTO>> GetUser(string username)
        {
            var output = Converter.Convert(await authLogic.GetOneUser(username));
            if (output == null)
            {
                return BadRequest("User not found");
            }
            return Ok(output);
        }

        /// <summary>
        /// Delete a user
        /// </summary>
        /// <param name="id">Id of the user to be deleted</param>
        [HttpDelete("{username}")]
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteUser(string username)
        {
            IdentityResult result;
            try
            {
                result = await this.authLogic.DeleteUser(username);
                if (result.Succeeded)
                return Ok("User deleted successfully");
            }
            catch (Exception ex)
            {

                return BadRequest(new { Error = ex.Message });
            }
            return BadRequest(result.Errors);
        }

        /// <summary>
        /// Update a user
        /// </summary>
        /// <param name="oldId">Prev. id</param>
        /// <param name="user">User to be updated</param>
        [HttpPut("{oldUsername}")]
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateUser(string oldUsername, [FromBody] userDTO user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            IdentityResult result;
            try
            {
                result = await this.authLogic.UpdateUser(oldUsername, user);
                if (result.Succeeded)
                    return Ok("User updated successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            return BadRequest(result.Errors);
        }

        /// <summary>
        /// Login/generate jwt token
        /// </summary>
        /// <param name="model">Login details</param>
        /// <returns>Hopefully a jwt token</returns>
        [HttpPut]
        [Route("login")]

        public async Task<ActionResult> Login([FromBody] LoginDTO model) 
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                return Ok(await authLogic.LoginUser(model));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}