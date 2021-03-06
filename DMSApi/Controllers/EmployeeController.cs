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
    public class EmployeeController : ApiController
    {
        private IEmployeeRepository employeeRepository;

        public EmployeeController()
        {
            this.employeeRepository = new EmployeeRepository();
        }

        public EmployeeController(IEmployeeRepository employeeRepository)
        {
            this.employeeRepository = employeeRepository;
        }

        [HttpGet, ActionName("GetAllEmployees")]
        public HttpResponseMessage GetAllEmployees()
        {
            var countries = employeeRepository.GetAllEmployees();
            var formatter = RequestFormat.JsonFormaterString();
            return Request.CreateResponse(HttpStatusCode.OK, countries, formatter);
        }
        [HttpGet, ActionName("GetEmplyieesForDropdown")]
        public HttpResponseMessage GetEmplyieesForDropdown()
        {
            var emp = employeeRepository.GetEmplyieesForDropdown();
            var formatter = RequestFormat.JsonFormaterString();
            return Request.CreateResponse(HttpStatusCode.OK, emp, formatter);
        }
        //[HttpGet, ActionName("GetPartywiseEmplyieesForDropdown")]
        //public HttpResponseMessage GetPartywiseEmplyieesForDropdown(long party_id)
        //{
        //    var emp = employeeRepository.GetPartywiseEmplyieesForDropdown(party_id);
        //    var formatter = RequestFormat.JsonFormaterString();
        //    return Request.CreateResponse(HttpStatusCode.OK, emp, formatter);
        //}

        //[HttpGet, ActionName("GetDropdownForSalesTarget")]
        //public HttpResponseMessage GetDropdownForSalesTarget()
        //{
        //    var emp = employeeRepository.GetDropdownForSalesTarget();
        //    var formatter = RequestFormat.JsonFormaterString();
        //    return Request.CreateResponse(HttpStatusCode.OK, emp, formatter);
        //}
        //[HttpGet, ActionName("GetDropdownForPaymentRequest")]
        //public HttpResponseMessage GetDropdownForPaymentRequest(string party_type_name)
        //{
        //    var emp = employeeRepository.GetDropdownForPaymentRequest(party_type_name);
        //    var formatter = RequestFormat.JsonFormaterString();
        //    return Request.CreateResponse(HttpStatusCode.OK, emp, formatter);
        //}

        [HttpGet, ActionName("GetEmployeeByEmployeeName")]
        public HttpResponseMessage GetEmployeeByEmployeeName(string employee_name)
        {
            var location = employeeRepository.GetEmployeeByEmployeeName(employee_name);
            var formatter = RequestFormat.JsonFormaterString();
            return Request.CreateResponse(HttpStatusCode.OK, location, formatter);
        }

        [System.Web.Http.HttpPost]
        public HttpResponseMessage Post([FromBody] EmployeeModel employeeModel)
        {

            try
            {
                if (string.IsNullOrEmpty(employeeModel.employee_name))
                {
                    var formatter = RequestFormat.JsonFormaterString();
                    return Request.CreateResponse(HttpStatusCode.OK, new Confirmation { output = "warning", msg = "Please Select Employee Name" }, formatter);

                }
                if (string.IsNullOrEmpty(employeeModel.gender))
                {
                    var formatter = RequestFormat.JsonFormaterString();
                    return Request.CreateResponse(HttpStatusCode.OK, new Confirmation { output = "error", msg = "Please Select Gender" }, formatter);

                }

                if (string.IsNullOrEmpty(employeeModel.marital_status))
                {
                    var formatter = RequestFormat.JsonFormaterString();
                    return Request.CreateResponse(HttpStatusCode.OK, new Confirmation { output = "error", msg = "Please Select Marital Status" }, formatter);

                }
                if (string.IsNullOrEmpty(employeeModel.designation_id.ToString()))
                {
                    var formatter = RequestFormat.JsonFormaterString();
                    return Request.CreateResponse(HttpStatusCode.OK, new Confirmation { output = "error", msg = "Please Select designation" }, formatter);

                }
                if (string.IsNullOrEmpty(employeeModel.department_id.ToString()))
                {
                    var formatter = RequestFormat.JsonFormaterString();
                    return Request.CreateResponse(HttpStatusCode.OK, new Confirmation { output = "error", msg = "Please Select Department" }, formatter);

                }
                else
                {
                    EmployeeModel insertEmployee = new EmployeeModel
                    {
                        employee_name = employeeModel.employee_name,
                        date_of_birth = employeeModel.date_of_birth,
                        date_of_join = employeeModel.date_of_join,
                        department_id = employeeModel.department_id,
                        designation_id = employeeModel.designation_id,
                        email_address = employeeModel.email_address,
                        mobile_number = employeeModel.mobile_number,
                        address = employeeModel.address,
                        gender = employeeModel.gender,
                        marital_status = employeeModel.marital_status,
                        blood_group = employeeModel.blood_group,
                        region_id = employeeModel.region_id,
                        area_id = employeeModel.area_id,
                        territory_id = employeeModel.territory_id,
                        is_active = employeeModel.is_active,
                        is_user = employeeModel.is_user,
                        is_deleted = false,
                        created_date = DateTime.Now,
                        created_by = employeeModel.created_by,
                        updated_date = DateTime.Now,
                        login_name = employeeModel.login_name,
                        password = employeeModel.password,
                        role_id = employeeModel.role_id,
                        updated_by = employeeModel.updated_by
                    };


                    /* user table Modified by Kiron*/
                    employeeRepository.AddEmployee(insertEmployee);
                    /* user table Modified by Kiron*/

                    var formatter = RequestFormat.JsonFormaterString();
                    return Request.CreateResponse(HttpStatusCode.OK, new Confirmation { output = "success", msg = "Employee save successfully" }, formatter);
                }

            }
            catch (Exception ex)
            {
                var formatter = RequestFormat.JsonFormaterString();
                return Request.CreateResponse(HttpStatusCode.OK, new Confirmation { output = "error", msg = ex.ToString() }, formatter);
            }
        }


        [System.Web.Http.HttpPut]
        public HttpResponseMessage Put([FromBody] EmployeeModel employeeModel)
        {
            try
            {

                if (string.IsNullOrEmpty(employeeModel.employee_name))
                {
                    var formatter = RequestFormat.JsonFormaterString();
                    return Request.CreateResponse(HttpStatusCode.OK, new Confirmation { output = "warning", msg = "Please Select Employee Name" }, formatter);

                }
                if (string.IsNullOrEmpty(employeeModel.gender))
                {
                    var formatter = RequestFormat.JsonFormaterString();
                    return Request.CreateResponse(HttpStatusCode.OK, new Confirmation { output = "error", msg = "Please Select Gender" }, formatter);

                }
                if (string.IsNullOrEmpty(employeeModel.blood_group))
                {
                    var formatter = RequestFormat.JsonFormaterString();
                    return Request.CreateResponse(HttpStatusCode.OK, new Confirmation { output = "error", msg = "Please Select Blood group" }, formatter);

                }
                if (string.IsNullOrEmpty(employeeModel.marital_status))
                {
                    var formatter = RequestFormat.JsonFormaterString();
                    return Request.CreateResponse(HttpStatusCode.OK, new Confirmation { output = "error", msg = "Please Select Marital Status" }, formatter);

                }
                if (string.IsNullOrEmpty(employeeModel.designation_id.ToString()))
                {
                    var formatter = RequestFormat.JsonFormaterString();
                    return Request.CreateResponse(HttpStatusCode.OK, new Confirmation { output = "error", msg = "Please Select designation" }, formatter);

                }
                if (string.IsNullOrEmpty(employeeModel.department_id.ToString()))
                {
                    var formatter = RequestFormat.JsonFormaterString();
                    return Request.CreateResponse(HttpStatusCode.OK, new Confirmation { output = "error", msg = "Please Select Department" }, formatter);

                }
                else
                {
                    EmployeeModel updateEmployee = new EmployeeModel
                    {
                        employee_id = employeeModel.employee_id,
                        employee_name = employeeModel.employee_name,
                        date_of_birth = employeeModel.date_of_birth,
                        date_of_join = employeeModel.date_of_join,
                        department_id = employeeModel.department_id,
                        designation_id = employeeModel.designation_id,
                        email_address = employeeModel.email_address,
                        mobile_number = employeeModel.mobile_number,
                        address = employeeModel.address,
                        gender = employeeModel.gender,
                        marital_status = employeeModel.marital_status,
                        blood_group = employeeModel.blood_group,
                        region_id = employeeModel.region_id,
                        area_id = employeeModel.area_id,
                        territory_id = employeeModel.territory_id,
                        is_active = employeeModel.is_active,
                        is_user = employeeModel.is_user,
                        user_id = employeeModel.user_id,
                        chk_pass = employeeModel.chk_pass,
                        updated_date = DateTime.Now,
                        login_name = employeeModel.login_name,
                        password = employeeModel.password,
                        role_id = employeeModel.role_id,
                        updated_by = employeeModel.updated_by
                    };
                    employeeRepository.EditEmployee(updateEmployee);

                    var formatter = RequestFormat.JsonFormaterString();
                    return Request.CreateResponse(HttpStatusCode.OK, new Confirmation { output = "success", msg = "Employee update successfully" }, formatter);
                }
                }
            catch (Exception ex)
            {
                var formatter = RequestFormat.JsonFormaterString();
                return Request.CreateResponse(HttpStatusCode.OK, new Confirmation { output = "error", msg = ex.ToString() }, formatter);
            }
        }

        [System.Web.Http.HttpDelete]
        public HttpResponseMessage Delete([FromBody]Models.employee employee)
        {
            try
            {
               
                bool updatCountry = employeeRepository.DeleteEmployee(employee.employee_id);

                var formatter = RequestFormat.JsonFormaterString();
                return Request.CreateResponse(HttpStatusCode.OK, new Confirmation { output = "success", msg = "Employee Delete Successfully." }, formatter);
            }
            catch (Exception ex)
            {
                var formatter = RequestFormat.JsonFormaterString();
                return Request.CreateResponse(HttpStatusCode.OK, new Confirmation { output = "error", msg = ex.ToString() }, formatter);
            }

        }

        [ActionName("GetEmployeeById")]
        [System.Web.Http.HttpPost]
        public HttpResponseMessage GetEmployeeById([FromBody]Models.employee employee)
        {
            var employeeId = employee.employee_id;

            var emp = employeeRepository.GetEmployeeById(employeeId);
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, emp);
            return response;
        }
    }
}