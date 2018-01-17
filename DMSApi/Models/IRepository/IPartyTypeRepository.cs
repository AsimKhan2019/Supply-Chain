﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSApi.Models.IRepository
{
    public interface IPartyTypeRepository
    {
        List<party_type> GetAllPartyType();
        List<party_type> GetAllPartyTypeForPromotion();
        List<party_type> GetAllPartyTypeNotADA();
        party_type GetPartyTypeByID(long party_type_id);
        object GetInternalPartyTypeByPartyId();
        bool InsertPartyType(party_type oPartyType,long created_by);
        bool DeletePartyType(long party_type_id, long updated_by);
        bool UpdatePartyType(party_type oPartyType, long updated_by);
        bool CheckDuplicatPartyPrefix(string party_prefix);
        bool CheckDuplicatPartyType(string party_type_name);
        List<party_type> GetClientPartyTypes();
        List<party_type> GetNormalClientPartyType();
        List<party_type> GetRetailerPartyType();
        object GetDealer();
        List<party_type> GetPartyTypesForReturn();
        List<party_type> GetPartyTypesForInitialBalance();
        object GetPartyTypeByAreaId(long areaId);



    }
}
