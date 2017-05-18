//---------------------------------------------------------------------
// <copyright file="ICustomPluralizationMapping.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       leil
// @backupOwner jeffreed
//---------------------------------------------------------------------
#if NETSTANDARD2_0

namespace System.Data.Entity.Design.PluralizationServices
{
    internal interface ICustomPluralizationMapping
    {
        void AddWord(string singular, string plural);
    }
}

#endif