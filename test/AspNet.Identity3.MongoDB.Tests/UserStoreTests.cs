﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using MongoDB.Driver;
using Xunit;

namespace AspNet.Identity3.MongoDB.Tests
{
	public class UserStoreTests : IDisposable
	{
		#region Contructor, Dispose and helper properties

		private readonly DatabaseFixture _databaseFixture;
		private readonly IMongoCollection<IdentityUser> _userCollection;
		private readonly string _collectionPrefix;

		private readonly UserStore<IdentityUser, IdentityRole> _userStore;
		private readonly IdentityErrorDescriber _errorDescriber;

		private readonly Claim _claim1;
		private readonly Claim _claim2;
		private readonly Claim _claim3;

		private readonly Claim _claim1SameType;

		private readonly IdentityClaim _identityClaim1;
		private readonly IdentityClaim _identityClaim2;
		private readonly IdentityClaim _identityClaim3;

		private readonly IdentityClaim _identityClaim1SameType;

		public UserStoreTests(string collectionPrefix)
		{
			_databaseFixture = new DatabaseFixture(collectionPrefix);
			_userCollection = _databaseFixture.GetCollection<IdentityUser>();
			_collectionPrefix = collectionPrefix;

			_errorDescriber = new IdentityErrorDescriber();
			_userStore = new UserStore<IdentityUser, IdentityRole>(_userCollection, _errorDescriber);


			_claim1 = new Claim("ClaimType1", "some value");
			_claim2 = new Claim("ClaimType2", "some other value");
			_claim3 = new Claim("other type", "some other value");

			_claim1SameType = new Claim(_claim1.Type, _claim1.Value + " different");

			_identityClaim1 = new IdentityClaim { ClaimType = _claim1.Type, ClaimValue = _claim1.Value };
			_identityClaim2 = new IdentityClaim { ClaimType = _claim2.Type, ClaimValue = _claim2.Value };
			_identityClaim3 = new IdentityClaim { ClaimType = _claim3.Type, ClaimValue = _claim3.Value };

			_identityClaim1SameType = new IdentityClaim { ClaimType = _claim1SameType.Type, ClaimValue = _claim1SameType.Value };
		}

		public void Dispose()
		{
			_databaseFixture.Dispose();
		}

		#endregion

		public class Constructors : UserStoreTests
		{
			public Constructors() : base(typeof(Constructors).Name) { }

			[Fact]
			public void Can_inisialise_from_connectionString()
			{
				// arrange
				string collectionName = "TestColName";
				var UserStore = new UserStoreHelper(_databaseFixture.ConnectionString, _databaseFixture.DatabaseName, collectionName);

				// assert
				Assert.Equal(_databaseFixture.DatabaseName, UserStore.DatabaseName);
				Assert.Equal(collectionName, UserStore.CollectionName);

				Assert.NotNull(UserStore.MongoClient);
				Assert.NotNull(UserStore.MongoDatabase);
				Assert.NotNull(UserStore.MongoCollection);

				Assert.Equal(_databaseFixture.DatabaseName, UserStore.MongoDatabase.DatabaseNamespace.DatabaseName);
				Assert.Equal(_databaseFixture.DatabaseName, UserStore.MongoCollection.CollectionNamespace.DatabaseNamespace.DatabaseName);
				Assert.Equal(collectionName, UserStore.MongoCollection.CollectionNamespace.CollectionName);
			}

			[Fact]
			public void Can_inisialise_from_connectionString_with_default_db_and_collection_names()
			{
				// arrange
				var UserStore = new UserStoreHelper(_databaseFixture.ConnectionString);

				// assert
				string defaultDatabaseName = "AspNetIdentity";
				string defaultCollectionName = "AspNetUsers";
				Assert.Equal(defaultDatabaseName, UserStore.DatabaseName);
				Assert.Equal(defaultCollectionName, UserStore.CollectionName);

				Assert.Equal(defaultDatabaseName, UserStore.MongoDatabase.DatabaseNamespace.DatabaseName);
				Assert.Equal(defaultDatabaseName, UserStore.MongoCollection.CollectionNamespace.DatabaseNamespace.DatabaseName);
				Assert.Equal(defaultCollectionName, UserStore.MongoCollection.CollectionNamespace.CollectionName);
			}

			[Fact]
			public void Can_inisialise_from_MongoClient()
			{
				// arrange
				string collectionName = "TestColName";
				var UserStore = new UserStoreHelper(_databaseFixture.GetMongoClient(), _databaseFixture.DatabaseName, collectionName);

				// assert
				Assert.Equal(_databaseFixture.DatabaseName, UserStore.DatabaseName);
				Assert.Equal(collectionName, UserStore.CollectionName);

				Assert.NotNull(UserStore.MongoClient);
				Assert.NotNull(UserStore.MongoDatabase);
				Assert.NotNull(UserStore.MongoCollection);

				Assert.Equal(_databaseFixture.DatabaseName, UserStore.MongoDatabase.DatabaseNamespace.DatabaseName);
				Assert.Equal(_databaseFixture.DatabaseName, UserStore.MongoCollection.CollectionNamespace.DatabaseNamespace.DatabaseName);
				Assert.Equal(collectionName, UserStore.MongoCollection.CollectionNamespace.CollectionName);
			}

			[Fact]
			public void Can_inisialise_from_MongoDatabase()
			{
				// arrange
				string collectionName = "TestColName";
				var UserStore = new UserStoreHelper(_databaseFixture.GetMongoDatabase(), collectionName);

				// assert
				Assert.Equal(_databaseFixture.DatabaseName, UserStore.DatabaseName);
				Assert.Equal(collectionName, UserStore.CollectionName);

				Assert.NotNull(UserStore.MongoClient);
				Assert.NotNull(UserStore.MongoDatabase);
				Assert.NotNull(UserStore.MongoCollection);

				Assert.Equal(_databaseFixture.DatabaseName, UserStore.MongoDatabase.DatabaseNamespace.DatabaseName);
				Assert.Equal(_databaseFixture.DatabaseName, UserStore.MongoCollection.CollectionNamespace.DatabaseNamespace.DatabaseName);
				Assert.Equal(collectionName, UserStore.MongoCollection.CollectionNamespace.CollectionName);
			}

			[Fact]
			public void Can_inisialise_from_MongoCollection()
			{
				// arrange
				string collectionName = _userCollection.CollectionNamespace.CollectionName;
				var UserStore = new UserStoreHelper(_userCollection);

				// assert
				Assert.Equal(_databaseFixture.DatabaseName, UserStore.DatabaseName);
				Assert.Equal(collectionName, UserStore.CollectionName);

				Assert.NotNull(UserStore.MongoClient);
				Assert.NotNull(UserStore.MongoDatabase);
				Assert.NotNull(UserStore.MongoCollection);

				Assert.Equal(_databaseFixture.DatabaseName, UserStore.MongoDatabase.DatabaseNamespace.DatabaseName);
				Assert.Equal(_databaseFixture.DatabaseName, UserStore.MongoCollection.CollectionNamespace.DatabaseNamespace.DatabaseName);
				Assert.Equal(collectionName, UserStore.MongoCollection.CollectionNamespace.CollectionName);
			}

		}

		public class IUserStoreTests : UserStoreTests
		{
			public IUserStoreTests(string collectionPrefix) : base(collectionPrefix) { }

			public class CreateAsyncMethod : IUserStoreTests
			{
				public CreateAsyncMethod() : base(typeof(CreateAsyncMethod).Name) { }

				[Fact]
				public async Task Create_user_returns_Success()
				{
					// arrange
					var user = new IdentityUser("Create_user_returns_Success");

					// act
					var result = await _userStore.CreateAsync(user);

					// assert
					IdentityResultAssert.IsSuccess(result);

					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					IdentityUserAssert.Equal(user, userFromDb);
				}


				[Fact]
				public async Task Creating_same_user_twice_returns_DuplicateUserName_error()
				{
					// arrange
					var user = new IdentityUser("Creating_same_user_twice_returns_DuplicateUserName_error");

					// act
					var result1 = await _userStore.CreateAsync(user);

					user.UserName = "a different name, but same Id";
					var result2 = await _userStore.CreateAsync(user);

					// assert
					IdentityResultAssert.IsSuccess(result1);

					var expectedError = IdentityResult.Failed(_errorDescriber.DuplicateUserName(user.ToString()));
					IdentityResultAssert.IsFailure(result2, expectedError.Errors.FirstOrDefault());
				}

				[Fact]
				public async Task Creating_two_different_users_but_same_UserName_returns_DuplicateUserName_error()
				{
					// arrange
					var user1 = new IdentityUser("Creating_two_different_users_but_same_UserName_returns_DuplicateUserName_error");
					var user2 = new IdentityUser(user1.UserName);

					// act
					var result1 = await _userStore.CreateAsync(user1);
					var result2 = await _userStore.CreateAsync(user2);

					// assert
					IdentityResultAssert.IsSuccess(result1);

					var expectedError = IdentityResult.Failed(_errorDescriber.DuplicateUserName(user2.ToString()));
					IdentityResultAssert.IsFailure(result2, expectedError.Errors.FirstOrDefault());
				}
			}

			public class UpdateAsyncMethod : IUserStoreTests
			{
				public UpdateAsyncMethod() : base(typeof(UpdateAsyncMethod).Name) { }

				[Fact]
				public async Task Update_user_returns_Success()
				{
					// arrange
					var user = new IdentityUser("Update_user_returns_Success");
					user.Claims.Add(_identityClaim1);

					// initial user creation
					await _userStore.CreateAsync(user);
					user.UserName = user.UserName + " different";
					user.Claims.Add(_identityClaim2);

					// act
					var result = await _userStore.UpdateAsync(user);

					// assert
					IdentityResultAssert.IsSuccess(result);

					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					IdentityUserAssert.Equal(user, userFromDb);
				}


				[Fact]
				public async Task Update_user_that_does_not_already_exists_inserts_new_record_and_returns_Success()
				{
					// arrange
					var user = new IdentityUser("Update_user_that_does_not_already_exists_inserts_new_record_and_returns_Success");

					// act
					var result = await _userStore.UpdateAsync(user);

					// assert
					IdentityResultAssert.IsSuccess(result);

					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					IdentityUserAssert.Equal(user, userFromDb);
				}

				[Fact]
				public async Task Can_update_user_multiple_times()
				{
					// arrange
					var user = new IdentityUser("Can_update_user_multiple_times");
					await _userStore.CreateAsync(user);

					// act
					user.Claims.Add(_identityClaim1);
					var result1 = await _userStore.UpdateAsync(user);

					user.UserName = user.UserName + " different";
					var result2 = await _userStore.UpdateAsync(user);

					// assert
					IdentityResultAssert.IsSuccess(result1);
					IdentityResultAssert.IsSuccess(result2);

					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					IdentityUserAssert.Equal(user, userFromDb);
				}

				[Fact]
				public async Task Updating_user_name_to_existing_name_returns_DuplicateUserName_error()
				{
					// arrange
					var user1 = new IdentityUser("Updating_user_name_to_existing_name_returns_DuplicateUserName_error");
					var user2 = new IdentityUser("Updating_user_name_to_existing_name_returns_DuplicateUserName_error different");

					await _userStore.CreateAsync(user1);
					await _userStore.CreateAsync(user2);

					// act
					user2.UserName = user1.UserName;
					var result3 = await _userStore.UpdateAsync(user2);

					// assert
					var expectedError = IdentityResult.Failed(_errorDescriber.DuplicateUserName(user2.ToString()));
					IdentityResultAssert.IsFailure(result3, expectedError.Errors.FirstOrDefault());
				}
			}

			public class DeleteAsyncMethod : IUserStoreTests
			{
				public DeleteAsyncMethod() : base(typeof(DeleteAsyncMethod).Name) { }

				[Fact]
				public async Task Delete_user_returns_Success()
				{
					// arrange
					var user = new IdentityUser("Delete_user_returns_Success");
					await _userStore.CreateAsync(user);


					// act
					var result = await _userStore.DeleteAsync(user);


					// assert
					IdentityResultAssert.IsSuccess(result);

					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					Assert.Null(userFromDb);
				}


				[Fact]
				public async Task Delete_user_that_does_not_exist_returns_Success()
				{
					// arrange
					var user = new IdentityUser("Delete_user_that_does_not_exist_returns_Success");


					// act
					var result = await _userStore.DeleteAsync(user);


					// assert
					IdentityResultAssert.IsSuccess(result);

					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					Assert.Null(userFromDb);
				}
			}

			public class FindByIdAsyncMethod : IUserStoreTests
			{
				public FindByIdAsyncMethod() : base(typeof(FindByIdAsyncMethod).Name) { }

				[Fact]
				public async Task Unknown_userId_returns_null()
				{
					// arrange
					var userId = "unknown userId";

					// act
					var result = await _userStore.FindByIdAsync(userId);

					// assert
					Assert.Null(result);
				}

				[Fact]
				public async Task Known_userId_returns_IdentityUser()
				{
					// arrange
					var user = new IdentityUser("Known_userId_returns_IdentityUser");
					await _userStore.CreateAsync(user);

					// act
					var result = await _userStore.FindByIdAsync(user.Id);

					// assert
					IdentityUserAssert.Equal(user, result);
				}
			}

			public class FindByNameAsyncMethod : IUserStoreTests
			{
				public FindByNameAsyncMethod() : base(typeof(FindByNameAsyncMethod).Name) { }

				[Fact]
				public async Task Unknown_normalizedUserName_returns_null()
				{
					// arrange
					var name = "unknown normalised name";

					// act
					var result = await _userStore.FindByNameAsync(name);

					// assert
					Assert.Null(result);
				}

				[Fact]
				public async Task Known_normalizedUserName_returns_IdentityUser()
				{
					// arrange
					var user = new IdentityUser("Known_normalizedUserName_returns_IdentityUser");
					user.NormalizedUserName = user.UserName;
					await _userStore.CreateAsync(user);

					// act
					var result = await _userStore.FindByNameAsync(user.NormalizedUserName);

					// assert
					IdentityUserAssert.Equal(user, result);
				}
			}

		}

		// TODO:
		public class IUserLoginStoreTests : UserStoreTests
		{
			public IUserLoginStoreTests(string collectionPrefix) : base(collectionPrefix) { }

			public class AddLoginAsyncMethod : IUserLoginStoreTests
			{
				public AddLoginAsyncMethod() : base(typeof(AddLoginAsyncMethod).Name)
				{
				}

				// TODO:
			}
		}

		public class IUserClaimStoreTests : UserStoreTests
		{
			public IUserClaimStoreTests(string collectionPrefix) : base(collectionPrefix)
			{
			}

			public class AddClaimsAsyncMethod : IUserClaimStoreTests
			{
				public AddClaimsAsyncMethod() : base(typeof(AddClaimsAsyncMethod).Name) { }

				[Fact]
				public async Task Adding_null_claims_to_user_does_not_update_database_user_record()
				{
					// arrange

					var user = new IdentityUser("Adding_new_claim_to_user_updates_database_user_record");
					await _userStore.CreateAsync(user);

					// act
					await _userStore.AddClaimsAsync(user, null);

					// assert

					// check user claims from memory
					Assert.NotNull(user.Claims);
					Assert.Empty(user.Claims);

					// check user claims from DB
					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					Assert.NotNull(userFromDb.Claims);
					Assert.Empty(userFromDb.Claims);
				}
				
				[Fact]
				public async Task Adding_empty_claims_to_user_does_not_update_database_user_record()
				{
					// arrange

					var user = new IdentityUser("Adding_new_claim_to_user_updates_database_user_record");
					await _userStore.CreateAsync(user);

					// act
					await _userStore.AddClaimsAsync(user, new List<Claim>());

					// assert

					// check user claims from memory
					Assert.NotNull(user.Claims);
					Assert.Empty(user.Claims);

					// check user claims from DB
					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					Assert.NotNull(userFromDb.Claims);
					Assert.Empty(userFromDb.Claims);
				}

				[Fact]
				public async Task Adding_new_claims_to_user_updates_database_user_record()
				{
					// arrange

					var user = new IdentityUser("Adding_new_claim_to_user_updates_database_user_record");
					await _userStore.CreateAsync(user);

					// act
					await _userStore.AddClaimsAsync(user, new List<Claim> { _claim1, _claim2 });

					// assert

					// check user claims from memory
					IdentityClaimAssert.Equal(new List<IdentityClaim> { _identityClaim1, _identityClaim2 }, user.Claims);

					// check user claims from DB
					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					IdentityClaimAssert.Equal(new List<IdentityClaim> { _identityClaim1, _identityClaim2 }, userFromDb.Claims);
				}

				[Fact]
				public async Task Adding_new_claim_to_user_with_null_claims_updates_database_user_record()
				{
					// arrange

					var user = new IdentityUser("Adding_new_claim_to_user_with_null_claims_updates_database_user_record");
					user.Claims = null;

					await _userStore.CreateAsync(user);

					// act
					await _userStore.AddClaimsAsync(user, new List<Claim> { _claim1, _claim2 });

					// assert

					// check user claims from memory
					IdentityClaimAssert.Equal(new List<IdentityClaim> { _identityClaim1, _identityClaim2 }, user.Claims);

					// check user claims from DB
					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					IdentityClaimAssert.Equal(new List<IdentityClaim> { _identityClaim1, _identityClaim2 }, userFromDb.Claims);
				}

				[Fact]
				public async Task Adding_existing_claim_to_user_does_not_update_database_user_record()
				{
					// arrange
					var user = new IdentityUser("Adding_existing_claim_to_user_does_not_update_database_user_record");
					user.Claims.Add(_identityClaim1);
					user.Claims.Add(_identityClaim2);
					await _userStore.CreateAsync(user);

					// act
					await _userStore.AddClaimsAsync(user, new List<Claim> { _claim1 });

					// assert

					// check user claims from memory
					IdentityClaimAssert.Equal(new List<IdentityClaim> { _identityClaim1, _identityClaim2 }, user.Claims);

					// check user claims from DB
					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					IdentityClaimAssert.Equal(new List<IdentityClaim> { _identityClaim1, _identityClaim2 }, userFromDb.Claims);
				}

				[Fact]
				public async Task Adding_new_claims_to_user_with_some_claims_updates_database_user_record()
				{
					// arrange

					var user = new IdentityUser("Adding_new_claims_to_user_with_some_claims_updates_database_user_record");
					user.Claims.Add(_identityClaim1);
					await _userStore.CreateAsync(user);

					// act
					await _userStore.AddClaimsAsync(user, new List<Claim> { _claim1, _claim2 });

					// assert

					// check user claims from memory
					IdentityClaimAssert.Equal(new List<IdentityClaim> { _identityClaim1, _identityClaim2 }, user.Claims);

					// check user claims from DB
					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					IdentityClaimAssert.Equal(new List<IdentityClaim> { _identityClaim1, _identityClaim2 }, userFromDb.Claims);
				}

				[Fact]
				public async Task Adding_multiple_claims_with_same_ClaimType_adds_multiple_claims_to_database()
				{
					// arrange
					var user = new IdentityUser("Adding_multiple_claims_with_same_ClaimType_adds_multiple_claims_to_database");
					await _userStore.CreateAsync(user);

					// act
					await _userStore.AddClaimsAsync(user, new List<Claim> { _claim1 });
					await _userStore.AddClaimsAsync(user, new List<Claim> { _claim1SameType });

					// assert
					// check user claims from memory
					IdentityClaimAssert.Equal(new List<IdentityClaim> { _identityClaim1, _identityClaim1SameType }, user.Claims);

					// check user claims from DB
					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					IdentityClaimAssert.Equal(new List<IdentityClaim> { _identityClaim1, _identityClaim1SameType }, userFromDb.Claims);
				}
			}

			public class RemoveClaimsAsyncMethod : IUserClaimStoreTests
			{
				public RemoveClaimsAsyncMethod() : base(typeof(RemoveClaimsAsyncMethod).Name)
				{
				}

				[Fact]
				public async Task Removing_unknown_claim_does_not_change_database_user_record()
				{
					// arrange
					var user = new IdentityUser("Removing_unknown_claim_does_not_change_database_user_record");
					user.Claims.Add(_identityClaim1);

					await _userStore.CreateAsync(user);

					// act
					await _userStore.RemoveClaimsAsync(user, new List<Claim> { _claim3 });

					// assert

					// check user claims from memory
					IdentityClaimAssert.Equal(new List<IdentityClaim> { _identityClaim1 }, user.Claims);

					// check user claims from DB
					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					IdentityClaimAssert.Equal(new List<IdentityClaim> { _identityClaim1 }, userFromDb.Claims);
				}

				[Fact]
				public async Task Removing_claim_from_null_user_claims_does_not_change_database_user_record()
				{
					// arrange
					var user = new IdentityUser("Removing_unknown_claim_and_user_claims_is_null_does_not_change_database_user_record");
					user.Claims = null;

					await _userStore.CreateAsync(user);

					// act
					await _userStore.RemoveClaimsAsync(user, new List<Claim> { _claim3 });

					// assert

					// check user claims from memory
					Assert.Equal(0, user.Claims.Count);

					// check user claims from DB
					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					Assert.Equal(0, userFromDb.Claims.Count);
				}

				[Fact]
				public async Task Remove_existing_claim_updates_database_user_record()
				{
					// arrange
					var user = new IdentityUser("Remove_existing_claim_updates_database_user_record");
					user.Claims.Add(_identityClaim1);
					user.Claims.Add(_identityClaim2);
					user.Claims.Add(_identityClaim3);
					await _userStore.CreateAsync(user);

					// act
					await _userStore.RemoveClaimsAsync(user, new List<Claim> { _claim1, _claim3 });

					// assert

					// check user claims from memory
					IdentityClaimAssert.Equal(new List<IdentityClaim> { _identityClaim2 }, user.Claims);

					// check user claims from DB
					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					IdentityClaimAssert.Equal(new List<IdentityClaim> { _identityClaim2 }, userFromDb.Claims);
				}

				[Fact]
				public async Task User_has_multiple_claims_with_same_ClaimType_removing_only_removes_claim_with_same_value()
				{
					// arrange
					var user =
						new IdentityUser("User_has_multiple_claims_with_same_ClaimType_removing_only_removes_claim_with_same_value");
					user.Claims.Add(_identityClaim1);
					user.Claims.Add(_identityClaim1SameType);
					await _userStore.CreateAsync(user);

					// act
					await _userStore.RemoveClaimsAsync(user, new List<Claim> { _claim1 });

					// assert

					// check user claims from memory
					IdentityClaimAssert.Equal(new List<IdentityClaim> { _identityClaim1SameType }, user.Claims);

					// check user claims from DB
					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					IdentityClaimAssert.Equal(new List<IdentityClaim> { _identityClaim1SameType }, userFromDb.Claims);
				}
			}
		}


		#region PROTECTED HELPER CLASS

		class UserStoreHelper : UserStore<IdentityUser, IdentityRole>
		{
			public UserStoreHelper(string connectionString, string databaseName = null, string collectionName = null) : base(connectionString, databaseName, collectionName) { }

			public UserStoreHelper(IMongoClient client, string databaseName = null, string collectionName = null) : base(client, databaseName, collectionName) { }

			public UserStoreHelper(IMongoDatabase database, string collectionName = null) : base(database, collectionName) { }

			public UserStoreHelper(IMongoCollection<IdentityUser> collection) : base(collection) { }

			#region helper methods to get protected fields

			public string DatabaseName => base._databaseName;
			public string CollectionName => base._collectionName;
			public IMongoClient MongoClient => base._client;
			public IMongoDatabase MongoDatabase => base._database;
			public IMongoCollection<IdentityUser> MongoCollection => base._collection;

			#endregion
		}

		#endregion
	}
}
