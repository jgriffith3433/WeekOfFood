namespace ContainerNinja.Contracts.Enum
{
    public enum UnitType
    {
        None = 0,
        Bulk = 1,
        Ounce = 2,
        Teaspoon = 3,
        Tablespoon = 4,
        Pound = 5,
        Cup = 6,
        Cloves = 7,
        Can = 8,
        Whole = 9,
        Package = 10,
        Bar = 11,
        Bun = 12,
        Bottle = 13,
        Slice = 14,
        Box = 15,
        Bag = 16,
        Gallon = 17,
        Gram = 18,
        Milliliters = 19,
        Leaves = 20,
        Handful = 21,
    }

    public static class UnitTypeExtensionMethods
    {
        public static UnitType UnitTypeFromString(this string unitTypeStr)
        {
            switch (unitTypeStr.ToLower())
            {
                case "":
                case "none":
                    return UnitType.None;
                case "bulk":
                    return UnitType.Bulk;
                case "ounce":
                case "ounces":
                    return UnitType.Ounce;
                case "teaspoon":
                case "teaspoons":
                    return UnitType.Teaspoon;
                case "tablespoon":
                case "tablespoons":
                    return UnitType.Tablespoon;
                case "pound":
                case "pounds":
                    return UnitType.Pound;
                case "cup":
                case "cups":
                    return UnitType.Cup;
                case "clove":
                case "cloves":
                    return UnitType.Cloves;
                case "can":
                case "cans":
                    return UnitType.Can;
                case "whole":
                case "wholes":
                    return UnitType.Whole;
                case "package":
                case "packages":
                    return UnitType.Package;
                case "bar":
                case "bars":
                    return UnitType.Bar;
                case "bun":
                case "buns":
                    return UnitType.Bun;
                case "bottle":
                case "bottles":
                    return UnitType.Bottle;
                case "slice":
                case "slices":
                    return UnitType.Slice;
                case "box":
                case "boxes":
                    return UnitType.Box;
                case "bag":
                case "bags":
                    return UnitType.Bag;
                case "gallon":
                case "gallons":
                    return UnitType.Gallon;
                case "gram":
                case "grams":
                    return UnitType.Gram;
                case "milliliter":
                case "milliliters":
                    return UnitType.Milliliters;
                case "leaf":
                case "leaves":
                    return UnitType.Leaves;
                case "handful":
                case "handfuls":
                    return UnitType.Handful;
                default:
                    return UnitType.None;
            }
        }
    }
}
