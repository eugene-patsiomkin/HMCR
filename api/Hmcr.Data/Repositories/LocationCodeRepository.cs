﻿using AutoMapper;
using Hmcr.Data.Database.Entities;
using Hmcr.Data.Repositories.Base;
using Hmcr.Model.Dtos.LocationCode;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Hmcr.Data.Repositories
{
   
    public interface ILocationCodeRepository
    {
        Task<IEnumerable<LocationCodeDropDownDto>> GetLocationCodes();
        Task<bool> DoesExistAsync(decimal locationCodeId);
        Task<LocationCodeDto> GetLocationCode(decimal locationCodeId);
    }
    
    public class LocationCodeRepository : HmcrRepositoryBase<HmrLocationCode>, ILocationCodeRepository
    {
        public LocationCodeRepository(AppDbContext dbContext, IMapper mapper)
               : base(dbContext, mapper)
        {
        }

        public async Task<IEnumerable<LocationCodeDropDownDto>> GetLocationCodes()
        {
            var entity = await DbSet.AsNoTracking()
                .ToListAsync();

            return Mapper.Map<IEnumerable<LocationCodeDropDownDto>>(entity);
        }

        public async Task<bool> DoesExistAsync(decimal locationCodeId)
        {
            return await DbSet.AnyAsync(lc => lc.LocationCodeId == locationCodeId);
        }

        public async Task<LocationCodeDto> GetLocationCode(decimal locationCodeId)
        {
            var entity = await DbSet.FirstOrDefaultAsync(lc => lc.LocationCodeId == locationCodeId);
            return Mapper.Map<LocationCodeDto>(entity);
        }
    }
}
 