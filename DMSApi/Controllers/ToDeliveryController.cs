﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using DMSApi.Models;
using DMSApi.Models.IRepository;
using DMSApi.Models.Repository;
using DMSApi.Models.StronglyType;

namespace DMSApi.Controllers
{
    //[EnableCors(origins: "*", headers: "*", methods: "*")]
    public class ToDeliveryController : ApiController
    {
        private IToDeliveryRepository toDeliveryRepository;

        public ToDeliveryController()
        {
            this.toDeliveryRepository = new ToDeliveryRepository();
        }

        public ToDeliveryController(IToDeliveryRepository toDeliveryRepository)
        {
            this.toDeliveryRepository = toDeliveryRepository;
        }

        [HttpGet, ActionName("GetAllToDelivery")]
        public HttpResponseMessage GetAllToDelivery()
        {
            var data = toDeliveryRepository.GetAllToDelivery();
            var formatter = RequestFormat.JsonFormaterString();
            return Request.CreateResponse(HttpStatusCode.OK, data, formatter);
        }

        [HttpGet, ActionName("GetToDeliveryById")]
        public HttpResponseMessage GetToDeliveryById(long to_delivery_master_id)
        {
            var data = toDeliveryRepository.GetToDeliveryById(to_delivery_master_id);
            var formatter = RequestFormat.JsonFormaterString();
            return Request.CreateResponse(HttpStatusCode.OK, data, formatter);
        }

        [HttpGet, ActionName("GetAllToDeliveryForRfd")]
        public HttpResponseMessage GetAllToDeliveryForRfd()
        {
            var data = toDeliveryRepository.GetAllToDeliveryForRfd();
            var formatter = RequestFormat.JsonFormaterString();
            return Request.CreateResponse(HttpStatusCode.OK, data, formatter);
        }

        [HttpGet, ActionName("GetMonthlyTransferReport")]
        public HttpResponseMessage GetMonthlyTransferReport(DateTime from_date, DateTime to_date, long from_warehouse_id)
        {
            var data = toDeliveryRepository.GetMonthlyTransferReport(from_date, to_date, from_warehouse_id);
            var formatter = RequestFormat.JsonFormaterString();
            return Request.CreateResponse(HttpStatusCode.OK, data, formatter);
        }

        [System.Web.Http.HttpPost]
        public HttpResponseMessage Post([FromBody] ToDeliveryModel toDeliveryModel)
        {

            try
            {
                if (string.IsNullOrEmpty(toDeliveryModel.ToDeliveryMasterData.from_warehouse_id.ToString()))
                {
                    var formatter = RequestFormat.JsonFormaterString();
                    return Request.CreateResponse(HttpStatusCode.OK, new Confirmation { output = "error", msg = "Please Select From Warehouse !!" }, formatter);

                }
                if (string.IsNullOrEmpty(toDeliveryModel.ToDeliveryMasterData.to_warehouse_id.ToString()))
                {
                    var formatter = RequestFormat.JsonFormaterString();
                    return Request.CreateResponse(HttpStatusCode.OK, new Confirmation { output = "error", msg = "Please Select To Warehouse !!" }, formatter);

                }
                else
                {

                    var x = toDeliveryRepository.AddToDelivery(toDeliveryModel);
                    if (x == 1)
                    {
                        var formatter = RequestFormat.JsonFormaterString();
                        return Request.CreateResponse(HttpStatusCode.OK, new Confirmation { output = "success", msg = "Transfered successfully" }, formatter);
                    }
                    else
                    {
                        var formatter = RequestFormat.JsonFormaterString();
                        return Request.CreateResponse(HttpStatusCode.OK, new Confirmation { output = "error", msg = "Error In Transfer !!!" }, formatter);
                    }
                }
            }
            catch (Exception ex)
            {
                var formatter = RequestFormat.JsonFormaterString();
                return Request.CreateResponse(HttpStatusCode.OK, new Confirmation { output = "error", msg = ex.ToString() }, formatter);
            }
        }

        [HttpPost, ActionName("CancelToDelivery")]
        public HttpResponseMessage CancelToDelivery([FromBody] ToDeliveryModel toDeliveryModel)
        {

            try
            {

                var x = toDeliveryRepository.CancelToDelivery(toDeliveryModel);
                if (x == 1)
                {
                    var formatter = RequestFormat.JsonFormaterString();
                    return Request.CreateResponse(HttpStatusCode.OK, new Confirmation { output = "success", msg = "Transfered Canceled successfully" }, formatter);
                }
                else
                {
                    var formatter = RequestFormat.JsonFormaterString();
                    return Request.CreateResponse(HttpStatusCode.OK, new Confirmation { output = "error", msg = "Error In Transfer Cancel !!!" }, formatter);
                }

            }
            catch (Exception ex)
            {
                var formatter = RequestFormat.JsonFormaterString();
                return Request.CreateResponse(HttpStatusCode.OK, new Confirmation { output = "error", msg = ex.ToString() }, formatter);
            }
        }

        [HttpPost, ActionName("RfdDelivery")]
        public HttpResponseMessage RfdDelivery([FromBody] ToDeliveryModel toDeliveryModel)
        {

            try
            {

                var x = toDeliveryRepository.RfdDelivery(toDeliveryModel);
                if (x == 1)
                {
                    var formatter = RequestFormat.JsonFormaterString();
                    return Request.CreateResponse(HttpStatusCode.OK, new Confirmation { output = "success", msg = "Transfered  successfully" }, formatter);
                }
                else
                {
                    var formatter = RequestFormat.JsonFormaterString();
                    return Request.CreateResponse(HttpStatusCode.OK, new Confirmation { output = "error", msg = "Error In Transfer !!!" }, formatter);
                }

            }
            catch (Exception ex)
            {
                var formatter = RequestFormat.JsonFormaterString();
                return Request.CreateResponse(HttpStatusCode.OK, new Confirmation { output = "error", msg = ex.ToString() }, formatter);
            }
        }

        //For purchase Order Report-----------------------------------------
        public object GetToDeliveryReportById(long to_delivery_master_id)
        {
            var data = toDeliveryRepository.GetToDeliveryReportById(to_delivery_master_id);
            var formatter = RequestFormat.JsonFormaterString();
            return Request.CreateResponse(HttpStatusCode.OK, data, formatter);
        }
    }
}