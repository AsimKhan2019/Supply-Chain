﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSApi.Models.IRepository
{
    public interface IPartyRepository
    {
        object GetAllParty();
        object GetAllPartyForMonthlyPartyTarget(long province_id, long city_id, long party_type_id, string target_month);
        object GetAllPartyWithRetailer();
        object GetAllRetailerByPartyId(long party_id);
        object GetPartyCodeById(long party_id);
        //to get master delar and brand shop.....
        object GetAllPartyByPartyTypeId();
        object GetMasterDealer();
        object GetPartyByPartyName(string party_name);
        long AddParty(party oParty);
        bool CheckDuplicatePartyName(string party_name);
        party GetPartyById(long party_id);

        object GetPartyInfoByPartyId(long party_id);
            
        bool EditParty(party oParty);
        long DeleteParty(long party_id, long? updated_by);
        List<party> GetPartyTypewisePartyForDropdown(long party_type_id);
        List<party> GetAreaNPartyTypewiseParty(long party_type_id, long area_id);
        object GetOnlyNormalClientParty();
        decimal GetPartyCreditLimit(long party_id);//19.01.2017
        string PartyBillingAddress(long party_id);//28.01.2017
        object GetRegionAreaTerritory(long party_id);

        List<party> GetAllPartyWithoutJournal();
        object GetPartyOpeningBalanceById(long party_id);
        //bool EditOpeningBalance(party oParty);
        //bool EditOpeningBalance(long party_id, decimal opening_balance);
        //bool EditOpeningBalance(long party_id, decimal opening_balance);
        bool EditOpeningBalance(StronglyType.OpeningBalanceModel OpeningBalanceModel);
        bool EditCreditLimit(party CreditLimit);
        object GetPartyByPartyTypeIdAndAreaId(long partyTypeId, long areaId);
    }
}
