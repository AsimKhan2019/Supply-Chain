﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSApi.Models.IRepository
{
    public interface IWarehouseRepository
    {
        List<warehouse> GetAllWarehouse();
        object GetAdaWarehouse();
        object GetWeWarehouse();
        object GetWarehouseByPartyId(long party_id);      
        object GetSalesWarehouseOnly();
        object GetWarehouseForDirectTransfer();
        object GetWarehouseForTransferOrder();
        warehouse GetWarehouseByPartyIdForAll(long party_id);
        object GetAllWarehouseForGridLoad();
        object GetWarehouseForEdit(long warehuse_id);
        bool CheckDuplicateWarehouse(string warehouse_name);
        long AddWarehouse(warehouse warehouse);
        warehouse GetWarehouseById(long warehouse_id);
        bool EditWarehouse(warehouse warehouse);
        bool DeleteWarehouse(long warehouse_id, long? updated_by);
    }
}
