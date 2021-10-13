using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using GamesBucket.App.Models;
using GamesBucket.DataAccess.Models;
using GamesBucket.DataAccess.Services.Games;
using GamesBucket.DataAccess.Services.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GamesBucket.App.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IGameService _gameService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;

        public ProfileController(IGameService gameService, 
            IUserService userService, IMapper mapper, UserManager<AppUser> userManager)
        {
            _gameService = gameService;
            _userService = userService;
            _mapper = mapper;
            _userManager = userManager;
        }
        
        public async Task<IActionResult> Index()
        {
            var loggedUser = await _userService.GetLoggedUser(User);
            if (loggedUser == null) return RedirectToAction("Error", "Home");
            
            var profile = new ProfileViewModel
            {
                Games = (await _gameService.GetGames(loggedUser))
                    .Where(g => g.Favorite || g.Played).ToList(),
                ProfileDetails = _mapper.Map<ProfileDetailsViewModel>(loggedUser),
                ProfileSecurity = _mapper.Map<ProfileSecurityViewModel>(loggedUser)
            };

            return View(profile);
        }
        
        [HttpPost]
        public async Task<IActionResult> SaveDetails(ProfileDetailsViewModel profileDetails)
        {
            StringBuilder message = new StringBuilder();
            
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(modelState => 
                    modelState.Errors.Select(x => x)))
                {
                    message.AppendLine(error.ErrorMessage);
                }
                
                return BadRequest(message.ToString());
            }

            var user = await _userService.GetLoggedUser(User);
            if(user == null) return NotFound("Invalid user id or no user found");
            
            _mapper.Map(profileDetails, user);
            user.UserName = profileDetails.Email;
            await _userService.UpdateUser(user);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SaveSecurity(ProfileSecurityViewModel profileSecurity)
        {
            StringBuilder message = new StringBuilder();
            
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(modelState => 
                    modelState.Errors.Select(x => x)))
                {
                    message.AppendLine(error.ErrorMessage);
                }
                
                return BadRequest(message.ToString());
            }
            
            var user = await _userService.GetLoggedUser(User);
            if(user == null) return NotFound("Invalid user id or no user found");
            if (!await _userManager.CheckPasswordAsync(user, profileSecurity.OldPassword))
                return BadRequest("Invalid old password");

            var result = await _userManager.ChangePasswordAsync(user, profileSecurity.OldPassword,
                profileSecurity.NewPassword);
            if (result.Succeeded) return Ok();

            foreach (var identityError in result.Errors)
            {
                message.AppendLine(identityError.Description);
            }

            return BadRequest(message.ToString());
        }
    }
}