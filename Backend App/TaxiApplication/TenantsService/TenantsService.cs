using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.DTOs;
using Common.Entities;
using Common.Enumerations;
using Common.Helpers;
using Common.Interfaces;
using Common.Mappers;
using Common.Models;
using Common.Repositories;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;

namespace TenantsService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class TenantsService : StatefulService,ITenantsService,IRating
    {

        private TenantsRepository dataRepo;
        private BlobHelper blobHelp;

        public TenantsService(StatefulServiceContext context)
            : base(context)
        {

            dataRepo = new TenantsRepository("TenantsTable");
            blobHelp= new BlobHelper();
            
        }

        public async Task<bool> AddNewUser(Tenant user)
        {
            var userDictionary = await StateManager.GetOrAddAsync<IReliableDictionary<Guid, Tenant>>("TenantEntities");

            try
            {
                using (var transaction = StateManager.CreateTransaction())
                {
                    if (!await CheckIfTenantAlreadyExists(user))
                    {

                        await userDictionary.AddAsync(transaction, user.Id, user);

                        //insert image of user in blob
                        CloudBlockBlob blob = blobHelp.GetBlockBlobReference("tenants", $"image_{user.Id}");
                        blob.Properties.ContentType = user.ImageFile.ContentType;
                        await blob.UploadFromByteArrayAsync(user.ImageFile.FileContent, 0, user.ImageFile.FileContent.Length);
                        string imageUrl = blob.Uri.AbsoluteUri;

                        //insert user in database
                        TenantEntity newUser = new TenantEntity(user, imageUrl);
                        TableOperation operation = TableOperation.Insert(newUser);
                        await dataRepo.Users.ExecuteAsync(operation);




                        await transaction.CommitAsync();
                        return true;
                    }
                    return false;
                }

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> ChangeDriverStatus(Guid id, bool status)
        {
            var users = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, Tenant>>("TenantEntities");
            using (var tx = this.StateManager.CreateTransaction())
            {
                ConditionalValue<Tenant> result = await users.TryGetValueAsync(tx, id);
                if (result.HasValue)
                {
                    Tenant user = result.Value;
                    user.IsBlocked = status;
                    await users.SetAsync(tx, id, user);

                    await dataRepo.UpdateEntity(id, status);

                    await tx.CommitAsync();

                    return true;
                }
                else return false;


            }
        }

        public async Task<TenantDTO> ChangeUserFields(UserForUpdateOverNetwork user)
        {
            var users = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, Tenant>>("TenantEntities");
            using (var tx = this.StateManager.CreateTransaction())
            {
                ConditionalValue<Tenant> result = await users.TryGetValueAsync(tx, user.Id);
                if (result.HasValue)
                {
                    Tenant userFromReliable = result.Value;

                    if (!string.IsNullOrEmpty(user.Email)) userFromReliable.Email = user.Email;

                    if (!string.IsNullOrEmpty(user.FirstName)) userFromReliable.FirstName = user.FirstName;

                    if (!string.IsNullOrEmpty(user.LastName)) userFromReliable.LastName = user.LastName;

                    if (!string.IsNullOrEmpty(user.Address)) userFromReliable.Address = user.Address;

                    if (user.Birthday != DateTime.MinValue) userFromReliable.Birthday = user.Birthday;

                    if (!string.IsNullOrEmpty(user.Password)) userFromReliable.Password = user.Password;

                    if (!string.IsNullOrEmpty(user.Username)) userFromReliable.Username = user.Username;

                    if (user.ImageFile != null && user.ImageFile.FileContent != null && user.ImageFile.FileContent.Length > 0) userFromReliable.ImageFile = user.ImageFile;

                    await users.TryRemoveAsync(tx, user.Id); 

                    await users.AddAsync(tx, userFromReliable.Id, userFromReliable);

                    if (user.ImageFile != null && user.ImageFile.FileContent != null && user.ImageFile.FileContent.Length > 0) 
                    {
                        CloudBlockBlob blob = blobHelp.GetBlockBlobReference("tenants", $"image_{userFromReliable.Id}"); 
                        await blob.DeleteIfExistsAsync(); 

                        CloudBlockBlob newblob = blobHelp.GetBlockBlobReference("users", $"image_{userFromReliable.Id}"); 
                        newblob.Properties.ContentType = userFromReliable.ImageFile.ContentType;
                        await newblob.UploadFromByteArrayAsync(userFromReliable.ImageFile.FileContent, 0, userFromReliable.ImageFile.FileContent.Length); 
                    }

                    await dataRepo.UpdateUser(user, userFromReliable); 
                    await tx.CommitAsync();
                    return TenantMapper.MapTenantToTenantDto(userFromReliable);

                }
                else return null;
            }
        }

        public async Task<List<DriverViewDTO>> GetNotVerifiedDrivers()
        {
            var users = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, Tenant>>("TenantEntities");
            List<DriverViewDTO> drivers = new List<DriverViewDTO>();
            using (var tx = this.StateManager.CreateTransaction())
            {
                var enumerable = await users.CreateEnumerableAsync(tx);
                using (var enumerator = enumerable.GetAsyncEnumerator())
                {
                    while (await enumerator.MoveNextAsync(default(CancellationToken)))
                    {
                        if (enumerator.Current.Value.TypeOfUser == PolicyRoles.Driver && enumerator.Current.Value.Status != Status.Odbijen)
                        {
                            drivers.Add(new DriverViewDTO(enumerator.Current.Value.Email, enumerator.Current.Value.FirstName, enumerator.Current.Value.LastName, enumerator.Current.Value.Username, enumerator.Current.Value.IsBlocked, enumerator.Current.Value.AverageRating, enumerator.Current.Value.Id, enumerator.Current.Value.Status));
                        }
                    }
                }
            }

            return drivers;
        }

        public async Task<TenantDTO> GetUserInfo(Guid id)
        {
            var users = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, Tenant>>("TenantEntities");
            using (var tx = this.StateManager.CreateTransaction())
            {
                ConditionalValue<Tenant> result = await users.TryGetValueAsync(tx, id);
                if (result.HasValue)
                {
                    TenantDTO user = TenantMapper.MapTenantToTenantDto(result.Value);
                    return user;
                }
                else return new TenantDTO();


            }
        }

        public async Task<List<DriverViewDTO>> ListDrivers()
        {
            var users = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, Tenant>>("TenantEntities");
            List<DriverViewDTO> drivers = new List<DriverViewDTO>();
            using (var tx = this.StateManager.CreateTransaction())
            {
                var enumerable = await users.CreateEnumerableAsync(tx);
                using (var enumerator = enumerable.GetAsyncEnumerator())
                {
                    while (await enumerator.MoveNextAsync(default(CancellationToken)))
                    {
                        if (enumerator.Current.Value.TypeOfUser == PolicyRoles.Driver)
                        {
                            drivers.Add(new DriverViewDTO(enumerator.Current.Value.Email, enumerator.Current.Value.FirstName, enumerator.Current.Value.LastName, enumerator.Current.Value.Username, enumerator.Current.Value.IsBlocked, enumerator.Current.Value.AverageRating, enumerator.Current.Value.Id, enumerator.Current.Value.Status));
                        }
                    }
                }
            }

            return drivers;
        }

        public async Task<List<TenantDTO>> ListUsers()
        {
            var users = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, Tenant>>("TenantEntities");

            List<TenantDTO> userList = new List<TenantDTO>();

            using (var tx = this.StateManager.CreateTransaction())
            {
                var enumerable = await users.CreateEnumerableAsync(tx);

                using (var enumerator = enumerable.GetAsyncEnumerator())
                {
                    while (await enumerator.MoveNextAsync(default(CancellationToken)))
                    {
                        userList.Add(TenantMapper.MapTenantToTenantDto(enumerator.Current.Value));
                    }
                }
            }

            return userList;
        }

        public async Task<LogedTenantDTO> LoginUser(LoginTenantDTO loginUserDTO)
        {
            var users = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, Tenant>>("TenantEntities");

            using (var tx = this.StateManager.CreateTransaction())
            {
                var enumerable = await users.CreateEnumerableAsync(tx);

                using (var enumerator = enumerable.GetAsyncEnumerator())
                {
                    while (await enumerator.MoveNextAsync(default(CancellationToken)))
                    {
                        if (enumerator.Current.Value.Email == loginUserDTO.Email && enumerator.Current.Value.Password == loginUserDTO.Password)
                        {
                            return new LogedTenantDTO(enumerator.Current.Value.Id, enumerator.Current.Value.TypeOfUser);
                        }
                    }
                }
            }
            return null;
        }

        public async Task<bool> VerifyDriver(Guid id, string email, string action)
        {
            var users = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, Tenant>>("TenantEntities");
            using (var tx = this.StateManager.CreateTransaction())
            {
                ConditionalValue<Tenant> result = await users.TryGetValueAsync(tx, id);
                if (result.HasValue)
                {
                    Tenant userForChange = result.Value;
                    if (action == "Prihvacen")
                    {
                        userForChange.IsVerified = true;
                        userForChange.Status = Status.Prihvacen;
                    }
                    else userForChange.Status = Status.Odbijen;

                    await users.SetAsync(tx, id, userForChange);

                    await dataRepo.UpdateDriverStatus(id, action);

                    await tx.CommitAsync();
                    return true;

                }
                else return false;
            }
        }

        //Dodati jos IRating i odavde nastaviti dalje

        public async Task<bool> AddRating(Guid driverId, int rating)
        {
            var users = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, Tenant>>("TenantEntities");
            bool operation = false;
            using (var tx = this.StateManager.CreateTransaction())
            {
                var enumerable = await users.CreateEnumerableAsync(tx);
                using (var enumerator = enumerable.GetAsyncEnumerator())
                {
                    while (await enumerator.MoveNextAsync(default(CancellationToken)))
                    {
                        if (enumerator.Current.Value.Id == driverId)
                        {
                            var user = enumerator.Current.Value;
                            user.NumOfRatings++;
                            user.SumOfRatings += rating;
                            user.AverageRating = (double)user.SumOfRatings / (double)user.NumOfRatings;


                            await users.SetAsync(tx, driverId, user); 

                            await dataRepo.UpdateDriverRating(user.Id, user.SumOfRatings, user.NumOfRatings, user.AverageRating);  // update user in db

                            await tx.CommitAsync();

                            operation = true;
                            break;
                        }
                    }
                }
            }

            return operation;
        }

        #region UtilityMethods

        private async Task<bool> CheckIfTenantAlreadyExists(Tenant user)
        {
            var users = await StateManager.GetOrAddAsync<IReliableDictionary<Guid, Tenant>>("TenantEntities");
            try
            {
                using (var tx = StateManager.CreateTransaction())
                {
                    var enumerable = await users.CreateEnumerableAsync(tx);

                    using (var enumerator = enumerable.GetAsyncEnumerator())
                    {
                        while (await enumerator.MoveNextAsync(default(CancellationToken)))
                        {
                            if (enumerator.Current.Value.Email == user.Email || enumerator.Current.Value.Id == user.Id || enumerator.Current.Value.Username == user.Username)
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task LoadTenants()
        {
            var userDictionary = await StateManager.GetOrAddAsync<IReliableDictionary<Guid, Tenant>>("TenantEntities");
            try
            {
                using (var transaction = StateManager.CreateTransaction())
                {
                    var users = dataRepo.GetAllUsers();
                    if (users.Count() == 0) return;
                    else
                    {
                        foreach (var user in users)
                        {
                            byte[] image = await blobHelp.DownloadImage(user.Id.ToString(),"tenants");
                            await userDictionary.AddAsync(transaction, user.Id, TenantMapper.MapUserEntityToUser(user, image));
                        }
                    }

                    await transaction.CommitAsync();

                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        => this.CreateServiceRemotingReplicaListeners();

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("myDictionary");
            var tenants = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, Tenant>>("TenantEntities");
            await LoadTenants();


            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (var tx = this.StateManager.CreateTransaction())
                {
                    var result = await myDictionary.TryGetValueAsync(tx, "Counter");

                    ServiceEventSource.Current.ServiceMessage(this.Context, "Current Counter Value: {0}",
                        result.HasValue ? result.Value.ToString() : "Value does not exist.");

                    await myDictionary.AddOrUpdateAsync(tx, "Counter", 0, (key, value) => ++value);

                    // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
                    // discarded, and nothing is saved to the secondary replicas.
                    await tx.CommitAsync();
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }

        
    }
}
