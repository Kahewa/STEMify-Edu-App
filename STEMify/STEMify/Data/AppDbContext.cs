﻿using STEMify.Models;
using Microsoft.EntityFrameworkCore;

namespace STEMify.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<DifficultyLevel> DifficultyLevels { get; set; }
    }

}
