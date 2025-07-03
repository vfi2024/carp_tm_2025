using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace carp_tm_delegati.Areas.Identity.Data;

// Add profile data for application users by adding properties to the carp_tm_delegatiUser class
public class carp_tm_delegatiUser : IdentityUser
{
    public string FirtName { get; set; }

    public string LastName { get; set; }
}

