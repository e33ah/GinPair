using GinPair.Models;
using Microsoft.EntityFrameworkCore;

namespace GinPair.Data
{
    public static class SeedData
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tonic>().HasData(
                new Tonic { TonicId = 1, TonicBrand = "FeverTree", TonicFlavour = "Indian"},
                new Tonic { TonicId = 2, TonicBrand = "FeverTree", TonicFlavour = "Light"},
                new Tonic { TonicId = 3, TonicBrand = "FeverTree", TonicFlavour = "Mediterranean" },
                new Tonic { TonicId = 4, TonicBrand = "FeverTree", TonicFlavour = "ElderFlower" },
                new Tonic { TonicId = 5, TonicBrand = "FeverTree", TonicFlavour = "Aromatic" }
            );
        }
    }

}
