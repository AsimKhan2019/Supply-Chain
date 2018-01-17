﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DMSApi.Models.IRepository
{
    public interface IProductPriceingReporsitory
    {
        object GetAllProductPriceing();
        bool AddProductPriceing(product_price_mapping productPrice,long create_by);
        product_price_mapping GetProductPriceMappingBtId(long priceingId);
        bool EditProductPricing(product_price_mapping productPrice,long update_by);
        bool DeleteProductPriceing(long priceingId);
        bool CheckDuplicatePriceing(product_price_mapping productPrice);
        object GetProductVersionByProductId(long product_id);
        object GetColorByProductId(long product_id);
        object GetProductwiseColor();
        object GetProductColorwiseVersion(long product_id, long color_id);
        object GetProductColorVersionwisePrice(long party_type_id,long product_id, long color_id, long product_version_id);

        object GetProductColorVersionwiseDealerDemoPrice(long party_type_id, long product_id, long color_id, long product_version_id);

        object GetProductColorVersionwiseB2BPrice(long party_type_id, long product_id, long color_id, long product_version_id);
        object GetProductColorVersionwiseDealerPrice(long party_type_id, long product_id, long color_id, long product_version_id);
        //object GetProductColorwiseVersion();

        object GetPriceingForLogReportPageView(long product_id, long color_id, long product_version_id);
        object GetLastGrn(long pId,long cId,long vId);

        object GetFilterWiseProductPricing(long catId, long pId, long cId, long vId);

    }
}