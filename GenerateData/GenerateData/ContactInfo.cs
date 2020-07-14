using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenerateData
{
    public partial class ContactInfo
    {
        [System.ComponentModel.DataAnnotations.Key]
        [IsFilterable]
        public string Id { get; set; }

        [IsSearchable, IsFacetable, IsFilterable]
        [Analyzer(AnalyzerName.AsString.EnLucene)]
        public string FirstName { get; set; }

        [IsSearchable, IsFacetable, IsFilterable]
        [Analyzer(AnalyzerName.AsString.EnLucene)]
        public string LastName { get; set; }

        [IsSearchable, IsFacetable, IsFilterable]
        [Analyzer(AnalyzerName.AsString.EnLucene)]
        public string Email { get; set; }

        [IsSearchable, IsFacetable, IsFilterable]
        public string PhoneNumber { get; set; }

        [IsSearchable, IsFacetable, IsFilterable]
        public string ZipCode { get; set; }
    }

    // helper class for data generation
    public class personName
    {
        public string firstName { get; set; }
        public string lastName { get; set; }

    }

    // helper class for data generation
    public class areaCodeGeoZip
    {
        public string stateFips { get; set; }
        public string state { get; set; }
        public string stateAbbr { get; set; }
        public string zipcode { get; set; }
        public string county { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string areaCode { get; set; }
        public string lat { get; set; }
        public string lon { get; set; }
    }
}
