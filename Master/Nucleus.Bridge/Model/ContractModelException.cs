using System;
using System.Collections.Generic;
using System.Linq;

namespace Nucleus.Bridge.Model;

/// <summary>The contract breaks the bridge conventions. <see cref="Exception.Message"/> lists every problem.</summary>
public sealed class ContractModelException : Exception
{
    internal ContractModelException(IReadOnlyList<string> errors)
        : base("The bridge contract is invalid:" + string.Concat(errors.Select(e => Environment.NewLine + "  - " + e))) =>
        Errors = errors;

    /// <summary>One message per problem, prefixed by its location, for example <c>ILibraryService.Get: …</c>.</summary>
    public IReadOnlyList<string> Errors { get; }
}
