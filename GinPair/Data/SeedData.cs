using GinPair.Models;
using Microsoft.EntityFrameworkCore;

namespace GinPair.Data;

public static class SeedData
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Gin>().HasData(
            new Gin { GinId = 1, GinName = "Rhubarb & Ginger", Distillery = "Whitley Neill" },
            new Gin { GinId = 2, GinName = "Blood Orange", Distillery = "Whitley Neill" },
            new Gin { GinId = 3, GinName = "Scottish Raspberry", Distillery = "Caorunn" },
            new Gin { GinId = 4, GinName = "Lantic", Distillery = "Skylark" }
        );

        modelBuilder.Entity<Tonic>().HasData(
            new Tonic { TonicId = 1, TonicBrand = "FeverTree", TonicFlavour = "Indian" },
            new Tonic { TonicId = 2, TonicBrand = "FeverTree", TonicFlavour = "Light" },
            new Tonic { TonicId = 3, TonicBrand = "FeverTree", TonicFlavour = "Mediterranean" },
            new Tonic { TonicId = 4, TonicBrand = "FeverTree", TonicFlavour = "ElderFlower" },
            new Tonic { TonicId = 5, TonicBrand = "FeverTree", TonicFlavour = "Aromatic" }
        );

        modelBuilder.Entity<Pairing>().HasData(
            new Pairing { PairingId = 1, GinId = 1, TonicId = 4 },
            new Pairing { PairingId = 2, GinId = 2, TonicId = 3 },
            new Pairing { PairingId = 3, GinId = 3, TonicId = 3 },
            new Pairing { PairingId = 4, GinId = 4, TonicId = 2}
            );
    }
}
