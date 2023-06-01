namespace ContainerNinja.Contracts.ChatAI;

public record ChatAICommandEditProductUnitType : ChatAICommand
{
    public string Product { get; set; }

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
