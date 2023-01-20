using CancerPredictionApi.DTO;
using CancerPredictionApi.Model;
using CancerPredictionApi.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CancerPredictionApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private IAuthRepository _authRepository;
        public AuthController(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        [HttpPost("/Register")]
        public async Task<ActionResult<ServiceResponse<string>>> Register(UserRegisterDto userd)
        {
            ServiceResponse<string> response = new ServiceResponse<string>();
            if(await _authRepository.UserExists(userd.emailId))
            {
                response.Success = false;
                response.Message = "User already exists";
            }
            else
            {
                User user1 = new User();
                _authRepository.CreatePasswordHash(userd.Password, out byte[] passwordHash, out byte[] passwordSalt);
                user1.PasswordHash= passwordHash;
                user1.PasswordSalt= passwordSalt;
                user1.emailId= userd.emailId;
                user1.customerName= userd.customerName;
                user1.Address = userd.Address;
                user1.MobileNo= userd.MobileNo;
                try
                {
                    await _authRepository.Add(user1);
                    response.Success = true;
                    response.Data=userd.emailId.ToString();
                    response.Message = "Success";
                    return Ok(response);
                }
                catch (Exception ex)
                {
                    response.Success = false;
                    response.Message= "Db validation failed/Db server down";
                    response.Data=ex.Message;
                }
            }
            return BadRequest(response);
        }
        [HttpPost("/Login")]
        public async Task<ActionResult<ServiceResponse<string>>> Login([FromBody] UserLoginDto userd)
        {
            ServiceResponse<string> response = new ServiceResponse<string>();
            var user = await _authRepository.UserGetById(userd.emailId);
            if (user == null)
            {
                response.Success = false;
                response.Message = "User does not exist";
            }
            else if (!_authRepository.VerifyPasswordHash(userd.password, user.PasswordHash, user.PasswordSalt))
            {
                response.Success = false;
                response.Message = "Incorrect credentials";
            }
            else
            {
                response.Success = true;
                response.Data = _authRepository.CreateToken(user);
                return Ok(response);
            }
            return BadRequest(response);
        }
    }
}
