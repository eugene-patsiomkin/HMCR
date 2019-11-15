﻿using Hmcr.Data.Database;
using Hmcr.Data.Database.Entities;
using Hmcr.Data.Repositories;
using Hmcr.Model;
using Hmcr.Model.Dtos.Party;
using Hmcr.Model.Dtos.User;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hmcr.Domain.Services
{
    public interface IUserService
    {
        Task<UserCurrentDto> GetCurrentUserAsync();
        Task<bool> ProcessFirstUserLoginAsync();
    }
    public class UserService : IUserService
    {
        private IUserRepository _userRepo;
        private IPartyRepository _partyRepo;
        private IUnitOfWork _unitOfWork;
        private HmcrCurrentUser _currentUser;

        public UserService(IUserRepository userRepo, IPartyRepository partyRepo, IUnitOfWork unitOfWork, HmcrCurrentUser currentUser)
        {
            _userRepo = userRepo;
            _partyRepo = partyRepo;
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<UserCurrentDto> GetCurrentUserAsync()
        {
            return await _userRepo.GetCurrentUserAsync();
        }

        public async Task<bool> ProcessFirstUserLoginAsync()
        {
            var userEntity = await _userRepo.GetCurrentActiveUserEntityAsync();

            if (userEntity == null)
            {
                return false;
            }

            if (userEntity.UserGuid == null)
            {
                if (userEntity.UserDirectory == _currentUser.AuthDirName.ToUpperInvariant())
                {
                    UpdateUserEntity(userEntity);
                    CreatePartyEntityIfNecessary();
                    _unitOfWork.Commit();
                }
                else
                {
                    //To do: does admin user enter DIR name or user type?
                    throw new HmcrException($"User[{_currentUser.UniversalId}] exists in the user table with a wrong directory name [{_currentUser.AuthDirName}].");
                }
            }

            return true;
        }

        private void UpdateUserEntity(HmrSystemUser userEntity)
        {
            userEntity.UserGuid = _currentUser.UserGuid;
            userEntity.BusinessGuid = _currentUser.BusinessGuid;
            userEntity.BusinessLegalName = _currentUser.BusinessLegalName;
            userEntity.UserType = _currentUser.UserType.ToUpperInvariant();
        }

        private async void CreatePartyEntityIfNecessary()
        {
            if (_currentUser.AuthDirName.ToUpperInvariant() == Constants.IDIR)
                return;

            var partyEntity = await _partyRepo.GetPartyEntityByGuidAsync(_currentUser.BusinessGuid);

            if (partyEntity != null)
                return;

            var party = new PartyDto
            {
                BusinessGuid = _currentUser.BusinessGuid,
                BusinessLegalName = _currentUser.BusinessLegalName.Trim(),
                BusinessNumber = Convert.ToDecimal(_currentUser.BusinessNumber),
                DisplayName = _currentUser.BusinessLegalName.Trim()
            };

            _partyRepo.Add(party);
        }
    }
}