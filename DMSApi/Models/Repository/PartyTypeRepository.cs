﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DMSApi.Models.IRepository;

namespace DMSApi.Models.Repository
{
    public class PartyTypeRepository : IPartyTypeRepository
    {
        private DMSEntities _entities;

        public PartyTypeRepository()
        {
            this._entities = new DMSEntities();
        }

        public List<party_type> GetAllPartyType()
        {
            List<party_type> partytypes = _entities.party_type.OrderBy(p => p.party_type_name).Where(p => p.is_deleted == false).ToList();
            return partytypes;
        }

        public List<party_type> GetAllPartyTypeForPromotion()
        {
            List<party_type> partytypes = _entities.party_type.OrderByDescending(p => p.party_type_id).Where(p => p.is_active == true && p.is_deleted == false && p.party_type_id != 1).OrderBy(p=>p.party_type_name).ToList();
            return partytypes;
        }

        public List<party_type> GetAllPartyTypeNotADA()
        {
            List<party_type> partytypes = _entities.party_type.Where(p => p.party_type_name != "Central").OrderByDescending(p => p.party_type_id).ToList();
            return partytypes;
        }

        public party_type GetPartyTypeByID(long party_type_id)
        {
            throw new NotImplementedException();
        }

        public bool InsertPartyType(party_type oPartyType, long created_by)
        {
            try
            {
                party_type insert_party_type = new party_type
                {
                    party_type_name = oPartyType.party_type_name,
                    party_prefix = oPartyType.party_prefix,
                    created_by = created_by,
                    created_date = DateTime.Now,
                    is_deleted = oPartyType.is_deleted = false,
                    is_active = oPartyType.is_active = true
                };
                _entities.party_type.Add(insert_party_type);
                _entities.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool DeletePartyType(long party_type_id, long updated_by)
        {
            try
            {
                party_type oPartyType = _entities.party_type.FirstOrDefault(c => c.party_type_id == party_type_id);
                oPartyType.is_deleted = true;
                oPartyType.updated_by = updated_by;
                oPartyType.updated_date = DateTime.Now;
                _entities.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool UpdatePartyType(party_type oPartyType, long updated_by)
        {
            try
            {
                party_type con = _entities.party_type.Find(oPartyType.party_type_id);
                con.party_type_name = oPartyType.party_type_name;
                con.party_prefix = oPartyType.party_prefix;
                con.updated_by = updated_by;
                con.updated_date = DateTime.Now;
                con.is_active = oPartyType.is_active;
                con.is_deleted = oPartyType.is_deleted = false;

                _entities.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool CheckDuplicatPartyType(string party_type_name)
        {
            var checkDuplicatePartyType = _entities.party_type.FirstOrDefault(c => c.party_type_name == party_type_name);

            bool return_type = checkDuplicatePartyType == null ? false : true;
            return return_type;
        }

        public List<party_type> GetClientPartyTypes()
        {
            //List<party_type> partytypes = _entities.party_type.Where(w => w.party_prefix == "MD" || w.party_prefix == "BS").OrderByDescending(p => p.party_type_id).ToList();
            List<party_type> partytypes = _entities.party_type.Where(w => w.is_deleted == false).OrderBy(p => p.party_type_name).ToList();
            return partytypes;
        }

        //For Checking Party Prefix
        public bool CheckDuplicatPartyPrefix(string party_prefix)
        {
            var checkDuplicatePartyPrefix = _entities.party_type.FirstOrDefault(c => c.party_prefix == party_prefix);
            bool return_type = checkDuplicatePartyPrefix == null ? false : true;
            return return_type;
        }


        public List<party_type> GetNormalClientPartyType()
        {
            List<party_type> partytypes = _entities.party_type.Where(w => w.party_prefix == "OC").ToList();
            return partytypes;
        }

        public List<party_type> GetRetailerPartyType()
        {
            List<party_type> partytypes = _entities.party_type.Where(w => w.party_prefix == "RET").ToList();
            return partytypes;
        }

        public object GetDealer()
        {
            try
            {
                var partyType = _entities.party_type.OrderByDescending(w => w.party_type_id).Where(w => w.party_prefix == "DEL").ToList();
                return partyType;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<party_type> GetPartyTypesForReturn()
        {
            List<party_type> partytypes = _entities.party_type.Where(w => w.is_deleted == false && w.is_active == true && (w.party_prefix == "DEL" || w.party_prefix == "B2B")).OrderByDescending(p => p.party_type_id).ToList();
            return partytypes;
        }

        public List<party_type> GetPartyTypesForInitialBalance()
        {
            List<party_type> partytypes = _entities.party_type.Where(w => w.is_deleted == false && w.is_active == true && (w.party_prefix == "DEL" || w.party_prefix == "B2B" || w.party_prefix == "ONL")).OrderBy(p => p.party_type_name).ToList();
            return partytypes;
        }


        public object GetInternalPartyTypeByPartyId()
        {
            try
            {
                var partyType = _entities.party_type.Where(w => w.party_prefix == "INT");
                return partyType;
            }
            catch (Exception)
            {

                throw;
            }
        }


        public object GetPartyTypeByAreaId(long areaId)
        {
            try
            {
                var partType = (from p in _entities.parties
                                join pt in _entities.party_type
                                    on p.party_type_id equals pt.party_type_id
                                    join a in _entities.areas on p.area_id equals a.area_id
                                where a.area_id == areaId

                              
                                select new
                                {
                                    party_id = p.party_id,
                                    party_name = p.party_name,
                                    party_type_id = pt.party_type_id,
                                    party_type_name = pt.party_type_name,
                                    area_id=a.area_id,
                                    area_name=a.area_name
                                });
                return partType;
            }
            catch (Exception)
            {

                return null;
            }
        }
    }
}