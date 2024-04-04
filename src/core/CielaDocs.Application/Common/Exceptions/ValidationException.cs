using FluentValidation.Results;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CielaDocs.Application.Common.Exceptions
{
    public sealed class ValidationException : ApplicationException
    {
        public ValidationException(IReadOnlyDictionary<string, string[]> errorsDictionary)
           : base("Validation Failure", "One or more validation errors occurred")
           => Errors = errorsDictionary;

        public IReadOnlyDictionary<string, string[]> Errors { get; }
    }
}
