using Core.Models;
using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Web.Config
{
    public static class SeedExtention
    {
        public static async Task SeedState(this AdtDbContext adtDbContext)
        {
            if (!await adtDbContext.States.AnyAsync())
            {
                var states = new List<State>();
                using (var sr = new StreamReader("Config/State.csv"))
                {
                    var fullText = await sr.ReadToEndAsync();
                    var rows = fullText.Split("\n").Skip(1);
                    states.AddRange(rows.Select(row => row.Split(','))
                        .Select(columns => new State
                        {
                            Id = Guid.NewGuid(),
                            Name = columns[0].Trim(),
                            Code = columns[1].Trim(),
                            LastModifiedBy = "System"
                        }));
                }
                await adtDbContext.States.AddRangeAsync(states);
                await adtDbContext.TrySaveChangesAsync();
            }
        }

        public static async Task SeedLocalgovernment(this AdtDbContext adtDbContext)

        {
            var states = await adtDbContext.States.ToListAsync();
            if (!await adtDbContext.localGovernments.AnyAsync())
            {
                var localGovernments = new List<LocalGovernment>();
                using (var sr = new StreamReader("Config/LGA.csv"))
                {
                    var fullText = await sr.ReadToEndAsync();
                    var rows = fullText.Split("\n").Skip(1);
                    localGovernments.AddRange(rows.Select(row => row.Split(','))
                        .Select(columns => new LocalGovernment
                        {
                            Id = Guid.NewGuid(),
                            Name = columns[0].Trim(),
                            StateId = states.First(x => x.Code == columns[1].Trim()).Id,
                            LastModifiedBy = "System"
                        }));
                }
                await adtDbContext.localGovernments.AddRangeAsync(localGovernments);
                await adtDbContext.TrySaveChangesAsync();
            }
        }
    }
}
