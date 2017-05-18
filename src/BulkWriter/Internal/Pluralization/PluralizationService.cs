//---------------------------------------------------------------------
// <copyright file="PluralizationService.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       venkatja
// @backupOwner willa
//---------------------------------------------------------------------
#if NETSTANDARD2_0
using System.Globalization;

namespace System.Data.Entity.Design.PluralizationServices
{
    internal abstract class PluralizationService
    {
        public CultureInfo Culture { get; protected set; }

        public abstract bool IsPlural(string word);
        public abstract bool IsSingular(string word);
        public abstract string Pluralize(string word);
        public abstract string Singularize(string word);

        /// <summary>
        /// Factory method for PluralizationService. Only support english pluralization.
        /// Please set the PluralizationService on the System.Data.Entity.Design.EntityModelSchemaGenerator
        /// to extend the service to other locales.
        /// </summary>
        /// <param name="culture">CultureInfo</param>
        /// <returns>PluralizationService</returns>
        public static PluralizationService CreateService(CultureInfo culture)
        {
            if (culture.TwoLetterISOLanguageName == "en")
            {
                return new EnglishPluralizationService();
            }
            else
            {
                throw new NotImplementedException(string.Format("The culture '{0}' is not supported.   Pluralization is currently only supported for the English language.", culture.DisplayName));
            }
        }
    }
}
#endif