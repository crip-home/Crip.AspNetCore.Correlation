using System.Collections.Generic;
using System.Threading.Tasks;

namespace Crip.AspNetCore.Correlation.Core31.Example.Clients;

public interface ITestControllerClient
{
    Task<Dictionary<string, string>> Test();
}