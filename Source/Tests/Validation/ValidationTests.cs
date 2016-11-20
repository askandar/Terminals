﻿using System;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Terminals.Data;
using Terminals.Data.DB;
using Terminals.Data.Validation;

namespace Tests.Validation
{
    [TestClass]
    public class ValidationTests
    {
        private static string longText;

        [ClassInitialize]
        public static void InitializeLongText(TestContext context)
        {
            StringBuilder longTextBuilder = new StringBuilder();
            for (int index = 0; index < 500; index++)
            {
                longTextBuilder.Append(index.ToString());
            }

            longText = longTextBuilder.ToString();
        }

        [TestMethod]
        public void Favorite_Validate_ReturnsErrors()
        {
            var favorite = new Favorite();
            favorite.Protocol = longText.Substring(0, 11);
            favorite.ServerName = longText;
            var results = Validations.Validate(favorite);
            Assert.AreEqual(3, results.Count(), "Some properties arent validated properly for Favorite");
        }

        [TestMethod]
        public void DbFavoriteValidationTest()
        {
            // created dbfavorite is not compleate, only necessary to make validable using IFavorite
            var favorite = new DbFavorite();
            favorite.ExecuteBeforeConnect = new DbBeforeConnectExecute();
            favorite.Security = new DbSecurityOptions();
            favorite.Details.LoadFieldsFromReferences();
            
            favorite.Protocol = longText.Substring(0, 11);
            favorite.ServerName = longText;
            favorite.Name = longText;
            favorite.Notes = longText;
            
            favorite.ExecuteBeforeConnect.Command = longText;
            favorite.ExecuteBeforeConnect.CommandArguments = longText;
            favorite.ExecuteBeforeConnect.InitialDirectory = longText;
            var results = Validations.Validate(favorite);

            Assert.AreEqual(9, results.Count(), "Some properties arent validated properly for DbFavorite");
            var serverNameErrors = results.Count(result => result.PropertyName == "ServerName");
            Assert.AreEqual(2, serverNameErrors, "DbFavorite ServerName wasnt validated properly");
            var protocolErrors = results.Count(result => result.PropertyName == "Protocol");
            Assert.AreEqual(2, protocolErrors, "DbFavorite protocol wasnt validated properly");
        }

        [TestMethod]
        public void EmptyGroupName_ValidateGroup_ReturnsError()
        {
            AssertGroupValidation(new Group());
        }

        [TestMethod]
        public void EmptyDbGroupName_ValidateGroup_ReturnsError()
        {
            AssertGroupValidation(new DbGroup());
        }

        private static void AssertGroupValidation(IGroup group)
        {
            group.Name = String.Empty;
            var results = Validations.Validate(group);
            Assert.AreEqual(1, results.Count, "Group name validation failed");
        }

        [TestMethod]
        public void Credential_Validate_ReturnsError()
        {
            AssertCredentialsValidation(new CredentialSet(), 1);
        }

        [TestMethod]
        public void DbCredential_Validate_ReturnsError()
        {
            AssertCredentialsValidation(new DbCredentialSet(), 2);
        }

        private static void AssertCredentialsValidation(ICredentialSet credentailSet, int expectedErrorsCount)
        {
            credentailSet.Name = longText;
            var results = Validations.Validate(credentailSet);
            Assert.AreEqual(expectedErrorsCount, results.Count(), "CredentailSet validation failed");
        }

        [TestMethod]
        public void LongDbGroupName_Validate_ReturnsNameError()
        {
            var group = new DbGroup();
            group.Name = longText;
            AssertNameOnlyValidation(group);
        }

        [TestMethod]
        public void EmptyDbGroupName_Validate_ReturnsNameError()
        {
            var group = new DbGroup();
            group.Name = string.Empty;
            AssertNameOnlyValidation(group);
        }

        private static void AssertNameOnlyValidation(DbGroup group)
        {
            ValidationStates results = Validations.ValidateNameProperty(group);
            Assert.AreEqual(1, results.Count(), "Group name validation failed");
            Assert.AreEqual(Validations.NAME_PROPERTY, results.First().PropertyName, "Failed property is not a 'Name'");
        }
    }
}