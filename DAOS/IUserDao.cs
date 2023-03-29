﻿using Microsoft.AspNetCore.Identity;
using snapnow.DTOS;
using snapnow.ErrorHandling;
using snapnow.Models;

namespace snapnow.DAOS;

public interface IUserDao : IDao<ApplicationUser, string>
{
    public DatabaseResponseModel<ApplicationUser> CheckPhoneNumber(string phoneNumber);
    public Task<DatabaseResponseModel<IdentityResult>> AddUserToRole(ApplicationUser identityUser, string roleName);
}