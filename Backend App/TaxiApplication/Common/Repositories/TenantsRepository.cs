using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Entities;
using Microsoft.WindowsAzure.Storage.Blob;
using Common.Models;

namespace Common.Repositories
{
    public class TenantsRepository
    {

        private CloudStorageAccount cloudAcc;
        private CloudTableClient tableClient;
        private CloudTable users;
        

        public CloudStorageAccount CloudAcc { get => cloudAcc; set => cloudAcc = value; }
        public CloudTableClient TableClient { get => tableClient; set => tableClient = value; }
        public CloudTable Users { get => users; set => users = value; }

        public TenantsRepository(string tableName)
        {
            
            
            CloudAcc = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("DataConnectionString"));
            TableClient = CloudAcc.CreateCloudTableClient(); 
            Users = TableClient.GetTableReference(tableName); 
            Users.CreateIfNotExistsAsync().GetAwaiter().GetResult();

        }


        public IEnumerable<TenantEntity> GetAllUsers()
        {
            var q = new TableQuery<TenantEntity>();
            var qRes = Users.ExecuteQuerySegmentedAsync(q, null).GetAwaiter().GetResult();
            return qRes.Results;
        }

        public async Task<bool> UpdateEntity(Guid id, bool status)
        {
            TableQuery<TenantEntity> driverQuery = new TableQuery<TenantEntity>()
        .Where(TableQuery.GenerateFilterConditionForGuid("Id", QueryComparisons.Equal, id));
            TableQuerySegment<TenantEntity> queryResult = await Users.ExecuteQuerySegmentedAsync(driverQuery, null);

            if (queryResult.Results.Count > 0)
            {
                TenantEntity user = queryResult.Results[0];
                user.IsBlocked = status;
                var operation = TableOperation.Replace(user);
                await Users.ExecuteAsync(operation);

                return true;
            }
            else
            {
                return false;
            }
        }


        public async Task UpdateDriverStatus(Guid id, string status)
        {
            TableQuery<TenantEntity> usersQuery = new TableQuery<TenantEntity>()
            .Where(TableQuery.GenerateFilterConditionForGuid("Id", QueryComparisons.Equal, id));
            TableQuerySegment<TenantEntity> queryResult = await Users.ExecuteQuerySegmentedAsync(usersQuery, null);


            if (queryResult.Results.Count > 0)
            {
                TenantEntity userFromTable = queryResult.Results[0];
                userFromTable.Status = status;
                if (status == "Prihvacen") userFromTable.IsVerified = true;
                else userFromTable.IsVerified = false;
                var operation = TableOperation.Replace(userFromTable);
                await Users.ExecuteAsync(operation);

            }

        }

        public async Task UpdateDriverRating(Guid id, int sumOfRating, int numOfRating, double averageRating)
        {
            TableQuery<TenantEntity> usersQuery = new TableQuery<TenantEntity>()
            .Where(TableQuery.GenerateFilterConditionForGuid("Id", QueryComparisons.Equal, id));
            TableQuerySegment<TenantEntity> queryResult = await Users.ExecuteQuerySegmentedAsync(usersQuery, null);


            if (queryResult.Results.Count > 0)
            {
                TenantEntity userFromTable = queryResult.Results[0];
                userFromTable.SumOfRatings = sumOfRating;
                userFromTable.NumOfRatings = numOfRating;
                userFromTable.AverageRating = averageRating;
                var operation = TableOperation.Replace(userFromTable);
                await Users.ExecuteAsync(operation);

            }

        }


        public async Task UpdateUser(UserForUpdateOverNetwork userOverNetwork, Tenant u)//Dovde se stiglo
        {

            TableQuery<TenantEntity> usersQuery = new TableQuery<TenantEntity>()
       .Where(TableQuery.GenerateFilterConditionForGuid("Id", QueryComparisons.Equal, userOverNetwork.Id));

            TableQuerySegment<TenantEntity> queryResult = await Users.ExecuteQuerySegmentedAsync(usersQuery, null);

            if (queryResult.Results.Count > 0)
            {
                TenantEntity userFromTable = queryResult.Results[0];
                userFromTable.Email = u.Email;
                userFromTable.FirstName = u.FirstName;
                userFromTable.LastName = u.LastName;
                userFromTable.Address = u.Address;
                userFromTable.Birthday = u.Birthday;
                userFromTable.Username = u.Username;
                userFromTable.Username = u.Username;
                userFromTable.ImageUrl = u.ImageUrl;
                var operation = TableOperation.Replace(userFromTable);
                await Users.ExecuteAsync(operation);
            }
        }






    }
}
