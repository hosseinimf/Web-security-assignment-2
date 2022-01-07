#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WS_uppgift2.Models;

namespace WS_uppgift2.Data
{
    public class WS_uppgift2Context : DbContext
    {
        public WS_uppgift2Context (DbContextOptions<WS_uppgift2Context> options)
            : base(options)
        {
        }

        public DbSet<Comments> Comments { get; set; }
        public DbSet<AppFile> AppFile { get; set; }
    }
}
