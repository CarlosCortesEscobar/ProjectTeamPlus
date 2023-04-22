using SteamProject.Models;

public interface IAwardCondition
{
    string BadgeName { get; }
    Task<bool> IsFulfilledAsync(User user);
    
}