using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ExceptionHandler;

public record ProblemDetailsDto(
    string? Title,
    string? Detail,
    int? Status
);
