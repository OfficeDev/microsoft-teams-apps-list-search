// <copyright file="UserController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace ConfigApp.Controllers
{
    using System.Collections.Generic;
    using System.Web.Mvc;
    using ConfigApp.Models;

    /// <summary>
    /// UserController
    /// </summary>
    [Authorize]
    public class UserController : Controller
    {
        /// <summary>
        /// TableStorageHelper
        /// </summary>
        private TableStorageHelper tableStorageHelper = new TableStorageHelper();

        /// <summary>
        /// To get users list
        /// </summary>
        /// <returns>view</returns>
        public ActionResult UsersList()
        {
            List<UserEntity> users = this.tableStorageHelper.GetTableDataByPartitionKey<UserEntity>("Users", "Admin");

            return this.View(users);
        }

        /// <summary>
        /// To add user
        /// </summary>
        /// <returns>view</returns>
        public ActionResult AddUser()
        {
            return this.View();
        }

        /// <summary>
        /// To save user
        /// </summary>
        /// <param name="formCollection">formCollection</param>
        /// <returns>to SaveUser</returns>
        public ViewResult SaveUser(FormCollection formCollection)
        {
            UserEntity userEntity = new UserEntity()
            {
                RowKey = formCollection["email"],
                PartitionKey = formCollection["selRole"],
                FirstName = formCollection["firstName"],
                LastName = formCollection["lastName"]
            };
            this.tableStorageHelper.AddRow<UserEntity>("Users", userEntity);
            List<UserEntity> users = this.tableStorageHelper.GetTableDataByPartitionKey<UserEntity>("Users", "Admin");
            return this.View("UsersList", users);
        }

        /// <summary>
        /// RemoveUser
        /// </summary>
        /// <param name="userEntity">UserEntity</param>
        /// <returns>view</returns>
        public ActionResult RemoveUser(UserEntity userEntity)
        {
            this.tableStorageHelper.DeleteRow<UserEntity>("Users", userEntity.RowKey, userEntity.PartitionKey);
            List<UserEntity> users = this.tableStorageHelper.GetTableDataByPartitionKey<UserEntity>("Users", "Admin");
            return this.View("UsersList", users);
        }
    }
}