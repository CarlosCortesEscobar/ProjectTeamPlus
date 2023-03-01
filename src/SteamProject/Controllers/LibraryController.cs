using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SteamProject.DAL.Abstract;
using SteamProject.Models;
using Microsoft.AspNetCore.Identity;
using SteamProject.Services;
using Microsoft.AspNetCore.Authorization;
using SteamProject.ViewModels;
using SteamProject.Models.DTO;

namespace SteamProject.Controllers;

public class LibraryController: Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IUserRepository _userRepository;
    private readonly IGameRepository _gameRepository;
    private readonly ISteamService _steamService;
    private readonly IUserGameInfoRepository _userGameInfoRepository;

    public LibraryController(UserManager<IdentityUser> userManager, IUserRepository userRepository, IGameRepository gameRepository,IUserGameInfoRepository userGameInfoRepository, ISteamService steamService)
    {
        _userManager = userManager;
        _userRepository = userRepository;
        _gameRepository = gameRepository;
        _steamService = steamService;
        _userGameInfoRepository = userGameInfoRepository;
    }

    [Authorize]
    public IActionResult Index(bool refresh)
    {
        if(refresh == null)
        {
            refresh = false;
        }
        var userLibraryVM = new UserLibraryVM();
        if(_userManager.GetUserId(User) is null)
        {
            return View();
        }
        else
        {
            var id = _userManager.GetUserId(User);
            var user = _userRepository.GetUser(id);
            userLibraryVM._user = user;
            var tempGameInfo = _userGameInfoRepository.GetAll(g => g.OwnerId == user.Id).ToList();
            userLibraryVM._games = new List<Game>();
            if(tempGameInfo.Count() == 0 || refresh)
            {
                var games = _steamService.GetGames(user.SteamId, user.Id);
                if(games == null)
                    return View();
                foreach(var game in games)
                {
                    try{
                        var temp1 = _gameRepository.GetAll(g => g.AppId == game.AppId).ToList();
                        var checkGameRepo = temp1.Count() == 0;
                        if(checkGameRepo)
                        {
                            var temp2 = _userGameInfoRepository.GetAll(g => g.Id == game.Id).ToList();
                            var checkUserGameRepo = temp2.Count() == 0;
                            if(checkUserGameRepo)
                            {
                                _userGameInfoRepository.AddOrUpdate(new UserGameInfo{
                                    OwnerId = user.Id,
                                    GameId = game.AppId,
                                    PlayTime = game.PlayTime,
                                    LastPlayed = game.LastPlayed,
                                    Hidden = false,
                                    Followed = false,
                                    Game = game,
                                    Owner = user
                                });
                                userLibraryVM._games.Add(game);
                            }
                            game.PlayTime = 0;
                            game.LastPlayed = 0;
                            _gameRepository.AddOrUpdate(game);
                        }
                    }
                    catch
                    {
                        throw new Exception("Current game couldn't be saved to the db!" + game.Name);
                    }
                    
                }
            }
            else
            {
                var games = tempGameInfo;
                if(games == null)
                    return View();
                foreach(var game in games)
                {  
                    var tempGame = _gameRepository.FindById(game.GameId);
                    userLibraryVM._games.Add(tempGame);
                }
            }
            return View(userLibraryVM);
        }
    }
    
    public IActionResult GameInfo(string appId)
    {
        return View();
    }
}