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

    public class OpeningBalanceCreditLimitController : ApiController
     {
         private IOpeningBalanceCreditLimitRepository openingbalancecreditlimitRepository;

         private OpeningBalanceCreditLimitController()
         {
             this.openingbalancecreditlimitRepository=new OpeningBalanceCreditLimitRepository();
         }
         public HttpResponseMessage GetOpeningBalanceCreditLimit()
         {
             var BalanceCreditLimit = openingbalancecreditlimitRepository.GetOpeningBalanceCreditLimit();
             var formatter = RequestFormat.JsonFormaterString();
             return Request.CreateResponse(HttpStatusCode.OK, BalanceCreditLimit, formatter);
         }

        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}