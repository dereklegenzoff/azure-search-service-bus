using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BulkIndexingAgent
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
}
