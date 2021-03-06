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

namespace DMSApi.Controllers
{
    //[EnableCors(origins: "*", headers: "*", methods: "*")]
    public class NotifierMailAccountController : ApiController
    {
        private INotifierMailAccountRepository notifierMailAccountRepository;

        public NotifierMailAccountController()
        {
            this.notifierMailAccountRepository = new NotifierMailAccountRepository();
        }

        public NotifierMailAccountController(INotifierMailAccountRepository notifierMailAccountRepository)
        {
            this.notifierMailAccountRepository = notifierMailAccountRepository;
        }

        [ActionName("GetNotifierMailAccount")]
        public HttpResponseMessage GetNotifierMailAccount()
        {
            var data = notifierMailAccountRepository.GetNotifierMailAccount();
            var formatter = RequestFormat.JsonFormaterString();
            return Request.CreateResponse(HttpStatusCode.OK, data, formatter);
        }

        [ActionName("GetActiveNotifierMailAccount")]
        public HttpResponseMessage GetActiveNotifierMailAccount()
        {
            var data = notifierMailAccountRepository.GetActiveNotifierMailAccount();
            var formatter = RequestFormat.JsonFormaterString();
            return Request.CreateResponse(HttpStatusCode.OK, data, formatter);
        }

        [HttpPost]
        public HttpResponseMessage Post([FromBody]Models.notifier_mail_account notifier_mail_account)
        {
            try
            {

                Models.notifier_mail_account insertNotifierMailAccount = new Models.notifier_mail_account
                {

                    account_title = notifier_mail_account.account_title,
                    account_email = notifier_mail_account.account_email,
                    accoutn_password = notifier_mail_account.accoutn_password,
                    is_active = true,
                    is_deleted = false,
                    created_by = notifier_mail_account.created_by,
                    created_date = DateTime.Now,
                    updated_by = notifier_mail_account.updated_by,
                    updated_date = DateTime.Now
                };
                bool save = notifierMailAccountRepository.InsertNotifierMailAccount(insertNotifierMailAccount);

                var formatter = RequestFormat.JsonFormaterString();
                return Request.CreateResponse(HttpStatusCode.OK, new Confirmation { output = "success", msg = "Account save successfully" }, formatter);


            }
            catch (Exception ex)
            {
                var formatter = RequestFormat.JsonFormaterString();
                return Request.CreateResponse(HttpStatusCode.OK, new Confirmation { output = "error", msg = ex.ToString() }, formatter);
            }
        }

        [HttpPut]
        public HttpResponseMessage Put([FromBody]Models.notifier_mail_account notifier_mail_account)
        {
            try
            {

                Models.notifier_mail_account updateNotifierMailAccount = new Models.notifier_mail_account

                    {
                        notifier_mail_account_id = notifier_mail_account.notifier_mail_account_id,
                        account_title = notifier_mail_account.account_title,
                        account_email = notifier_mail_account.account_email,
                        accoutn_password = notifier_mail_account.accoutn_password,
                        is_active = notifier_mail_account.is_active,
                        updated_by = notifier_mail_account.updated_by,
                        updated_date = DateTime.Now

                    };

                bool irepoUpdate = notifierMailAccountRepository.UpdateNotifierMailAccount(updateNotifierMailAccount);

                if (irepoUpdate == true)
                {
                    var formatter = RequestFormat.JsonFormaterString();
                    return Request.CreateResponse(HttpStatusCode.OK, new Confirmation { output = "success", msg = "Account Update successfully" }, formatter);
                }
                else
                {
                    var formatter = RequestFormat.JsonFormaterString();
                    return Request.CreateResponse(HttpStatusCode.OK, new Confirmation { output = "success", msg = "Update Failed" }, formatter);
                }

            }
            catch (Exception ex)
            {
                var formatter = RequestFormat.JsonFormaterString();
                return Request.CreateResponse(HttpStatusCode.OK, new Confirmation { output = "error", msg = ex.ToString() }, formatter);
            }
        }

        [HttpDelete]
        public HttpResponseMessage Delete([FromBody]Models.notifier_mail_account notifier_mail_account)
        {
            try
            {
                bool updateNotifierMailAccount = notifierMailAccountRepository.DeleteNotifierMailAccount(notifier_mail_account.notifier_mail_account_id);

                var formatter = RequestFormat.JsonFormaterString();
                return Request.CreateResponse(HttpStatusCode.OK, new Confirmation { output = "success", msg = "Account Delete Successfully." }, formatter);
            }
            catch (Exception ex)
            {
                var formatter = RequestFormat.JsonFormaterString();
                return Request.CreateResponse(HttpStatusCode.OK, new Confirmation { output = "error", msg = ex.ToString() }, formatter);
            }
        }
    }
}