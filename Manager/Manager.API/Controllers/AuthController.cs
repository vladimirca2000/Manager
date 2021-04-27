using System;
using System.Threading.Tasks;
using AutoMapper;
using Manager.API.Token;
using Manager.API.Utilities;
using Manager.API.ViewModels;
using Manager.Services.DTO;
using Manager.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Manager.API.Controllers
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ITokenGenerator _tokenGenerator;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public AuthController(IConfiguration configuration, ITokenGenerator tokenGenerator, IMapper mapper, IUserService userService)
        {
            _mapper = mapper;
            _configuration = configuration;
            _tokenGenerator = tokenGenerator;
            _userService = userService;
        }

        [HttpPost]
        [Route("/api/v1/auth/login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel loginViewModel)
        {
            try
            {
                //var userDTO = _mapper.Map<UserDTO>(loginViewModel);

                var userBanco = await _userService.GetByName(loginViewModel.Login);

                if(userBanco == null)
                {
                    return Ok(new ResultViewModel
                    {
                        Message = "Usuario informado não existe.",
                        Success = true,
                        Data = null
                    });
                }
                else
                {
                    if (userBanco.Password != loginViewModel.Password)
                    {
                        return Ok(new ResultViewModel
                        {
                            Message = "Senha errada.",
                            Success = true,
                            Data = null
                        });
                    }
                }

                //var tokenLogin =  _configuration["Jwt:Login"];
                //var tokenPassword =  _configuration["Jwt:Password"];

                //if (loginViewModel.Login == tokenLogin && loginViewModel.Password == tokenPassword)
                if (loginViewModel.Login == userBanco.Name && loginViewModel.Password == userBanco.Password)
                    {
                    return Ok(new ResultViewModel
                    {
                        Message = "Usuário autenticado com sucesso!",
                        Success = true,
                        Data = new
                        {
                            Token = _tokenGenerator.GenerateToken(),
                            TokenExpires = DateTime.UtcNow.AddHours(int.Parse(_configuration["Jwt:HoursToExpire"]))
                        }
                    });
                }
                else
                {
                    return StatusCode(401, Responses.UnauthorizedErrorMessage());
                }
            }
            catch (Exception)
            {
                return StatusCode(500, Responses.ApplicationErrorMessage());
            }
        }
    }
}