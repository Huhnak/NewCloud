﻿using APIServer.Authorization;
using APIServer.Helpers;
using APIServer.Models.Users;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Crypto.Generators;
using APIServer.Entities;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;

namespace APIServer.Services;

public interface IUserService
{
    AuthenticateResponse Authenticate(AuthenticateRequest model, string ipAddress);
    AuthenticateResponse RefreshToken(string token, string ipAddress);
    void Register(RegisterRequest model);

    void RevokeToken(string token, string ipAddress);
    IEnumerable<User> GetAll();
    User GetById(int id);
}

public class UserService : IUserService
{
    private DataContext _context;
    private IJwtUtils _jwtUtils;
    private readonly AppSettings _appSettings;

    public UserService(
        DataContext context,
        IJwtUtils jwtUtils,
        IOptions<AppSettings> appSettings)
    {
        _context = context;
        _jwtUtils = jwtUtils;
        _appSettings = appSettings.Value;
    }
    public void Register([FromForm] RegisterRequest model)
    {
        if (_context.User.Any(u => u.Username == model.Username))
            throw new AppException($"Username '{model.Username}' is already taken");
        User user = new User()
        {
            Username = model.Username,
            LastName = model.LastName,
            FirstName = model.FirstName,
            Size = (long)134217728,
            SpaceLeft = (long)134217728,
        };
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
        _context.User.Add(user);
        _context.SaveChanges();
    }

    public AuthenticateResponse Authenticate(AuthenticateRequest model, string ipAddress)
    {
        var user = _context.User.SingleOrDefault(x => x.Username == model.Username);
        
        // validate
        if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            throw new AppException("Username or password is incorrect");

        // authentication successful so generate jwt and refresh tokens
        var jwtToken = _jwtUtils.GenerateJwtToken(user);
        var refreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);
        user.RefreshTokens.Add(refreshToken);


        // remove old refresh tokens from user
        removeOldRefreshTokens(user);

        // save changes to db
        _context.SaveChanges();

        return new AuthenticateResponse(user, jwtToken, refreshToken.Token);
    }

    public AuthenticateResponse RefreshToken(string token, string ipAddress)
    {
        var user = getUserByRefreshToken(token);
        var refreshToken = user.RefreshTokens.Single(x => x.Token == token);
        if (refreshToken.IsRevoked)
        {
            // revoke all descendant tokens in case this token has been compromised
            revokeDescendantRefreshTokens(refreshToken, user, ipAddress, $"Attempted reuse of revoked ancestor token: {token}");
            _context.Update(user);
            _context.SaveChanges();
        }

        if (!refreshToken.IsActive)
            throw new AppException("Invalid token");

        // replace old refresh token with a new one (rotate token)
        var newRefreshToken = rotateRefreshToken(refreshToken, ipAddress);
        user.RefreshTokens.Add(newRefreshToken);

        // remove old refresh tokens from user
        removeOldRefreshTokens(user);

        // save changes to db
        _context.Update(user);
        _context.SaveChanges();

        // generate new jwt
        var jwtToken = _jwtUtils.GenerateJwtToken(user);

        return new AuthenticateResponse(user, jwtToken, newRefreshToken.Token);
    }

    public void RevokeToken(string token, string ipAddress)
    {
        var user = getUserByRefreshToken(token);
        var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

        if (!refreshToken.IsActive)
            throw new AppException("Invalid token");

        // revoke token and save
        revokeRefreshToken(refreshToken, ipAddress, "Revoked without replacement");
        _context.Update(user);
        _context.SaveChanges();
    }

    public IEnumerable<User> GetAll()
    {
        return _context.User;
    }

    public User GetById(int id)
    {
        var user = _context.User.Find(id);
        if (user == null) throw new KeyNotFoundException("User not found");
        return user;
    }

    // helper methods

    public User getUserByRefreshToken(string token)
    {
        var user = _context.User.Include(u => u.RefreshTokens).SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));
        //var user = _context.User.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));
        if (user == null)
            throw new AppException("Invalid token");

        return user;
    }

    private RefreshToken rotateRefreshToken(RefreshToken refreshToken, string ipAddress)
    {
        var newRefreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);
        revokeRefreshToken(refreshToken, ipAddress, "Replaced by new token", newRefreshToken.Token);
        return newRefreshToken;
    }

    private void removeOldRefreshTokens(User user)
    {
        // remove old inactive refresh tokens from user based on TTL in app settings
        user.RefreshTokens.RemoveAll(x =>
            !x.IsActive &&
            x.Created.AddDays(_appSettings.RefreshTokenTTL) <= DateTime.UtcNow);
    }

    private void revokeDescendantRefreshTokens(RefreshToken refreshToken, User user, string ipAddress, string reason)
    {
        // recursively traverse the refresh token chain and ensure all descendants are revoked
        if (!string.IsNullOrEmpty(refreshToken.ReplacedByToken))
        {
            var childToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken.ReplacedByToken);
            if (childToken.IsActive)
                revokeRefreshToken(childToken, ipAddress, reason);
            else
                revokeDescendantRefreshTokens(childToken, user, ipAddress, reason);
        }
    }

    private void revokeRefreshToken(RefreshToken token, string ipAddress, string reason = null, string replacedByToken = null)
    {
        token.Revoked = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;
        token.ReasonRevoked = reason;
        token.ReplacedByToken = replacedByToken;
    }
}
