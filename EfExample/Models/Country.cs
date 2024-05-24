﻿namespace EfExample.Models;

public partial class Country
{
    public int IdCountry { get; set; }
    public string Name { get; set; }
    
    public virtual ICollection<CountryTrip> CountryTrips { get; set; } = new List<CountryTrip>();
}