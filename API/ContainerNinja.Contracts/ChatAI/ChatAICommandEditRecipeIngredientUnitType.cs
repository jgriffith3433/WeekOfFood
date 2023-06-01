﻿namespace ContainerNinja.Contracts.ChatAI;

public record ChatAICommandEditRecipeIngredientUnitType : ChatAICommand
{
    public string Recipe { get; set; }

    public string Name { get; set; }
    public string Ingredient
    {
        get { return Name; }
        set { Name = value; }
    }
    public string UnitType { get; set; }
    public string New
    {
        get { return UnitType; }
        set { UnitType = value; }
    }
    public string New_UnitType
    {
        get { return UnitType; }
        set { UnitType = value; }
    }
}
