
using TransformadoresApp.Models;

namespace TransformadoresApp.Data
{
    public static class DbSeeder
    {
        // =====================
        // SEED
        // =====================

        public static void Seed(ApplicationDbContext context)
        {
            SeedUnitOfMeasures(context);
        }

        // =====================
        // UNIT OF MEASURES
        // =====================

        private static void SeedUnitOfMeasures(ApplicationDbContext context)
        {
            var defaultUnits = new[]
            {
                new { Name = "Kilogramo", Abbreviation = "kg" },
                new { Name = "Metro", Abbreviation = "m" },
                new { Name = "Unidad", Abbreviation = "und" }
            };

            var unitsToAdd = defaultUnits
                .Where(defaultUnit => !context.UnitOfMeasures.Any(unit =>
                    unit.Abbreviation == defaultUnit.Abbreviation ||
                    unit.Name == defaultUnit.Abbreviation ||
                    unit.Name == defaultUnit.Name))
                .Select(defaultUnit => new UnitOfMeasure
                {
                    Name = defaultUnit.Name,
                    Abbreviation = defaultUnit.Abbreviation
                })
                .ToList();

            if (unitsToAdd.Count == 0)
            {
                return;
            }

            context.UnitOfMeasures.AddRange(unitsToAdd);
            context.SaveChanges();
        }
    }
}
