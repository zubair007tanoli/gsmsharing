using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Fluxdox.Models
{
    /// <summary>
    /// Request model for generating a presigned URL.
    /// </summary>
    public class PresignRequest
    {
        [Required(ErrorMessage = "File name is required.")]
        public string FileName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for initiating a merge operation.
    /// </summary>
    public class MergeRequest
    {
        [MinLength(1, ErrorMessage = "At least one file key is required for merging.")]
        public List<string> FileKeys { get; set; } = new List<string>();
    }
}