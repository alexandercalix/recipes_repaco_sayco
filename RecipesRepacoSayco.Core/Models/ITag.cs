using System;

namespace RecipesRepacoSayco.Core.Models;

public interface ITag
{
    string Name { get; }
    string Datatype { get; }
    string Address { get; }
    bool Quality { get; }
    int? Length { get; }
    object Value { get; set; }
}
