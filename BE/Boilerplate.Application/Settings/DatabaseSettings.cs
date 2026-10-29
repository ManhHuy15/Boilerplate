using System;
using System.Collections.Generic;
using System.Text;

namespace Boilerplate.Application.Settings
{
    public sealed class DatabaseSettings
    {
        public const string SectionName = "Database";

        public string ConnectionString { get; init; } = string.Empty;
    }
}
